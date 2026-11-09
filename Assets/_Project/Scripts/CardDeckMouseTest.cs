#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using UnityEngine;

namespace Hatdon.CardStacker
{
    public sealed class CardDeckMouseTest : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void StartIfRequested()
        {
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-cardDeckMouseTest")>=0)
                new GameObject("Deck and mouse verification").AddComponent<CardDeckMouseTest>();
        }
        private IEnumerator Start()
        {
            yield return null;
            try
            {
                var c=FindFirstObjectByType<CardStackerControls>();var s=c.Placement;var m=c.Manipulator;
                Require(!s.HasCard(0) && !s.HasCard(1) && c.Selected==null && c.Deck.Remaining==52,"Table begins empty with a full deck");
                Vector2 stock=Camera.main.WorldToScreenPoint(c.Deck.StockPosition);
                Require(c.Deck.IsHit(stock),"Deck can be picked in the 3D scene");
                var first=c.DrawFromDeck(stock);Require(first!=null && s.HasCard(0) && !s.HasCard(1) && c.Deck.Remaining==51,"One draw creates exactly one held card");
                Vector2 target=Camera.main.WorldToScreenPoint(new Vector3(-.1f,.06f,0));
                for(int i=1;i<=30;i++)m.DragPointerTo(Vector2.Lerp(stock,target,i/30f),false);
                m.EndDrag();
                Require(Vector3.Distance(first.position,new Vector3(-.1f,.06f,0))<.0001f,"Initial deck drag follows a safe plane above the desk");
                Require(c.Deck.Remaining==51,"Dragging does not draw additional cards");
                c.FocusSelection();

                Vector3 position=first.position;Quaternion rotation=first.rotation;
                Vector2 centre=Camera.main.WorldToScreenPoint(position);
                m.SetTool(CardManipulator.Tool.Move);m.BeginPointerDrag(centre);m.DragPointerTo(centre+new Vector2(25,15),false);m.EndDrag();
                Vector3 normal=first.position-position;
                Require(normal.magnitude>.002f && Mathf.Abs(Vector3.Dot(normal,Camera.main.transform.forward))<.00001f,"W movement stays in the camera view plane");
                Pose(first,position,rotation);m.BeginPointerDrag(centre);m.DragPointerTo(centre+new Vector2(25,15),true);m.EndDrag();
                Require(Mathf.Abs((first.position-position).magnitude/normal.magnitude-.1f)<.002f,"Shift gives 10x fine movement");
                Pose(first,position,rotation);
                m.BeginPointerDrag(centre);
                m.DragPointerTo(centre,false,1);
                Vector3 afterScroll=first.position;
                Require(Vector3.Dot(afterScroll-position,Camera.main.transform.forward)<-.005f,"Wheel brings a held card nearer in Move mode");
                m.DragPointerTo(centre,false);
                Require(Vector3.Distance(first.position,afterScroll)<.000001f,"Holding still after scrolling does not jump");
                m.DragPointerTo(centre+new Vector2(25,15),false);
                Require(Vector3.Distance(first.position,afterScroll)>.002f && Mathf.Abs(Vector3.Dot(first.position-afterScroll,Camera.main.transform.forward))<.00001f,"The same grab continues moving across the new depth plane");
                m.EndDrag();
                Pose(first,position,rotation);
                m.SetTool(CardManipulator.Tool.Depth);m.BeginPointerDrag(centre);m.DragPointerTo(centre+Vector2.up*20,false);m.EndDrag();
                Vector3 depth=first.position-position;
                Require(depth.magnitude>.001f && Vector3.Cross(depth,Camera.main.transform.forward).magnitude<.00001f,"Q depth follows camera forward");
                Pose(first,position,rotation);
                m.SetTool(CardManipulator.Tool.Rotate);m.BeginPointerDrag(centre);m.DragPointerTo(centre+new Vector2(35,20),false);m.EndDrag();
                float angle=Quaternion.Angle(rotation,first.rotation);
                Require(angle>5 && first.position==position,"E arcball rotates around a fixed card midpoint");
                Pose(first,position,rotation);m.BeginPointerDrag(centre);m.DragPointerTo(centre+new Vector2(35,20),true);m.EndDrag();
                Require(Mathf.Abs(Quaternion.Angle(rotation,first.rotation)/angle-.1f)<.005f && first.position==position,"Shift fine rotation keeps the pivot fixed");
                Pose(first,position,rotation);
                m.SetTool(CardManipulator.Tool.Roll);m.BeginPointerDrag(centre);m.DragPointerTo(centre+Vector2.right*30,false);m.EndDrag();
                Require(Quaternion.Angle(rotation,first.rotation)>5 && first.position==position,"T twist preserves card centre");
                Pose(first,position,rotation);s.LockHand(0);c.HandVisuals.Refresh();
                Require(c.HandVisuals.VisibleCount==1,"Lock shows one ghost hand");
                Require(Vector3.Dot(c.HandVisuals.FingerDirection(0),Vector3.down)>.999f,"Ghost fingers point down regardless of card rotation");
                c.SelectCard(first);c.HandVisuals.Refresh();
                Require(c.HandVisuals.VisibleCount==0,"Reopening a lock hides its ghost hand");s.LockHand(0);

                c.ResetView();var second=c.Deck.Draw();Require(second!=null && second!=first && s.HasCard(1) && c.Deck.Remaining==50,"Second draw uses the other free hand");
                Pose(second,new Vector3(.1f,.06f,0),Quaternion.Euler(-90,0,0));
                int count=c.AllCards.Count;Require(c.Deck.Draw()==null && c.AllCards.Count==count && c.Deck.Remaining==50,"Full hands cannot consume another card");
                c.SelectCard(second);s.LockHand(1);c.HandVisuals.Refresh();
                Require(c.HandVisuals.VisibleCount==2,"Both locks show both ghost hands");
                Require(s.CommitBoth(),"Drawn pair commits normally");c.HandVisuals.Refresh();
                Require(c.HandVisuals.VisibleCount==0,"Release removes all ghost hands");
                Require(!s.HasCard(0) && !s.HasCard(1) && !first.isKinematic && !second.isKinematic,"Commit frees both hands and leaves both cards dynamic");
                c.ActivateHand(0);var third=c.Deck.Draw();
                Require(third!=null && third!=first && third!=second && c.Deck.Remaining==49,"A new card can be drawn after committing");
                Require(first.gameObject.activeInHierarchy && second.gameObject.activeInHierarchy && !first.isKinematic && !second.isKinematic,"Drawing the next card does not reset existing structure");
                m.EndDrag();c.SelectCard(third);Pose(third,new Vector3(.1f,.15f,0),Quaternion.identity);
                Require(!m.Translate(Vector3.down*.12f),"Newly drawn card respects already placed card contacts");
                s.ResetCards();Require(c.Deck.Remaining==52 && !s.HasCard(0) && !s.HasCard(1) && FindObjectsByType<Rigidbody>(FindObjectsSortMode.None).Length==0,"Reset clears table and refills deck");
                Debug.Log("HATDON_DECK_MOUSE_TEST_OK: empty start, deck picking, single draw, 2-hand cap, plane/depth/fine/arcball/twist, fixed rotation pivot, continuing after commit, placed contact and reset passed.");
                Application.Quit(0);
            }
            catch(Exception error){Debug.LogException(error);Application.Quit(1);}
        }
        private static void Pose(Rigidbody body,Vector3 p,Quaternion q){body.position=p;body.rotation=q;Physics.SyncTransforms();}
        private static void Require(bool value,string message){if(!value)throw new Exception(message);}
    }
}
#endif
