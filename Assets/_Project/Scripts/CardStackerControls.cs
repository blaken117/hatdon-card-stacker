using UnityEngine;
using UnityEngine.SceneManagement;

namespace Hatdon.CardStacker
{
    public sealed class CardStackerControls : MonoBehaviour
    {
        public Rigidbody Selected { get; private set; }
        public Rigidbody[] Hands { get; private set; }
        public Vector3 Pivot { get; private set; }
        private Camera view;
        private Rigidbody hovered;
        private LineRenderer[] outlines;
        private Vector3 defaultPosition;
        private Quaternion defaultRotation;
        private bool orbiting, panning;
        private Vector2 lastPointer;
        private Material outlineMaterial;
        private GUIStyle heading, text, muted, button;
        private static readonly Rect Guide = new Rect(24, 24, 352, 310);
        private static readonly Rect Views = new Rect(1476, 24, 420, 90);
        private static readonly Rect HandPanel = new Rect(590, 954, 740, 102);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void StartControls()
        {
            if (SceneManager.GetActiveScene().name == "CardStacker" && Camera.main != null &&
                Camera.main.GetComponent<CardStackerControls>() == null)
                Camera.main.gameObject.AddComponent<CardStackerControls>();
        }

        private void Awake()
        {
            view = GetComponent<Camera>();
            defaultPosition = transform.position;
            defaultRotation = transform.rotation;
            Pivot = new Vector3(0, 0.04f, 0);
            Hands = FindObjectsByType<Rigidbody>(FindObjectsSortMode.None);
            System.Array.Sort(Hands, (a, b) => a.transform.position.x.CompareTo(b.transform.position.x));
            outlines = new LineRenderer[Hands.Length];
            outlineMaterial = new Material(Shader.Find("Sprites/Default"));
            for (int i = 0; i < Hands.Length; i++)
            {
                var edge = new GameObject("Selection outline").AddComponent<LineRenderer>();
                edge.transform.SetParent(Hands[i].transform, false);
                edge.sharedMaterial = outlineMaterial;
                edge.useWorldSpace = true;
                edge.positionCount = 4;
                edge.loop = true;
                edge.widthMultiplier = 0.0012f;
                edge.numCornerVertices = 3;
                edge.enabled = false;
                outlines[i] = edge;
            }
        }

        public bool IsOverInterface(Vector2 screenPoint)
        {
            var p = new Vector2(screenPoint.x * 1920f / Screen.width, (Screen.height - screenPoint.y) * 1080f / Screen.height);
            return Guide.Contains(p) || Views.Contains(p) || HandPanel.Contains(p);
        }

        public Rigidbody PickCard(Vector2 screenPoint)
        {
            if (IsOverInterface(screenPoint)) return null;
            return Physics.Raycast(view.ScreenPointToRay(screenPoint), out var hit, 10f)
                ? hit.rigidbody : null;
        }

        public void SelectCard(Rigidbody card)
        {
            Selected = System.Array.IndexOf(Hands, card) >= 0 ? card : null;
        }

        private void Update()
        {
            Vector2 pointer = Input.mousePosition;
            bool overUI = IsOverInterface(pointer);
            hovered = overUI ? null : PickCard(pointer);
            if (Input.GetMouseButtonDown(0) && !overUI) SelectCard(hovered);
            if (Input.GetKeyDown(KeyCode.Escape)) SelectCard(null);
            if (Input.GetKeyDown(KeyCode.F)) FocusSelection();
            if (Input.GetKeyDown(KeyCode.Home)) ResetView();
            if (Input.GetMouseButtonDown(1) && !overUI)
            {
                orbiting = true;
                Pivot = Selected != null ? Selected.worldCenterOfMass : new Vector3(0, 0.04f, 0);
            }
            if (Input.GetMouseButtonDown(2) && !overUI) panning = true;
            Vector2 delta = pointer - lastPointer;
            if (orbiting && Input.GetMouseButton(1)) Orbit(delta);
            if (panning && Input.GetMouseButton(2)) Pan(delta);
            if (!Input.GetMouseButton(1)) orbiting = false;
            if (!Input.GetMouseButton(2)) panning = false;
            if (!overUI && Mathf.Abs(Input.mouseScrollDelta.y) > 0) Zoom(Input.mouseScrollDelta.y);
            lastPointer = pointer;
        }

        public void Orbit(Vector2 pixels)
        {
            transform.RotateAround(Pivot, Vector3.up, pixels.x * 0.22f);
            float pitch = Mathf.Asin(Mathf.Clamp(-transform.forward.y, -1, 1)) * Mathf.Rad2Deg;
            float next = Mathf.Clamp(pitch - pixels.y * 0.22f, 10f, 85f);
            transform.RotateAround(Pivot, transform.right, next - pitch);
            KeepAboveTable();
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
            for (int i = 0; i < Hands.Length; i++)
            {
                var card = Hands[i];
                var line = outlines[i];
                line.enabled = card == Selected || card == hovered;
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
            string[] rows = { "Left-click card   Select", "Left-click table   Deselect", "Right-drag   Orbit camera", "Middle-drag   Pan camera", "Mouse wheel   Zoom", "F   Focus selected card", "Home   Reset camera", "Esc   Deselect" };
            for (int i = 0; i < rows.Length; i++) GUI.Label(new Rect(42, 78 + i * 27, 315, 27), rows[i], text);
            GUI.Label(new Rect(1494, 34, 390, 25), "ALIGNMENT VIEWS", muted);
            if (GUI.Button(new Rect(1494, 66, 120, 34), "Front", button)) QuickView(0);
            if (GUI.Button(new Rect(1626, 66, 120, 34), "Side", button)) QuickView(1);
            if (GUI.Button(new Rect(1758, 66, 120, 34), "Overhead", button)) QuickView(2);
            GUI.Label(new Rect(610, 966, 700, 25), Selected == null ? "No card selected — click a card or choose a hand" : "Selected: " + (Selected == Hands[0] ? "left-hand card" : "right-hand card"), text);
            for (int i = 0; i < Mathf.Min(2, Hands.Length); i++)
            {
                GUI.backgroundColor = Selected == Hands[i] ? new Color(1, 0.75f, 0.3f) : Color.white;
                if (GUI.Button(new Rect(610 + i * 350, 1002, 330, 36), (i == 0 ? "LEFT HAND" : "RIGHT HAND") + "  ·  Holding card", button)) SelectCard(Hands[i]);
            }
            GUI.backgroundColor = Color.white;
            GUI.Label(new Rect(24, 1000, 530, 55), "Selection + camera prototype\nCards stay held. Movement and release come next.", muted);
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
            if (outlines != null) foreach (var line in outlines) if (line != null) Destroy(line.gameObject);
            if (outlineMaterial != null) Destroy(outlineMaterial);
        }
    }
}
