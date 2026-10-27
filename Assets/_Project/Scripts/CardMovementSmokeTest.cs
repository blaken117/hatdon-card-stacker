#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using UnityEngine;

namespace Hatdon.CardStacker
{
    public sealed class CardMovementSmokeTest : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void StartIfRequested()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-cardMovementSmokeTest") >= 0)
                new GameObject("Movement smoke test").AddComponent<CardMovementSmokeTest>();
        }

        private IEnumerator Start()
        {
            yield return null;
            try
            {
                var c = FindFirstObjectByType<CardStackerControls>();
                Require(c != null && c.Hands.Length == 2, "Controls initialised");
                var left = c.Hands[0]; var right = c.Hands[1];
                Vector3 start = left.position, other = right.position;
                Quaternion rotation = left.rotation, otherRotation = right.rotation;
                c.SelectCard(left); var m = c.Manipulator;
                Require(m.Translate(new Vector3(.01f, .01f, .01f)), "Free three-axis movement");
                Require(Vector3.Distance(left.position, start + new Vector3(.01f, .01f, .01f)) < .00001f, "Requested translation applied");
                foreach (Vector3 axis in new[] { Vector3.right, Vector3.up, Vector3.forward })
                    Require(m.Rotate(axis, 8), "Free three-axis rotation");
                Pose(left, start, rotation); c.FocusSelection();
                m.SetTool(CardManipulator.Tool.Move);
                Vector2 begin = Camera.main.WorldToScreenPoint(start + Vector3.right * m.HandleSize * .7f);
                Vector2 axisPixels = (Vector2)Camera.main.WorldToScreenPoint(start + Vector3.right * m.HandleSize) - (Vector2)Camera.main.WorldToScreenPoint(start);
                Vector2 end = begin + axisPixels.normalized * 20;
                Require(m.BeginDrag(begin) && m.ActiveAxis == 0, "Move handle screen picking");
                m.DragTo(end, false); float normal = left.position.x - start.x; m.EndDrag();
                Pose(left, start, rotation);
                Require(m.BeginDrag(begin), "Fine move handle picking");
                m.DragTo(end, true); float fine = left.position.x - start.x; m.EndDrag();
                Require(normal > .001f && Mathf.Abs(fine / normal - .1f) < .002f, "Shift gives one-tenth movement without a jump");
                Pose(left, start, rotation);
                m.SetTool(CardManipulator.Tool.Rotate);
                Vector3 radial = (Vector3.up * Mathf.Cos(.65f) + Vector3.forward * Mathf.Sin(.65f)) * m.HandleSize * .8f;
                begin = Camera.main.WorldToScreenPoint(start + radial);
                Vector2 tangent = (Vector2)Camera.main.WorldToScreenPoint(start + Quaternion.AngleAxis(1, Vector3.right) * radial) - begin;
                Require(m.BeginDrag(begin) && m.ActiveAxis == 0, "Rotation ring screen picking");
                m.DragTo(begin + tangent.normalized * 12, false); m.EndDrag();
                float normalAngle = Quaternion.Angle(left.rotation, rotation);
                Require(normalAngle > 1, "Dragging the ring rotates the card");
                Pose(left, start, rotation);
                Require(m.BeginDrag(begin), "Fine rotation ring picking");
                m.DragTo(begin + tangent.normalized * 12, true); m.EndDrag();
                Require(Mathf.Abs(Quaternion.Angle(left.rotation, rotation) / normalAngle - .1f) < .01f, "Shift gives one-tenth rotation");
                Pose(left, start, rotation);
                Require(!m.Translate(Vector3.down * .1f), "Table blocks downward translation");
                Require(left.position.y > .0439f, "Card bottom remains above tabletop");
                Require(m.Translate(Vector3.up * .02f), "Can move away from contact");
                Pose(left, start, rotation);
                Require(!m.Translate(Vector3.right * .12f), "Other held card blocks movement");
                Require(left.position.x < -.0179f, "Card cannot pass through other card");
                Pose(left, new Vector3(other.x, start.y, -.02f), rotation);
                Require(!m.Translate(Vector3.forward * .04f), "Thin-face crossing is swept, not just checked at endpoint");
                Require(left.position.z < -.00079f, "Thin faces do not tunnel");
                Pose(left, new Vector3(start.x, .05f, start.z), rotation);
                Require(!m.Rotate(Vector3.forward, 45), "Table blocks rotating corner");
                Require(Quaternion.Angle(rotation, left.rotation) < 44.9f, "Rotation stops before penetrating table");
                var collider = left.GetComponent<Collider>();
                foreach (var obstacle in FindObjectsByType<Collider>(FindObjectsSortMode.None))
                    if (obstacle != collider && Physics.ComputePenetration(collider, left.position, left.rotation, obstacle, obstacle.transform.position, obstacle.transform.rotation, out _, out float depth))
                        Require(depth < .000002f, "Final pose has no material penetration");
                Require(right.position == other && right.rotation == otherRotation && right.isKinematic && left.isKinematic, "Second hand remains fixed; neither card is released");
                Pose(left, start, rotation); c.SelectCard(null);
                Require(!m.Translate(Vector3.one), "No selection cannot move a card");
                Debug.Log("HATDON_MOVEMENT_TEST_OK: XYZ movement/rotation, handle/ring picking, 10x fine control, table/card contact, thin-face sweep and independent held cards.");
                Application.Quit(0);
            }
            catch (Exception error) { Debug.LogException(error); Application.Quit(1); }
        }

        private static void Pose(Rigidbody card, Vector3 position, Quaternion rotation)
        { card.position = position; card.rotation = rotation; Physics.SyncTransforms(); }
        private static void Require(bool passed, string message) { if (!passed) throw new Exception(message); }
    }
}
#endif
