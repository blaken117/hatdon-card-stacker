using UnityEngine;

namespace Hatdon.CardStacker
{
    public sealed class CardModeToolbar : MonoBehaviour
    {
        private static readonly CardManipulator.Tool[] Modes = { CardManipulator.Tool.Move, CardManipulator.Tool.Depth, CardManipulator.Tool.Rotate, CardManipulator.Tool.Roll };
        private readonly Texture2D[] icons = new Texture2D[4];
        private CardStackerControls controls;
        private GUIStyle keyLabel;

        public static Rect ButtonRect(int index) => new Rect(680 + index * 144, 924, 128, 128);
        public static bool Contains(Vector2 screenPoint)
        {
            Vector2 point = new Vector2(screenPoint.x * 1920 / Screen.width, (Screen.height - screenPoint.y) * 1080 / Screen.height);
            for (int i = 0; i < 4; i++) if (ButtonRect(i).Contains(point)) return true;
            return false;
        }

        public void Initialise(CardStackerControls owner)
        {
            controls = owner;
            string[] names={"move","depth","rotate","twist"};
            for (int i = 0; i < 4; i++) icons[i] = Resources.Load<Texture2D>("UIIcons/"+names[i]);
        }

        private void OnGUI()
        {
            if (controls == null || (controls.Admin!=null && (controls.Admin.IsOpen || controls.Admin.HideHud))) return;
            keyLabel ??= new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 17, fontStyle = FontStyle.Bold };
            Matrix4x4 old = GUI.matrix;Color oldColor = GUI.color;
            GUI.matrix = Matrix4x4.Scale(new Vector3(Screen.width / 1920f, Screen.height / 1080f, 1));
            for (int i = 0; i < 4; i++)
            {
                Rect rect = ButtonRect(i);
                if (GUI.Button(rect, GUIContent.none, GUIStyle.none)) controls.SetHandlingMode(Modes[i]);
                bool active = controls.Manipulator.ActiveMode == Modes[i];
                bool hover = rect.Contains(Event.current.mousePosition);
                GUI.color = active ? new Color(.19f,.32f,.35f,.97f) : hover ? new Color(.14f,.22f,.25f,.97f) : new Color(.055f,.09f,.105f,.92f);
                GUI.DrawTexture(rect, Texture2D.whiteTexture);
                GUI.color = active ? new Color(1,.79f,.33f) : new Color(.85f,.92f,.94f);
                if (Modes[i] == CardManipulator.Tool.Rotate)
                {
                    GUI.DrawTexture(new Rect(rect.x + 29, rect.y + 9, 70, 70), icons[i]);
                    GUI.Label(new Rect(rect.x, rect.y + 81, rect.width, 30), "Hold " + CardStackerControls.HoldRotateKey, keyLabel);
                }
                else GUI.DrawTexture(new Rect(rect.x+20,rect.y+20,88,88), icons[i]);
                if (active) GUI.DrawTexture(new Rect(rect.x+16,rect.yMax-7,96,4),Texture2D.whiteTexture);
            }
            GUI.color = oldColor;GUI.matrix = old;
        }

    }
}
