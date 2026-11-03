#if HATDON_CAPTURE
using UnityEngine;

namespace Hatdon.CardStacker
{
    public sealed partial class CardStackerDevlogCapture
    {
        private bool deckTake;
        private Vector2 deckTarget,mouseStart;
        private Vector3 mousePivot;

        private void DemonstrateDeck(int frame)
        {
            showPointer=true;
            PullFromDeck(frame,60,60,new Vector3(-.09f,.06f,.01f));
            if(frame==138)controls.FocusSelection();
            MouseGesture(frame,150,36,CardManipulator.Tool.Depth,new Vector2(0,-25));
            MouseGesture(frame,195,55,CardManipulator.Tool.Rotate,new Vector2(25,-95));
            if(frame==251)gestureFrom=pointer;
            if(frame>=251 && frame<270)pointer=Vector2.Lerp(gestureFrom,new Vector2(770,112),Ease((frame-251)/18f));
            if(frame==270)controls.Placement.LockHand(0);
            if(frame==285)controls.ResetView();
            PullFromDeck(frame,330,60,new Vector3(.09f,.06f,.03f));
            MouseGesture(frame,405,35,CardManipulator.Tool.Move,new Vector2(20,25));
            MouseGesture(frame,450,30,CardManipulator.Tool.Roll,new Vector2(30,0));
            if(frame==481)gestureFrom=pointer;
            if(frame>=481 && frame<500)pointer=Vector2.Lerp(gestureFrom,new Vector2(1135,112),Ease((frame-481)/18f));
            if(frame==500)controls.Placement.LockHand(1);
            if(frame==501)gestureFrom=pointer;
            if(frame>=501 && frame<525)pointer=Vector2.Lerp(gestureFrom,new Vector2(1325,112),Ease((frame-501)/23f));
            if(frame==525)
            {
                if(!controls.Placement.CommitBoth()){Debug.LogError("Deck take commit failed.");Application.Quit(13);}
                else Debug.Log("HATDON_DECK_PAIR_PLACED: Both hands freed; placed bodies remain dynamic.");
            }
            PullFromDeck(frame,610,60,new Vector3(-.04f,.06f,-.10f));
            if(frame==690)controls.FocusSelection();
            MouseGesture(frame,705,40,CardManipulator.Tool.Rotate,new Vector2(-25,-90));
            MouseGesture(frame,760,30,CardManipulator.Tool.Roll,new Vector2(-22,0),true);
            if(frame==810){controls.BeginOrbit();gestureFrom=pointer;naturalDragging=true;}
            if(frame>=810 && frame<870)
            {
                Vector2 next=gestureFrom+new Vector2(45,0)*Ease((frame-810)/59f);
                controls.Orbit(next-pointer);pointer=next;
            }
            if(frame==870)naturalDragging=false;
        }

        private void PullFromDeck(int frame,int start,int length,Vector3 destination)
        {
            if(frame==start-30)gestureFrom=pointer;
            if(frame>=start-30 && frame<start)
            {
                Vector2 stock=Camera.main.WorldToScreenPoint(controls.Deck.StockPosition);
                pointer=Vector2.Lerp(gestureFrom,stock,Ease((frame-start+30)/29f));
            }
            if(frame==start)
            {
                if(!controls.Deck.IsHit(pointer) || controls.DrawFromDeck(pointer)==null)
                {Debug.LogError("Deck take could not pick/draw the stock at "+frame);Application.Quit(14);}
                gestureFrom=pointer;deckTarget=Camera.main.WorldToScreenPoint(destination);naturalDragging=true;
                Debug.Log("HATDON_DECK_DRAW: "+controls.Deck.Remaining+" cards remain.");
            }
            if(frame>start && frame<=start+length)
            {
                pointer=Vector2.Lerp(gestureFrom,deckTarget,Ease((float)(frame-start)/length));
                controls.Manipulator.DragPointerTo(pointer,false);
                if(frame==start+length){controls.Manipulator.EndDrag();naturalDragging=false;}
            }
        }

        private void MouseGesture(int frame,int start,int length,CardManipulator.Tool mode,Vector2 delta,bool fine=false)
        {
            if(frame<start || frame>start+length)return;
            var m=controls.Manipulator;
            if(frame==start){gestureFrom=pointer;mouseStart=Camera.main.WorldToScreenPoint(controls.Selected.position);m.SetTool(mode);}
            int offset=frame-start;
            if(offset<6){pointer=Vector2.Lerp(gestureFrom,mouseStart,Ease(offset/5f));return;}
            if(offset==6)
            {
                pointer=mouseStart;mousePivot=controls.Selected.position;
                if(controls.PickCard(pointer)!=controls.Selected || !m.BeginPointerDrag(pointer))
                {Debug.LogError("Deck take could not grab its selected card.");Application.Quit(15);}
                naturalDragging=true;
            }
            pointer=mouseStart+delta*Ease((float)(offset-6)/(length-6));m.DragPointerTo(pointer,fine);
            if(offset==length)
            {
                if((mode==CardManipulator.Tool.Rotate || mode==CardManipulator.Tool.Roll) && Vector3.Distance(mousePivot,controls.Selected.position)>.000001f)
                {Debug.LogError("Mouse rotation moved the card pivot.");Application.Quit(16);}
                m.EndDrag();naturalDragging=false;
            }
        }

        private bool DeckTakeValid()
        {
            int placed=0;foreach(var card in controls.AllCards)if(card!=null && card.gameObject.activeInHierarchy && !card.isKinematic)placed++;
            bool valid=controls.Deck.Remaining==49 && placed==2 && controls.Placement.HasCard(0) && !controls.Placement.HasCard(1);
            if(valid)Debug.Log("HATDON_DECK_TAKE_OK: Three cards drawn singly; two placed bodies remain; third held with fixed-centre mouse rotation.");
            return valid;
        }
    }
}
#endif
