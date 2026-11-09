using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace Hatdon.CardStacker
{
    public sealed class CardStackerControls : MonoBehaviour
    {
        public const KeyCode HoldRotateKey = KeyCode.E;
        public const KeyCode HoldTiltKey = KeyCode.T;
        public Rigidbody Selected { get; private set; }
        public Rigidbody[] Hands { get; private set; }
        public Vector3 Pivot { get; private set; }
        public CardManipulator Manipulator { get; private set; }
        public CardPlacementSession Placement { get; private set; }
        public CardDeck Deck { get; private set; }
        public CardHandVisuals HandVisuals { get; private set; }
        public CardPickupDrop Pickup { get; private set; }
        public CardRecordingAdmin Admin { get; private set; }
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
            HandVisuals=gameObject.AddComponent<CardHandVisuals>();HandVisuals.Initialise(this);
            gameObject.AddComponent<CardModeToolbar>().Initialise(this);
            Pickup=gameObject.AddComponent<CardPickupDrop>();Pickup.Initialise(this);
            Admin=gameObject.AddComponent<CardRecordingAdmin>();Admin.Initialise(this);
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
            return (Admin!=null && Admin.IsOpen) || (!(Admin!=null && Admin.HideHud) && CardModeToolbar.Contains(screenPoint));
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
            if(Input.GetKeyDown(KeyCode.Escape)){Admin.SetOpen(!Admin.IsOpen);return;}
            if(Admin.IsOpen)return;
            if(Input.GetKeyDown(KeyCode.Space))Admin.TogglePlayback();
            if(Input.GetKeyDown(KeyCode.Period))Admin.StepFrame();
            if(Input.GetKeyDown(KeyCode.F10))Admin.HideHud=!Admin.HideHud;
            Vector2 pointer = Input.mousePosition;
            bool overUI = IsOverInterface(pointer);
            hovered = overUI ? null : PickCard(pointer);
            if (Input.GetMouseButtonDown(0) && !overUI)
            {
                if(Deck.IsHit(pointer))DrawFromDeck(pointer);
                else{SelectCard(hovered);if(CanManipulate){Manipulator.BeginPointerDrag(pointer);orbiting=panning=false;}}
            }
            if (Input.GetMouseButton(0) && Manipulator.Dragging)
                Manipulator.DragPointerTo(pointer, Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift),
                    Input.mouseScrollDelta.y, Input.GetKey(HoldRotateKey), Input.GetKey(HoldTiltKey));
            if (!Input.GetMouseButton(0) && Manipulator.Dragging) Manipulator.EndDrag();
            if (Input.GetKeyDown(KeyCode.W)) SetMouseTool(CardManipulator.Tool.Move,pointer);
            if (Input.GetKeyDown(KeyCode.L) && Selected!=null) Placement.LockHand(System.Array.IndexOf(Hands,Selected));
            if (Input.GetKeyDown(KeyCode.Return)) Placement.CommitBoth();
            if (Input.GetKeyDown(KeyCode.R)) { Placement.ResetCards();Admin.SetPaused(false); }
            if (Input.GetKeyDown(KeyCode.F8)) { Pickup.StartDrop(System.Environment.TickCount);FramePickup();Admin.SetPaused(false); }
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
        public void SetHandlingMode(CardManipulator.Tool mode) => SetMouseTool(mode,Input.mousePosition);
        public void CancelGestures(){orbiting=panning=false;hovered=null;Manipulator.EndDrag();lastPointer=Input.mousePosition;}
        public void SetCameraView(Vector3 position,Vector3 target){Pivot=target;transform.position=position;transform.LookAt(target);KeepAboveTable();}
        public void RestoreCameraView(Vector3 position,Quaternion rotation,Vector3 pivot){Pivot=pivot;transform.SetPositionAndRotation(position,rotation);KeepAboveTable();}

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
        public void FramePickup()
        {
            Pivot=new Vector3(0,.24f,0);transform.position=new Vector3(.65f,.88f,-1.02f);transform.LookAt(Pivot);
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
                line.enabled = !(Admin!=null && Admin.HideHud) && card.gameObject.activeInHierarchy && (card == Selected || card == hovered);
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
