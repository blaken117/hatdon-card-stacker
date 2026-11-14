using UnityEngine;

namespace Hatdon.CardStacker
{
    public sealed class CardDeck : MonoBehaviour
    {
        public int Remaining { get; private set; }=52;
        public Vector3 StockPosition=>stock.position+Vector3.up*(.012f*stock.localScale.y+.001f);
        private CardStackerControls controls;
        private Transform stock;
        private Collider hit;
        public void Initialise(CardStackerControls owner){controls=owner;stock=GameObject.Find("Spare deck").transform;hit=stock.GetComponent<Collider>();}
        public bool IsHit(Vector2 pixel)
        {
            return Physics.Raycast(Camera.main.ScreenPointToRay(pixel),out var result,10,~0,QueryTriggerInteraction.Collide) && result.collider==hit;
        }
        public Rigidbody Draw()
        {
            if(Remaining<=0){controls.Placement.Notify("The deck is empty. Reset the table for a fresh deck.");return null;}
            var card=controls.Placement.DrawCard(controls.ActiveHand,StockPosition,stock.rotation*Quaternion.Euler(-90,0,0),52-Remaining);
            if(card==null)return null;
            SetRemaining(Remaining-1);
            Physics.SyncTransforms();return card;
        }
        public void SetRemaining(int remaining)
        {
            Remaining=Mathf.Clamp(remaining,0,52);
            stock.localScale=new Vector3(1,Mathf.Max(.08f,Remaining/52f),1);
            stock.gameObject.SetActive(Remaining>0);
        }
        public void ResetStock(){Remaining=52;stock.gameObject.SetActive(true);stock.localScale=Vector3.one;Physics.SyncTransforms();}
        public void EmptyForDrop(){Remaining=0;stock.gameObject.SetActive(false);}
    }
}
