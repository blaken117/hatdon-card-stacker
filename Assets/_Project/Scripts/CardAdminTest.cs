#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.IO;
using System.Collections;
using UnityEngine;
namespace Hatdon.CardStacker
{
    public sealed class CardAdminTest : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void StartIfRequested()
        {
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-cardAdminTest")>=0)new GameObject("Admin menu verification").AddComponent<CardAdminTest>();
        }
        private IEnumerator Start()
        {
            yield return null;
            var controls=FindFirstObjectByType<CardStackerControls>();var admin=controls.Admin;
            try
            {
                Require(Path.GetFileName(admin.ProgressPath).StartsWith("admin-test-"),"Verification uses a separate progress file");
                Require(admin.Shots.Length>=3,"Shot catalogue loads");
                var card=controls.Deck.Draw();Vector3 held=card.position;
                controls.Manipulator.BeginPointerDrag(Camera.main.WorldToScreenPoint(held));
                admin.SetOpen(true);
                Require(Time.timeScale==0 && !controls.Manipulator.Dragging && controls.Selected==card && card.position==held,"Menu pauses and cancels drags without releasing the held card");
                Require(controls.IsOverInterface(new Vector2(20,20)),"Modal menu blocks all scene input");
                admin.PreparePickup(520811);
                Require(controls.Pickup.IsPrepared && admin.Paused && controls.Pickup.Cards.Count==52,"Pickup prepares all 52 before release");
                var positions=new Vector3[52];
                for(int i=0;i<52;i++){positions[i]=controls.Pickup.Cards[i].position;Require(controls.Pickup.Cards[i].isKinematic,"Prepared cloud cannot fall early");}
                admin.SetOpen(false);Require(Time.timeScale==0,"Prepared scene remains paused after closing menu");
                admin.StepFrame();Require(admin.Paused && !controls.Pickup.IsPrepared && Time.timeScale==0 && controls.Pickup.Cards[0].position.y<positions[0].y,"Single-frame step releases and advances physics while staying paused");
                admin.SetSpeed(.5f);admin.TogglePlayback();Require(Mathf.Approximately(Time.timeScale,.5f),"Resume applies selected slow motion");
                admin.SetOpen(true);admin.SetOpen(false);Require(Mathf.Approximately(Time.timeScale,.5f),"Menu round-trip preserves playing state and speed");
                admin.PreparePickup(520811);for(int i=0;i<52;i++)Require(Vector3.Distance(positions[i],controls.Pickup.Cards[i].position)<.000001f,"Same seed repeats every initial position");
                admin.PreparePickup(520812);Require(Vector3.Distance(positions[0],controls.Pickup.Cards[0].position)>.001f,"New seed gives a fresh cloud");
                admin.HideHud=true;
                Require(!controls.IsOverInterface(new Vector2(744f/1920*Screen.width,Screen.height-988f/1080*Screen.height)),"Hidden HUD doesn't block scene input");
                admin.SaveCamera();Vector3 camera=controls.transform.position;controls.Pan(new Vector2(50,25));admin.RestoreCamera();
                Require(Vector3.Distance(camera,controls.transform.position)<.000001f,"Camera bookmark restores");
                admin.MarkRecorded(admin.Shots[0].id,true,"test-take.mp4");admin.NoteFor(admin.Shots[0].id).recorded=false;admin.ReloadProgress();
                Require(admin.NoteFor(admin.Shots[0].id).recorded && admin.NoteFor(admin.Shots[0].id).filePath=="test-take.mp4" && admin.NoteFor(admin.Shots[0].id).seed=="520812","Recorded state, clip path and seed survive reload");
                admin.PrepareHands();Require(controls.HandVisuals.VisibleCount==2 && controls.Deck.Remaining==50,"Ghost hand preset uses two real deck cards");
                controls.Placement.ResetCards();Require(controls.Pickup.Cards.Count==0 && !controls.Pickup.IsPrepared && controls.Deck.Remaining==52,"Reset clears prepared state and restores stock");
                admin.MarkRecorded(admin.Shots[0].id,false,"");
                admin.HideHud=false;admin.SetSpeed(1);admin.PreparePickup(520811);admin.SetOpen(true);
                Debug.Log("HATDON_ADMIN_TEST_OK: menu pause/input, prepared release/step, seed replay, slow motion, hidden HUD, camera bookmark, saved progress and hand preset passed.");
            }
            catch(Exception error){Debug.LogException(error);Application.Quit(1);yield break;}
            yield return new WaitForEndOfFrame();
            string path=Path.Combine(Application.dataPath,"../admin-menu-preview.png");
            if(File.Exists(path))File.Delete(path);
            ScreenCapture.CaptureScreenshot(path);
            for(int frame=0;frame<90 && !File.Exists(path);frame++)yield return null;
            yield return null;
            if(File.Exists(path)){Debug.Log("HATDON_ADMIN_PREVIEW_OK: "+path);Application.Quit(0);}
            else{Debug.LogError("Admin preview screenshot not saved.");Application.Quit(2);}
        }
        private static void Require(bool value,string message){if(!value)throw new Exception(message);}
    }
}
#endif
