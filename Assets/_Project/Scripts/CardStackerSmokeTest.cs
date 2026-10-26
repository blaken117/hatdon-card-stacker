#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace Hatdon.CardStacker
{
    // Runs only when explicitly requested in an editor/development player.
    public sealed class CardStackerSmokeTest : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void StartIfRequested()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-cardStackerSmokeTest") >= 0)
                new GameObject("Development smoke test").AddComponent<CardStackerSmokeTest>();
        }

        private IEnumerator Start()
        {
            yield return null;
            var controls = FindFirstObjectByType<CardStackerControls>();
            try
            {
                Require(controls != null && controls.Hands.Length == 2, "Two hand slots initialise");
                Require(controls.Selected == null, "No initial selection");
                Vector3 cameraPosition = controls.transform.position;
                Quaternion cameraRotation = controls.transform.rotation;
                var positions = new[] { controls.Hands[0].position, controls.Hands[1].position };
                var rotations = new[] { controls.Hands[0].rotation, controls.Hands[1].rotation };
                Physics.SyncTransforms();
                foreach (var card in controls.Hands)
                {
                    Vector2 pixel = Camera.main.WorldToScreenPoint(card.worldCenterOfMass);
                    Require(controls.PickCard(pixel) == card, "Screen picking hits the visible card");
                    controls.SelectCard(card);
                    Require(controls.Selected == card, "Either hand can become active");
                    Require(controls.transform.position == cameraPosition && controls.transform.rotation == cameraRotation,
                        "Selection does not recenter the camera");
                }
                Require(controls.IsOverInterface(new Vector2(50f / 1920f * Screen.width, Screen.height - 50f / 1080f * Screen.height)), "Control guide blocks scene input");
                controls.SelectCard(null);
                Require(controls.Selected == null, "Deselect works");
                controls.SelectCard(controls.Hands[0]);
                controls.FocusSelection();
                Require(Vector3.Distance(controls.Pivot, controls.Selected.worldCenterOfMass) < 0.001f, "Focus targets selected card");
                controls.Orbit(new Vector2(120, 10000));
                controls.Orbit(new Vector2(-240, -10000));
                Require(controls.transform.position.y >= 0.035f, "Orbit stays above table");
                controls.Pan(new Vector2(100000, -100000));
                Require(Mathf.Abs(controls.Pivot.x) <= 0.5f && Mathf.Abs(controls.Pivot.z) <= 0.4f, "Pan remains bounded");
                controls.Zoom(500);
                Require(Vector3.Distance(controls.transform.position, controls.Pivot) >= 0.119f, "Zoom near limit");
                controls.Zoom(-500);
                Require(Vector3.Distance(controls.transform.position, controls.Pivot) <= 1.601f, "Zoom far limit");
                for (int i = 0; i < 3; i++)
                {
                    controls.QuickView(i);
                    Require(controls.transform.position.y > 0, "Alignment views stay above table");
                }
                controls.ResetView();
                Require(controls.transform.position == cameraPosition && controls.transform.rotation == cameraRotation, "Home restores camera");
                for (int i = 0; i < 2; i++)
                    Require(controls.Hands[i].isKinematic && controls.Hands[i].position == positions[i] && controls.Hands[i].rotation == rotations[i],
                        "Camera and selection preserve both held cards");
                controls.SelectCard(controls.Hands[0]);
                Debug.Log("HATDON_CONTROLS_TEST_OK: picking, selection, independent held cards, focus, orbit, pan, zoom, presets and reset passed.");
            }
            catch (Exception error)
            {
                Debug.LogException(error);
                Application.Quit(1);
                yield break;
            }
            // Capture after a real player frame, including the immediate-mode HUD.
            yield return new WaitForSecondsRealtime(0.5f);
            string output = Path.Combine(Application.dataPath, "../controls-preview-1920x1080.png");
            if (File.Exists(output)) File.Delete(output);
            ScreenCapture.CaptureScreenshot(output);
            for (int i = 0; i < 60 && !File.Exists(output); i++) yield return null;
            yield return null;
            if (!File.Exists(output)) { Debug.LogError("Screenshot did not complete."); Application.Quit(2); }
            else { Debug.Log("HATDON_CONTROLS_PREVIEW_OK: " + output); Application.Quit(0); }
        }

        private static void Require(bool passed, string message)
        {
            if (!passed) throw new Exception(message);
        }
    }
}
#endif
