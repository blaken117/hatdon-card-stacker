using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace Hatdon.CardStacker
{
    // Human-operated scene preparation and shot notes. This component never records video.
    public sealed class CardRecordingAdmin : MonoBehaviour
    {
        [Serializable] public sealed class Shot { public string id,title,preset,instructions; }
        [Serializable] private sealed class ShotList { public Shot[] shots; }
        [Serializable] public sealed class TakeNote { public string id,filePath,updatedUtc,seed; public bool recorded; }
        [Serializable] private sealed class Progress
        {
            public List<TakeNote> takes=new List<TakeNote>();
            public bool hasView;public Vector3 cameraPosition,pivot;public Quaternion cameraRotation;
        }
        private CardStackerControls controls;
        private Progress progress=new Progress();
        private GUIStyle title,text,small,button,section,toggle,input;
        private int selected;
        private string seedText="520811",notice="Prepare a shot, start your screen recorder, then use Space to play.";
        private Vector2 shotScroll;
        public bool IsOpen { get; private set; }
        public bool Paused { get; private set; }
        public bool HideHud { get; set; }
        public float PlaybackSpeed { get; private set; }=1;
        public Shot[] Shots { get; private set; }
        public string ProgressPath { get; private set; }

        public void Initialise(CardStackerControls owner)
        {
            controls=owner;Application.runInBackground=true;
            Shots=JsonUtility.FromJson<ShotList>(Resources.Load<TextAsset>("DevlogShots").text).shots;
            ProgressPath=Path.Combine(Application.persistentDataPath,"devlog-recording-progress.json");
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            var args=Environment.GetCommandLineArgs();int option=Array.IndexOf(args,"-adminTestState");
            if(option>=0 && option+1<args.Length)ProgressPath=args[option+1];
#endif
            ReloadProgress();ApplyClock();
        }
        public void ReloadProgress()
        {
            try{if(File.Exists(ProgressPath))progress=JsonUtility.FromJson<Progress>(File.ReadAllText(ProgressPath)) ?? new Progress();}
            catch(Exception error){Debug.LogWarning("Recording notes could not be loaded: "+error.Message);}
            if(progress.takes==null)progress.takes=new List<TakeNote>();
            if(Shots!=null && Shots.Length>selected && !string.IsNullOrEmpty(NoteFor(Shots[selected].id).seed))seedText=NoteFor(Shots[selected].id).seed;
        }

        public void SetOpen(bool open){IsOpen=open;controls.CancelGestures();ApplyClock();}
        public void SetPaused(bool paused){Paused=paused;ApplyClock();}
        public void SetSpeed(float speed){PlaybackSpeed=Mathf.Clamp(speed,.1f,1);ApplyClock();}
        private void ApplyClock()=>Time.timeScale=(IsOpen || Paused) ? 0 : PlaybackSpeed;
        public void TogglePlayback()
        {
            if(controls.Pickup.IsPrepared){controls.Pickup.ReleasePrepared();SetPaused(false);}
            else SetPaused(!Paused);
        }
        public void StepFrame()
        {
            if(!Paused && !IsOpen)return;
            if(controls.Pickup.IsPrepared)controls.Pickup.ReleasePrepared();
            SetPaused(true);var previous=Physics.simulationMode;
            try
            {
                Physics.simulationMode=SimulationMode.Script;
                var models=FindObjectsByType<CardAerodynamics>(FindObjectsSortMode.None);
                for(int step=0;step<4;step++){foreach(var model in models)model.ApplyForces();Physics.Simulate(CardPlacementSession.Step);}
            }
            finally{Physics.simulationMode=previous;}
        }

        public void PreparePickup(int seed)
        {
            controls.CancelGestures();controls.Pickup.PrepareDrop(seed);controls.FramePickup();SetPaused(true);
            seedText=seed.ToString();notice="52 cards are ready. Close this menu, start your recorder, then press Space to release.";
        }
        public void PrepareHands()
        {
            controls.Placement.ResetCards();
            for(int i=0;i<2;i++)
            {
                var card=controls.Deck.Draw();card.position=new Vector3(i==0 ? -.045f : .045f,.07f,0);
                card.rotation=i==0 ? Quaternion.Euler(0,25,0) : Quaternion.Euler(35,-25,20);
                controls.Placement.LockHand(i);
            }
            Physics.SyncTransforms();controls.HandVisuals.Refresh();
            controls.SetCameraView(new Vector3(.30f,.37f,-.55f),new Vector3(0,.14f,0));SetPaused(false);
            notice="Ghost hands are ready. Close the menu and record. Enter releases both cards.";
        }
        public void PrepareSelected()
        {
            var shot=Shots[selected];
            if(shot.preset=="pickup")
            {
                if(!int.TryParse(seedText,out int seed)){notice="Enter a whole-number seed first.";return;}
                PreparePickup(seed);
            }
            else if(shot.preset=="hands")PrepareHands();
            else if(shot.preset=="normal"){controls.Placement.ResetCards();controls.ResetView();HideHud=false;SetPaused(false);}
            SetOpen(false);
        }

        public TakeNote NoteFor(string id)
        {
            var note=progress.takes.Find(item=>item.id==id);
            if(note==null){note=new TakeNote{id=id,filePath=""};progress.takes.Add(note);}return note;
        }
        public int PendingCount { get {int total=0;foreach(var shot in Shots)if(!NoteFor(shot.id).recorded)total++;return total;} }
        public void MarkRecorded(string id,bool recorded,string filePath)
        {
            var note=NoteFor(id);note.recorded=recorded;note.filePath=filePath;note.updatedUtc=DateTime.UtcNow.ToString("o");SaveProgress();
        }
        private void SaveProgress()
        {
            try{NoteFor(Shots[selected].id).seed=seedText;Directory.CreateDirectory(Path.GetDirectoryName(ProgressPath));File.WriteAllText(ProgressPath,JsonUtility.ToJson(progress,true));notice="Recording notes saved locally.";}
            catch(Exception error){notice="Could not save recording notes: "+error.Message;}
        }
        public void SaveCamera()
        {
            progress.cameraPosition=controls.transform.position;progress.cameraRotation=controls.transform.rotation;progress.pivot=controls.Pivot;progress.hasView=true;SaveProgress();
        }
        public void RestoreCamera(){if(progress.hasView)controls.RestoreCameraView(progress.cameraPosition,progress.cameraRotation,progress.pivot);}

        private bool Button(float x,float y,float width,string label)=>GUI.Button(new Rect(x,y,width,42),label,button);
        private void OnGUI()
        {
            if(!IsOpen)return;
            if(title==null)
            {
                title=new GUIStyle(GUI.skin.label){fontSize=28,fontStyle=FontStyle.Bold};
                section=new GUIStyle(GUI.skin.label){fontSize=21,fontStyle=FontStyle.Bold};
                text=new GUIStyle(GUI.skin.label){fontSize=18,wordWrap=true};
                small=new GUIStyle(text){fontSize=16};button=new GUIStyle(GUI.skin.button){fontSize=18};
                toggle=new GUIStyle(GUI.skin.toggle){fontSize=18};input=new GUIStyle(GUI.skin.textField){fontSize=18};
            }
            Matrix4x4 matrix=GUI.matrix;Color color=GUI.color;
            GUI.matrix=Matrix4x4.Scale(new Vector3(Screen.width/1920f,Screen.height/1080f,1));
            GUI.color=new Color(0,0,0,.7f);GUI.DrawTexture(new Rect(0,0,1920,1080),Texture2D.whiteTexture);
            GUI.color=new Color(.06f,.10f,.12f,.99f);GUI.DrawTexture(new Rect(330,100,1260,880),Texture2D.whiteTexture);GUI.color=Color.white;
            GUI.Label(new Rect(366,126,800,42),"DEV LOG / ADMIN TOOLS",title);
            GUI.Label(new Rect(366,176,1000,28),"Game paused while this menu is open. This panel prepares scenes; use your own screen recorder.",small);
            if(Button(1410,128,140,"Resume · Esc"))SetOpen(false);
            GUI.Label(new Rect(366,225,285,32),"SHOTS TO RECORD · "+PendingCount,section);
            shotScroll=GUI.BeginScrollView(new Rect(366,267,300,335),shotScroll,new Rect(0,0,276,Mathf.Max(330,Shots.Length*72)));
            for(int i=0;i<Shots.Length;i++)
            {
                GUI.backgroundColor=selected==i ? new Color(.5f,.78f,.83f) : Color.white;
                string state=NoteFor(Shots[i].id).recorded ? "Recorded" : "Pending";
                if(GUI.Button(new Rect(0,i*72,276,62),Shots[i].title+"\n"+state,button)){selected=i;if(!string.IsNullOrEmpty(NoteFor(Shots[i].id).seed))seedText=NoteFor(Shots[i].id).seed;}
            }
            GUI.EndScrollView();GUI.backgroundColor=Color.white;
            if(Button(366,624,276,"Save recording notes"))SaveProgress();
            if(Button(366,680,276,"Copy notes file path")){GUIUtility.systemCopyBuffer=ProgressPath;notice="Recording notes file path copied.";}
            if(Button(366,736,276,"1920 × 1080 window"))Screen.SetResolution(1920,1080,FullScreenMode.Windowed);
            GUI.Label(new Rect(366,784,276,26),"Current: "+Screen.width+" × "+Screen.height,small);
            Shot active=Shots[selected];var note=NoteFor(active.id);
            GUI.Label(new Rect(706,225,800,36),active.title,section);
            GUI.Label(new Rect(706,274,800,130),active.instructions,text);
            GUI.Label(new Rect(706,413,70,32),"Seed",text);
            seedText=GUI.TextField(new Rect(774,408,210,36),seedText,20,input);
            if(Button(1000,405,180,"New random seed"))seedText=Environment.TickCount.ToString();
            if(Button(1196,405,354,"Prepare shot & close menu"))PrepareSelected();
            GUI.Label(new Rect(706,468,300,32),"PLAYBACK",section);
            if(Button(706,510,180,Paused ? "Resume / release" : "Pause"))TogglePlayback();
            if(Button(900,510,160,"Step 1/60 s"))StepFrame();
            float[] speeds={.25f,.5f,1};
            for(int i=0;i<3;i++)
            {
                GUI.backgroundColor=Mathf.Approximately(PlaybackSpeed,speeds[i]) ? new Color(.5f,.78f,.83f) : Color.white;
                if(Button(1074+i*160,510,148,speeds[i].ToString("0.##")+"x speed"))SetSpeed(speeds[i]);
            }
            GUI.backgroundColor=Color.white;
            HideHud=GUI.Toggle(new Rect(706,570,820,34),HideHud,"  Hide HUD / selection markers for clean takes · F10",toggle);
            GUI.Label(new Rect(706,618,350,30),"CAMERA / SCENE",section);
            if(Button(706,658,180,"Wide drop view"))controls.FramePickup();
            if(Button(900,658,160,"Table view"))controls.SetCameraView(new Vector3(.46f,.48f,-.65f),new Vector3(0,.025f,0));
            if(Button(1074,658,148,"Save view"))SaveCamera();
            GUI.enabled=progress.hasView;if(Button(1236,658,148,"Restore view"))RestoreCamera();GUI.enabled=true;
            if(Button(1398,658,152,"Reset table")){controls.Placement.ResetCards();controls.ResetView();SetPaused(false);}
            GUI.Label(new Rect(706,718,800,28),"Optional footage file path (for your editing handoff)",small);
            note.filePath=GUI.TextField(new Rect(706,751,594,36),note.filePath ?? "",4096,input);
            if(Button(1314,747,236,note.recorded ? "Mark pending again" : "Mark as recorded"))MarkRecorded(active.id,!note.recorded,note.filePath);
            GUI.Label(new Rect(366,822,1180,52),notice,text);
            GUI.Label(new Rect(366,893,1180,50),"Outside menu: Space play/pause · . single frame · F10 HUD · Esc menu · L lock · Tab hand · Enter release · R reset",small);
            GUI.color=color;GUI.matrix=matrix;
        }
        private void OnDestroy(){Time.timeScale=1;}
    }
}
