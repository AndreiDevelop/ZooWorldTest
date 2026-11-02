using System;
using UnityEditor;
using UnityEngine;
using ZooWorld.Animals;

namespace ZooWorld.EditorTools
{
    public static class IterationThreeSetup
    {
        private const string BasePath = "Assets/Prefabs/Animals/Animal.prefab";

        [MenuItem("ZooWorld/Setup/Install Food Chain Contacts")]
        public static void InstallContacts()
        {
            if (EditorApplication.isPlaying)
            {
                throw new InvalidOperationException("Install prefab contacts outside Play Mode.");
            }

            var root = PrefabUtility.LoadPrefabContents(BasePath);
            try
            {
                var adapter = root.GetComponent<AnimalContactAdapter>();
                if (adapter == null)
                {
                    adapter = root.AddComponent<AnimalContactAdapter>();
                }

                var data = new SerializedObject(adapter);
                data.FindProperty("_view").objectReferenceValue = root.GetComponent<AnimalView>();
                data.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(root, BasePath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }

            AssetDatabase.SaveAssets();
            Debug.Log("ZooWorld contact adapter installed on the shared animal base prefab.");
        }
    }
}
