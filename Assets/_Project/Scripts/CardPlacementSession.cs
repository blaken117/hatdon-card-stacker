using UnityEngine;

namespace Hatdon.CardStacker
{
    public sealed class CardPlacementSession : MonoBehaviour
    {
        public bool IsCommitted { get; private set; }
        public bool BothLocked => locked[0] && locked[1] && !IsCommitted;
        public string Status { get; private set; } = "Adjust each card, then lock each hand.";
        public const float CardMass = .0018f, CardThickness = .00032f, Step = 1f / 240f;
        private CardStackerControls controls;
        private readonly bool[] locked = new bool[2];
        private Vector3[] startingPositions;
        private Quaternion[] startingRotations;
        private PhysicsMaterial paper, felt;

        public void Initialise(CardStackerControls owner)
        {
            controls = owner;
            startingPositions = new Vector3[2]; startingRotations = new Quaternion[2];
            Time.fixedDeltaTime = Step;
            Physics.defaultSolverIterations = 64;
            Physics.defaultSolverVelocityIterations = 16;
            Physics.bounceThreshold = .1f;
            paper = new PhysicsMaterial("Paper on paper") { staticFriction=.48f, dynamicFriction=.32f, bounciness=0, frictionCombine=PhysicsMaterialCombine.Average, bounceCombine=PhysicsMaterialCombine.Minimum };
            felt = new PhysicsMaterial("Felt playing mat") { staticFriction=.70f, dynamicFriction=.52f, bounciness=0, frictionCombine=PhysicsMaterialCombine.Average, bounceCombine=PhysicsMaterialCombine.Minimum };
            var table = GameObject.Find("Table").GetComponent<Collider>();
            table.sharedMaterial = felt; table.contactOffset=.000025f;
            for(int i=0;i<2;i++)
            {
                var card=controls.Hands[i];
                startingPositions[i]=card.position; startingRotations[i]=card.rotation;
                Vector3 size=card.transform.localScale;size.z=CardThickness;card.transform.localScale=size;
                var collider=card.GetComponent<Collider>();collider.sharedMaterial=paper;collider.contactOffset=.000025f;
                card.mass=CardMass;card.useGravity=true;card.constraints=RigidbodyConstraints.None;
                card.linearDamping=.04f;card.angularDamping=.08f;
                card.solverIterations=64;card.solverVelocityIterations=16;
                card.maxDepenetrationVelocity=.12f;card.maxAngularVelocity=35;
                card.sleepThreshold=.0000001f;
                card.inertiaTensorRotation=Quaternion.identity;
                float w=size.x,h=size.y,t=size.z;
                card.inertiaTensor=new Vector3(CardMass*(h*h+t*t)/12,CardMass*(w*w+t*t)/12,CardMass*(w*w+h*h)/12);
                card.collisionDetectionMode=CollisionDetectionMode.ContinuousSpeculative;
                card.isKinematic=true;
            }
            Physics.SyncTransforms();
        }

        public bool IsLocked(int index) => index>=0 && index<2 && locked[index];
        public bool CanAdjust(Rigidbody card)
        {
            int i=System.Array.IndexOf(controls.Hands,card);
            return i>=0 && !IsCommitted && !locked[i] && card.isKinematic;
        }

        public void BeginAdjust(Rigidbody card)
        {
            int i=System.Array.IndexOf(controls.Hands,card);
            if(i<0 || IsCommitted) return;
            locked[i]=false;
            Status=(i==0 ? "Left" : "Right")+" hand adjusting. Lock it when ready.";
        }

        public bool LockHand(int index)
        {
            if(IsCommitted || index<0 || index>1) return false;
            locked[index]=true;
            if(controls.Selected==controls.Hands[index]) controls.SelectCard(null);
            controls.Manipulator.EndDrag();
            Status=BothLocked ? "Both hands locked. Commit both to test the balance." : (index==0 ? "Left" : "Right")+" hand locked. Select either hand to adjust.";
            return true;
        }

        public static Vector3 ColliderPosition(Collider collider) => collider.attachedRigidbody!=null && collider.transform==collider.attachedRigidbody.transform ? collider.attachedRigidbody.position : collider.transform.position;
        public static Quaternion ColliderRotation(Collider collider) => collider.attachedRigidbody!=null && collider.transform==collider.attachedRigidbody.transform ? collider.attachedRigidbody.rotation : collider.transform.rotation;

        public bool CommitBoth()
        {
            if(IsCommitted)return false;
            if(!BothLocked) { Status="Lock both hands before committing.";return false; }
            Physics.SyncTransforms();
            foreach(var card in controls.Hands)
            {
                var shape=card.GetComponent<Collider>();
                foreach(var other in FindObjectsByType<Collider>(FindObjectsSortMode.None))
                    if(other!=shape && other.enabled && !other.isTrigger &&
                       Physics.ComputePenetration(shape,card.position,card.rotation,other,ColliderPosition(other),ColliderRotation(other),out _,out float depth) && depth>.000005f)
                    { Status="Cards overlap. Adjust the placement before committing.";return false; }
            }
            controls.Manipulator.EndDrag();
            // Both change state in the same update, before the next physics tick.
            foreach(var card in controls.Hands)
            {
                card.isKinematic=false;
                card.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;
                card.linearVelocity=Vector3.zero;card.angularVelocity=Vector3.zero;
                card.WakeUp();
            }
            IsCommitted=true;
            Status="Both released. Gravity and surface contact are active.";
            controls.SelectCard(null);
            return true;
        }

        public void ResetCards()
        {
            controls.Manipulator.EndDrag();
            for(int i=0;i<2;i++)
            {
                var card=controls.Hands[i];
                if(!card.isKinematic) { card.linearVelocity=Vector3.zero;card.angularVelocity=Vector3.zero; }
                card.isKinematic=true;card.collisionDetectionMode=CollisionDetectionMode.ContinuousSpeculative;
                card.position=startingPositions[i];card.rotation=startingRotations[i];locked[i]=false;
            }
            IsCommitted=false;Physics.SyncTransforms();controls.SelectCard(null);
            Status="Cards reset. Adjust and lock each hand, then commit both.";
        }

        private void OnDestroy() { if(paper!=null) Destroy(paper);if(felt!=null) Destroy(felt); }
    }
}
