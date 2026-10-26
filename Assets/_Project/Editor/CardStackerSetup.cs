using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Hatdon.CardStacker.Editor
{
    public static class CardStackerSetup
    {
        private const string ScenePath = "Assets/_Project/Scenes/CardStacker.unity";

        [MenuItem("Hatdon/Card Stacker/Create Setup Scene")]
        public static void CreateScene()
        {
            if (File.Exists(ScenePath) && !Application.isBatchMode &&
                !EditorUtility.DisplayDialog("Recreate setup scene?", "This replaces the setup scene. Save any work you want to keep first.", "Recreate", "Cancel")) return;
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Directory.CreateDirectory("Assets/_Project/Scenes");
            Directory.CreateDirectory("Assets/_Project/Materials");
            EditorSettings.serializationMode = SerializationMode.ForceText;
            VersionControlSettings.mode = "Visible Meta Files";
            Time.fixedDeltaTime = 0.01f;
            Physics.defaultSolverIterations = 16;
            Physics.defaultSolverVelocityIterations = 8;
            PlayerSettings.companyName = "Hatdon";
            PlayerSettings.productName = "Card Stacker";
            PlayerSettings.defaultScreenWidth = 1920;
            PlayerSettings.defaultScreenHeight = 1080;
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            RenderSettings.ambientLight = new Color(0.6f, 0.65f, 0.7f);
            var table = GameObject.CreatePrimitive(PrimitiveType.Cube);
            table.name = "Table";
            table.transform.position = new Vector3(0, -0.025f, 0);
            table.transform.localScale = new Vector3(0.8f, 0.05f, 0.6f);
            table.GetComponent<Renderer>().sharedMaterial = Material("Table", new Color(0.18f, 0.24f, 0.23f));
            var friction = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>("Assets/_Project/Materials/CardContact.asset");
            if (friction == null)
            {
                friction = new PhysicsMaterial("CardContact") { staticFriction = 0.65f, dynamicFriction = 0.45f, bounciness = 0, frictionCombine = PhysicsMaterialCombine.Average };
                AssetDatabase.CreateAsset(friction, "Assets/_Project/Materials/CardContact.asset");
            }
            table.GetComponent<Collider>().sharedMaterial = friction;
            for (int i = 0; i < 2; i++)
            {
                var card = GameObject.CreatePrimitive(PrimitiveType.Cube);
                card.name = i == 0 ? "Left Hand - Held Test Card" : "Right Hand - Held Test Card";
                card.transform.localScale = new Vector3(0.063f, 0.088f, 0.0008f);
                card.transform.position = new Vector3(i == 0 ? -0.045f : 0.045f, 0.07f, 0);
                card.GetComponent<Renderer>().sharedMaterial = Material(i == 0 ? "CardRed" : "CardCream", i == 0 ? new Color(0.7f, 0.12f, 0.16f) : new Color(0.95f, 0.91f, 0.8f));
                card.GetComponent<Collider>().sharedMaterial = friction;
                card.GetComponent<Collider>().contactOffset = 0.0001f;
                var body = card.AddComponent<Rigidbody>();
                body.mass = 0.0018f;
                body.isKinematic = true; // Setup props only; hand controls are a later milestone.
                body.interpolation = RigidbodyInterpolation.Interpolate;
                body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            }
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            cameraObject.transform.position = new Vector3(0.34f, 0.28f, -0.4f);
            cameraObject.transform.LookAt(new Vector3(0, 0.04f, 0));
            camera.nearClipPlane = 0.005f;
            camera.farClipPlane = 20;
            camera.backgroundColor = new Color(0.09f, 0.12f, 0.15f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            cameraObject.AddComponent<AudioListener>();
            var lightObject = new GameObject("Key Light");
            lightObject.transform.rotation = Quaternion.Euler(45, -30, 0);
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            light.shadows = LightShadows.Soft;
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            Validate();
            Debug.Log("HATDON_SETUP_OK: Scene created and validated. Interactive controls are not implemented.");
        }

        private static UnityEngine.Material Material(string name, Color color)
        {
            string path = "Assets/_Project/Materials/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<UnityEngine.Material>(path);
            if (material == null)
            {
                material = new UnityEngine.Material(Shader.Find("Standard"));
                AssetDatabase.CreateAsset(material, path);
            }
            material.color = color;
            return material;
        }

        public static void Validate()
        {
            var cards = UnityEngine.Object.FindObjectsByType<Rigidbody>(FindObjectsSortMode.None);
            if (cards.Length != 2) throw new Exception("Expected exactly two held test cards.");
            foreach (var card in cards)
                if (!card.isKinematic || card.mass <= 0 || card.GetComponent<Collider>() == null)
                    throw new Exception("Invalid held-card physics setup.");
            if (Camera.main == null || GameObject.Find("Table") == null || !File.Exists(ScenePath))
                throw new Exception("Setup scene is incomplete.");
        }

        public static void CapturePreview()
        {
            EditorSceneManager.OpenScene(ScenePath);
            Validate();
            var args = Environment.GetCommandLineArgs();
            int option = Array.IndexOf(args, "-previewOutput");
            if (option < 0 || option + 1 >= args.Length)
                throw new ArgumentException("Supply -previewOutput with an output PNG path.");
            string output = Path.GetFullPath(args[option + 1]);
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var target = new RenderTexture(1920, 1080, 24);
            var previous = RenderTexture.active;
            var image = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
            try
            {
                Camera.main.targetTexture = target;
                Camera.main.Render();
                RenderTexture.active = target;
                image.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
                image.Apply();
                File.WriteAllBytes(output, image.EncodeToPNG());
                Debug.Log("HATDON_PREVIEW_OK: " + output);
            }
            finally
            {
                Camera.main.targetTexture = null;
                RenderTexture.active = previous;
                target.Release();
                UnityEngine.Object.DestroyImmediate(target);
                UnityEngine.Object.DestroyImmediate(image);
            }
        }
    }
}
