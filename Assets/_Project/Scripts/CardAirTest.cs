#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using UnityEngine;

namespace Hatdon.CardStacker
{
    public sealed class CardAirTest : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void StartIfRequested()
        {
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-cardAirTest")>=0)
                new GameObject("Card air verification").AddComponent<CardAirTest>();
        }

        private IEnumerator Start()
        {
            yield return null;
            try
            {
                Physics.simulationMode=SimulationMode.Script;
                var controls=FindFirstObjectByType<CardStackerControls>();controls.Placement.PrepareTestPair();
                var flat=controls.Hands[0];var edge=controls.Hands[1];
                Require(CardAerodynamics.PatchForce(Vector3.zero,Vector3.up,.005544f)==Vector3.zero,"Still air adds no force at rest");
                for(int i=0;i<100;i++)
                {
                    Vector3 velocity=new Vector3(Mathf.Sin(i*.7f),Mathf.Cos(i*.3f),Mathf.Sin(i*.4f))*3;
                    Vector3 force=CardAerodynamics.PatchForce(velocity,Vector3.up,.005544f);
                    Require(Vector3.Dot(velocity,force)<=0,"Air resistance removes rather than adds energy");
                }
                Pose(flat,new Vector3(-.14f,.35f,0),Quaternion.Euler(90,0,0));
                Pose(edge,new Vector3(.14f,.35f,0),Quaternion.identity);
                controls.Placement.LockHand(0);controls.Placement.LockHand(1);
                Require(controls.Placement.CommitBoth(),"Air drop releases both cards normally");
                Step(.15f);
                Require(flat.position.y>edge.position.y+.002f && Mathf.Abs(flat.linearVelocity.y)<Mathf.Abs(edge.linearVelocity.y)*.95f,"Broadside card meets more air resistance than edge-first card");
                Debug.Log("HATDON_AIR_DROP_RESULT: after 0.15s broadside speed "+(-flat.linearVelocity.y).ToString("F4")+"m/s; edge-first "+(-edge.linearVelocity.y).ToString("F4")+"m/s.");

                controls.Placement.PrepareTestPair();
                Pose(flat,new Vector3(-.14f,.30f,0),Quaternion.identity);
                flat.isKinematic=false;flat.useGravity=false;flat.angularVelocity=Vector3.right*10;
                Step(.2f);
                Require(flat.angularVelocity.magnitude<9.99f,"Distributed air forces oppose tumble");
                flat.linearVelocity=Vector3.zero;flat.angularVelocity=Vector3.zero;flat.Sleep();
                flat.GetComponent<CardAerodynamics>().ApplyForces();
                Require(flat.IsSleeping(),"Air does not wake resting cards");
                controls.Placement.ResetCards();
                Debug.Log("HATDON_AIR_TEST_OK: orientation-dependent fall, dissipative air forces, tumble resistance, rest and reset passed.");
                Application.Quit(0);
            }
            catch(Exception error){Debug.LogException(error);Application.Quit(1);}
        }

        private static void Step(float seconds)
        {
            var models=FindObjectsByType<CardAerodynamics>(FindObjectsSortMode.None);
            for(int i=0;i<Mathf.RoundToInt(seconds/CardPlacementSession.Step);i++)
            {
                foreach(var model in models)model.ApplyForces();
                Physics.Simulate(CardPlacementSession.Step);
            }
        }
        private static void Pose(Rigidbody body,Vector3 position,Quaternion rotation){body.position=position;body.rotation=rotation;Physics.SyncTransforms();}
        private static void Require(bool success,string message){if(!success)throw new Exception(message);}
    }
}
#endif
