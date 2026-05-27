using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TrainAI.Presentation;

namespace TrainAI.Editor
{
    public static class AutoFixCrowd
    {
        [InitializeOnLoadMethod]
        static void RunFix()
        {
            EditorApplication.delayCall += () =>
            {
                if (Application.isPlaying) return;
                
                var scene = EditorSceneManager.GetSceneByPath("Assets/Scenes/TrainAI/10_World.unity");
                if (scene.IsValid() && scene.isLoaded)
                {
                    FixCrowdInScene(scene);
                }
            };
        }

        static void FixCrowdInScene(UnityEngine.SceneManagement.Scene scene)
        {
            var existing = GameObject.Find("CrowdSpawner");
            if (existing != null) return;

            var go = new GameObject("CrowdSpawner");
            var comp = go.AddComponent<CrowdSpawner>();
            
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/TrainAI/NPC.prefab");
            comp.npcPrefab = prefab;
            comp.spawnCount = 20;

            EditorUtility.SetDirty(comp);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[AutoFixCrowd] Added 20 NPC CrowdSpawner to scene.");
        }
    }
}
