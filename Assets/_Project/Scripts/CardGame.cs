using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

namespace Hatdon.CardStacker
{
    public sealed partial class CardGame : MonoBehaviour
    {
        private const int Practice = -1;
        private const int Recording = -2;
        [Serializable] private sealed class SavedCard
        {
            public Vector3 position;
            public Quaternion rotation;
            public int number;
            public int slot;
            public bool held;
            public bool sleeping;
            public Vector3 velocity;
            public Vector3 spin;
        }
        [Serializable] private sealed class Table
        {
            public int remaining = 52;
            public bool completed;
            public bool guide = true;
            public List<SavedCard> cards = new List<SavedCard>();
        }

        private CardStackerControls controls;
        private string directory;
        private readonly Dictionary<int,Table> tables = new Dictionary<int,Table>();
        private readonly List<Rigidbody> candidates = new List<Rigidbody>(52);
        private readonly int[] matches = new int[52];
        private readonly bool[] visited = new bool[52];
        private int active = Recording;
        private bool menu = true;
        private bool confirmReset;
        private bool pausedBeforeReset;
        private bool recordingPreview;
        private Table table;
        private string previousSave, saveIssue;
        private float nextSave, nextMatch, matchSince = -1;
        private GameObject guideRoot;
        private Material guideMaterial;

        public bool MenuOpen => menu;
        public bool BlocksTable => menu || confirmReset;
        internal bool IsComplete => table != null && table.completed;
        internal bool GuideVisible => table != null && table.guide && guideRoot != null && guideRoot.activeSelf;

        internal void ToggleGuide()
        {
            table.guide = !table.guide;
            guideRoot.SetActive(table.guide && !controls.Admin.HideHud);
            Save();
        }

        public void Initialise(CardStackerControls owner, string saveDirectory)
        {
            controls = owner;
            directory = saveDirectory;
            controls.Admin.SetPaused(true);
        }

        private string PathFor(int id) => Path.Combine(directory, id == Practice ? "practice.json" : $"level-{id + 1:00}.json");

        private Table Read(int id)
        {
            if (tables.TryGetValue(id,out Table cached)) return cached;
            try
            {
                string path = PathFor(id);
                if (File.Exists(path))
                {
                    var saved = JsonUtility.FromJson<Table>(File.ReadAllText(path));
                    if (saved?.cards != null && saved.cards.Count <= 52)
                        return tables[id] = saved;
                }
            }
            catch (Exception error) when (error is IOException || error is UnauthorizedAccessException || error is ArgumentException)
            {
                Debug.LogWarning("Could not read Card Stacker save: " + error.Message);
                saveIssue = "A table save could not be read.";
            }
            return tables[id] = new Table();
        }

        public void OpenTable(int id)
        {
            Save();
            recordingPreview = false;
            active = id;
            table = Read(id);
            previousSave = JsonUtility.ToJson(table);
            controls.Placement.ResetCards();
            foreach (var card in table.cards)
            {
                Rigidbody restored = controls.Placement.RestoreCard(card.slot,card.position,card.rotation,card.number,card.held);
                if (restored.isKinematic) continue;
                if (card.sleeping) restored.Sleep();
                else
                {
                    restored.linearVelocity = card.velocity;
                    restored.angularVelocity = card.spin;
                }
            }
            controls.SelectCard(null);
            controls.Deck.SetRemaining(table.remaining);
            Physics.SyncTransforms();
            controls.Placement.Notify(table.cards.Count == 0 ? "Drag a card from the deck." :
                controls.Placement.BothLocked ? "Click Commit pair to release both cards." : "Table restored.");
            controls.ResetView();
            controls.Admin.HideHud = false;
            controls.Admin.SetPaused(false);
            nextSave = nextMatch = Time.unscaledTime;
            matchSince = -1;
            menu = false;
            BuildGuide();
        }

        public void ShowMenu()
        {
            Save();
            recordingPreview = false;
            active = Recording;
            table = null;
            menu = true;
            controls.CancelGestures();
            controls.Admin.SetPaused(true);
            ClearGuide();
        }

        public void OpenRecording()
        {
            if (active != Recording || menu)
            {
                Save();
                active = Recording;
                table = null;
                menu = false;
                confirmReset = false;
                ClearGuide();
                controls.Placement.ResetCards();
                controls.ResetView();
            }
        }

        public void PreviewChallenge()
        {
            OpenRecording();
            recordingPreview = true;
            active = 3;
            table = new Table();
            controls.Admin.HideHud = false;
            controls.Admin.SetPaused(false);
            BuildGuide();
        }

        public void RequestReset()
        {
            if (active == Recording)
            {
                controls.ResetTable();
                return;
            }
            confirmReset = true;
            pausedBeforeReset = controls.Admin.Paused;
            controls.CancelGestures();
            controls.Admin.SetPaused(true);
        }

        public void CancelReset()
        {
            confirmReset = false;
            controls.Admin.SetPaused(pausedBeforeReset);
        }

        public void HandleEscape()
        {
            if (confirmReset) CancelReset();
            else if (!menu) ShowMenu();
        }

        internal void ResetCurrentTable()
        {
            table.completed = false;
            matchSince = -1;
            controls.ResetTable();
            Save();
            confirmReset = false;
        }

        private void Save()
        {
            if (table == null || recordingPreview) return;
            table.remaining = controls.Deck.Remaining;
            table.cards.Clear();
            foreach (var card in controls.AllCards)
            {
                if (!card.gameObject.activeInHierarchy) continue;
                int slot = Array.IndexOf(controls.Hands,card);
                if (!controls.Placement.HasCard(slot)) slot = -1;
                table.cards.Add(new SavedCard
                {
                    slot = slot, position = card.position, rotation = card.rotation,
                    number = controls.Placement.CardNumber(card), held = controls.Placement.IsLocked(slot),
                    sleeping = !card.isKinematic && card.IsSleeping(),
                    velocity = card.isKinematic ? Vector3.zero : card.linearVelocity,
                    spin = card.isKinematic ? Vector3.zero : card.angularVelocity
                });
            }

            string json = JsonUtility.ToJson(table);
            if (json == previousSave) return;
            try
            {
                Directory.CreateDirectory(directory);
                string path = PathFor(active);
                string pending = path + ".tmp";
                File.WriteAllText(pending,json);
                if (File.Exists(path)) File.Replace(pending,path,null);
                else File.Move(pending,path);
                previousSave = json;
                saveIssue = null;
            }
            catch (Exception error) when (error is IOException || error is UnauthorizedAccessException)
            {
                saveIssue = "Could not save this table. Check that the save folder is writable.";
                Debug.LogWarning("Could not save Card Stacker table: " + error.Message);
            }
        }

        private void Update()
        {
            if (guideRoot != null) guideRoot.SetActive(table.guide && !controls.Admin.HideHud && !controls.Admin.IsOpen);
            if (table == null) return;
            float now = Time.unscaledTime;
            if (now >= nextSave) { nextSave = now + 2f; Save(); }
            if (active < 0 || table.completed || now < nextMatch || Time.timeScale == 0) return;
            nextMatch = now + .2f;
            if (controls.Placement.HasCard(0) || controls.Placement.HasCard(1))
            {
                matchSince = -1;
                return;
            }
            candidates.Clear();
            foreach (var card in controls.AllCards)
                if (card.gameObject.activeInHierarchy && !card.isKinematic &&
                    card.linearVelocity.sqrMagnitude < .0001f && card.angularVelocity.sqrMagnitude < .04f)
                    candidates.Add(card);
            if (!CardChallenges.Matches(CardChallenges.Levels[active],candidates,matches,visited))
            {
                matchSince = -1;
                return;
            }
            if (matchSince < 0) matchSince = Time.time;
            if (Time.time - matchSince < 3f) return;
            table.completed = true;
            Save();
        }

        private void BuildGuide()
        {
            ClearGuide();
            if (active < 0) return;
            guideMaterial ??= new Material(Shader.Find("Sprites/Default")) { color = new Color(.38f,.91f,.88f,.72f) };
            guideRoot = new GameObject("Challenge guide");
            foreach (Pose pose in CardChallenges.Levels[active].Cards)
            {
                var ghost = new GameObject("Ghost card");
                ghost.transform.SetParent(guideRoot.transform);
                ghost.transform.SetPositionAndRotation(pose.position,pose.rotation);
                var line = ghost.AddComponent<LineRenderer>();
                line.sharedMaterial = guideMaterial;
                line.useWorldSpace = false;
                line.loop = true;
                line.widthMultiplier = .0012f;
                line.shadowCastingMode = ShadowCastingMode.Off;
                line.receiveShadows = false;
                line.positionCount = 4;
                line.SetPositions(new[]
                {
                    new Vector3(-.0315f,-.044f,0), new Vector3(-.0315f,.044f,0),
                    new Vector3(.0315f,.044f,0), new Vector3(.0315f,-.044f,0)
                });
            }
            guideRoot.SetActive(table.guide);
        }

        private void ClearGuide()
        {
            if (guideRoot == null) return;
            guideRoot.SetActive(false);
            Destroy(guideRoot);
            guideRoot = null;
        }

        private void OnApplicationFocus(bool focused) { if (!focused) Save(); }
        private void OnApplicationPause(bool paused) { if (paused) Save(); }
        private void OnApplicationQuit() => Save();
        private void OnDestroy() { ClearGuide(); if (guideMaterial != null) Destroy(guideMaterial); }
    }
}
