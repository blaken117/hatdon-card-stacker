using UnityEngine;

namespace Hatdon.CardStacker
{
    public sealed class CardPlacementSession : MonoBehaviour
    {
        public bool IsCommitted { get; private set; }
        public bool BothLocked => HasCard(0) && HasCard(1) && locked[0] && locked[1];
        public string Status { get; private set; }="Drag the first card from the deck.";
        public const float CardMass=.0018f,CardThickness=.00032f,Step=1f/240f;
        private CardStackerControls controls;
        private readonly bool[] locked=new bool[2];
        private readonly bool[] drawn=new bool[2];
        private Rigidbody[] originals;
        private Vector3[] startingPositions;
        private Quaternion[] startingRotations;
        private PhysicsMaterial paper,felt;

        public void Initialise(CardStackerControls owner)
        {
            controls=owner;originals=(Rigidbody[])owner.Hands.Clone();
            startingPositions=new Vector3[2];startingRotations=new Quaternion[2];
            Time.fixedDeltaTime=Step;Physics.defaultSolverIterations=64;Physics.defaultSolverVelocityIterations=16;Physics.bounceThreshold=.1f;
            paper=new PhysicsMaterial("Paper on paper"){staticFriction=.48f,dynamicFriction=.32f,bounciness=0,frictionCombine=PhysicsMaterialCombine.Average,bounceCombine=PhysicsMaterialCombine.Minimum};
            felt=new PhysicsMaterial("Felt playing mat"){staticFriction=.70f,dynamicFriction=.52f,bounciness=0,frictionCombine=PhysicsMaterialCombine.Average,bounceCombine=PhysicsMaterialCombine.Minimum};
            var table=GameObject.Find("Table").GetComponent<Collider>();table.sharedMaterial=felt;table.contactOffset=.000025f;
            for(int i=0;i<2;i++)
            {
                startingPositions[i]=originals[i].position;startingRotations[i]=originals[i].rotation;
                ConfigureCard(originals[i]);originals[i].gameObject.SetActive(false);
            }
            Physics.SyncTransforms();
        }

        public void ConfigureCard(Rigidbody card)
        {
            Vector3 size=card.transform.localScale;size.z=CardThickness;card.transform.localScale=size;
            var collider=card.GetComponent<Collider>();collider.sharedMaterial=paper;collider.contactOffset=.000025f;
            if(!card.isKinematic){card.linearVelocity=Vector3.zero;card.angularVelocity=Vector3.zero;}
            card.isKinematic=true;card.mass=CardMass;card.useGravity=true;card.constraints=RigidbodyConstraints.None;
            card.linearDamping=0;card.angularDamping=0;card.solverIterations=64;card.solverVelocityIterations=16;
            if(card.GetComponent<CardAerodynamics>()==null)card.gameObject.AddComponent<CardAerodynamics>();
            card.maxDepenetrationVelocity=.12f;card.maxAngularVelocity=35;card.sleepThreshold=.0000001f;
            card.inertiaTensorRotation=Quaternion.identity;
            float w=size.x,h=size.y,t=size.z;
            card.inertiaTensor=new Vector3(CardMass*(h*h+t*t)/12,CardMass*(w*w+t*t)/12,CardMass*(w*w+h*h)/12);
            card.collisionDetectionMode=CollisionDetectionMode.ContinuousSpeculative;
        }

        public bool HasCard(int i)=>i>=0 && i<2 && drawn[i] && controls.Hands[i]!=null && controls.Hands[i].gameObject.activeInHierarchy && controls.Hands[i].isKinematic;
        public bool IsLocked(int i)=>HasCard(i) && locked[i];
        public bool CanAdjust(Rigidbody card)
        {
            int i=System.Array.IndexOf(controls.Hands,card);
            return i>=0 && HasCard(i) && !locked[i];
        }
        public int FreeHand(int preferred=0)
        {
            if(!HasCard(preferred))return preferred;
            return !HasCard(1-preferred) ? 1-preferred : -1;
        }

        public Rigidbody DrawCard(int preferred,Vector3 position,Quaternion rotation,int number)
        {
            int i=FreeHand(preferred);
            if(i<0){Status="Adjust either card, then let go to release the pair.";return null;}
            Rigidbody card;
            if(!drawn[i] && controls.Hands[i]==originals[i])card=originals[i];
            else
            {
                card=Instantiate(originals[i]);
                var oldOutline=card.transform.Find("Selection outline");
                if(oldOutline!=null){oldOutline.gameObject.SetActive(false);Destroy(oldOutline.gameObject);}
                controls.RegisterCard(card);
            }
            controls.Hands[i]=card;ConfigureCard(card);card.gameObject.SetActive(true);
            card.position=position;card.rotation=rotation;card.name="Drawn card "+(number+1);
            CardTableArt.StyleDrawnCard(card,number);
            drawn[i]=true;locked[i]=false;IsCommitted=false;Physics.SyncTransforms();
            controls.SelectCard(card);
            return card;
        }

        public void BeginAdjust(Rigidbody card)
        {
            int i=System.Array.IndexOf(controls.Hands,card);
            if(i<0 || !HasCard(i))return;
            locked[i]=false;
            Status=HasCard(1-i) ? "Let go to release the pair." : "Position the first card, then draw the second.";
        }
        public Rigidbody CreateDropCard(Vector3 position,Quaternion rotation,int number)
        {
            var card=Instantiate(originals[number%2]);
            var oldOutline=card.transform.Find("Selection outline");
            if(oldOutline!=null){oldOutline.gameObject.SetActive(false);Destroy(oldOutline.gameObject);}
            controls.RegisterCard(card);ConfigureCard(card);card.gameObject.SetActive(true);
            card.position=position;card.rotation=rotation;card.name="Pickup card "+(number+1);
            CardTableArt.StyleDrawnCard(card,number);return card;
        }
        public bool LockHand(int i)
        {
            if(!HasCard(i))return false;
            locked[i]=true;if(controls.Selected==controls.Hands[i])controls.SelectCard(null);
            controls.Manipulator.EndDrag();
            Status=BothLocked ? "Both hands locked. Commit both to test the balance." : (i==0 ? "Left" : "Right")+" hand locked. Draw or adjust the other card.";
            return true;
        }
        public void Notify(string message){Status=message;}
        public static Vector3 ColliderPosition(Collider c)=>c.attachedRigidbody!=null && c.transform==c.attachedRigidbody.transform ? c.attachedRigidbody.position : c.transform.position;
        public static Quaternion ColliderRotation(Collider c)=>c.attachedRigidbody!=null && c.transform==c.attachedRigidbody.transform ? c.attachedRigidbody.rotation : c.transform.rotation;

        public bool CommitBoth()
        {
            if(IsCommitted)return false;
            if(!BothLocked){Status="Fill and lock both hands before committing.";return false;}
            return ReleasePair();
        }

        public bool FinishPlacement()
        {
            if (!HasCard(0) || !HasCard(1))
            {
                Status = "First card placed. Draw the second card.";
                return false;
            }
            return ReleasePair();
        }

        private bool ReleasePair()
        {
            Physics.SyncTransforms();
            foreach(var card in controls.Hands)
            {
                var shape=card.GetComponent<Collider>();
                foreach(var other in FindObjectsByType<Collider>(FindObjectsSortMode.None))
                    if(other!=shape && other.enabled && !other.isTrigger && Physics.ComputePenetration(shape,card.position,card.rotation,other,ColliderPosition(other),ColliderRotation(other),out _,out float depth) && depth>.000005f)
                    {Status="Cards overlap. Adjust either card and let go again.";return false;}
            }
            controls.Manipulator.EndDrag();
            foreach(var card in controls.Hands)
            {
                card.isKinematic=false;card.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;
                card.linearVelocity=Vector3.zero;card.angularVelocity=Vector3.zero;card.WakeUp();
            }
            locked[0]=locked[1]=false;IsCommitted=true;controls.PairCommitted();
            Status=controls.Deck.Remaining > 0 ? "Pair released. Draw the next card." : "Deck empty. Reset cards to start over.";
            return true;
        }

        public void ResetCards()
        {
            controls.Manipulator.EndDrag();controls.RestoreOriginalCards(originals);
            for(int i=0;i<2;i++)
            {
                ConfigureCard(originals[i]);originals[i].position=startingPositions[i];originals[i].rotation=startingRotations[i];
                originals[i].gameObject.SetActive(false);locked[i]=drawn[i]=false;
            }
            IsCommitted=false;Physics.SyncTransforms();controls.SelectCard(null);controls.Deck.ResetStock();
            controls.Pickup?.ClearState();
            Status="Table reset. Drag a card from the deck.";
        }
        // Explicit fixture for regression checks; normal play starts with empty hands.
        public void PrepareTestPair()
        {
            ResetCards();for(int i=0;i<2;i++)DrawCard(i,startingPositions[i],startingRotations[i],i);controls.SelectCard(null);
        }
        private void OnDestroy(){if(paper!=null)Destroy(paper);if(felt!=null)Destroy(felt);}
    }
}
