#if HATDON_CAPTURE
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace Hatdon.CardStacker
{
    // An opt-in, deterministic demonstration of the actual gameplay controls.
    public sealed class CardStackerDevlogCapture : MonoBehaviour
    {
        private const int Fps = 30;
        private int frames = 1260, milestone = 1;
        private System.Diagnostics.Process encoder;
        private System.Threading.Tasks.Task<string> encoderErrors;
        private CardStackerControls controls;
        private Vector2 pointer;
        private bool showPointer;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void StartIfRequested()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-recordDevlog") >= 0)
                new GameObject("Devlog capture").AddComponent<CardStackerDevlogCapture>();
        }

        private static string Argument(string name)
        {
            var args = Environment.GetCommandLineArgs();
            int i = Array.IndexOf(args, name);
            if (i < 0 || i + 1 >= args.Length) throw new ArgumentException("Missing " + name);
            return args[i + 1];
        }

        private IEnumerator Start()
        {
            yield return null;
            controls = FindFirstObjectByType<CardStackerControls>();
            if (controls == null || controls.Hands.Length != 2 || Screen.width != 1920 || Screen.height != 1080)
            { UnityEngine.Debug.LogError("Capture needs two cards and a 1920x1080 window."); Application.Quit(1); yield break; }
            controls.CaptureMode = true;
            Application.runInBackground = true;
            var arguments = Environment.GetCommandLineArgs();
            int milestoneOption = Array.IndexOf(arguments, "-recordMilestone");
            if (milestoneOption >= 0 && milestoneOption + 1 < arguments.Length && arguments[milestoneOption + 1] == "2")
            { milestone = 2; frames = 1350; }
            controls.SelectCard(null);
            controls.ResetView();
            Time.captureFramerate = Fps;
            QualitySettings.vSyncCount = 0;
            try
            {
                string output = Path.GetFullPath(Argument("-videoOutput"));
                Directory.CreateDirectory(Path.GetDirectoryName(output));
                encoder = new System.Diagnostics.Process { StartInfo = new ProcessStartInfo {
                    FileName = Argument("-ffmpegPath"),
                    Arguments = "-y -hide_banner -loglevel error -f image2pipe -framerate 30 -c:v png -i pipe:0 -an -c:v libx264 -preset fast -crf 16 -pix_fmt yuv420p -movflags +faststart \"" + output + "\"",
                    UseShellExecute = false, CreateNoWindow = true, RedirectStandardInput = true, RedirectStandardError = true
                }};
                encoder.Start();
                encoderErrors = encoder.StandardError.ReadToEndAsync();
            }
            catch (Exception error)
            { UnityEngine.Debug.LogException(error); Application.Quit(2); yield break; }
            var endOfFrame = new WaitForEndOfFrame();
            for (int frame = 0; frame < frames; frame++)
            {
                if (milestone == 1) Demonstrate(frame); else DemonstrateMovement(frame);
                yield return endOfFrame;
                bool failed = false;
                Texture2D image = null;
                try
                {
                    image = ScreenCapture.CaptureScreenshotAsTexture();
                    byte[] png = image.EncodeToPNG();
                    encoder.StandardInput.BaseStream.Write(png, 0, png.Length);
                }
                catch (Exception error) { UnityEngine.Debug.LogException(error); failed = true; }
                finally { if (image != null) Destroy(image); }
                if (failed) { encoder.StandardInput.Close(); Application.Quit(3); yield break; }
                if (frame % 180 == 0) UnityEngine.Debug.Log("HATDON_CAPTURE_FRAME " + frame + "/" + frames);
            }
            encoder.StandardInput.Close();
            encoder.WaitForExit();
            if (encoder.ExitCode != 0) { UnityEngine.Debug.LogError(encoderErrors.Result); Application.Quit(4); }
            else { UnityEngine.Debug.Log("HATDON_CAPTURE_OK: " + frames + " frames, " + (frames / Fps) + " seconds, 1920x1080 at 30fps."); Application.Quit(0); }
        }

        private void DemonstrateMovement(int frame)
        {
            showPointer = false;
            var m = controls.Manipulator;
            if (frame == 0) { controls.SelectCard(controls.Hands[0]); controls.FocusSelection(); }
            if (frame >= 90 && frame < 210) m.Translate(Vector3.left * (.02f / 120));
            if (frame >= 210 && frame < 330) m.Translate(Vector3.up * (.018f / 120));
            if (frame >= 330 && frame < 420) m.Translate(Vector3.forward * (.012f / 90));
            if (frame == 420)
            {
                pointer = Camera.main.WorldToScreenPoint(controls.Selected.position + Vector3.right * m.HandleSize * .7f);
                if (!m.BeginDrag(pointer)) UnityEngine.Debug.LogError("Fine-control demonstration could not pick a handle.");
            }
            if (frame >= 420 && frame < 510)
            {
                Vector2 direction = (Vector2)Camera.main.WorldToScreenPoint(controls.Selected.position + Vector3.right * m.HandleSize) - (Vector2)Camera.main.WorldToScreenPoint(controls.Selected.position);
                pointer += direction.normalized * .35f; m.DragTo(pointer, true); showPointer = true;
            }
            if (frame == 510) m.SetTool(CardManipulator.Tool.Rotate);
            if (frame >= 510 && frame < 630) m.Rotate(Vector3.forward, .25f);
            if (frame >= 630 && frame < 720) m.Rotate(Vector3.right, 15f / 90);
            if (frame >= 720 && frame < 810) m.Rotate(Vector3.up, 20f / 90);
            if (frame == 810) { controls.SelectCard(controls.Hands[1]); m.SetTool(CardManipulator.Tool.Move); }
            if (frame >= 810 && frame < 870) m.Translate(Vector3.up * (.02f / 60));
            if (frame == 870) m.SetTool(CardManipulator.Tool.Rotate);
            if (frame >= 870 && frame < 960) m.Rotate(Vector3.up, -20f / 90);
            if (frame == 960) { controls.SelectCard(controls.Hands[0]); m.SetTool(CardManipulator.Tool.Move); }
            if (frame >= 990 && frame < 1110) m.Translate(Vector3.down * .0007f);
            if (frame >= 1110 && frame < 1200) m.Translate(Vector3.right * .001f);
            if (frame >= 1200 && frame < 1230) m.Translate(Vector3.up * (.02f / 30));
            if (frame == 1230) { controls.BeginOrbit(); m.SetTool(CardManipulator.Tool.Rotate); }
            if (frame >= 1230) controls.Orbit(new Vector2(.6f, 0));
        }

        private void Demonstrate(int frame)
        {
            showPointer = false;
            if (frame == 90) controls.CaptureHover(Camera.main.WorldToScreenPoint(controls.Hands[0].worldCenterOfMass));
            if (frame >= 90 && frame < 240)
            { pointer = Camera.main.WorldToScreenPoint(controls.Hands[0].worldCenterOfMass); showPointer = true; }
            if (frame == 150) controls.SelectCard(controls.PickCard(pointer));
            if (frame == 240) { controls.CaptureHover(new Vector2(0, 0)); controls.SelectCard(null); }
            if (frame >= 300 && frame < 360)
            { pointer = Camera.main.WorldToScreenPoint(controls.Hands[1].worldCenterOfMass); showPointer = true; }
            if (frame == 300) { controls.CaptureHover(pointer); controls.SelectCard(controls.PickCard(pointer)); }
            if (frame == 360) { controls.CaptureHover(new Vector2(0, 0)); controls.SelectCard(controls.Hands[0]); }
            if (frame >= 360 && frame < 420) { pointer = new Vector2(775, 60); showPointer = true; }
            if (frame == 420) controls.FocusSelection();
            if (frame == 480) controls.BeginOrbit();
            if (frame >= 480 && frame < 630) controls.Orbit(new Vector2(1.4f, 0.2f));
            if (frame >= 630 && frame < 720) controls.Pan(new Vector2(0.45f, -0.1f));
            if (frame == 720 || frame == 750) controls.Zoom(1);
            if (frame == 810) { controls.ResetView(); controls.QuickView(0); }
            if (frame == 900) controls.QuickView(1);
            if (frame == 990) controls.QuickView(2);
            if (frame == 1080) controls.ResetView();
            if (frame == 1170) controls.SelectCard(null);
        }

        private void OnGUI()
        {
            if (!showPointer) return;
            Color old = GUI.color;
            GUI.color = Color.white;
            GUI.DrawTexture(new Rect(pointer.x - 3, Screen.height - pointer.y - 3, 6, 6), Texture2D.whiteTexture);
            GUI.color = old;
        }

        private void OnDestroy()
        {
            Time.captureFramerate = 0;
            if (encoder != null) encoder.Dispose();
        }
    }
}
#endif
