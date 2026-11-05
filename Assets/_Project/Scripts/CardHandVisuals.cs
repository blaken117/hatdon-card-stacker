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
        public Vector3 FingerDirection(int index) => grips[index].up;

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
                materials[i].color = i == 0 ? new Color(.86f, .93f, 1, .18f) : new Color(.98f, .95f, .89f, .18f);
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
                Vector3 size=card.transform.localScale;
                float top=.5f*(Mathf.Abs((card.rotation*Vector3.right).y)*size.x+Mathf.Abs((card.rotation*Vector3.up).y)*size.y+Mathf.Abs((card.rotation*Vector3.forward).y)*size.z);
                Vector3 facing=Vector3.ProjectOnPlane(Camera.main.transform.position-card.position,Vector3.up).normalized;
                if(facing.sqrMagnitude<.1f)facing=Vector3.back;
                grips[i].SetPositionAndRotation(card.position+Vector3.up*(top+.105f),Quaternion.LookRotation(facing,Vector3.down));
                grips[i].localScale = new Vector3(i == 0 ? 1 : -1, 1, 1);
            }
        }

        private void LateUpdate() => Refresh();

        private static Mesh MakeHand()
        {
            var v=new List<Vector3>();var t=new List<int>();
            Vector2[] edge={new Vector2(-.023f,-.025f),new Vector2(-.032f,-.003f),new Vector2(-.026f,.033f),new Vector2(-.014f,.042f),new Vector2(.014f,.040f),new Vector2(.027f,.028f),new Vector2(.030f,-.003f),new Vector2(.019f,-.025f)};
            for(int i=0;i<edge.Length;i++)
            {
                Vector2 a=edge[i],b=edge[(i+1)%edge.Length];
                Vector3 frontA=new Vector3(a.x,a.y,.006f),frontB=new Vector3(b.x,b.y,.006f);
                Vector3 backA=new Vector3(a.x,a.y,-.006f),backB=new Vector3(b.x,b.y,-.006f);
                Face(v,t,new Vector3(-.004f,.005f,.014f),frontA,frontB);
                Face(v,t,new Vector3(.003f,.003f,-.010f),backB,backA);
                Face(v,t,frontA,backA,frontB);Face(v,t,frontB,backA,backB);
            }
            Digit(v,t,new Vector3(-.020f,.026f,0),new Vector3(-.025f,.091f,.001f),.012f,.011f);
            Digit(v,t,new Vector3(-.006f,.031f,0),new Vector3(-.006f,.102f,.001f),.012f,.011f);
            Digit(v,t,new Vector3(.009f,.030f,0),new Vector3(.011f,.096f,0),.0115f,.0105f);
            Digit(v,t,new Vector3(.022f,.023f,0),new Vector3(.028f,.079f,-.001f),.010f,.0095f);
            Digit(v,t,new Vector3(-.025f,-.010f,.002f),new Vector3(-.052f,.037f,.002f),.014f,.012f);
            Digit(v,t,new Vector3(0,-.081f,-.001f),new Vector3(.001f,-.022f,0),.024f,.017f);
            var mesh=new Mesh{name="Faceted five-finger ghost hand"};mesh.SetVertices(v);mesh.SetTriangles(t,0);mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
        }

        private static void Digit(List<Vector3> v,List<int> t,Vector3 start,Vector3 end,float width,float depth)
        {
            Vector3 axis=(end-start).normalized,side=Vector3.Cross(axis,Vector3.forward).normalized;
            float[] along={0,.48f,.91f,1};float[] taper={1,.96f,.78f,.55f};
            var rings=new Vector3[4,8];
            for(int r=0;r<4;r++)for(int s=0;s<8;s++)
            {
                float angle=(22.5f+s*45)*Mathf.Deg2Rad;
                rings[r,s]=Vector3.Lerp(start,end,along[r])+side*Mathf.Cos(angle)*width*.5f*taper[r]+Vector3.forward*Mathf.Sin(angle)*depth*.5f*taper[r];
            }
            for(int r=0;r<3;r++)for(int s=0;s<8;s++)
            {
                int next=(s+1)%8;
                Face(v,t,rings[r,s],rings[r,next],rings[r+1,s]);Face(v,t,rings[r,next],rings[r+1,next],rings[r+1,s]);
            }
            for(int s=0;s<8;s++){Face(v,t,end,rings[3,s],rings[3,(s+1)%8]);Face(v,t,start,rings[0,(s+1)%8],rings[0,s]);}
        }
        private static void Face(List<Vector3> v,List<int> t,Vector3 a,Vector3 b,Vector3 c)
        {
            int i=v.Count;v.Add(a);v.Add(b);v.Add(c);t.Add(i);t.Add(i+1);t.Add(i+2);
        }
        private void OnDestroy()
        {
            foreach(var grip in grips)if(grip!=null)Destroy(grip.gameObject);
            foreach(var material in materials)if(material!=null)Destroy(material);
            if(mesh!=null)Destroy(mesh);
        }
    }
}
