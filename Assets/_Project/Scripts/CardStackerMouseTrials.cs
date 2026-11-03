#if HATDON_CAPTURE
using UnityEngine;

namespace Hatdon.CardStacker
{
    public sealed partial class CardStackerDevlogCapture
    {
        private bool mouseTrials,mouseContact,validateMouseOnly,tableStopSeen,cardStopSeen;
        private float mouseLean;
        private Vector2 trialTwist,trialSwing;
        private Vector3 trialPosition,trialPivot,lockedTrialPosition,assembledRight;
        private Quaternion lockedTrialRotation;

        private void DemonstrateMouseTrials(int frame)
        {
            showPointer=true;
            if(frame==0)pointer=new Vector2(1230,330);
            float radians=mouseLean*Mathf.Deg2Rad;
            float height=controls.Hands[0].transform.localScale.y,thickness=CardPlacementSession.CardThickness;
            float x=(height*Mathf.Sin(radians)+thickness*Mathf.Cos(radians))*.5f;
            float y=(height*Mathf.Cos(radians)+thickness*Mathf.Sin(radians))*.5f+.000005f;
            Vector3 left=new Vector3(-x,y,0),right=new Vector3(x,y,0);
            PullFromDeck(frame,60,40,new Vector3(-.105f,.06f,0));
            if(frame==110){controls.FocusSelection();controls.QuickView(0);}
            OrientMouse(frame,120,Quaternion.AngleAxis(-mouseLean,Vector3.forward)*Quaternion.AngleAxis(90,Vector3.up));
            PositionMouse(frame,260,30,left,true);
            PositionMouse(frame,300,40,left,false);
            if(frame==341)CheckTrial(Vector3.Distance(controls.Selected.position,left)<.00003f,"Left card reaches its foot placement");
            if(frame==350)
            {
                controls.Placement.LockHand(0);lockedTrialPosition=controls.Hands[0].position;lockedTrialRotation=controls.Hands[0].rotation;
                controls.HandVisuals.Refresh();CheckTrial(controls.HandVisuals.VisibleCount==1,"Left hand visible when locked");
            }
            if(frame==365)controls.ResetView();
            // Carry the second flat card behind the first locked card before approaching the tent.
            PullFromDeck(frame,390,40,new Vector3(.105f,.06f,.16f));
            if(frame==440){controls.FocusSelection();controls.QuickView(0);}
            if(mouseContact)
            {
                Vector3 below=new Vector3(.105f,-.012f,.16f),above=new Vector3(.105f,.06f,.16f);
                PositionMouse(frame,450,30,below,true);
                PositionMouse(frame,490,40,below,false,true);
                if(frame==531)
                {
                    var card=controls.Selected;
                    tableStopSeen=controls.Manipulator.ContactBlocked && card.position.y>=.00015f && card.position.y<.001f;
                    CheckTrial(tableStopSeen,"Table blocks the downward mouse drag");
                    Debug.Log("HATDON_MOUSE_TABLE_STOP: Requested 12mm below the table; card stops at "+(card.position.y*1000).ToString("F4")+"mm centre height.");
                }
                PositionMouse(frame,550,30,above,true);
                PositionMouse(frame,590,30,above,false);
            }
            int delay=mouseContact ? 200 : 0;
            OrientMouse(frame,450+delay,Quaternion.AngleAxis(mouseLean,Vector3.forward)*Quaternion.AngleAxis(-90,Vector3.up));
            Vector3 staging=new Vector3(x,.10f,0);
            PositionMouse(frame,590+delay,30,staging,true);
            PositionMouse(frame,630+delay,40,staging,false);
            if(frame==672+delay)controls.FocusSelection();
            PositionMouse(frame,680+delay,30,right,true);
            PositionMouse(frame,720+delay,40,right,false);
            if(frame==761+delay)
            {
                CheckTrial(Vector3.Distance(controls.Selected.position,right)<.00015f,"Right card reaches the leaning pair; actual "+controls.Selected.position.ToString("F6")+"; target "+right.ToString("F6"));
                assembledRight=controls.Selected.position;
                Debug.Log("HATDON_MOUSE_PLACED: Right card stops within "+(Vector3.Distance(assembledRight,right)*1000).ToString("F4")+"mm of the target through swept contact.");
            }
            if(frame>=762+delay)right=assembledRight;
            if(frame==762+delay)controls.FocusSelection();
            if(frame==765+delay){controls.BeginOrbit();gestureFrom=pointer;naturalDragging=true;}
            if(frame>=765+delay && frame<786+delay)
            {
                Vector2 next=gestureFrom+new Vector2(120,0)*Ease((frame-765-delay)/20f);
                controls.Orbit(next-pointer);pointer=next;
            }
            if(frame==786+delay)naturalDragging=false;
            if(mouseContact)
            {
                PositionMouse(frame,1000,36,right-Vector3.right*.012f,false,true);
                if(frame==1037)
                {
                    cardStopSeen=controls.Manipulator.ContactBlocked && controls.Selected.position.x>right.x-.00005f;
                    CheckTrial(cardStopSeen && controls.Placement.IsLocked(0) && controls.Hands[0].position==lockedTrialPosition && controls.Hands[0].rotation==lockedTrialRotation,"Locked card blocks crossing and stays still");
                    Debug.Log("HATDON_MOUSE_CARD_STOP: 12mm crossing request blocked; locked card position and rotation unchanged.");
                }
                PositionMouse(frame,1060,30,right+Vector3.right*.008f,false);
                PositionMouse(frame,1110,30,right,false);
            }
            int lockFrame=mouseContact ? 1150 : 790,releaseFrame=mouseContact ? 1190 : 830;
            if(frame==lockFrame)
            {
                controls.Placement.LockHand(1);controls.HandVisuals.Refresh();
                CheckTrial(controls.HandVisuals.VisibleCount==2,"Both locked hands visible");
                Debug.Log("HATDON_GHOST_HANDS_LOCKED: Two translucent grips; visuals have no physics support.");
            }
            if(frame==releaseFrame)
            {
                releasedPositions=new[]{controls.Hands[0].position,controls.Hands[1].position};
                CheckTrial(controls.Placement.CommitBoth(),"Mouse-built pair commits");controls.HandVisuals.Refresh();
                CheckTrial(controls.HandVisuals.VisibleCount==0,"Both grips disappear on release");
                Debug.Log("HATDON_MOUSE_PAIR_RELEASED: "+mouseLean+" degrees; no pose assignment, constraints, impulse or post-release correction.");
            }
            if(frame==releaseFrame+12){controls.SelectCard(controls.Hands[0]);controls.FocusSelection();}
            if(frame==releaseFrame+35){controls.BeginOrbit();gestureFrom=pointer;naturalDragging=true;}
            if(frame>=releaseFrame+35 && frame<releaseFrame+125)
            {
                Vector2 next=gestureFrom+new Vector2(125,-8)*Ease((frame-releaseFrame-35)/89f);
                controls.Orbit(next-pointer);pointer=next;
            }
            if(frame==releaseFrame+125)naturalDragging=false;
        }

        // Invert the public mouse mapping for a repeatable demonstration. Never set a card pose.
        // Twist followed by two <=90-degree trackball strokes can reach any target orientation.
        private void OrientMouse(int frame,int start,Quaternion target)
        {
            if(frame==start)
            {
                trialPivot=controls.Selected.position;
                Quaternion camera=Camera.main.transform.rotation;
                Quaternion delta=Quaternion.Inverse(camera)*(target*Quaternion.Inverse(controls.Selected.rotation))*camera;
                Quaternion twist=new Quaternion(0,0,delta.z,delta.w).normalized;
                Quaternion swing=delta*Quaternion.Inverse(twist);
                float angle=Mathf.DeltaAngle(0,2*Mathf.Atan2(twist.z,twist.w)*Mathf.Rad2Deg);
                trialTwist=new Vector2(-angle/.35f,0);
                Vector3 end=Quaternion.Slerp(Quaternion.identity,swing,.5f)*Vector3.forward;
                trialSwing=new Vector2(end.x,end.y)*(120*Screen.height/1080f);
            }
            MouseGesture(frame,start,30,CardManipulator.Tool.Roll,trialTwist);
            MouseGesture(frame,start+40,40,CardManipulator.Tool.Rotate,trialSwing);
            MouseGesture(frame,start+90,40,CardManipulator.Tool.Rotate,trialSwing);
            if(frame==start+131)
                CheckTrial(Quaternion.Angle(controls.Selected.rotation,target)<.08f && Vector3.Distance(trialPivot,controls.Selected.position)<.000001f,"Mouse rotation reaches target around an unchanged centre; angle error "+Quaternion.Angle(controls.Selected.rotation,target)+"; position "+controls.Selected.position);
        }

        private void PositionMouse(int frame,int start,int length,Vector3 target,bool depth,bool expectBlocked=false)
        {
            if(frame<start || frame>start+length)return;
            var m=controls.Manipulator;var camera=Camera.main;
            if(frame==start)
            {
                trialPosition=controls.Selected.position;gestureFrom=pointer;
                mouseStart=camera.WorldToScreenPoint(trialPosition);m.SetTool(depth ? CardManipulator.Tool.Depth : CardManipulator.Tool.Move);
            }
            int offset=frame-start;
            if(offset<6){pointer=Vector2.Lerp(gestureFrom,mouseStart,Ease(offset/5f));return;}
            if(offset==6)
            {
                pointer=mouseStart;
                CheckTrial(controls.PickCard(pointer)==controls.Selected && m.BeginPointerDrag(pointer),"Selected card can be grabbed by the mouse");naturalDragging=true;
            }
            Vector3 desired=Vector3.Lerp(trialPosition,target,Ease((float)(offset-6)/(length-6)));
            if(depth)
            {
                float metres=2*Vector3.Distance(camera.transform.position,controls.Selected.position)*Mathf.Tan(camera.fieldOfView*Mathf.Deg2Rad/2)/Screen.height;
                pointer.y+=Vector3.Dot(desired-controls.Selected.position,camera.transform.forward)/metres;
            }
            else
            {
                // Project the destination onto the fixed W plane, matching a real view-plane drag.
                desired-=camera.transform.forward*Vector3.Dot(desired-trialPosition,camera.transform.forward);
                pointer=camera.WorldToScreenPoint(desired);
            }
            m.DragPointerTo(pointer,false);
            if(offset==length)
            {
                if(!expectBlocked && depth)CheckTrial(Mathf.Abs(Vector3.Dot(target-controls.Selected.position,camera.transform.forward))<.00003f,"Mouse depth reaches its target plane; error "+Vector3.Dot(target-controls.Selected.position,camera.transform.forward)+"; position "+controls.Selected.position+"; blocked "+m.ContactBlocked);
                m.EndDrag();naturalDragging=false;
            }
        }

        private bool MouseTrialsValid()
        {
            bool valid=controls.Placement.IsCommitted && releasedPositions!=null && controls.Deck.Remaining==50;
            float drift=0;
            for(int i=0;i<2;i++)
            {
                var card=controls.Hands[i];
                valid&=!card.isKinematic && card.constraints==RigidbodyConstraints.None && card.position.y>.025f && Vector3.Dot(card.rotation*Vector3.up,Vector3.up)>.7f;
                if(releasedPositions!=null)drift=Mathf.Max(drift,Vector3.Distance(card.position,releasedPositions[i]));
            }
            valid&=drift<.005f && controls.HandVisuals.VisibleCount==0 && (!mouseContact || (tableStopSeen && cardStopSeen));
            if(valid)Debug.Log("HATDON_MOUSE_TRIAL_OK: "+mouseLean+" degree pair standing after 13.6 released seconds; drift "+(drift*1000).ToString("F4")+"mm; contact tests "+mouseContact);
            else Debug.LogError("Mouse trial final state failed; drift "+drift);
            return valid;
        }

        private void CheckTrial(bool success,string message)
        {
            if(success)return;
            Debug.LogError("MOUSE_TRIAL_FAILED at frame "+Time.frameCount+": "+message);Application.Quit(22);
        }
    }
}
#endif
