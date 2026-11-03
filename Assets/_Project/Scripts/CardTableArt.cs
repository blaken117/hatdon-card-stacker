using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Hatdon.CardStacker
{
    // Cosmetic dressing only: creates no colliders or rigid bodies.
    public sealed class CardTableArt : MonoBehaviour
    {
        private readonly List<Object> owned = new List<Object>();
        private readonly Dictionary<string, Material> materials = new Dictionary<string, Material>();

        public static void Apply()
        {
            if (GameObject.Find("Tabletop atmosphere") != null) return;
            new GameObject("Tabletop atmosphere").AddComponent<CardTableArt>().Create();
        }

        private Material Surface(string name, string color, string texture = null, float smoothness = .12f, float metal = 0)
        {
            if (materials.TryGetValue(name, out var existing)) return existing;
            ColorUtility.TryParseHtmlString(color, out var tint);
            var material = new Material(Shader.Find("Standard")) { name = name, color = tint };
            material.SetFloat("_Glossiness", smoothness); material.SetFloat("_Metallic", metal);
            if (texture != null)
            {
                var map = Resources.Load<Texture2D>("Art/" + texture);
                if (map == null) throw new System.InvalidOperationException("Missing art texture: " + texture);
                map.anisoLevel = 8;
                map.wrapMode = texture.Contains("felt") || texture.Contains("grain") ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
                material.mainTexture = map;
            }
            materials[name] = material; owned.Add(material); return material;
        }

        private GameObject Shape(string name, PrimitiveType primitive, Vector3 position, Vector3 scale, Material material, Transform parent = null)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent != null ? parent : transform, false);
            go.transform.localPosition = position; go.transform.localScale = scale;
            go.AddComponent<MeshFilter>().sharedMesh = Resources.GetBuiltinResource<Mesh>(primitive + ".fbx");
            var renderer = go.AddComponent<MeshRenderer>(); renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.On; renderer.receiveShadows = true;
            return go;
        }

        private List<Vector2> CardOutline()
        {
            var points = new List<Vector2>();
            const float rx = .045f, ry = .0322f;
            Vector2[] centers = { new Vector2(.5f-rx,.5f-ry), new Vector2(-.5f+rx,.5f-ry), new Vector2(-.5f+rx,-.5f+ry), new Vector2(.5f-rx,-.5f+ry) };
            for (int c = 0; c < 4; c++) for (int i = 0; i < 10; i++)
            {
                float a = (c * 90 + i * 9) * Mathf.Deg2Rad;
                points.Add(centers[c] + new Vector2(Mathf.Cos(a)*rx, Mathf.Sin(a)*ry));
            }
            return points;
        }

        private Mesh CardFace(bool back, float z)
        {
            var points = CardOutline();
            var vertices = new List<Vector3> { new Vector3(0,0,z) };
            var uv = new List<Vector2> { new Vector2(.5f,.5f) };
            var triangles = new List<int>();
            foreach (var p in points) { vertices.Add(new Vector3(p.x,p.y,z)); uv.Add(new Vector2(back ? .5f-p.x : p.x+.5f,p.y+.5f)); }
            for (int i = 0; i < points.Count; i++)
            {
                int a = i+1, b = (i+1)%points.Count+1;
                triangles.Add(0); triangles.Add(back ? a : b); triangles.Add(back ? b : a);
            }
            var mesh = new Mesh { name = back ? "Rounded card back" : "Rounded card face" };
            mesh.SetVertices(vertices); mesh.SetUVs(0,uv); mesh.SetTriangles(triangles,0); mesh.RecalculateNormals();
            owned.Add(mesh); return mesh;
        }

        private Mesh CardEdges()
        {
            var points = CardOutline(); var vertices = new List<Vector3>(); var triangles = new List<int>();
            for (int i = 0; i < points.Count; i++)
            {
                var a = points[i]; var b = points[(i+1)%points.Count]; int n = vertices.Count;
                vertices.Add(new Vector3(a.x,a.y,-.5f)); vertices.Add(new Vector3(a.x,a.y,.5f));
                vertices.Add(new Vector3(b.x,b.y,.5f)); vertices.Add(new Vector3(b.x,b.y,-.5f));
                triangles.AddRange(new[] {n,n+1,n+2,n,n+2,n+3});
            }
            var mesh = new Mesh { name = "Rounded paper edge" }; mesh.SetVertices(vertices); mesh.SetTriangles(triangles,0); mesh.RecalculateNormals(); owned.Add(mesh); return mesh;
        }

        private void PrintedFace(Transform card, bool back, Material material)
        {
            var face = new GameObject(back ? "Patterned card back" : "Printed playing-card face");
            face.transform.SetParent(card,false);
            face.AddComponent<MeshFilter>().sharedMesh = CardFace(back,back ? .52f : -.52f);
            var renderer = face.AddComponent<MeshRenderer>(); renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.On;
        }

        private void Create()
        {
            var wood = Surface("Walnut", "#ffffff", "walnut-grain", .23f);
            var felt = Surface("Woven green felt", "#ffffff", "green-felt", .02f);
            var paper = Surface("Paper edges", "#e5deca", null, .05f);
            var brass = Surface("Aged brass", "#b99651", null, .4f, .65f);
            var charcoal = Surface("Lamp enamel", "#20312e", null, .32f, .3f);
            var back = Surface("Navy patterned deck", "#ffffff", "deck-back", .10f);
            var frontA = Surface("Ace of spades", "#ffffff", "ace-spades", .10f);
            var front7 = Surface("Seven of hearts", "#ffffff", "seven-hearts", .10f);
            var table = GameObject.Find("Table");
            table.GetComponent<Renderer>().sharedMaterial = wood;
            table.GetComponent<Renderer>().enabled=false; // Keep its collider; the dressed desktop and felt render the surface.
            Shape("Walnut desktop",PrimitiveType.Cube,new Vector3(0,-.024f,0),new Vector3(.94f,.044f,.82f),wood);
            Shape("Felt playing surface",PrimitiveType.Cube,new Vector3(0,-.000505f,0),new Vector3(.799f,.001f,.599f),felt);
            Shape("Desktop bevel / apron",PrimitiveType.Cube,new Vector3(0,-.052f,0),new Vector3(.91f,.025f,.79f),wood);
            var thread = Surface("Felt stitch", "#829878",null,.02f);
            // Small individual stitches keep the playing boundary understated.
            for (int i=0;i<66;i++)
            {
                float x=-.383f+i*.0116f;
                foreach(float z in new[]{-.284f,.284f}) Shape("Felt stitch",PrimitiveType.Cube,new Vector3(x,.00025f,z),new Vector3(.004f,.0001f,.0005f),thread);
            }
            for (int i=0;i<49;i++)
            {
                float z=-.277f+i*.0115f;
                foreach(float x in new[]{-.385f,.385f}) Shape("Felt stitch",PrimitiveType.Cube,new Vector3(x,.00025f,z),new Vector3(.0005f,.0001f,.004f),thread);
            }
            var cards = FindObjectsByType<Rigidbody>(FindObjectsSortMode.None);
            System.Array.Sort(cards,(a,b)=>a.position.x.CompareTo(b.position.x));
            for(int i=0;i<cards.Length;i++)
            {
                cards[i].GetComponent<MeshFilter>().sharedMesh = CardEdges();
                cards[i].GetComponent<MeshRenderer>().sharedMaterial = paper;
                PrintedFace(cards[i].transform,false,i==0 ? frontA : front7);
                PrintedFace(cards[i].transform,true,back);
            }

            // Spare deck on the wooden margin; decorative objects have no physics.
            var deck = new GameObject("Spare deck"); deck.transform.SetParent(transform,false);
            deck.transform.localPosition=new Vector3(-.25f,.001f,.35f); deck.transform.localRotation=Quaternion.Euler(0,-12,0);
            for(int i=0;i<12;i++) Shape("Stacked paper",PrimitiveType.Cube,new Vector3(0,i*.0008f,0),new Vector3(.063f,.00065f,.088f),paper,deck.transform);
            var top = new GameObject("Top card");top.transform.SetParent(deck.transform,false);
            top.transform.localPosition=new Vector3(0,.0095f,0);top.transform.localScale=new Vector3(.063f,.088f,.0008f);top.transform.localRotation=Quaternion.Euler(-90,0,0);
            PrintedFace(top.transform,true,back);

            var lamp = new GameObject("Warm reading lamp");lamp.transform.SetParent(transform,false);lamp.transform.localPosition=new Vector3(.35f,0,.32f);
            Shape("Lamp base",PrimitiveType.Cylinder,new Vector3(0,.007f,0),new Vector3(.09f,.007f,.09f),charcoal,lamp.transform);
            Shape("Brass foot",PrimitiveType.Cylinder,new Vector3(0,.015f,0),new Vector3(.045f,.002f,.045f),brass,lamp.transform);
            Shape("Lamp stem",PrimitiveType.Cylinder,new Vector3(0,.15f,0),new Vector3(.009f,.135f,.009f),brass,lamp.transform);
            Shape("Shade",PrimitiveType.Sphere,new Vector3(-.025f,.285f,-.012f),new Vector3(.15f,.065f,.115f),charcoal,lamp.transform);
            var bulb = Surface("Warm lamp glow", "#ffe3ab",null,.2f);
            bulb.EnableKeyword("_EMISSION");bulb.SetColor("_EmissionColor",new Color(1,.58f,.2f)*1.4f);
            Shape("Lamp diffuser",PrimitiveType.Sphere,new Vector3(-.025f,.269f,-.012f),new Vector3(.122f,.012f,.089f),bulb,lamp.transform);
            var lampLight = new GameObject("Warm desk light");lampLight.transform.SetParent(lamp.transform,false);lampLight.transform.localPosition=new Vector3(-.025f,.25f,-.02f);
            var glow=lampLight.AddComponent<Light>();glow.type=LightType.Point;glow.color=new Color(1,.75f,.46f);glow.intensity=.42f;glow.range=1.0f;
            lamp.transform.localScale=Vector3.one*.76f;

            var pot = Surface("Terracotta", "#9d6246",null,.05f);
            var soil = Surface("Soil", "#27251e");var leaf=Surface("Plant leaves", "#426b44",null,.1f);
            Shape("Small plant pot",PrimitiveType.Cylinder,new Vector3(-.405f,.027f,.31f),new Vector3(.062f,.027f,.062f),pot);
            Shape("Pot soil",PrimitiveType.Cylinder,new Vector3(-.405f,.0545f,.31f),new Vector3(.053f,.001f,.053f),soil);
            for(int i=0;i<7;i++)
            {
                float a=i*Mathf.PI*2/7;
                var sprig=Shape("Plant leaf",PrimitiveType.Sphere,new Vector3(-.405f+Mathf.Cos(a)*.018f,.078f,.31f+Mathf.Sin(a)*.018f),new Vector3(.018f,.055f,.009f),leaf);
                sprig.transform.localRotation=Quaternion.Euler(Mathf.Sin(a)*35, a*Mathf.Rad2Deg,Mathf.Cos(a)*-35);
            }

            var floor=Surface("Dark timber floor", "#867263","walnut-grain",.12f);floor.mainTextureScale=new Vector2(4,4);
            var wall=Surface("Quiet teal walls", "#344746",null,.02f);
            Shape("Room floor",PrimitiveType.Cube,new Vector3(0,-.74f,0),new Vector3(4,.04f,4),floor);
            Shape("Back wall",PrimitiveType.Cube,new Vector3(0,.2f,1.05f),new Vector3(4,2,.04f),wall);
            Shape("Left wall",PrimitiveType.Cube,new Vector3(-1.2f,.2f,0),new Vector3(.04f,2,4),wall);
            foreach(float x in new[]{-.35f,.35f}) foreach(float z in new[]{-.28f,.28f})
                Shape("Table leg",PrimitiveType.Cube,new Vector3(x,-.39f,z),new Vector3(.045f,.67f,.045f),wood);
            for(int i=0;i<8;i++) Shape("Wall timber trim",PrimitiveType.Cube,new Vector3(-1.1f+i*.32f,-.29f,1.02f),new Vector3(.008f,.72f,.012f),wood);
            Shape("Wainscot cap",PrimitiveType.Cube,new Vector3(0,.07f,1.01f),new Vector3(2.5f,.018f,.025f),wood);
            Shape("Picture frame",PrimitiveType.Cube,new Vector3(-.12f,.34f,1.013f),new Vector3(.36f,.25f,.018f),wood);
            var print=Surface("Print paper", "#d2c4a3",null,.02f);
            Shape("Quiet wall print",PrimitiveType.Cube,new Vector3(-.12f,.34f,1.001f),new Vector3(.332f,.222f,.003f),print);
            var printInk=Surface("Print ink", "#546e64",null,.02f);
            Shape("Print horizon",PrimitiveType.Cube,new Vector3(-.12f,.312f,.998f),new Vector3(.29f,.052f,.002f),printInk);
            var printSun=Surface("Print ochre", "#b1804b",null,.02f);
            var sunDisc=Shape("Print sun",PrimitiveType.Cylinder,new Vector3(-.19f,.372f,.997f),new Vector3(.047f,.001f,.047f),printSun);
            sunDisc.transform.localRotation=Quaternion.Euler(90,0,0);

            RenderSettings.ambientMode=AmbientMode.Trilight;
            RenderSettings.ambientSkyColor=new Color(.37f,.42f,.45f);
            RenderSettings.ambientEquatorColor=new Color(.31f,.32f,.29f);
            RenderSettings.ambientGroundColor=new Color(.19f,.16f,.13f);
            var key=GameObject.Find("Key Light").GetComponent<Light>();
            key.color=new Color(1,.91f,.77f);key.intensity=.85f;key.shadowBias=.005f;key.shadowNormalBias=.01f;key.shadowResolution=LightShadowResolution.VeryHigh;
            QualitySettings.shadowDistance=2;QualitySettings.shadows=ShadowQuality.All;QualitySettings.shadowResolution=ShadowResolution.VeryHigh;
            var fillObject=new GameObject("Cool window fill");fillObject.transform.SetParent(transform,false);fillObject.transform.rotation=Quaternion.Euler(35,145,0);
            var fill=fillObject.AddComponent<Light>();fill.type=LightType.Directional;fill.color=new Color(.68f,.82f,1);fill.intensity=.24f;
            Camera.main.backgroundColor=new Color(.13f,.18f,.18f);
        }

        private void OnDestroy() { foreach(var item in owned) if(item!=null) Destroy(item); }
    }
}
