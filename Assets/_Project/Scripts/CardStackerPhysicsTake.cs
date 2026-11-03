#if HATDON_CAPTURE
using UnityEngine;

namespace Hatdon.CardStacker
{
    public sealed partial class CardStackerDevlogCapture
    {
        private bool physicsTake;
        private Vector3 placementStart;
        private Quaternion placementRotation;
        private Vector3[] releasedPositions;

        private void DemonstratePhysics(int frame)
        {
            showPointer=true;
            float a=28*Mathf.Deg2Rad,h=controls.Hands[0].transform.localScale.y,t=controls.Hands[0].transform.localScale.z;
            float x=(h*Mathf.Sin(a)+t*Mathf.Cos(a))*.5f;
            float y=(h*Mathf.Cos(a)+t*Mathf.Sin(a))*.5f+.000005f;
            if(frame==0){pointer=new Vector2(1200,300);gestureFrom=pointer;}
            if(frame<20)pointer=Vector2.Lerp(gestureFrom,(Vector2)Camera.main.WorldToScreenPoint(controls.Hands[0].worldCenterOfMass),Ease(frame/19f));
            if(frame==20)controls.SelectCard(controls.PickCard(pointer));
            if(frame==30)controls.FocusSelection();
            PlacementGesture(frame,40,50,1,90,true);
            PlacementGesture(frame,100,40,2,-28,true);
            PlacementGesture(frame,150,32,0,-x,false);
            PlacementGesture(frame,190,35,1,y,false);
            if(frame==226)gestureFrom=pointer;
            if(frame>=226 && frame<240)pointer=Vector2.Lerp(gestureFrom,new Vector2(770,112),Ease((frame-226)/13f));
            if(frame==240)controls.Placement.LockHand(0);
            if(frame==250)gestureFrom=pointer;
            if(frame>=250 && frame<265)pointer=Vector2.Lerp(gestureFrom,new Vector2(955,112),Ease((frame-250)/14f));
            if(frame==265){controls.SelectCard(controls.Hands[1]);controls.FocusSelection();}
            PlacementGesture(frame,280,50,1,-90,true);
            PlacementGesture(frame,340,40,2,28,true);
            PlacementGesture(frame,390,32,0,x,false);
            PlacementGesture(frame,430,35,1,y,false);
            if(frame==466)gestureFrom=pointer;
            if(frame>=466 && frame<480)pointer=Vector2.Lerp(gestureFrom,new Vector2(1135,112),Ease((frame-466)/13f));
            if(frame==480)controls.Placement.LockHand(1);
            if(frame==490)gestureFrom=pointer;
            if(frame>=490 && frame<510)pointer=Vector2.Lerp(gestureFrom,new Vector2(1325,112),Ease((frame-490)/19f));
            if(frame==510)
            {
                releasedPositions=new[]{controls.Hands[0].position,controls.Hands[1].position};
                if(!controls.Placement.CommitBoth()){Debug.LogError("Physics take commit failed: "+controls.Placement.Status);Application.Quit(8);}
                Debug.Log("HATDON_PHYSICS_TAKE_COMMIT: Both cards released with no constraints or pose correction.");
            }
            if(frame==525){controls.SelectCard(controls.Hands[0]);controls.FocusSelection();}
            if(frame==550)gestureFrom=pointer;
            if(frame>=550 && frame<580)pointer=Vector2.Lerp(gestureFrom,new Vector2(1450,320),Ease((frame-550)/29f));
            if(frame==610){controls.BeginOrbit();gestureFrom=pointer;naturalDragging=true;}
            if(frame>=610 && frame<700)
            {
                Vector2 next=gestureFrom+new Vector2(95,-5)*Ease((frame-610)/89f);
                controls.Orbit(next-pointer);pointer=next;
            }
            if(frame==700)naturalDragging=false;
        }

        // Closed-loop cursor input uses precisely the same projected-axis mapping as a mouse drag.
        // No card transform or rigid-body pose is assigned by this recording helper.
        private void PlacementGesture(int frame,int start,int length,int axisIndex,float target,bool rotate)
        {
            if(frame<start || frame>start+length)return;
            var m=controls.Manipulator;Vector3 axis=CaptureAxes[axisIndex];
            if(frame==start)
            {
                m.SetTool(rotate ? CardManipulator.Tool.Rotate : CardManipulator.Tool.Move);
                placementStart=controls.Selected.position;placementRotation=controls.Selected.rotation;
                gestureFrom=pointer;bool picked=false;
                for(int attempt=0;attempt<24 && !picked;attempt++)
                {
                    if(rotate)
                    {
                        Vector3 u=CaptureAxes[(axisIndex+1)%3],v=Vector3.Cross(axis,u);
                        float angle=.55f+attempt*.25f;
                        gestureClick=Camera.main.WorldToScreenPoint(placementStart+(u*Mathf.Cos(angle)+v*Mathf.Sin(angle))*m.HandleSize*.8f);
                    }
                    else gestureClick=Camera.main.WorldToScreenPoint(placementStart+axis*m.HandleSize*(.35f+attempt*.025f));
                    picked=m.BeginDrag(gestureClick) && m.ActiveAxis==axisIndex;m.EndDrag();
                }
                if(!picked){Debug.LogError("Triangle recording cannot pick axis at "+frame);Application.Quit(9);}
            }
            int offset=frame-start;
            if(offset<6){pointer=Vector2.Lerp(gestureFrom,gestureClick,Ease(offset/5f));return;}
            if(offset==6){pointer=gestureClick;m.BeginDrag(pointer);naturalDragging=true;}
            float progress=Ease((float)(offset-6)/(length-6));
            Vector3 center=controls.Selected.position;
            if(rotate)
            {
                Quaternion relative=controls.Selected.rotation*Quaternion.Inverse(placementRotation);
                float actual=2*Mathf.Atan2(Vector3.Dot(new Vector3(relative.x,relative.y,relative.z),axis),relative.w)*Mathf.Rad2Deg;
                float desired=target*progress;
                Vector3 radial=m.CaptureRotationRadial;
                Vector2 tangent=(Vector2)Camera.main.WorldToScreenPoint(center+Quaternion.AngleAxis(1,axis)*radial)-(Vector2)Camera.main.WorldToScreenPoint(center+radial);
                pointer+=tangent*(desired-actual);
            }
            else
            {
                float desired=Mathf.Lerp(Vector3.Dot(placementStart,axis),target,progress);
                float delta=desired-Vector3.Dot(center,axis);
                Vector2 projected=(Vector2)Camera.main.WorldToScreenPoint(center+axis*m.HandleSize)-(Vector2)Camera.main.WorldToScreenPoint(center);
                pointer+=projected*(delta/m.HandleSize);
            }
            if(!m.Dragging){Debug.LogError("Triangle recording lost a drag at "+frame);Application.Quit(10);}
            m.DragTo(pointer,false);
            if(offset==length){m.EndDrag();naturalDragging=false;}
        }

        private bool PhysicsTakeStanding()
        {
            if(!controls.Placement.IsCommitted || releasedPositions==null)return false;
            float drift=0;
            foreach(var card in controls.Hands)
                if(card.isKinematic || card.position.y<.025f || Vector3.Dot(card.rotation*Vector3.up,Vector3.up)<.7f)return false;
            for(int i=0;i<2;i++)drift=Mathf.Max(drift,Vector3.Distance(controls.Hands[i].position,releasedPositions[i]));
            Debug.Log("HATDON_PHYSICS_TAKE_STANDING: Released triangle stayed standing for 13 seconds; maximum displacement "+(drift*1000).ToString("F4")+" mm.");
            return drift<.005f;
        }
    }
}
#endif
