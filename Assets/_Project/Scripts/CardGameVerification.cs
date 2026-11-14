#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Hatdon.CardStacker
{
    public sealed class CardGameVerification : MonoBehaviour
    {
        private IEnumerator Start()
        {
            yield return null;
            var controls = GetComponent<CardStackerControls>();
            var game = controls.Game;
            var args = Environment.GetCommandLineArgs();
            if (Array.IndexOf(args,"-verifyScreens") >= 0)
            {
                yield return new WaitForSecondsRealtime(.5f);
                ScreenCapture.CaptureScreenshot(Path.Combine(Application.dataPath,"../challenge-menu.png"));
                yield return new WaitForSecondsRealtime(.5f);
                game.OpenTable(3);
                yield return new WaitForSecondsRealtime(.5f);
                ScreenCapture.CaptureScreenshot(Path.Combine(Application.dataPath,"../challenge-guide.png"));
                yield return new WaitForSecondsRealtime(.5f);
                Application.Quit(0);
                yield break;
            }
            bool reload = Array.IndexOf(args,"-verifyReload") >= 0;
            try
            {
                Require(game.MenuOpen,"Game starts in the main menu");
                if (reload)
                {
                    game.OpenTable(0);
                    Require(controls.Deck.Remaining == 49 && controls.Placement.IsLocked(0),"Deck and staged card restore after relaunch");
                    Require(game.IsComplete && !game.GuideVisible,"Completion and hidden guide restore after relaunch");
                    Require(Vector3.Distance(controls.Hands[0].position,new Vector3(.19f,.10f,0)) < .000001f,"Staged card position restores exactly");
                    int released = 0;
                    foreach (var card in controls.AllCards)
                        if (card.gameObject.activeInHierarchy && !card.isKinematic) released++;
                    Require(released == 2,"Both released cards restore as physical cards");
                    game.OpenTable(1);
                    Require(controls.Deck.Remaining == 52,"Reset level stays empty after relaunch");
                    game.OpenTable(-1);
                    Require(controls.Deck.Remaining == 51,"Practice survives independently");
                }
                else
                {
                    for (int level = 0; level < CardChallenges.Levels.Length; level++)
                    {
                        game.OpenTable(level);
                        var layout = CardChallenges.Levels[level];
                        var preview = GameObject.Find("Challenge guide");
                        Require(preview.transform.childCount == layout.Cards.Count &&
                            preview.GetComponentsInChildren<Collider>().Length == 0,"Guide matches layout and has no colliders");
                        var cards = new List<Rigidbody>();
                        foreach (Pose pose in layout.Cards)
                        {
                            var card = controls.Deck.Draw();
                            card.position = pose.position;
                            card.rotation = pose.rotation;
                            controls.Placement.CommitSelectedCard();
                            controls.Placement.ReleaseSingle();
                            Require(!card.isKinematic,layout.Name + " card " + (cards.Count + 1) + ": " + controls.Placement.Status);
                            cards.Add(card);
                        }
                        Physics.SyncTransforms();
                        Physics.simulationMode = SimulationMode.Script;
                        var models = FindObjectsByType<CardAerodynamics>(FindObjectsSortMode.None);
                        for (int tick = 0; tick < 4 * 240; tick++)
                        {
                            foreach (var model in models) model.ApplyForces();
                            Physics.Simulate(CardPlacementSession.Step);
                        }
                        Physics.simulationMode = SimulationMode.FixedUpdate;
                        Require(CardChallenges.Matches(layout,cards,new int[52],new bool[52]),layout.Name + " remains standing after four physics seconds");
                        Debug.Log("HATDON_CHALLENGE_OK " + layout.Name);
                        game.ResetCurrentTable();
                        game.ShowMenu();
                    }
                    game.OpenTable(0);
                    foreach (Pose pose in CardChallenges.Levels[0].Cards)
                    {
                        var card = controls.Deck.Draw();
                        card.position = pose.position;
                        card.rotation = pose.rotation;
                        controls.Placement.CommitSelectedCard();
                    }
                    Require(controls.Placement.CommitBoth(),"Pair release still works");
                }
            }
            catch (Exception error)
            {
                Debug.LogException(error);
                Application.Quit(1);
                yield break;
            }

            if (!reload) yield return new WaitForSeconds(4f);
            try
            {
                if (!reload)
                {
                    var staged = controls.Deck.Draw();
                    staged.position = new Vector3(.19f,.10f,0);
                    controls.Placement.CommitSelectedCard();
                    game.ToggleGuide();
                    game.ShowMenu();
                    game.OpenTable(-1);
                    controls.Deck.Draw();
                    game.ShowMenu();
                    game.OpenTable(1);
                    controls.Deck.Draw();
                    game.ResetCurrentTable();
                    game.ShowMenu();
                    string directory = args[Array.IndexOf(args,"-verifyCardGame")+1];
                    Require(File.ReadAllText(Path.Combine(directory,"level-01.json")).Contains("\"completed\":true"),"Completed challenge persisted");
                }
                Debug.Log("HATDON_GAME_VERIFY_OK");
                Application.Quit(0);
            }
            catch (Exception error)
            {
                Debug.LogException(error);
                Application.Quit(1);
            }
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
#endif
