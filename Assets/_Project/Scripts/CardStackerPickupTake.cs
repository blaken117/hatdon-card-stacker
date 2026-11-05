#if HATDON_CAPTURE
using UnityEngine;

namespace Hatdon.CardStacker
{
    public sealed partial class CardStackerDevlogCapture
    {
        private bool pickupTake;
        private Vector3 pickupCameraStart;
        private void DemonstratePickup(int frame)
        {
            showPointer=false;
            if(frame==0)
            {
                controls.Placement.PrepareTestPair();
                controls.Hands[0].rotation=Quaternion.Euler(0,25,0);
                controls.Hands[1].rotation=Quaternion.Euler(35,-25,20);
                controls.Placement.LockHand(0);controls.Placement.LockHand(1);controls.HandVisuals.Refresh();
                controls.transform.position=new Vector3(.30f,.37f,-.55f);controls.transform.LookAt(new Vector3(0,.14f,0));
            }
            if(frame==179)
            {
                CheckTrial(controls.HandVisuals.VisibleCount==2 && Vector3.Dot(controls.HandVisuals.FingerDirection(0),Vector3.down)>.999f && Vector3.Dot(controls.HandVisuals.FingerDirection(1),Vector3.down)>.999f,"Both full ghost hands point down from above tilted cards");
                Debug.Log("HATDON_HANDS_ABOVE_OK: Full five-finger meshes stay above and point down; two different card tilts.");
            }
            if(frame==180){controls.Pickup.StartDrop(520811);controls.FramePickup();}
            if(frame==360)pickupCameraStart=controls.transform.position;
            if(frame>=360 && frame<=450)
            {
                controls.transform.position=Vector3.Lerp(pickupCameraStart,new Vector3(.46f,.48f,-.65f),Ease((frame-360)/90f));
                controls.transform.LookAt(Vector3.Lerp(new Vector3(0,.24f,0),new Vector3(0,.025f,0),Ease((frame-360)/90f)));
            }
        }
        private bool PickupTakeValid()
        {
            int landed=0;
            foreach(var card in controls.Pickup.Cards)
            {
                if(card==null || card.isKinematic || !float.IsFinite(card.position.y) || card.position.y<-.74f || card.position.y>.1f)return false;
                if(card.position.y>-.02f)landed++;
            }
            bool valid=controls.Pickup.Cards.Count==52 && controls.Deck.Remaining==0 && controls.HandVisuals.VisibleCount==0;
            if(valid)Debug.Log("HATDON_PICKUP_TAKE_OK: All 52 cards fell and remain finite; "+landed+" on the desktop, "+(52-landed)+" below; no held cards or ghost supports.");
            return valid;
        }
    }
}
#endif
