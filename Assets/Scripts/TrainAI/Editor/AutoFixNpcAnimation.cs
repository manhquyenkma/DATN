using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TrainAI.Presentation;

namespace TrainAI.Editor
{
    public static class AutoFixNpcAnimation
    {
        [MenuItem("Tools/Fix NPC Animation")]
        static void FixNpcPrefab()
        {
            string npcPath = "Assets/Prefabs/TrainAI/NPC.prefab";
            string playerPath = "Assets/Prefabs/TrainAI/Player.prefab";

            var npcPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(npcPath);
            var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(playerPath);
            
            if (npcPrefab == null || playerPrefab == null)
            {
                Debug.LogError("Could not find NPC or Player prefab.");
                return;
            }

            using (var editingScope = new PrefabUtility.EditPrefabContentsScope(npcPath))
            {
                var root = editingScope.prefabContentsRoot;
                
                var anim = root.GetComponentInChildren<Animator>();
                var playerAnim = playerPrefab.GetComponentInChildren<Animator>();
                
                if (anim != null && playerAnim != null)
                {
                    // Assign the player's animator controller so the NPC knows how to walk
                    anim.runtimeAnimatorController = playerAnim.runtimeAnimatorController;

                    // Disable root motion so it doesn't fight with NavMeshAgent causing jitter.
                    anim.applyRootMotion = false;

                    // Ensure driver exists
                    if (root.GetComponent<NpcAnimatorDriver>() == null)
                    {
                        root.AddComponent<NpcAnimatorDriver>();
                    }
                }
            }
            Debug.Log("[AutoFixNpcAnimation] Fixed NPC.prefab to have Animator and visual model!");
        }
        
        [InitializeOnLoadMethod]
        static void AutoRun()
        {
            EditorApplication.delayCall += () =>
            {
                if (Application.isPlaying) return;
                FixNpcPrefab();
            };
        }
    }
}
