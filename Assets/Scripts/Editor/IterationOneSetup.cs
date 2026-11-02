using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using ZooWorld.Infrastructure;

namespace ZooWorld.EditorTools
{
    public static class IterationOneSetup
    {
        private const string IntroPath = "Assets/Scenes/Intro.unity";
        private const string MainPath = "Assets/Scenes/Main.unity";
        private const string HudPath = "Assets/Scenes/HUD.unity";
        private const string ConfigurationPath = "Assets/SO/Bootstrap/SceneFlow.asset";

        [MenuItem("ZooWorld/Setup/Create Iteration 1 Scenes")]
        public static void CreateScenes()
        {
            if (EditorApplication.isPlaying)
            {
                throw new InvalidOperationException("Create scenes outside Play Mode.");
            }

            for (var index = 0; index < SceneManager.sceneCount; index++)
            {
                if (SceneManager.GetSceneAt(index).isDirty)
                {
                    throw new InvalidOperationException("Save modified scenes before running iteration setup.");
                }
            }

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(IntroPath) != null
                || AssetDatabase.LoadAssetAtPath<SceneAsset>(MainPath) != null
                || AssetDatabase.LoadAssetAtPath<SceneAsset>(HudPath) != null)
            {
                throw new InvalidOperationException("Iteration scenes already exist; inspect and edit them in place.");
            }

            EnsureFolder("Assets/SO");
            EnsureFolder("Assets/SO/Bootstrap");
            var configuration = ScriptableObject.CreateInstance<SceneFlowSO>();
            AssetDatabase.CreateAsset(configuration, ConfigurationPath);
            var serialized = new SerializedObject(configuration);
            serialized.FindProperty("_mainScenePath").stringValue = MainPath;
            serialized.FindProperty("_hudScenePath").stringValue = HudPath;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            configuration.Validate();
            AssetDatabase.SaveAssets();

            if (!AssetDatabase.CopyAsset("Assets/Scenes/SampleScene.unity", MainPath))
            {
                throw new InvalidOperationException("Could not copy the template scene to Main.");
            }

            var main = EditorSceneManager.OpenScene(MainPath, OpenSceneMode.Single);
            var mainController = new GameObject("MainSceneRoot").AddComponent<MainSceneController>();
            Camera camera = null;
            foreach (var root in main.GetRootGameObjects())
            {
                var candidate = root.GetComponentInChildren<Camera>();
                if (candidate != null)
                {
                    camera = candidate;
                }
            }

            SetReference(mainController, "_worldCamera", camera);
            EditorSceneManager.SaveScene(main);

            var hud = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            new GameObject("HudSceneRoot").AddComponent<HudSceneRoot>();
            EditorSceneManager.SaveScene(hud, HudPath);

            var intro = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var bootstrap = new GameObject("SceneFlow").AddComponent<SceneFlowController>();
            SetReference(bootstrap, "_configuration", AssetDatabase.LoadAssetAtPath<SceneFlowSO>(ConfigurationPath));
            EditorSceneManager.SaveScene(intro, IntroPath);

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(IntroPath, true),
                new EditorBuildSettingsScene(MainPath, true),
                new EditorBuildSettingsScene(HudPath, true)
            };
            EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(IntroPath);
            AssetDatabase.SaveAssets();
            Debug.Log("ZooWorld iteration 1 scenes and explicit startup references created.");
        }

        private static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                var separator = path.LastIndexOf('/');
                AssetDatabase.CreateFolder(path.Substring(0, separator), path.Substring(separator + 1));
            }
        }

        private static void SetReference(UnityEngine.Object target, string field, UnityEngine.Object value)
        {
            if (value == null)
            {
                throw new InvalidOperationException($"Required reference '{field}' is missing on '{target.name}'.");
            }

            var serialized = new SerializedObject(target);
            serialized.FindProperty(field).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
