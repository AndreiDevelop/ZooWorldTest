using System;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using ZooWorld.Infrastructure;
using ZooWorld.UI;
using Object = UnityEngine.Object;

namespace ZooWorld.EditorTools
{
    public static class IterationFourSetup
    {
        private const string PrefabFolder = "Assets/Prefabs/UI";
        private const string SettingsPath = "Assets/SO/UI/HudPresentation.asset";

        [MenuItem("ZooWorld/Setup/Create Iteration 4 HUD")]
        public static void Create()
        {
            if (EditorApplication.isPlaying || EditorSceneManager.GetActiveScene().isDirty)
            {
                throw new InvalidOperationException("Save the current scene and leave Play Mode before HUD setup.");
            }

            if (AssetDatabase.LoadAssetAtPath<HudPresentationSO>(SettingsPath) != null)
            {
                throw new InvalidOperationException("HUD content already exists; edit it in place.");
            }

            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
                "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
            if (font == null)
            {
                throw new InvalidOperationException("Import vendor TMP Essential Resources non-interactively before HUD setup.");
            }

            EnsureFolder("Assets/Prefabs", "UI");
            EnsureFolder("Assets/SO", "UI");
            var settings = ScriptableObject.CreateInstance<HudPresentationSO>();
            AssetDatabase.CreateAsset(settings, SettingsPath);
            var counterPrefab = CreateCounters(font);
            var labelPrefab = CreateLabel(font);
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/HUD.unity");
            HudSceneRoot root = null;
            foreach (var item in scene.GetRootGameObjects())
            {
                if (item.TryGetComponent<HudSceneRoot>(out var candidate))
                {
                    root = candidate;
                }
            }

            if (root == null)
            {
                throw new InvalidOperationException("HUD requires its existing HudSceneRoot.");
            }

            var countersCanvas = CreateCanvas("CountersCanvas", root.transform, 10);
            var counterObject = (GameObject)PrefabUtility.InstantiatePrefab(counterPrefab, countersCanvas.transform);
            var counterRect = (RectTransform)counterObject.transform;
            counterRect.anchorMin = Vector2.one;
            counterRect.anchorMax = Vector2.one;
            counterRect.pivot = Vector2.one;
            counterRect.anchoredPosition = new Vector2(-24f, -24f);
            var feedbackCanvas = CreateCanvas("FeedbackCanvas", root.transform, 20);
            var feedbackRoot = Rect("Feedback", feedbackCanvas.transform, Vector2.zero, Vector2.one, Vector2.zero);
            feedbackRoot.offsetMin = Vector2.zero;
            feedbackRoot.offsetMax = Vector2.zero;
            var data = new SerializedObject(root);
            data.FindProperty("_counters").objectReferenceValue = counterObject.GetComponent<DeathCountersView>();
            data.FindProperty("_settings").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<HudPresentationSO>(SettingsPath);
            data.FindProperty("_labelPrefab").objectReferenceValue = labelPrefab.GetComponent<TastyLabelView>();
            data.FindProperty("_feedbackRoot").objectReferenceValue = feedbackRoot;
            data.ApplyModifiedPropertiesWithoutUndo();
            root.ValidateConfiguration();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene("Assets/Scenes/Intro.unity");
            Debug.Log("Iteration 4 HUD saved: separate counter/feedback canvases, TMP prefabs and presentation SO.");
        }

        private static GameObject CreateCounters(TMP_FontAsset font)
        {
            var rect = Rect("DeathCounters", null, Vector2.one, Vector2.one, new Vector2(264f, 116f));
            var image = rect.gameObject.AddComponent<Image>();
            image.color = new Color(0.035f, 0.075f, 0.085f, 0.92f);
            image.raycastTarget = false;
            var heading = Text("Heading", rect, font, "DEATHS", 14f, new Color(0.65f, 0.76f, 0.72f));
            SetRow(heading.rectTransform, -16f, 20f);
            var prey = Text("DeadPrey", rect, font, "Dead prey: 0", 22f, new Color(0.76f, 1f, 0.73f));
            SetRow(prey.rectTransform, -43f, 28f);
            var predators = Text("DeadPredators", rect, font, "Dead predators: 0", 22f, new Color(1f, 0.84f, 0.52f));
            SetRow(predators.rectTransform, -76f, 28f);
            var view = rect.gameObject.AddComponent<DeathCountersView>();
            var data = new SerializedObject(view);
            data.FindProperty("_prey").objectReferenceValue = prey;
            data.FindProperty("_predators").objectReferenceValue = predators;
            data.ApplyModifiedPropertiesWithoutUndo();
            var prefab = PrefabUtility.SaveAsPrefabAsset(rect.gameObject, PrefabFolder + "/DeathCounters.prefab");
            Object.DestroyImmediate(rect.gameObject);
            return prefab;
        }

        private static GameObject CreateLabel(TMP_FontAsset font)
        {
            var rect = Rect("TastyLabel", null, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(116f, 34f));
            var text = Text("Text", rect, font, "Tasty!", 24f, new Color(1f, 0.94f, 0.56f));
            text.alignment = TextAlignmentOptions.Center;
            text.fontStyle = FontStyles.Bold;
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = Vector2.zero;
            text.rectTransform.offsetMax = Vector2.zero;
            var shadow = text.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0.02f, 0.04f, 0.04f, 1f);
            shadow.effectDistance = new Vector2(1.5f, -1.5f);
            var group = rect.gameObject.AddComponent<CanvasGroup>();
            group.interactable = false;
            group.blocksRaycasts = false;
            var view = rect.gameObject.AddComponent<TastyLabelView>();
            var data = new SerializedObject(view);
            data.FindProperty("_rect").objectReferenceValue = rect;
            data.FindProperty("_text").objectReferenceValue = text;
            data.FindProperty("_group").objectReferenceValue = group;
            data.ApplyModifiedPropertiesWithoutUndo();
            rect.gameObject.SetActive(false);
            var prefab = PrefabUtility.SaveAsPrefabAsset(rect.gameObject, PrefabFolder + "/TastyLabel.prefab");
            Object.DestroyImmediate(rect.gameObject);
            return prefab;
        }

        private static Canvas CreateCanvas(string name, Transform parent, int sortingOrder)
        {
            var rect = Rect(name, parent, Vector2.zero, Vector2.one, Vector2.zero);
            var canvas = rect.gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            var scaler = rect.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        private static RectTransform Rect(string name, Transform parent, Vector2 minimum, Vector2 maximum, Vector2 size)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = minimum;
            rect.anchorMax = maximum;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            return rect;
        }

        private static TextMeshProUGUI Text(string name, Transform parent, TMP_FontAsset font,
            string content, float size, Color color)
        {
            var rect = Rect(name, parent, Vector2.zero, Vector2.one, Vector2.zero);
            var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.font = font;
            text.text = content;
            text.fontSize = size;
            text.color = color;
            text.raycastTarget = false;
            text.alignment = TextAlignmentOptions.MidlineLeft;
            return text;
        }

        private static void SetRow(RectTransform rect, float top, float height)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(-32f, height);
            rect.anchoredPosition = new Vector2(0f, top);
        }

        private static void EnsureFolder(string parent, string name)
        {
            if (!AssetDatabase.IsValidFolder(parent + "/" + name))
            {
                AssetDatabase.CreateFolder(parent, name);
            }
        }
    }
}
