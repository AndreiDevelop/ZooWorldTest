using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using ZooWorld.Animals;
using ZooWorld.Animals.Content;
using ZooWorld.Domain.Animals;
using ZooWorld.Infrastructure;
using ZooWorld.Simulation.Content;
using Object = UnityEngine.Object;

namespace ZooWorld.EditorTools
{
    public static class IterationTwoSetup
    {
        private const string BasePath = "Assets/Prefabs/Animals/Animal.prefab";
        private const string SimulationPath = "Assets/SO/Simulation/ZooSimulation.asset";

        [MenuItem("ZooWorld/Setup/Create Iteration 2 Content")]
        public static void CreateContent()
        {
            if (EditorApplication.isPlaying || SceneManager.GetActiveScene().isDirty)
            {
                throw new InvalidOperationException("Save the current scene and leave Play Mode before setup.");
            }

            if (AssetDatabase.LoadAssetAtPath<GameObject>(BasePath) != null)
            {
                throw new InvalidOperationException("Iteration 2 content exists; inspect and edit it in place.");
            }

            foreach (var folder in new[]
            {
                "Assets/Materials", "Assets/Materials/Animals", "Assets/Materials/World",
                "Assets/Prefabs", "Assets/Prefabs/Animals", "Assets/SO/Animals", "Assets/SO/Movement",
                "Assets/SO/Simulation", "Assets/Art", "Assets/Art/Animals"
            })
            {
                EnsureFolder(folder);
            }

            var frogMaterial = CreateMaterial("Assets/Materials/Animals/Frog.mat", new Color(0.28f, 0.72f, 0.37f));
            var snakeMaterial = CreateMaterial("Assets/Materials/Animals/Snake.mat", new Color(0.95f, 0.55f, 0.19f));
            var eyeMaterial = CreateMaterial("Assets/Materials/Animals/Eye.mat", new Color(0.98f, 0.97f, 0.85f));
            var pupilMaterial = CreateMaterial("Assets/Materials/Animals/Pupil.mat", new Color(0.07f, 0.12f, 0.12f));
            CreateMaterial("Assets/Materials/World/Ground.mat", new Color(0.24f, 0.38f, 0.32f));
            var contact = new PhysicsMaterial("Animal Contact")
            {
                dynamicFriction = 0f,
                staticFriction = 0f,
                bounciness = 0.75f,
                bounceCombine = PhysicsMaterialCombine.Maximum
            };
            AssetDatabase.CreateAsset(contact, "Assets/Settings/AnimalContact.physicMaterial");
            var basePrefab = CreateBase(contact);
            var frog = CreateVariant(basePrefab, "Frog", frogMaterial, eyeMaterial, pupilMaterial, true);
            var snake = CreateVariant(basePrefab, "Snake", snakeMaterial, eyeMaterial, pupilMaterial, false);
            var jump = CreateAsset<JumpMovementSO>("Assets/SO/Movement/FrogJump.asset");
            SetFloat(jump, "_interval", 1.2f);
            SetFloat(jump, "_duration", 0.45f);
            SetFloat(jump, "_distance", 1.8f);
            SetFloat(jump, "_height", 0.7f);
            SetFloat(jump, "_recoveryAcceleration", 4f);
            var linear = CreateAsset<LinearMovementSO>("Assets/SO/Movement/SnakeLinear.asset");
            SetFloat(linear, "_speed", 2f);
            SetFloat(linear, "_recoveryAcceleration", 4f);
            var frogIcon = CreateIcon("Frog", frogMaterial.color);
            var snakeIcon = CreateIcon("Snake", snakeMaterial.color);
            var frogDefinition = CreateDefinition("Frog", "frog", TrophicRole.Prey, frog, jump, frogIcon,
                new Vector2(0.8f, 1.2f));
            var snakeDefinition = CreateDefinition("Snake", "snake", TrophicRole.Predator, snake, linear, snakeIcon,
                new Vector2(0.9f, 1.3f));
            var settings = CreateAsset<SimulationSO>(SimulationPath);
            var data = new SerializedObject(settings);
            var animals = data.FindProperty("_animals");
            animals.arraySize = 2;
            animals.GetArrayElementAtIndex(0).objectReferenceValue = frogDefinition;
            animals.GetArrayElementAtIndex(1).objectReferenceValue = snakeDefinition;
            data.FindProperty("_spawnInterval").vector2Value = new Vector2(1f, 2f);
            data.FindProperty("_turnInterval").vector2Value = new Vector2(1.5f, 3.5f);
            data.FindProperty("_spawnPadding").floatValue = 0.5f;
            data.FindProperty("_spawnSeparation").floatValue = 0.15f;
            data.FindProperty("_spawnAttempts").intValue = 8;
            data.FindProperty("_maxInactivePerPrefab").intValue = 16;
            data.ApplyModifiedPropertiesWithoutUndo();
            settings.Validate();
            AssetDatabase.SaveAssets();
            ConfigureMain();
            EditorSceneManager.OpenScene("Assets/Scenes/Intro.unity");
            Debug.Log("ZooWorld iteration 2 content created and validated.");
        }

        private static AnimalView CreateBase(PhysicsMaterial contact)
        {
            var root = new GameObject("Animal");
            root.SetActive(false);
            var body = root.AddComponent<Rigidbody>();
            body.useGravity = false;
            body.linearDamping = 0f;
            body.angularDamping = 0f;
            body.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            var collider = root.AddComponent<SphereCollider>();
            collider.center = new Vector3(0f, 0.5f, 0f);
            collider.radius = 0.5f;
            collider.sharedMaterial = contact;
            var visual = new GameObject("VisualRoot").transform;
            visual.SetParent(root.transform, false);
            var view = root.AddComponent<AnimalView>();
            SetReference(view, "_body", body);
            SetReference(view, "_visual", visual);
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, BasePath);
            Object.DestroyImmediate(root);
            return prefab.GetComponent<AnimalView>();
        }

        private static AnimalView CreateVariant(AnimalView source, string name, Material material,
            Material eyeMaterial, Material pupilMaterial, bool frog)
        {
            var root = (GameObject)PrefabUtility.InstantiatePrefab(source.gameObject);
            root.name = name;
            var visual = root.GetComponent<AnimalView>().Visual;
            if (frog)
            {
                Primitive(visual, "Body", PrimitiveType.Sphere, new Vector3(0f, 0.36f, 0f),
                    new Vector3(0.85f, 0.55f, 0.85f), material);
                for (var side = -1; side <= 1; side += 2)
                {
                    Primitive(visual, "Leg", PrimitiveType.Sphere, new Vector3(side * 0.35f, 0.2f, -0.2f),
                        new Vector3(0.4f, 0.3f, 0.55f), material);
                }
            }
            else
            {
                var body = Primitive(visual, "Body", PrimitiveType.Capsule, new Vector3(0f, 0.3f, -0.1f),
                    new Vector3(0.4f, 0.7f, 0.4f), material);
                body.localRotation = Quaternion.Euler(90f, 0f, 0f);
                Primitive(visual, "Head", PrimitiveType.Sphere, new Vector3(0f, 0.38f, 0.5f),
                    new Vector3(0.65f, 0.45f, 0.65f), material);
            }

            for (var side = -1; side <= 1; side += 2)
            {
                var z = frog ? 0.25f : 0.65f;
                Primitive(visual, "Eye", PrimitiveType.Sphere, new Vector3(side * 0.23f, 0.64f, z),
                    Vector3.one * 0.24f, eyeMaterial);
                Primitive(visual, "Pupil", PrimitiveType.Sphere, new Vector3(side * 0.23f, 0.75f, z + 0.03f),
                    Vector3.one * 0.12f, pupilMaterial);
            }

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, $"Assets/Prefabs/Animals/{name}.prefab");
            Object.DestroyImmediate(root);
            return prefab.GetComponent<AnimalView>();
        }

        private static AnimalDefinitionSO CreateDefinition(string displayName, string id, TrophicRole role,
            AnimalView prefab, MovementSO movement, Sprite icon, Vector2 sizeRange)
        {
            var definition = CreateAsset<AnimalDefinitionSO>($"Assets/SO/Animals/{displayName}.asset");
            var data = new SerializedObject(definition);
            data.FindProperty("_id").stringValue = id;
            data.FindProperty("_displayName").stringValue = displayName;
            data.FindProperty("_role").enumValueIndex = (int)role;
            data.FindProperty("_prefab").objectReferenceValue = prefab;
            data.FindProperty("_movement").objectReferenceValue = movement;
            data.FindProperty("_icon").objectReferenceValue = icon;
            data.FindProperty("_sizeRange").vector2Value = sizeRange;
            data.ApplyModifiedPropertiesWithoutUndo();
            definition.ValidateForSimulation();
            return definition;
        }

        private static void ConfigureMain()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");
            MainSceneController main = null;
            foreach (var root in scene.GetRootGameObjects())
            {
                var controller = root.GetComponent<MainSceneController>();
                if (controller != null)
                {
                    main = controller;
                }
            }

            if (main == null)
            {
                throw new InvalidOperationException("Main scene root was not found.");
            }

            var animalsRoot = new GameObject("Animals").transform;
            animalsRoot.SetParent(main.transform, false);
            SetReference(main, "_animalsRoot", animalsRoot);
            SetReference(main, "_simulation", AssetDatabase.LoadAssetAtPath<SimulationSO>(SimulationPath));
            var camera = main.WorldCamera;
            camera.orthographic = true;
            camera.orthographicSize = 8.5f;
            camera.transform.SetPositionAndRotation(new Vector3(0f, 24f, 0f), Quaternion.Euler(90f, 0f, 0f));
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = Vector3.down * 0.025f;
            ground.transform.localScale = new Vector3(20f, 1f, 20f);
            ground.GetComponent<Renderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/Materials/World/Ground.mat");
            Object.DestroyImmediate(ground.GetComponent<Collider>());
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        private static Transform Primitive(Transform parent, string name, PrimitiveType type, Vector3 position,
            Vector3 scale, Material material)
        {
            var child = GameObject.CreatePrimitive(type);
            child.name = name;
            child.transform.SetParent(parent, false);
            child.transform.localPosition = position;
            child.transform.localScale = scale;
            child.GetComponent<Renderer>().sharedMaterial = material;
            Object.DestroyImmediate(child.GetComponent<Collider>());
            return child.transform;
        }

        private static Sprite CreateIcon(string name, Color color)
        {
            const int Size = 64;
            var texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
            for (var y = 0; y < Size; y++)
            {
                for (var x = 0; x < Size; x++)
                {
                    var point = new Vector2((x + 0.5f) / Size, (y + 0.5f) / Size) * 2f - Vector2.one;
                    texture.SetPixel(x, y, point.sqrMagnitude <= 0.85f ? color : Color.clear);
                }
            }

            texture.Apply();
            var path = $"Assets/Art/Animals/{name}Icon.png";
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = Size;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static Material CreateMaterial(string path, Color color)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = color;
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static T CreateAsset<T>(string path) where T : ScriptableObject
        {
            var asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void SetFloat(Object target, string field, float value)
        {
            var data = new SerializedObject(target);
            data.FindProperty(field).floatValue = value;
            data.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetReference(Object target, string field, Object value)
        {
            if (value == null)
            {
                throw new InvalidOperationException($"Required reference '{field}' is missing.");
            }

            var data = new SerializedObject(target);
            data.FindProperty(field).objectReferenceValue = value;
            data.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                var split = path.LastIndexOf('/');
                AssetDatabase.CreateFolder(path.Substring(0, split), path.Substring(split + 1));
            }
        }
    }
}
