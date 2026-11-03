using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Hatdon.CardStacker
{
    // A visual grip only: no collider, joint, force, or support for a released card.
    public sealed class CardHandVisuals : MonoBehaviour
    {
        private CardStackerControls controls;
        private readonly Transform[] grips = new Transform[2];
        private readonly Material[] materials = new Material[2];
        private Mesh mesh;
        public int VisibleCount => (grips[0].gameObject.activeSelf ? 1 : 0) + (grips[1].gameObject.activeSelf ? 1 : 0);

        public void Initialise(CardStackerControls owner)
        {
            controls = owner;
            mesh = MakeHand();
            for (int i = 0; i < 2; i++)
            {
                var hand = new GameObject(i == 0 ? "Left translucent grip" : "Right translucent grip");
                grips[i] = hand.transform;
                hand.AddComponent<MeshFilter>().sharedMesh = mesh;
                materials[i] = new Material(Resources.Load<Shader>("GhostHand"));
                materials[i].color = i == 0 ? new Color(.65f, .88f, 1, .20f) : new Color(1, .83f, .65f, .20f);
                var renderer = hand.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = materials[i];
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                hand.SetActive(false);
            }
        }

        public void Refresh()
        {
            for (int i = 0; i < 2; i++)
            {
                bool visible = controls.Placement.IsLocked(i);
                grips[i].gameObject.SetActive(visible);
                if (!visible) continue;
                var card = controls.Hands[i];
                // Independent roots avoid inheriting the card's very thin Z scale or being cloned with it.
                grips[i].SetPositionAndRotation(card.position, card.rotation);
                grips[i].localScale = new Vector3(i == 0 ? 1 : -1, 1, 1);
            }
        }

        private void LateUpdate() => Refresh();

        private static Mesh MakeHand()
        {
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            AddPalm(vertices, triangles, new Vector3(0, -.024f, .014f), new Vector3(.024f, .019f, .010f));
            AddPalm(vertices, triangles, new Vector3(0, -.043f, .018f), new Vector3(.012f, .013f, .008f));
            AddFinger(vertices, triangles, new Vector3(-.014f,-.031f,.013f),new Vector3(-.050f,-.025f,.012f),new Vector3(-.043f,.016f,-.001f),new Vector3(-.029f,.005f,-.002f),.0065f);
            AddFinger(vertices, triangles, new Vector3(.012f,-.031f,.014f),new Vector3(.037f,-.030f,.013f),new Vector3(.045f,.000f,-.002f),new Vector3(.029f,-.006f,-.002f),.0075f);
            var result = new Mesh { name = "Cartoon palm, finger and thumb" };
            result.SetVertices(vertices);result.SetTriangles(triangles,0);result.RecalculateNormals();result.RecalculateBounds();
            return result;
        }

        private static void AddPalm(List<Vector3> v,List<int> t,Vector3 centre,Vector3 size)
        {
            int start=v.Count;const int rings=10,sides=16;
            for(int r=0;r<=rings;r++)for(int s=0;s<=sides;s++)
            {
                float a=Mathf.PI*r/rings,b=2*Mathf.PI*s/sides;
                v.Add(centre+Vector3.Scale(size,new Vector3(Mathf.Sin(a)*Mathf.Cos(b),Mathf.Cos(a),Mathf.Sin(a)*Mathf.Sin(b))));
            }
            for(int r=0;r<rings;r++)for(int s=0;s<sides;s++)Quad(t,start+r*(sides+1)+s,sides+1);
        }

        private static void AddFinger(List<Vector3> v,List<int> t,Vector3 a,Vector3 b,Vector3 c,Vector3 d,float radius)
        {
            int start=v.Count;const int rings=24,sides=12;
            for(int r=0;r<=rings;r++)
            {
                float u=(float)r/rings,w=1-u;
                Vector3 point=w*w*w*a+3*w*w*u*b+3*w*u*u*c+u*u*u*d;
                Vector3 tangent=(3*w*w*(b-a)+6*w*u*(c-b)+3*u*u*(d-c)).normalized;
                Vector3 x=Vector3.Cross(tangent,Vector3.forward).normalized,y=Vector3.Cross(tangent,x);
                float width=radius*(1-.22f*u)*Mathf.Sqrt(Mathf.Clamp01((1-u)*12));
                for(int s=0;s<=sides;s++){float angle=2*Mathf.PI*s/sides;v.Add(point+width*(Mathf.Cos(angle)*x+Mathf.Sin(angle)*y));}
            }
            for(int r=0;r<rings;r++)for(int s=0;s<sides;s++)Quad(t,start+r*(sides+1)+s,sides+1);
        }

        private static void Quad(List<int> t,int a,int stride)
        {
            t.Add(a);t.Add(a+1);t.Add(a+stride);t.Add(a+1);t.Add(a+stride+1);t.Add(a+stride);
        }

        private void OnDestroy()
        {
            foreach(var grip in grips)if(grip!=null)Destroy(grip.gameObject);
            foreach(var material in materials)if(material!=null)Destroy(material);
            if(mesh!=null)Destroy(mesh);
        }
    }
}
