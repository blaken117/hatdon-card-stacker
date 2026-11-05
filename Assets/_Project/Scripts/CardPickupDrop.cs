using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hatdon.CardStacker
{
    // Explicit F8/recording experiment; normal play still begins with the deck.
    public sealed class CardPickupDrop : MonoBehaviour
    {
        private CardStackerControls controls;
        public readonly List<Rigidbody> Cards=new List<Rigidbody>();
        private readonly List<Vector3> spins=new List<Vector3>();
        public bool IsPrepared { get; private set; }
        public void Initialise(CardStackerControls owner)=>controls=owner;
        public void StartDrop(int seed)
        {
            PrepareDrop(seed);ReleasePrepared();
        }
        public void ClearState(){Cards.Clear();spins.Clear();IsPrepared=false;}
        public void PrepareDrop(int seed)
        {
            controls.Placement.ResetCards();ClearState();
            var random=new System.Random(seed);
            for(int i=0;i<52;i++)
            {
                Rigidbody card=null;bool clear=false;
                for(int attempt=0;attempt<160 && !clear;attempt++)
                {
                    Vector3 position=new Vector3(Range(random,-.26f,.26f),Range(random,.25f,.68f),Range(random,-.18f,.18f));
                    Quaternion rotation=Quaternion.Euler(Range(random,0,360),Range(random,0,360),Range(random,0,360));
                    if(card==null)card=controls.Placement.CreateDropCard(position,rotation,i);
                    else{card.position=position;card.rotation=rotation;}
                    clear=true;var shape=card.GetComponent<Collider>();
                    foreach(var other in Cards)
                        if(Physics.ComputePenetration(shape,card.position,card.rotation,other.GetComponent<Collider>(),other.position,other.rotation,out _,out float depth) && depth>0){clear=false;break;}
                }
                if(!clear)throw new InvalidOperationException("Cannot find a non-overlapping pickup spawn.");
                Cards.Add(card);
            }
            Physics.SyncTransforms();
            foreach(var card in Cards)spins.Add(new Vector3(Range(random,-2,2),Range(random,-2,2),Range(random,-2,2)));
            IsPrepared=true;
            controls.Deck.EmptyForDrop();controls.HandVisuals.Refresh();
            Debug.Log("HATDON_PICKUP_PREPARED: 52 separated random poses; seed "+seed);
        }
        public void ReleasePrepared()
        {
            if(!IsPrepared)return;
            for(int i=0;i<Cards.Count;i++)
            {
                var card=Cards[i];
                card.isKinematic=false;card.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;
                card.linearVelocity=Vector3.zero;
                card.angularVelocity=spins[i];card.WakeUp();
            }
            IsPrepared=false;
            Debug.Log("HATDON_PICKUP_SPAWN: 52 non-overlapping dynamic cards released.");
        }
        private static float Range(System.Random random,float a,float b)=>Mathf.Lerp(a,b,(float)random.NextDouble());
    }
}
