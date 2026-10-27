using UnityEngine;

namespace Hatdon.CardStacker
{
    public sealed class CardManipulator : MonoBehaviour
    {
        public enum Tool { Move, Rotate }
        public Tool Mode { get; private set; }
        public bool Dragging { get; private set; }
        public bool ContactBlocked { get; private set; }
        public bool FineAdjustment { get; private set; }
        public int ActiveAxis { get; private set; } = -1;
        private CardStackerControls controls;
        private Camera view;
        private Collider[] obstacles;
        private readonly Vector3[] axes = { Vector3.right, Vector3.up, Vector3.forward };
        private readonly Color[] colors = { new Color(1, .3f, .3f), new Color(.35f, 1, .5f), new Color(.3f, .65f, 1) };
        private LineRenderer[] lines;
        private Material material;
        private Vector2 previousPointer;
        private Vector3 rotationRadial;
        private float contactUntil;
        public float HandleSize => Mathf.Clamp(Vector3.Distance(view.transform.position, controls.Selected.position) * .14f, .018f, .2f);

        public void Initialise(CardStackerControls owner)
        {
            controls = owner; view = GetComponent<Camera>();
            obstacles = FindObjectsByType<Collider>(FindObjectsSortMode.None);
            material = new Material(Resources.Load<Shader>("CardGizmo"));
            lines = new LineRenderer[3];
            for (int i = 0; i < 3; i++)
            {
                var go = new GameObject("Card handle " + "XYZ"[i]);
                go.transform.SetParent(transform, false);
                var line = go.AddComponent<LineRenderer>();
                line.sharedMaterial = material;
                line.useWorldSpace = true;
                line.numCornerVertices = 2;
                line.enabled = false;
                lines[i] = line;
            }
        }

        public void SetTool(Tool tool) { EndDrag(); Mode = tool; }
        public void EndDrag() { Dragging = false; ActiveAxis = -1; FineAdjustment = false; }

        private Vector3 RingPoint(int axis, float angle, float size)
        {
            Vector3 u = axes[(axis + 1) % 3], v = Vector3.Cross(axes[axis], u);
            return controls.Selected.position + (u * Mathf.Cos(angle) + v * Mathf.Sin(angle)) * size;
        }

        private float SegmentDistance(Vector2 p, Vector2 a, Vector2 b, out float t)
        {
            Vector2 d = b - a;
            t = d.sqrMagnitude < .01f ? 0 : Mathf.Clamp01(Vector2.Dot(p - a, d) / d.sqrMagnitude);
            return Vector2.Distance(p, a + d * t);
        }

        public bool BeginDrag(Vector2 pointer)
        {
            if (controls.Selected == null) return false;
            float best = 12 * Screen.height / 1080f;
            int found = -1; Vector3 radial = Vector3.zero;
            float size = HandleSize;
            for (int i = 0; i < 3; i++)
            {
                if (Mode == Tool.Move)
                {
                    Vector2 a = view.WorldToScreenPoint(controls.Selected.position + axes[i] * size * .2f);
                    Vector2 b = view.WorldToScreenPoint(controls.Selected.position + axes[i] * size);
                    if (Vector2.Distance(a, b) < 16) continue;
                    float distance = SegmentDistance(pointer, a, b, out _);
                    if (distance < best) { best = distance; found = i; }
                }
                else for (int j = 0; j < 96; j++)
                {
                    Vector3 a = RingPoint(i, j * Mathf.PI * 2 / 96, size * .8f);
                    Vector3 b = RingPoint(i, (j + 1) * Mathf.PI * 2 / 96, size * .8f);
                    float distance = SegmentDistance(pointer, view.WorldToScreenPoint(a), view.WorldToScreenPoint(b), out float t);
                    if (distance < best) { best = distance; found = i; radial = Vector3.Lerp(a, b, t) - controls.Selected.position; }
                }
            }
            if (found < 0) return false;
            ActiveAxis = found; rotationRadial = radial; previousPointer = pointer; Dragging = true;
            return true;
        }

        public void DragTo(Vector2 pointer, bool fine)
        {
            if (!Dragging || controls.Selected == null) return;
            FineAdjustment = fine;
            Vector2 delta = pointer - previousPointer; previousPointer = pointer;
            float precision = fine ? .1f : 1;
            Vector3 center = controls.Selected.position, axis = axes[ActiveAxis];
            if (Mode == Tool.Move)
            {
                Vector2 a = view.WorldToScreenPoint(center), b = view.WorldToScreenPoint(center + axis * HandleSize);
                Vector2 projected = b - a;
                if (projected.sqrMagnitude < 1) return;
                float distance = Vector2.Dot(delta, projected) / projected.sqrMagnitude * HandleSize * precision;
                Translate(axis * Mathf.Clamp(distance, -.05f, .05f));
            }
            else
            {
                Vector2 a = view.WorldToScreenPoint(center + rotationRadial);
                Vector2 b = view.WorldToScreenPoint(center + Quaternion.AngleAxis(1, axis) * rotationRadial);
                Vector2 tangent = b - a;
                if (tangent.sqrMagnitude < .02f) return;
                float degrees = Mathf.Clamp(Vector2.Dot(delta, tangent) / tangent.sqrMagnitude * precision, -15, 15);
                Quaternion before = controls.Selected.rotation;
                Rotate(axis, degrees);
                float accepted = Quaternion.Angle(before, controls.Selected.rotation) * Mathf.Sign(degrees);
                rotationRadial = Quaternion.AngleAxis(accepted, axis) * rotationRadial;
            }
        }

        public bool Translate(Vector3 delta)
        {
            if (controls.Selected == null) return false;
            Vector3 target = controls.Selected.position + Vector3.ClampMagnitude(delta, .12f);
            target.x = Mathf.Clamp(target.x, -.36f, .36f);
            target.z = Mathf.Clamp(target.z, -.26f, .26f);
            target.y = Mathf.Clamp(target.y, 0, .4f);
            return ApplyPose(target, controls.Selected.rotation);
        }

        public bool Rotate(Vector3 axis, float degrees)
        {
            if (controls.Selected == null || axis.sqrMagnitude < .1f) return false;
            return ApplyPose(controls.Selected.position, Quaternion.AngleAxis(Mathf.Clamp(degrees, -90, 90), axis.normalized) * controls.Selected.rotation);
        }

        private bool ClearAt(Vector3 position, Quaternion rotation)
        {
            Collider card = controls.Selected.GetComponent<Collider>();
            foreach (var other in obstacles)
            {
                if (other == null || other == card || !other.enabled || other.isTrigger || !other.gameObject.activeInHierarchy) continue;
                if (Physics.ComputePenetration(card, position, rotation, other, other.transform.position, other.transform.rotation,
                    out _, out float depth) && depth > .000001f) return false;
            }
            return true;
        }

        private bool ApplyPose(Vector3 position, Quaternion rotation)
        {
            var card = controls.Selected;
            Vector3 start = card.position; Quaternion initial = card.rotation;
            // Small swept increments stop thin cards tunnelling during translation or rotation.
            int steps = Mathf.Max(1, Mathf.CeilToInt(Mathf.Max(Vector3.Distance(start, position) / .0002f, Quaternion.Angle(initial, rotation) / .2f)));
            float safe = 0; bool blocked = false;
            for (int i = 1; i <= steps; i++)
            {
                float t = (float)i / steps;
                if (ClearAt(Vector3.Lerp(start, position, t), Quaternion.Slerp(initial, rotation, t))) safe = t;
                else
                {
                    float lo = safe, hi = t;
                    for (int j = 0; j < 8; j++)
                    {
                        float mid = (lo + hi) * .5f;
                        if (ClearAt(Vector3.Lerp(start, position, mid), Quaternion.Slerp(initial, rotation, mid))) lo = mid; else hi = mid;
                    }
                    safe = lo; blocked = true; break;
                }
            }
            card.position = Vector3.Lerp(start, position, safe);
            card.rotation = Quaternion.Slerp(initial, rotation, safe);
            Physics.SyncTransforms();
            if (blocked) { ContactBlocked = true; contactUntil = Time.unscaledTime + .6f; }
            return !blocked;
        }

        private void LateUpdate()
        {
            if (controls == null || lines == null) return;
            if (Time.unscaledTime > contactUntil) ContactBlocked = false;
            if (controls.Selected == null) EndDrag();
            for (int i = 0; i < 3; i++)
            {
                var line = lines[i]; line.enabled = controls.Selected != null;
                if (!line.enabled) continue;
                float size = HandleSize;
                line.widthMultiplier = size * (ActiveAxis == i ? .035f : .021f);
                line.startColor = line.endColor = ActiveAxis == i ? Color.white : colors[i];
                line.loop = Mode == Tool.Rotate;
                if (Mode == Tool.Move)
                {
                    line.positionCount = 5;
                    Vector3 origin = controls.Selected.position, tip = origin + axes[i] * size;
                    Vector3 side = Vector3.Cross(axes[i], view.transform.forward).normalized * size * .08f;
                    line.SetPosition(0, origin + axes[i] * size * .12f); line.SetPosition(1, tip);
                    line.SetPosition(2, tip - axes[i] * size * .16f + side); line.SetPosition(3, tip);
                    line.SetPosition(4, tip - axes[i] * size * .16f - side);
                }
                else
                {
                    line.positionCount = 96;
                    for (int j = 0; j < 96; j++) line.SetPosition(j, RingPoint(i, j * Mathf.PI * 2 / 96, size * .8f));
                }
            }
        }

        public void DrawAxisLabels(GUIStyle style)
        {
            if (controls.Selected == null || Mode != Tool.Move) return;
            for (int i = 0; i < 3; i++)
            {
                Vector3 p = view.WorldToScreenPoint(controls.Selected.position + axes[i] * HandleSize * 1.12f);
                if (p.z <= 0) continue;
                Color old = GUI.color; GUI.color = colors[i];
                GUI.Label(new Rect(p.x * 1920f / Screen.width - 10, (Screen.height - p.y) * 1080f / Screen.height - 12, 30, 30), "XYZ"[i].ToString(), style);
                GUI.color = old;
            }
        }

        private void OnDestroy()
        {
            if (lines != null) foreach (var line in lines) if (line != null) Destroy(line.gameObject);
            if (material != null) Destroy(material);
        }
    }
}
