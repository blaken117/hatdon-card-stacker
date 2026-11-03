using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace Hatdon.CardStacker
{
    public sealed class CardStackerControls : MonoBehaviour
    {
        public Rigidbody Selected { get; private set; }
        public Rigidbody[] Hands { get; private set; }
        public Vector3 Pivot { get; private set; }
        public CardManipulator Manipulator { get; private set; }
        public CardPlacementSession Placement { get; private set; }
        public CardDeck Deck { get; private set; }
        public int ActiveHand=>lastHandIndex;
        public List<Rigidbody> AllCards { get; }=new List<Rigidbody>();
        public bool CanManipulate => Selected!=null && Placement.CanAdjust(Selected);
        private Camera view;
        private Rigidbody hovered;
        private readonly Dictionary<Rigidbody,LineRenderer> outlines=new Dictionary<Rigidbody,LineRenderer>();
        private Vector3 defaultPosition;
        private Quaternion defaultRotation;
        private bool orbiting, panning;
        private int lastHandIndex;
        private Vector2 lastPointer;
        private Material outlineMaterial;
        private GUIStyle heading, text, muted, button;
        private static readonly Rect Guide = new Rect(24, 24, 352, 492);
        private static readonly Rect PosePanel = new Rect(24, 540, 380, 246);
        private static readonly Rect DeckPanel = new Rect(420,24,450,68);
        private static readonly Rect Views = new Rect(1476, 24, 420, 90);
        private static readonly Rect HandPanel = new Rect(460, 904, 1000, 152);
#if HATDON_CAPTURE
        public bool CaptureMode { get; set; }
        public void CaptureHover(Vector2 point) { hovered = PickCard(point); }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void StartControls()
        {
            if (SceneManager.GetActiveScene().name == "CardStacker" && Camera.main != null &&
                Camera.main.GetComponent<CardStackerControls>() == null)
                Camera.main.gameObject.AddComponent<CardStackerControls>();
        }

        private void Awake()
        {
            CardTableArt.Apply();
            view = GetComponent<Camera>();
            defaultPosition = transform.position;
            defaultRotation = transform.rotation;
            Pivot = new Vector3(0, 0.04f, 0);
            Hands = FindObjectsByType<Rigidbody>(FindObjectsSortMode.None);
            System.Array.Sort(Hands, (a, b) => a.transform.position.x.CompareTo(b.transform.position.x));
            Placement=gameObject.AddComponent<CardPlacementSession>();Placement.Initialise(this);
            outlineMaterial = new Material(Shader.Find("Sprites/Default"));
            foreach(var card in Hands)RegisterCard(card);
            Deck=gameObject.AddComponent<CardDeck>();Deck.Initialise(this);
            Manipulator = gameObject.AddComponent<CardManipulator>();
            Manipulator.Initialise(this);
        }

        public void RegisterCard(Rigidbody card)
        {
                if(outlines.ContainsKey(card))return;
                AllCards.Add(card);
                var edge = new GameObject("Selection outline").AddComponent<LineRenderer>();
                edge.transform.SetParent(card.transform, false);
                edge.sharedMaterial = outlineMaterial;
                edge.useWorldSpace = true;
                edge.positionCount = 4;
                edge.loop = true;
                edge.widthMultiplier = 0.0012f;
                edge.numCornerVertices = 3;
                edge.enabled = false;
                outlines[card] = edge;
        }

        public void RestoreOriginalCards(Rigidbody[] originals)
        {
            SelectCard(null);
            foreach(var card in AllCards.ToArray())
            {
                if(card==null)continue;
                if(!card.isKinematic){card.linearVelocity=Vector3.zero;card.angularVelocity=Vector3.zero;}
                card.isKinematic=true;card.gameObject.SetActive(false);
                if(System.Array.IndexOf(originals,card)<0){outlines.Remove(card);Destroy(card.gameObject);AllCards.Remove(card);}
            }
            for(int i=0;i<2;i++)Hands[i]=originals[i];lastHandIndex=0;
        }

        public bool IsOverInterface(Vector2 screenPoint)
        {
            var p = new Vector2(screenPoint.x * 1920f / Screen.width, (Screen.height - screenPoint.y) * 1080f / Screen.height);
            return Guide.Contains(p) || Views.Contains(p) || HandPanel.Contains(p) || DeckPanel.Contains(p) || (Selected != null && PosePanel.Contains(p));
        }

        public Rigidbody PickCard(Vector2 screenPoint)
        {
            if (IsOverInterface(screenPoint)) return null;
            return Physics.Raycast(view.ScreenPointToRay(screenPoint), out var hit, 10f)
                ? hit.rigidbody : null;
        }

        public void SelectCard(Rigidbody card)
        {
            if (Manipulator != null) Manipulator.EndDrag();
            Selected = card!=null && AllCards.Contains(card) && card.gameObject.activeInHierarchy ? card : null;
            if(Selected!=null && System.Array.IndexOf(Hands,Selected)>=0 && Placement.HasCard(System.Array.IndexOf(Hands,Selected)))lastHandIndex=System.Array.IndexOf(Hands,Selected);
            if(Selected!=null) Placement.BeginAdjust(Selected);
        }

        public void ActivateHand(int i)
        {
            lastHandIndex=i;
            if(Placement.HasCard(i))SelectCard(Hands[i]);
            else{SelectCard(null);Placement.Notify((i==0 ? "Left" : "Right")+" hand is free. Drag a card from the deck.");}
        }
        public void SwitchHand(){ActivateHand(1-lastHandIndex);}
        public void PairCommitted(){SelectCard(null);lastHandIndex=0;}
        public Rigidbody DrawFromDeck(Vector2 pixel)
        {
            var card=Deck.Draw();
            if(card!=null){Manipulator.SetTool(CardManipulator.Tool.Move);Manipulator.BeginDeckDrag(pixel);orbiting=panning=false;}
            return card;
        }

        private void Update()
        {
#if HATDON_CAPTURE
            if (CaptureMode) return;
#endif
            Vector2 pointer = Input.mousePosition;
            bool overUI = IsOverInterface(pointer);
            hovered = overUI ? null : PickCard(pointer);
            if (Input.GetMouseButtonDown(0) && !overUI)
            {
                if(Deck.IsHit(pointer))DrawFromDeck(pointer);
                else{SelectCard(hovered);if(CanManipulate){Manipulator.BeginPointerDrag(pointer);orbiting=panning=false;}}
            }
            if (Input.GetMouseButton(0) && Manipulator.Dragging)
                Manipulator.DragPointerTo(pointer, Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
            if (!Input.GetMouseButton(0) && Manipulator.Dragging) Manipulator.EndDrag();
            if (Input.GetKeyDown(KeyCode.W)) SetMouseTool(CardManipulator.Tool.Move,pointer);
            if (Input.GetKeyDown(KeyCode.Q)) SetMouseTool(CardManipulator.Tool.Depth,pointer);
            if (Input.GetKeyDown(KeyCode.E)) SetMouseTool(CardManipulator.Tool.Rotate,pointer);
            if (Input.GetKeyDown(KeyCode.T)) SetMouseTool(CardManipulator.Tool.Roll,pointer);
            if (Input.GetKeyDown(KeyCode.Escape)) SelectCard(null);
            if (Input.GetKeyDown(KeyCode.L) && Selected!=null) Placement.LockHand(System.Array.IndexOf(Hands,Selected));
            if (Input.GetKeyDown(KeyCode.Return)) Placement.CommitBoth();
            if (Input.GetKeyDown(KeyCode.R)) Placement.ResetCards();
            if (Input.GetKeyDown(KeyCode.Tab)) SwitchHand();
            if (Input.GetKeyDown(KeyCode.F)) FocusSelection();
            if (Input.GetKeyDown(KeyCode.Home)) ResetView();
            if (Input.GetMouseButtonDown(1) && !overUI && !Manipulator.Dragging)
            {
                orbiting = true;
                BeginOrbit();
            }
            if (Input.GetMouseButtonDown(2) && !overUI && !Manipulator.Dragging) panning = true;
            Vector2 delta = pointer - lastPointer;
            if (orbiting && Input.GetMouseButton(1) && !Manipulator.Dragging) Orbit(delta);
            if (panning && Input.GetMouseButton(2) && !Manipulator.Dragging) Pan(delta);
            if (!Input.GetMouseButton(1)) orbiting = false;
            if (!Input.GetMouseButton(2)) panning = false;
            if (!overUI && !Manipulator.Dragging && Mathf.Abs(Input.mouseScrollDelta.y) > 0) Zoom(Input.mouseScrollDelta.y);
            lastPointer = pointer;
        }

        private void SetMouseTool(CardManipulator.Tool mode,Vector2 pixel)
        {
            bool resume=Manipulator.Dragging;Manipulator.SetTool(mode);
            if(resume && CanManipulate)Manipulator.BeginPointerDrag(pixel);
        }

        public void Orbit(Vector2 pixels)
        {
            transform.RotateAround(Pivot, Vector3.up, pixels.x * 0.22f);
            float pitch = Mathf.Asin(Mathf.Clamp(-transform.forward.y, -1, 1)) * Mathf.Rad2Deg;
            float next = Mathf.Clamp(pitch - pixels.y * 0.22f, 10f, 85f);
            transform.RotateAround(Pivot, transform.right, next - pitch);
            KeepAboveTable();
        }

        public void BeginOrbit()
        {
            Pivot = Selected != null ? Selected.worldCenterOfMass : new Vector3(0, 0.04f, 0);
        }

        public void Pan(Vector2 pixels)
        {
            float distance = Vector3.Distance(transform.position, Pivot);
            float metresPerPixel = 2 * distance * Mathf.Tan(view.fieldOfView * Mathf.Deg2Rad / 2) / Screen.height;
            Vector3 shift = (-transform.right * pixels.x - transform.up * pixels.y) * metresPerPixel;
            Vector3 next = Pivot + shift;
            next.x = Mathf.Clamp(next.x, -0.5f, 0.5f);
            next.z = Mathf.Clamp(next.z, -0.4f, 0.4f);
            next.y = Mathf.Clamp(next.y, 0.02f, 0.3f);
            transform.position += next - Pivot;
            Pivot = next;
            KeepAboveTable();
        }

        public void Zoom(float scroll)
        {
            Vector3 offset = transform.position - Pivot;
            float distance = Mathf.Clamp(offset.magnitude * Mathf.Exp(-scroll * 0.12f), 0.12f, 1.6f);
            transform.position = Pivot + offset.normalized * distance;
            KeepAboveTable();
        }

        private void KeepAboveTable()
        {
            Vector3 p = transform.position;
            p.y = Mathf.Max(0.035f, p.y);
            transform.position = p;
        }

        public void FocusSelection()
        {
            Pivot = Selected != null ? Selected.worldCenterOfMass : new Vector3(0, 0.04f, 0);
            float distance = Selected != null ? 0.26f : 0.58f;
            transform.position = Pivot - transform.forward * distance;
            KeepAboveTable();
            transform.LookAt(Pivot);
        }

        public void ResetView()
        {
            Pivot = new Vector3(0, 0.04f, 0);
            transform.SetPositionAndRotation(defaultPosition, defaultRotation);
        }

        public void QuickView(int index)
        {
            Pivot = Selected != null ? Selected.worldCenterOfMass : Pivot;
            float distance = Mathf.Clamp(Vector3.Distance(transform.position, Pivot), 0.26f, 1.2f);
            Quaternion rotation = Quaternion.Euler(index == 2 ? 89.5f : 18f, index == 1 ? 90f : 0f, 0);
            transform.SetPositionAndRotation(Pivot - rotation * Vector3.forward * distance, rotation);
        }

        private void LateUpdate()
        {
            foreach(var item in outlines)
            {
                var card = item.Key;
                var line = item.Value;
                if(card==null || line==null)continue;
                line.enabled = card.gameObject.activeInHierarchy && (card == Selected || card == hovered);
                if (!line.enabled) continue;
                Color color = card == Selected ? new Color(1, 0.74f, 0.24f) : new Color(0.48f, 0.88f, 1);
                line.startColor = line.endColor = color;
                float side = card.transform.InverseTransformPoint(transform.position).z >= 0 ? 0.65f : -0.65f;
                line.SetPosition(0, card.transform.TransformPoint(new Vector3(-0.51f, -0.51f, side)));
                line.SetPosition(1, card.transform.TransformPoint(new Vector3(-0.51f, 0.51f, side)));
                line.SetPosition(2, card.transform.TransformPoint(new Vector3(0.51f, 0.51f, side)));
                line.SetPosition(3, card.transform.TransformPoint(new Vector3(0.51f, -0.51f, side)));
            }
        }

        private void OnGUI()
        {
            if (heading == null)
            {
                heading = new GUIStyle(GUI.skin.label) { fontSize = 21, fontStyle = FontStyle.Bold };
                text = new GUIStyle(GUI.skin.label) { fontSize = 18 };
                muted = new GUIStyle(text); muted.normal.textColor = new Color(0.7f, 0.8f, 0.81f);
                button = new GUIStyle(GUI.skin.button) { fontSize = 18, alignment = TextAnchor.MiddleCenter };
            }
            Matrix4x4 old = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(new Vector3(Screen.width / 1920f, Screen.height / 1080f, 1));
            Panel(Guide); Panel(Views); Panel(HandPanel);
            GUI.Label(new Rect(42, 38, 310, 30), "CONTROLS", heading);
            string[] rows = { "Drag from deck   Draw one card", "Left-drag card   Adjust", "W   Move left/right/up/down", "Q   Depth — nearer / farther", "E   Rotate about card centre", "T   Twist about view axis", "Shift   Fine adjustment", "L   Lock hand · Tab   Switch", "Enter   Commit locked pair", "R   Reset table and deck", "Right-drag   Orbit camera", "Middle-drag   Pan camera", "Mouse wheel   Zoom", "F   Focus · Home   Reset view", "Esc / empty space   Deselect" };
            for (int i = 0; i < rows.Length; i++) GUI.Label(new Rect(42, 78 + i * 27, 315, 27), rows[i], text);
            GUI.Label(new Rect(1494, 34, 390, 25), "ALIGNMENT VIEWS", muted);
            if (GUI.Button(new Rect(1494, 66, 120, 34), "Front", button)) QuickView(0);
            if (GUI.Button(new Rect(1626, 66, 120, 34), "Side", button)) QuickView(1);
            if (GUI.Button(new Rect(1758, 66, 120, 34), "Overhead", button)) QuickView(2);
            Panel(DeckPanel);
            GUI.Label(new Rect(438,34,420,26),"DECK  ·  "+Deck.Remaining+" cards remaining",heading);
            GUI.Label(new Rect(438,64,420,24),"Drag one card into a free hand",muted);
            if (Selected != null)
            {
                Panel(PosePanel);
                GUI.Label(new Rect(42, 554, 340, 26), !Selected.isKinematic ? "INSPECT PLACED CARD" : "MOUSE CONTROL", heading);
                GUI.enabled=CanManipulate;
                GUI.backgroundColor = Manipulator.Mode == CardManipulator.Tool.Move ? new Color(1, .75f, .3f) : Color.white;
                if (GUI.Button(new Rect(42, 590, 160, 34), "Move  W", button)) Manipulator.SetTool(CardManipulator.Tool.Move);
                GUI.backgroundColor = Manipulator.Mode == CardManipulator.Tool.Rotate ? new Color(1, .75f, .3f) : Color.white;
                if (GUI.Button(new Rect(212, 590, 170, 34), "Rotate  E", button)) Manipulator.SetTool(CardManipulator.Tool.Rotate);
                GUI.backgroundColor=Manipulator.Mode==CardManipulator.Tool.Depth ? new Color(1,.75f,.3f) : Color.white;
                if(GUI.Button(new Rect(42,632,160,34),"Depth  Q",button))Manipulator.SetTool(CardManipulator.Tool.Depth);
                GUI.backgroundColor=Manipulator.Mode==CardManipulator.Tool.Roll ? new Color(1,.75f,.3f) : Color.white;
                if(GUI.Button(new Rect(212,632,170,34),"Twist  T",button))Manipulator.SetTool(CardManipulator.Tool.Roll);
                GUI.enabled=true;
                GUI.backgroundColor = Color.white;
                Vector3 p = Selected.position * 1000, a = Selected.rotation.eulerAngles;
                a = new Vector3(Mathf.DeltaAngle(0, a.x), Mathf.DeltaAngle(0, a.y), Mathf.DeltaAngle(0, a.z));
                GUI.Label(new Rect(42, 680, 345, 26), $"mm   X {p.x:F1}   Y {p.y:F1}   Z {p.z:F1}", text);
                GUI.Label(new Rect(42, 710, 345, 26), $"deg  X {a.x:F1}   Y {a.y:F1}   Z {a.z:F1}", text);
                GUI.Label(new Rect(42, 750, 345, 26), !Selected.isKinematic ? "Placed · 1.8 g · gravity active" : Manipulator.ContactBlocked ? "Contact — adjustment limited" : Manipulator.FineAdjustment ? "Fine adjustment · 10% sensitivity" : "Left-drag the card in this mode", muted);
            }
            GUI.Label(new Rect(480, 916, 960, 26),Placement.Status,text);
            for (int i = 0; i < Mathf.Min(2, Hands.Length); i++)
            {
                GUI.backgroundColor = Selected == Hands[i] ? new Color(1, 0.75f, 0.3f) : Color.white;
                if(!Placement.HasCard(i))GUI.backgroundColor=ActiveHand==i ? new Color(.6f,.8f,.8f) : Color.white;
                string state=!Placement.HasCard(i) ? "Empty" : Placement.IsLocked(i) ? "Locked" : "Adjust";
                if (GUI.Button(new Rect(480+i*365, 950, 225, 36), (i == 0 ? "LEFT" : "RIGHT")+" · "+state,button)) ActivateHand(i);
                GUI.backgroundColor=Color.white;GUI.enabled=Placement.HasCard(i) && !Placement.IsLocked(i);
                if(GUI.Button(new Rect(715+i*365,950,110,36),"Lock",button)) Placement.LockHand(i);
                GUI.enabled=true;
            }
            GUI.backgroundColor = Color.white;
            GUI.enabled=Placement.BothLocked;
            if(GUI.Button(new Rect(1210,950,230,36),Placement.IsCommitted ? "Pair placed" : "Commit pair · Enter",button)) Placement.CommitBoth();
            GUI.enabled=true;
            if(GUI.Button(new Rect(480,1003,210,32),"Reset table · R",button)) Placement.ResetCards();
            GUI.Label(new Rect(715,1006,725,30),"Two hands at a time. Committed cards stay on the table.",muted);
            GUI.Label(new Rect(24, 1000, 410, 55), "Drag a card from the deck.\nChoose a mode, then drag to adjust.", muted);
            GUI.matrix = old;
        }

        private static void Panel(Rect rect)
        {
            Color old = GUI.color;
            GUI.color = new Color(0.05f, 0.09f, 0.11f, 0.94f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = old;
        }

        private void OnDestroy()
        {
            foreach (var line in outlines.Values) if (line != null) Destroy(line.gameObject);
            if (outlineMaterial != null) Destroy(outlineMaterial);
        }

        private void OnApplicationFocus(bool focused)
        {
#if HATDON_CAPTURE
            if(CaptureMode)return;
#endif
            if (!focused)
            {
                orbiting = panning = false;
                if (Manipulator != null) Manipulator.EndDrag();
            }
        }
    }
}
