#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using UnityEngine;
namespace Hatdon.CardStacker
{
    public sealed class CardPickupTest : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void StartIfRequested()
        {
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-cardPickupTest")>=0)new GameObject("Pickup verification").AddComponent<CardPickupTest>();
        }
        private IEnumerator Start()
        {
            yield return null;
            try
            {
                var controls=FindFirstObjectByType<CardStackerControls>();Physics.simulationMode=SimulationMode.Script;
                controls.Pickup.StartDrop(520811);
                var cards=controls.Pickup.Cards;
                Require(cards.Count==52 && controls.Deck.Remaining==0 && !controls.Placement.HasCard(0) && !controls.Placement.HasCard(1),"Exactly 52 loose cards, empty stock and hands");
                for(int i=0;i<52;i++)
                {
                    Require(cards[i].position.y>=.25f && !cards[i].isKinematic,"All cards begin airborne and dynamic");
                    for(int j=0;j<i;j++)Require(!Physics.ComputePenetration(cards[i].GetComponent<Collider>(),cards[i].position,cards[i].rotation,cards[j].GetComponent<Collider>(),cards[j].position,cards[j].rotation,out _,out float depth) || depth<.000001f,"Initial cards do not interpenetrate");
                }
                var air=FindObjectsByType<CardAerodynamics>(FindObjectsSortMode.None);
                for(int i=0;i<2880;i++){foreach(var model in air)model.ApplyForces();Physics.Simulate(CardPlacementSession.Step);}
                int desktop=0;
                foreach(var card in cards)
                {
                    Require(float.IsFinite(card.position.y) && card.position.y>-.74f && card.position.y<.1f,"All cards have fallen onto physical surfaces without invalid/explosive motion");
                    if(card.position.y>-.02f)desktop++;
                }
                controls.Placement.ResetCards();
                Require(controls.Pickup.Cards.Count==0 && controls.Deck.Remaining==52 && FindObjectsByType<Rigidbody>(FindObjectsSortMode.None).Length==0,"Reset cleans up all 52 cards and refills stock");
                Debug.Log("HATDON_PICKUP_TEST_OK: 52 separated random spawns, 12-second physical fall, "+desktop+" on desktop, finite bodies and complete reset.");Application.Quit(0);
            }
            catch(Exception error){Debug.LogException(error);Application.Quit(1);}
        }
        private static void Require(bool condition,string message){if(!condition)throw new Exception(message);}
    }
}
#endif
