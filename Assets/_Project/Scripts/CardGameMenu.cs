using UnityEngine;

namespace Hatdon.CardStacker
{
    public sealed partial class CardGame
    {
        private static readonly Color Panel = new Color(.052f,.089f,.092f,.97f);
        private static readonly Color Tile = new Color(.10f,.17f,.17f);
        private static readonly Color Accent = new Color(.47f,.88f,.78f);
        private static readonly Color Warm = new Color(1f,.82f,.52f);
        private static readonly Color Ink = new Color(.92f,.96f,.92f);
        private static readonly Color Muted = new Color(.67f,.77f,.73f);
        private GUIStyle title, heading, body, small;

        private static readonly Rect MenuButton = new Rect(46,42,164,46);
        private static readonly Rect GuideButton = new Rect(1672,42,202,46);

        public bool Contains(Vector2 screenPoint)
        {
            if (BlocksTable) return true;
            if (controls.Admin.HideHud) return false;
            Vector2 point = new Vector2(screenPoint.x*1920/Screen.width,(Screen.height-screenPoint.y)*1080/Screen.height);
            return MenuButton.Contains(point) || (active >= 0 && GuideButton.Contains(point)) ||
                (active != Recording && controls.Placement.HeldSingleSlot >= 0 && SingleButton.Contains(point));
        }

        private static readonly Rect SingleButton = new Rect(1328,953,148,70);

        private void OnGUI()
        {
            if (controls.Admin.IsOpen) return;
            if (title == null)
            {
                title = Style(75,FontStyle.Bold);
                heading = Style(27,FontStyle.Bold);
                body = Style(21,FontStyle.Normal);
                small = Style(16,FontStyle.Bold);
            }

            var matrix = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(new Vector3(Screen.width/1920f,Screen.height/1080f,1));
            if (menu) DrawMenu();
            else if (confirmReset) DrawResetConfirmation();
            else if (!controls.Admin.HideHud) DrawTableHud();
            GUI.matrix = matrix;
        }

        private void DrawMenu()
        {
            Fill(new Rect(0,0,1920,1080),new Color(.025f,.04f,.042f,.72f));
            Fill(new Rect(214,92,1492,896),Panel);
            Fill(new Rect(214,92,1492,4),Accent);
            Label(new Rect(286,148,420,28),"HATDON",small,Accent);
            Label(new Rect(280,176,920,99),"Card Stacker",title,Ink);
            Label(new Rect(286,277,1000,35),"Build a house of cards, one placement at a time.",body,Muted);
            if (Button(new Rect(1374,190,250,76),"Practice",Accent,Panel))
            {
                OpenTable(Practice);
                return;
            }
            Label(new Rect(286,362,350,49),"Challenges",heading,Ink);
            int completed = 0;
            for (int i = 0; i < CardChallenges.Levels.Length; i++) if (Read(i).completed) completed++;
            Label(new Rect(1410,372,210,32),$"{completed} / 10 completed",small,Warm);
            for (int i = 0; i < CardChallenges.Levels.Length; i++)
            {
                var layout = CardChallenges.Levels[i];
                var saved = Read(i);
                var rect = new Rect(286+(i%5)*272,426+(i/5)*170,252,150);
                bool clicked = Button(rect,"",Tile,Ink);
                Label(new Rect(rect.x+17,rect.y+14,210,28),$"{i + 1:00}   /   {layout.Cards.Count} CARDS",small,Muted);
                Label(new Rect(rect.x+17,rect.y+49,223,44),layout.Name,heading,Ink);
                string status = saved.completed ? "Completed" : saved.cards.Count > 0 ? "In progress" : "New table";
                Label(new Rect(rect.x+17,rect.y+104,220,25),status,small,saved.completed ? Warm : Accent);
                if (clicked) { OpenTable(i); return; }
            }
            Label(new Rect(286,872,1020,44),saveIssue ?? "Your cards are saved separately on each table.",body,Muted);
            if (Button(new Rect(1476,866,148,52),"Quit",Tile,Ink)) Application.Quit();
        }

        private void DrawTableHud()
        {
            if (Button(MenuButton,"Main menu",Panel,Ink)) { ShowMenu(); return; }
            if (active == Recording) return;
            Fill(new Rect(46,106,546,102),Panel);
            string name = active == Practice ? "Practice" : $"{active + 1:00}   {CardChallenges.Levels[active].Name}";
            Label(new Rect(64,112,490,45),name,heading,Ink);
            string hint = active == Practice ? "Build freely with a full deck." : CardChallenges.Levels[active].Hint;
            Label(new Rect(64,157,516,37),saveIssue ?? hint,small,Muted);
            if (active >= 0)
            {
                if (Button(GuideButton,table.guide ? "Hide guide" : "Show guide",Panel,Ink))
                    ToggleGuide();
                if (table.completed)
                {
                    Fill(new Rect(1610,106,264,40),Panel);
                    Label(new Rect(1628,109,235,34),"Challenge completed",small,Warm);
                }
            }
            if (controls.Placement.HeldSingleSlot >= 0 && Button(SingleButton,"Release card",Tile,Ink,small))
                controls.Placement.ReleaseSingle();
        }

        private void DrawResetConfirmation()
        {
            Fill(new Rect(0,0,1920,1080),new Color(0,0,0,.65f));
            Fill(new Rect(650,386,620,278),Panel);
            Label(new Rect(690,418,500,50),"Reset this table?",heading,Ink);
            Label(new Rect(690,480,520,45),"This clears its cards and challenge progress.",body,Muted);
            if (Button(new Rect(690,560,245,58),"Keep building",Tile,Ink)) CancelReset();
            else if (Button(new Rect(949,560,281,58),"Reset table",Warm,Panel)) ResetCurrentTable();
        }

        private static GUIStyle Style(int size, FontStyle fontStyle) =>
            new GUIStyle(GUI.skin.label) { fontSize = size, fontStyle = fontStyle, alignment = TextAnchor.MiddleLeft };

        private static void Label(Rect rect, string value, GUIStyle style, Color color)
        {
            style.normal.textColor = color;
            GUI.Label(rect,value,style);
        }

        private static void Fill(Rect rect, Color tint)
        {
            Color previous = GUI.color;
            GUI.color = tint;
            GUI.DrawTexture(rect,Texture2D.whiteTexture);
            GUI.color = previous;
        }

        private bool Button(Rect rect, string value, Color fill, Color ink, GUIStyle labelStyle = null)
        {
            bool clicked = GUI.Button(rect,GUIContent.none,GUIStyle.none);
            bool hover = rect.Contains(Event.current.mousePosition);
            Fill(rect,hover ? Color.Lerp(fill,ink,.14f) : fill);
            Label(new Rect(rect.x+18,rect.y,rect.width-34,rect.height),value,labelStyle ?? body,ink);
            return clicked;
        }
    }
}
