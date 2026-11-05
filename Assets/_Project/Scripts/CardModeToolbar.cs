using System.Collections.Generic;
using UnityEngine;

namespace Hatdon.CardStacker
{
    public sealed class CardModeToolbar : MonoBehaviour
    {
        private static readonly CardManipulator.Tool[] Modes = { CardManipulator.Tool.Move, CardManipulator.Tool.Depth, CardManipulator.Tool.Rotate, CardManipulator.Tool.Roll };
        private readonly Texture2D[] icons = new Texture2D[4];
        private CardStackerControls controls;
        private struct Stroke { public Vector2 a,b; public Stroke(Vector2 start,Vector2 end){a=start;b=end;} }

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
            for (int i = 0; i < 4; i++) icons[i] = MakeIcon(i);
        }

        private void OnGUI()
        {
            if (controls == null) return;
            Matrix4x4 old = GUI.matrix;Color oldColor = GUI.color;
            GUI.matrix = Matrix4x4.Scale(new Vector3(Screen.width / 1920f, Screen.height / 1080f, 1));
            for (int i = 0; i < 4; i++)
            {
                Rect rect = ButtonRect(i);
                if (GUI.Button(rect, GUIContent.none, GUIStyle.none)) controls.SetHandlingMode(Modes[i]);
                bool active = controls.Manipulator.Mode == Modes[i];
                bool hover = rect.Contains(Event.current.mousePosition);
                GUI.color = active ? new Color(.19f,.32f,.35f,.97f) : hover ? new Color(.14f,.22f,.25f,.97f) : new Color(.055f,.09f,.105f,.92f);
                GUI.DrawTexture(rect, Texture2D.whiteTexture);
                GUI.color = active ? new Color(1,.79f,.33f) : new Color(.85f,.92f,.94f);
                GUI.DrawTexture(new Rect(rect.x+20,rect.y+20,88,88), icons[i]);
                if (active) GUI.DrawTexture(new Rect(rect.x+16,rect.yMax-7,96,4),Texture2D.whiteTexture);
            }
            GUI.color = oldColor;GUI.matrix = old;
        }

        private static Texture2D MakeIcon(int index)
        {
            var lines = new List<Stroke>();
            if(index==0)
            {
                Arrow(lines,new Vector2(48,48),new Vector2(48,13));Arrow(lines,new Vector2(48,48),new Vector2(48,83));
                Arrow(lines,new Vector2(48,48),new Vector2(13,48));Arrow(lines,new Vector2(48,48),new Vector2(83,48));
            }
            else if(index==1)
            {
                Box(lines,32,13,32,17);Box(lines,18,65,60,20);
                Arrow(lines,new Vector2(48,36),new Vector2(48,58));
                lines.Add(new Stroke(new Vector2(26,60),new Vector2(35,36)));lines.Add(new Stroke(new Vector2(70,60),new Vector2(61,36)));
            }
            else if(index==2)
            {
                Arc(lines,new Vector2(48,48),new Vector2(35,19),-30,295);
                Arc(lines,new Vector2(48,48),new Vector2(19,35),25,345);
                Arrow(lines,new Vector2(73,23),new Vector2(82,37));
                Arrow(lines,new Vector2(27,18),new Vector2(43,13));
            }
            else
            {
                Box(lines,31,31,34,34);
                Arc(lines,new Vector2(48,48),new Vector2(36,36),-65,220);
                Arrow(lines,new Vector2(25,19),new Vector2(13,34));
            }
            const int size=128;var pixels=new Color[size*size];
            for(int y=0;y<size;y++)for(int x=0;x<size;x++)
            {
                Vector2 p=new Vector2((x+.5f)*96/size,(y+.5f)*96/size);float distance=1000;
                foreach(var line in lines)
                {
                    Vector2 segment=line.b-line.a;
                    float t=Mathf.Clamp01(Vector2.Dot(p-line.a,segment)/segment.sqrMagnitude);
                    distance=Mathf.Min(distance,Vector2.Distance(p,line.a+segment*t));
                }
                pixels[(size-1-y)*size+x]=new Color(1,1,1,Mathf.Clamp01((2.1f-distance)*size/96));
            }
            var icon=new Texture2D(size,size,TextureFormat.RGBA32,false){name="Handling icon "+index,filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};
            icon.SetPixels(pixels);icon.Apply(false,true);return icon;
        }

        private static void Arrow(List<Stroke> lines,Vector2 a,Vector2 b)
        {
            lines.Add(new Stroke(a,b));Vector2 d=(a-b).normalized*10,p=new Vector2(-d.y,d.x)*.7f;
            lines.Add(new Stroke(b,b+d+p));lines.Add(new Stroke(b,b+d-p));
        }
        private static void Box(List<Stroke> lines,float x,float y,float w,float h)
        {
            Vector2 a=new Vector2(x,y),b=new Vector2(x+w,y),c=new Vector2(x+w,y+h),d=new Vector2(x,y+h);
            lines.Add(new Stroke(a,b));lines.Add(new Stroke(b,c));lines.Add(new Stroke(c,d));lines.Add(new Stroke(d,a));
        }
        private static void Arc(List<Stroke> lines,Vector2 centre,Vector2 radius,float start,float end)
        {
            Vector2 previous=centre+Vector2.Scale(radius,new Vector2(Mathf.Cos(start*Mathf.Deg2Rad),Mathf.Sin(start*Mathf.Deg2Rad)));
            for(int i=1;i<=36;i++)
            {
                float angle=Mathf.Lerp(start,end,i/36f)*Mathf.Deg2Rad;
                Vector2 next=centre+Vector2.Scale(radius,new Vector2(Mathf.Cos(angle),Mathf.Sin(angle)));
                lines.Add(new Stroke(previous,next));previous=next;
            }
        }
        private void OnDestroy(){foreach(var icon in icons)if(icon!=null)Destroy(icon);}
    }
}
