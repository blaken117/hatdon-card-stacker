using UnityEngine;

namespace Hatdon.CardStacker
{
    public sealed class CardModeToolbar : MonoBehaviour
    {
        private static readonly CardManipulator.Tool[] Modes = { CardManipulator.Tool.Move, CardManipulator.Tool.Rotate, CardManipulator.Tool.Roll };
        public static int ButtonCount => Modes.Length + 1;
        private readonly Texture2D[] icons = new Texture2D[Modes.Length];
        private CardStackerControls controls;
        private GUIStyle keyLabel;
        private GUIStyle actionButton;

        public static Rect ButtonRect(int index) => new Rect(680 + index * 144, 924, 128, 128);
        public static bool Contains(Vector2 screenPoint)
        {
            Vector2 point = new Vector2(screenPoint.x * 1920 / Screen.width, (Screen.height - screenPoint.y) * 1080 / Screen.height);
            for (int i = 0; i < ButtonCount; i++) if (ButtonRect(i).Contains(point)) return true;
            return false;
        }

        public void Initialise(CardStackerControls owner)
        {
            controls = owner;
            string[] names={"move","rotate","twist"};
            for (int i = 0; i < Modes.Length; i++) icons[i] = Resources.Load<Texture2D>("UIIcons/"+names[i]);
        }

        private void OnGUI()
        {
            if (controls == null || (controls.Admin!=null && (controls.Admin.IsOpen || controls.Admin.HideHud))) return;
            keyLabel ??= new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 17, fontStyle = FontStyle.Bold };
            actionButton ??= new GUIStyle(GUI.skin.button) { fontSize = 17, fontStyle = FontStyle.Bold };
            Matrix4x4 old = GUI.matrix;Color oldColor = GUI.color;
            GUI.matrix = Matrix4x4.Scale(new Vector3(Screen.width / 1920f, Screen.height / 1080f, 1));
            for (int i = 0; i < Modes.Length; i++)
            {
                Rect rect = ButtonRect(i);
                if (GUI.Button(rect, GUIContent.none, GUIStyle.none)) controls.SetHandlingMode(Modes[i]);
                bool active = controls.Manipulator.ActiveMode == Modes[i];
                bool hover = rect.Contains(Event.current.mousePosition);
                GUI.color = active ? new Color(.19f,.32f,.35f,.97f) : hover ? new Color(.14f,.22f,.25f,.97f) : new Color(.055f,.09f,.105f,.92f);
                GUI.DrawTexture(rect, Texture2D.whiteTexture);
                GUI.color = active ? new Color(1,.79f,.33f) : new Color(.85f,.92f,.94f);
                if (Modes[i] != CardManipulator.Tool.Move)
                {
                    GUI.DrawTexture(new Rect(rect.x + 29, rect.y + 9, 70, 70), icons[i]);
                    string label = Modes[i] == CardManipulator.Tool.Rotate
                        ? "Hold " + CardStackerControls.HoldRotateKey
                        : "Tilt · Hold " + CardStackerControls.HoldTiltKey;
                    GUI.Label(new Rect(rect.x, rect.y + 81, rect.width, 30), label, keyLabel);
                }
                else GUI.DrawTexture(new Rect(rect.x+20,rect.y+20,88,88), icons[i]);
                if (active) GUI.DrawTexture(new Rect(rect.x+16,rect.yMax-7,96,4),Texture2D.whiteTexture);
            }
            GUI.color = oldColor;
            if (GUI.Button(ButtonRect(Modes.Length), "Reset cards", actionButton)) controls.ResetTable();
            GUI.Label(new Rect(630, 887, 660, 28), controls.Placement.Status, keyLabel);
            GUI.matrix = old;
        }

    }
}
