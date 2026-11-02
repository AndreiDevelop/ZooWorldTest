using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using ZooWorld.Infrastructure;
using ZooWorld.Simulation;
using Object = UnityEngine.Object;

namespace ZooWorld.EditorTools
{
    public static class WallLayoutSetup
    {
        private const string Folder = "Assets/Prefabs/World";

        [MenuItem("ZooWorld/Setup/Create Reference Walls")]
        public static void Create()
        {
            if (EditorApplication.isPlaying || EditorSceneManager.GetActiveScene().isDirty)
            {
                throw new InvalidOperationException("Save the current scene and leave Play Mode before creating walls.");
            }

            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");
            MainSceneController main = null;
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name == "Obstacles")
                {
                    throw new InvalidOperationException("Main already contains a wall layout; edit existing prefab instances.");
                }

                if (root.TryGetComponent<MainSceneController>(out var controller))
                {
                    main = controller;
                }
            }

            if (main == null)
            {
                throw new InvalidOperationException("Main scene controller is missing.");
            }

            if (!AssetDatabase.IsValidFolder(Folder))
            {
                AssetDatabase.CreateFolder("Assets/Prefabs", "World");
            }

            if (!AssetDatabase.IsValidFolder("Assets/Materials/World"))
            {
                AssetDatabase.CreateFolder("Assets/Materials", "World");
            }

            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.name = "Wall";
            material.color = new Color(0.58f, 0.60f, 0.58f);
            AssetDatabase.CreateAsset(material, "Assets/Materials/World/Wall.mat");
            var segment = new GameObject("WallSegment");
            var block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = "Block";
            block.transform.SetParent(segment.transform, false);
            block.transform.localPosition = Vector3.up * 0.575f;
            block.transform.localScale = new Vector3(3f, 1.15f, 0.7f);
            block.GetComponent<Renderer>().sharedMaterial = material;
            block.GetComponent<BoxCollider>().sharedMaterial =
                AssetDatabase.LoadAssetAtPath<PhysicsMaterial>("Assets/Settings/AnimalContact.physicMaterial");
            var straight = PrefabUtility.SaveAsPrefabAsset(segment, Folder + "/WallSegment.prefab");
            Object.DestroyImmediate(segment);

            var corner = new GameObject("WallCorner");
            Place(straight, corner.transform, Vector3.zero, 0f, 1f);
            Place(straight, corner.transform, new Vector3(1.15f, 0f, -1.25f), 90f, 0.6f);
            var elbow = PrefabUtility.SaveAsPrefabAsset(corner, Folder + "/WallCorner.prefab");
            Object.DestroyImmediate(corner);

            var obstacles = new GameObject("Obstacles").AddComponent<WorldObstacles>();
            var parent = obstacles.transform;
            var halfHeight = main.WorldCamera.orthographicSize;
            var halfWidth = halfHeight * 16f / 9f;
            var colliders = new List<Collider>();
            Add(straight, parent, colliders, halfWidth, halfHeight, 0.135f, 0.94f);
            Add(straight, parent, colliders, halfWidth, halfHeight, 0.50f, 0.94f);
            Add(straight, parent, colliders, halfWidth, halfHeight, 0.80f, 0.94f);
            Add(straight, parent, colliders, halfWidth, halfHeight, 0.045f, 0.42f, 90f, 1.13f);
            Add(straight, parent, colliders, halfWidth, halfHeight, 0.045f, 0.12f, 90f, 1.13f);
            Add(straight, parent, colliders, halfWidth, halfHeight, 0.995f, 0.42f, 90f, 1.13f);
            Add(straight, parent, colliders, halfWidth, halfHeight, 0.995f, 0.12f, 90f, 1.13f);
            Add(elbow, parent, colliders, halfWidth, halfHeight, 0.38f, 0.59f);
            Add(elbow, parent, colliders, halfWidth, halfHeight, 0.34f, 0.33f, 180f);
            Add(straight, parent, colliders, halfWidth, halfHeight, 0.72f, 0.37f);
            Add(straight, parent, colliders, halfWidth, halfHeight, 0.215f, 0.15f);
            Add(straight, parent, colliders, halfWidth, halfHeight, 0.875f, 0.15f);
            Add(straight, parent, colliders, halfWidth, halfHeight, 0.36f, -0.02f);
            Add(straight, parent, colliders, halfWidth, halfHeight, 0.805f, -0.02f);

            var map = new SerializedObject(obstacles);
            var entries = map.FindProperty("_colliders");
            entries.arraySize = colliders.Count;
            for (var index = 0; index < colliders.Count; index++)
            {
                entries.GetArrayElementAtIndex(index).objectReferenceValue = colliders[index];
            }

            map.ApplyModifiedPropertiesWithoutUndo();
            var data = new SerializedObject(main);
            data.FindProperty("_obstacles").objectReferenceValue = obstacles;
            data.ApplyModifiedPropertiesWithoutUndo();
            obstacles.Validate();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene("Assets/Scenes/Intro.unity");
            Debug.Log("Main reference layout saved: 14 wall prefab instances, 16 static colliders.");
        }

        private static GameObject Place(GameObject prefab, Transform parent, Vector3 position,
            float rotation, float length)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.transform.localPosition = position;
            instance.transform.localRotation = Quaternion.Euler(0f, rotation, 0f);
            instance.transform.localScale = new Vector3(length, 1f, 1f);
            return instance;
        }

        private static void Add(GameObject prefab, Transform parent, List<Collider> colliders,
            float halfWidth, float halfHeight, float horizontal, float vertical,
            float rotation = 0f, float length = 1f)
        {
            var position = new Vector3(Mathf.LerpUnclamped(-halfWidth, halfWidth, horizontal), 0f,
                Mathf.LerpUnclamped(-halfHeight, halfHeight, vertical));
            var instance = Place(prefab, parent, position, rotation, length);
            colliders.AddRange(instance.GetComponentsInChildren<Collider>());
        }
    }
}
