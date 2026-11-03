#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace Hatdon.CardStacker
{
    public sealed class CardBalanceTest : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void StartIfRequested()
        {
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-cardBalanceTest")>=0)
                new GameObject("Balance verification").AddComponent<CardBalanceTest>();
        }

        private IEnumerator Start()
        {
            yield return null;
            try
            {
                Physics.simulationMode=SimulationMode.Script;
                var c=FindFirstObjectByType<CardStackerControls>();var s=c.Placement;
                s.PrepareTestPair();
                var left=c.Hands[0];var right=c.Hands[1];
                Require(!s.CommitBoth(),"Cannot commit without both locks");
                c.SelectCard(left);s.LockHand(0);Require(!s.CanAdjust(left),"Locked card cannot move");
                c.SwitchHand();Require(c.Selected==right && s.IsLocked(0),"Switch after locking selects other hand without reopening the first");
                Require(!s.CommitBoth(),"One locked hand is insufficient");
                Vector3 saved=left.position;c.SelectCard(left);
                Require(!s.IsLocked(0) && s.CanAdjust(left) && left.position==saved,"Reopening hand unlocks without moving it");
                s.PrepareTestPair();
                Pose(left,new Vector3(-.12f,.3f,0),Quaternion.Euler(20,0,0));
                Pose(right,new Vector3(.12f,.3f,0),Quaternion.Euler(-20,0,0));
                s.LockHand(0);s.LockHand(1);Vector3 before=left.position;
                Require(s.CommitBoth(),"Both locked hands commit");
                Require(!left.isKinematic && !right.isKinematic && left.position==before,"Commit releases without teleporting");
                Require(left.linearVelocity==Vector3.zero && left.angularVelocity==Vector3.zero,"No artificial release impulse");
                string releasedStatus=s.Status;
                Require(!s.CommitBoth() && s.Status==releasedStatus,"Repeated commit does not disturb physics or its status");
                Simulate(.1f);
                Require(Mathf.Abs(left.linearVelocity.y+.981f)<.02f,"Gravity is active at the configured scale");
                c.SelectCard(left);Require(!c.Manipulator.Translate(Vector3.up*.01f),"Released cards cannot be moved by handles");
                Simulate(4);
                Require(left.position.y<.002f && left.position.y>-.001f,"Unsupported card reaches the tabletop without tunnelling");
                Require(Mathf.Abs(Vector3.Dot(left.rotation*Vector3.forward,Vector3.up))>.8f,"Unsupported tilted card falls flat");
                s.PrepareTestPair();Require(left.isKinematic && right.isKinematic && !s.IsCommitted && !s.BothLocked,"Reset restores editable held cards");

                Pose(left,new Vector3(0,.1f,0),Quaternion.identity);Pose(right,left.position,Quaternion.identity);
                s.LockHand(0);s.LockHand(1);Require(!s.CommitBoth() && left.isKinematic && right.isKinematic,"Overlap is rejected before release");
                s.PrepareTestPair();

                // Assemble the same triangle through the real placement API, not a pose preset.
                const float lean=28;
                float radians=lean*Mathf.Deg2Rad,h=left.transform.localScale.y,t=left.transform.localScale.z;
                float targetX=(h*Mathf.Sin(radians)+t*Mathf.Cos(radians))*.5f;
                float targetY=(h*Mathf.Cos(radians)+t*Mathf.Sin(radians))*.5f+.000005f;
                for(int i=0;i<2;i++)
                {
                    var card=c.Hands[i];c.SelectCard(card);
                    c.Manipulator.Rotate(Vector3.up,i==0 ? 90 : -90);
                    c.Manipulator.Rotate(Vector3.forward,i==0 ? -lean : lean);
                    c.Manipulator.Translate(Vector3.right*((i==0 ? -targetX : targetX)-card.position.x));
                    c.Manipulator.Translate(Vector3.up*(targetY-card.position.y));
                    Require(Mathf.Abs(card.position.y-targetY)<.00002f,"Triangle can be placed with contact-aware controls");
                    if(i==1)
                    {
                        Vector3 fixedPosition=left.position;Quaternion fixedRotation=left.rotation;
                        Require(s.IsLocked(0),"First hand remains locked while adjusting the second");
                        Require(!c.Manipulator.Translate(Vector3.left*.012f),"Attempt to drag through locked card is stopped");
                        Require(left.position==fixedPosition && left.rotation==fixedRotation,"Locked card is neither pushed nor rotated");
                        Require(right.position.x>targetX-.00005f,"Moving card remains on its own side of locked contact");
                        c.Manipulator.Translate(Vector3.right*(targetX-right.position.x));
                    }
                    s.LockHand(i);
                }
                Require(s.CommitBoth(),"Manually assembled triangle commits");
                Simulate(20);
                Require(left.position.y>.025f && right.position.y>.025f,"Control-assembled triangle remains standing");
                Debug.Log("HATDON_MANUAL_TRIANGLE_OK: Contact-aware placement API, lock both and commit remains standing for 20 seconds.");

                s.PrepareTestPair();Triangle(left,right,28);
                Pose(right,right.position+Vector3.forward*.068f,right.rotation);
                s.LockHand(0);s.LockHand(1);Require(s.CommitBoth(),"Misaligned cards can commit when they do not overlap");
                Simulate(5);
                foreach(var card in c.Hands)
                    Require(card.position.y<.002f && card.position.y>-.001f && Mathf.Abs(Vector3.Dot(card.rotation*Vector3.forward,Vector3.up))>.85f,"Cards with separated top edges fall flat instead of auto-balancing");
                Debug.Log("HATDON_MISALIGNED_TEST_OK: 68mm depth offset leaves top edges unsupported; both cards fall flat with gravity only.");

                var observations=new System.Collections.Generic.List<string>();
                foreach(float degrees in new[]{24f,28f,32f})
                {
                    s.PrepareTestPair();Triangle(left,right,degrees);
                    var a=left.gameObject.GetComponent<BalanceContactProbe>() ?? left.gameObject.AddComponent<BalanceContactProbe>();
                    var b=right.gameObject.GetComponent<BalanceContactProbe>() ?? right.gameObject.AddComponent<BalanceContactProbe>();
                    a.Clear();b.Clear();
                    Vector3 l=left.position,r=right.position;Quaternion lq=left.rotation,rq=right.rotation;
                    s.LockHand(0);s.LockHand(1);Require(s.CommitBoth(),"Triangle placement passes overlap check");
                    Simulate(20);
                    bool standing=left.position.y>.025f && right.position.y>.025f &&
                        Vector3.Dot(left.rotation*Vector3.up,Vector3.up)>.7f && Vector3.Dot(right.rotation*Vector3.up,Vector3.up)>.7f;
                    float drift=Mathf.Max(Vector3.Distance(l,left.position),Vector3.Distance(r,right.position));
                    float angle=Mathf.Max(Quaternion.Angle(lq,left.rotation),Quaternion.Angle(rq,right.rotation));
                    observations.Add($"{degrees} deg: standing={standing}, drift={drift*1000:F4} mm, angle={angle:F4} deg, contacts={a.Table}/{a.Other}/{b.Table}/{b.Other}");
                    Debug.Log("HATDON_TRIANGLE_RESULT: "+observations[observations.Count-1]);
                    Require(standing && drift<.005f && angle<5,"Triangle stays supported for 20 seconds");
                    Require(a.Table && b.Table && a.Other && b.Other,"Both cards physically contact the table and each other");
                    Require(!left.isKinematic && !right.isKinematic && left.constraints==RigidbodyConstraints.None && FindObjectsByType<Joint>(FindObjectsSortMode.None).Length==0,"No weld, freeze or kinematic trick holds the triangle");
                    left.AddForceAtPosition(Vector3.right*.0002f,left.worldCenterOfMass+Vector3.up*.025f,ForceMode.Impulse);
                    Simulate(3);
                    Require(Quaternion.Angle(lq,left.rotation)>12 || Vector3.Distance(l,left.position)>.01f,"A bump can disturb the formation");
                }
                string output=Path.Combine(Application.dataPath,"../balance-test-results.txt");
                File.WriteAllLines(output,observations);
                Debug.Log("HATDON_BALANCE_TEST_OK: lock/edit/commit/reset, overlap guard, gravity, unsupported fall, three triangle angles, actual contacts and perturbation passed.");
                Application.Quit(0);
            }
            catch(Exception error){Debug.LogException(error);Application.Quit(1);}
        }

        public static void Triangle(Rigidbody left,Rigidbody right,float degrees)
        {
            float a=degrees*Mathf.Deg2Rad,h=left.transform.localScale.y,t=left.transform.localScale.z;
            float x=(h*Mathf.Sin(a)+t*Mathf.Cos(a))*.5f;
            float y=(h*Mathf.Cos(a)+t*Mathf.Sin(a))*.5f+.000005f;
            Pose(left,new Vector3(-x,y,0),Quaternion.AngleAxis(-degrees,Vector3.forward)*Quaternion.AngleAxis(90,Vector3.up));
            Pose(right,new Vector3(x,y,0),Quaternion.AngleAxis(degrees,Vector3.forward)*Quaternion.AngleAxis(-90,Vector3.up));
        }

        private static void Pose(Rigidbody body,Vector3 p,Quaternion q){body.position=p;body.rotation=q;Physics.SyncTransforms();}
        private static void Simulate(float seconds){for(int i=0;i<Mathf.RoundToInt(seconds/CardPlacementSession.Step);i++)Physics.Simulate(CardPlacementSession.Step);}
        private static void Require(bool value,string message){if(!value)throw new Exception(message);}
    }

    public sealed class BalanceContactProbe : MonoBehaviour
    {
        public bool Table,Other;
        public void Clear(){Table=Other=false;}
        private void OnCollisionEnter(Collision collision){Observe(collision);}
        private void OnCollisionStay(Collision collision){Observe(collision);}
        private void Observe(Collision collision){if(collision.gameObject.name=="Table")Table=true;if(collision.rigidbody!=null)Other=true;}
    }
}
#endif
