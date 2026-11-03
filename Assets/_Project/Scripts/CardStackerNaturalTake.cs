#if HATDON_CAPTURE
using UnityEngine;

namespace Hatdon.CardStacker
{
    public sealed partial class CardStackerDevlogCapture
    {
        private bool naturalTake, naturalDragging, artTake;
        private Texture2D naturalCursor;
        private Vector2 gestureFrom, gestureClick, gestureStart;
        private Vector3 gestureCenter, gestureRadial;
        private static readonly Vector3[] CaptureAxes = { Vector3.right, Vector3.up, Vector3.forward };

        private void DemonstrateArt(int frame)
        {
            showPointer = true;
            if (frame == 0) pointer = new Vector2(1220,330);
            if (frame < 60)
            {
                Vector2 target = Camera.main.WorldToScreenPoint(controls.Hands[0].worldCenterOfMass);
                pointer = Vector2.Lerp(new Vector2(1220,330),target,Ease(frame/59f));
                controls.CaptureHover(pointer);
            }
            if (frame == 60) { controls.SelectCard(controls.PickCard(pointer)); controls.CaptureHover(Vector2.zero); }
            if (frame == 75) controls.FocusSelection();
            if (frame == 90) { controls.BeginOrbit(); gestureFrom=pointer; naturalDragging=true; }
            if (frame >= 90 && frame < 135)
            {
                Vector2 next=gestureFrom+new Vector2(60,8)*Ease((frame-90)/44f);
                controls.Orbit(next-pointer);pointer=next;
            }
            if (frame == 135) naturalDragging=false;
            Gesture(frame,156,110,1,180,true,false);
            Gesture(frame,300,85,1,-180,true,false);
            if (frame == 400) gestureFrom=pointer;
            if (frame >= 400 && frame < 425) pointer=Vector2.Lerp(gestureFrom,new Vector2(1120,60),Ease((frame-400)/24f));
            if (frame == 425) { controls.SelectCard(controls.Hands[1]); controls.FocusSelection(); }
            Gesture(frame,440,60,1,.012f,false,false);
            Gesture(frame,515,65,2,12,true,false);
            if (frame == 600) { controls.SelectCard(null);controls.ResetView(); }
            if (frame == 610) { controls.BeginOrbit();gestureFrom=pointer;naturalDragging=true; }
            if (frame >= 610 && frame < 675)
            {
                Vector2 next=gestureFrom+new Vector2(80,-12)*Ease((frame-610)/64f);
                controls.Orbit(next-pointer);pointer=next;
            }
            if (frame == 675) naturalDragging=false;
        }

        private void DemonstrateNatural(int frame)
        {
            showPointer = true;
            var m = controls.Manipulator;
            if (frame == 0) pointer = new Vector2(1200, 300);
            if (frame < 24)
            {
                Vector2 card = Camera.main.WorldToScreenPoint(controls.Hands[0].worldCenterOfMass);
                pointer = Vector2.Lerp(new Vector2(1200, 300), card, Ease(frame / 23f));
                controls.CaptureHover(pointer);
            }
            if (frame == 24) { controls.SelectCard(controls.PickCard(pointer)); controls.CaptureHover(Vector2.zero); }
            if (frame == 40) controls.FocusSelection();

            // Short drags separated by inspection pauses and small corrections.
            Gesture(frame, 54, 45, 0, -.015f, false, false);
            Gesture(frame, 114, 39, 1, .018f, false, false);
            Gesture(frame, 165, 30, 2, -.009f, false, false);
            Gesture(frame, 201, 20, 2, .0025f, false, false);
            Gesture(frame, 234, 36, 0, .0016f, false, true);
            Gesture(frame, 282, 42, 2, -23f, true, false);
            Gesture(frame, 330, 19, 2, 3f, true, false);
            Gesture(frame, 354, 39, 0, 10f, true, false);
            Gesture(frame, 399, 29, 1, -9f, true, false);

            if (frame == 432) gestureFrom = pointer;
            if (frame >= 432 && frame < 450)
                pointer = Vector2.Lerp(gestureFrom, new Vector2(1120, 60), Ease((frame - 432) / 17f));
            if (frame == 450) controls.SelectCard(controls.Hands[1]);
            Gesture(frame, 462, 34, 1, .014f, false, false);
            Gesture(frame, 507, 42, 2, 20f, true, false);
            Gesture(frame, 558, 27, 1, 8f, true, false);
            if (frame == 588) gestureFrom = pointer;
            if (frame >= 588 && frame < 603)
                pointer = Vector2.Lerp(gestureFrom, new Vector2(775, 60), Ease((frame - 588) / 14f));
            if (frame == 603) controls.SelectCard(controls.Hands[0]);
            Gesture(frame, 609, 42, 1, -.055f, false, false);
            Gesture(frame, 663, 20, 1, .004f, false, true);
            Gesture(frame, 687, 25, 0, .03f, false, false);
            if (frame == 720) { controls.BeginOrbit(); gestureFrom = pointer; naturalDragging = true; }
            if (frame >= 720 && frame < 756)
            {
                Vector2 next = gestureFrom + new Vector2(95, -8) * Ease((frame - 720) / 35f);
                controls.Orbit(next - pointer); pointer = next;
            }
            if (frame == 756) naturalDragging = false;
        }

        private void Gesture(int frame, int start, int length, int axisIndex, float amount, bool rotate, bool fine)
        {
            if (frame < start || frame > start + length) return;
            var m = controls.Manipulator;
            Vector3 axis = CaptureAxes[axisIndex];
            if (frame == start)
            {
                m.SetTool(rotate ? CardManipulator.Tool.Rotate : CardManipulator.Tool.Move);
                gestureFrom = pointer;
                gestureCenter = controls.Selected.position;
                bool picked = false;
                // Pick a visible portion of the intended handle, away from ring intersections.
                for (int attempt = 0; attempt < 16 && !picked; attempt++)
                {
                    if (rotate)
                    {
                        Vector3 u = CaptureAxes[(axisIndex + 1) % 3], v = Vector3.Cross(axis, u);
                        float angle = .55f + attempt * .33f;
                        gestureRadial = (u * Mathf.Cos(angle) + v * Mathf.Sin(angle)) * m.HandleSize * .8f;
                        gestureClick = Camera.main.WorldToScreenPoint(gestureCenter + gestureRadial);
                    }
                    else gestureClick = Camera.main.WorldToScreenPoint(gestureCenter + axis * m.HandleSize * (.55f + attempt * .025f));
                    picked = m.BeginDrag(gestureClick) && m.ActiveAxis == axisIndex;
                    m.EndDrag();
                }
                if (!picked) { Debug.LogError("Natural take cannot pick axis " + axisIndex + " at frame " + frame); Application.Quit(5); }
            }
            int offset = frame - start;
            if (offset < 7)
            {
                pointer = Vector2.Lerp(gestureFrom, gestureClick, Ease(offset / 6f));
                return;
            }
            if (offset == 7)
            {
                pointer = gestureClick; gestureStart = pointer;
                if (!m.BeginDrag(pointer) || m.ActiveAxis != axisIndex)
                { Debug.LogError("Natural take lost handle at frame " + frame); Application.Quit(6); }
                naturalDragging = true;
            }
            float t = (float)(offset - 7) / (length - 7);
            float progress = Ease(t) + .012f * Mathf.Sin(t * Mathf.PI * 2) * Mathf.Sin(t * Mathf.PI);
            if (rotate)
                pointer = Camera.main.WorldToScreenPoint(gestureCenter + Quaternion.AngleAxis(amount * progress / (fine ? .1f : 1), axis) * gestureRadial);
            else
            {
                Vector2 delta = (Vector2)Camera.main.WorldToScreenPoint(gestureCenter + axis * amount / (fine ? .1f : 1)) - (Vector2)Camera.main.WorldToScreenPoint(gestureCenter);
                pointer = gestureStart + delta * progress;
            }
            m.DragTo(pointer, fine);
            if (offset == length) { m.EndDrag(); naturalDragging = false; }
        }

        private static float Ease(float t) { t = Mathf.Clamp01(t); return t * t * (3 - 2 * t); }

        private void DrawNaturalCursor()
        {
            if (naturalCursor == null)
            {
                naturalCursor = new Texture2D(20, 28, TextureFormat.RGBA32, false);
                Vector2[] edge = { new Vector2(1,1), new Vector2(1,22), new Vector2(6,17), new Vector2(10,26), new Vector2(15,24), new Vector2(11,16), new Vector2(18,16) };
                Vector2[] fill = { new Vector2(3,5), new Vector2(3,18), new Vector2(7,14), new Vector2(11,23), new Vector2(12,23), new Vector2(8,14), new Vector2(14,14) };
                for (int y = 0; y < 28; y++) for (int x = 0; x < 20; x++)
                {
                    Vector2 p = new Vector2(x, y);
                    naturalCursor.SetPixel(x, 27-y, Inside(p, fill) ? Color.white : Inside(p, edge) ? new Color(.03f,.04f,.05f) : Color.clear);
                }
                naturalCursor.Apply();
            }
            Color old = GUI.color;
            GUI.color = naturalDragging ? new Color(1,.86f,.45f) : Color.white;
            GUI.DrawTexture(new Rect(pointer.x - 1, Screen.height - pointer.y - 1, 20, 28), naturalCursor);
            GUI.color = old;
        }

        private static bool Inside(Vector2 p, Vector2[] polygon)
        {
            bool inside = false;
            for (int i = 0, j = polygon.Length-1; i < polygon.Length; j = i++)
                if ((polygon[i].y > p.y) != (polygon[j].y > p.y) && p.x < (polygon[j].x-polygon[i].x)*(p.y-polygon[i].y)/(polygon[j].y-polygon[i].y)+polygon[i].x)
                    inside = !inside;
            return inside;
        }
    }
}
#endif
