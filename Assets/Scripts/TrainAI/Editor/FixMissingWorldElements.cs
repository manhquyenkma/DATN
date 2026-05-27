using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using TrainAI.Presentation;

namespace TrainAI.Editor
{
    public static class FixMissingWorldElements
    {
        [MenuItem("Tools/Build Game/8. Fix Missing World Elements (Area & NPC)")]
        public static void Fix()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/TrainAI/10_World.unity", OpenSceneMode.Single);
            
            // Add Area_LopHoc_Door
            AddArea("Area_LopHoc_Door", "Assets/_Data/Interactables/Interactable_LopHoc_Door.asset", new Vector3( 12, 0,  42), new Vector3(2, 2, 2));
            // Add Area_KTX_Door
            AddArea("Area_KTX_Door", "Assets/_Data/Interactables/Interactable_KTX_Door.asset", new Vector3(-30, 0,  35), new Vector3(2, 2, 2));
            // Add Area_NhaAn_Door
            AddArea("Area_NhaAn_Door", "Assets/_Data/Interactables/Interactable_NhaAn_Door.asset", new Vector3(-55, 0, -22), new Vector3(2, 2, 2));

            // Wait, NpcStagedSpawner now automatically adds dialogue interaction for DaiDoiTruong
            // But we must make sure NpcStagedSpawner is in the scene and has DaiDoiTruong
            FixNpcSpawner();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[FixMissingWorldElements] Fixed missing elements in 10_World!");
        }

        static void AddArea(string name, string soPath, Vector3 pos, Vector3 scale)
        {
            var existing = GameObject.Find(name);
            if (existing != null) return;

            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.position = pos;
            go.transform.localScale = scale;
            
            var box = go.GetComponent<BoxCollider>();
            box.isTrigger = true;
            
            var marker = go.AddComponent<InteractableMarker>();
            var so = AssetDatabase.LoadAssetAtPath<TrainAI.SO.Base.InteractableSO>(soPath);
            if (so != null)
            {
                var soObj = new SerializedObject(marker);
                var prop = soObj.FindProperty("interactable");
                if (prop != null)
                {
                    prop.objectReferenceValue = so;
                    soObj.ApplyModifiedPropertiesWithoutUndo();
                }
            }
            else
            {
                Debug.LogWarning($"[FixMissingWorldElements] Missing SO: {soPath}");
            }
            
            // Set semi-transparent yellow material just so it's visible but not ugly
            var renderer = go.GetComponent<Renderer>();
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            mat.color = new Color(1f, 0.8f, 0.2f, 0.5f);
            // Make it transparent
            mat.SetFloat("_Mode", 3);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
            renderer.sharedMaterial = mat;
        }

        static void FixNpcSpawner()
        {
            var existingNpc = GameObject.Find("NPC_DaiDoiTruong");
            if (existingNpc == null)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/TrainAI/NPC.prefab");
                if (prefab != null)
                {
                    existingNpc = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                    existingNpc.name = "NPC_DaiDoiTruong";
                    existingNpc.transform.position = new Vector3(78, 0, -22); // Cong Chinh
                    existingNpc.transform.rotation = Quaternion.Euler(0, 180, 0);

                    // Attach missing components for interaction since it won't go through NpcStagedSpawner
                    var marker = existingNpc.AddComponent<InteractableMarker>();
                    
                    var interSO = ScriptableObject.CreateInstance<TrainAI.SO.Base.InteractableSO>();
                    interSO.id = "Talk_DaiDoiTruong";
                    interSO.bypassQuestGate = true;
                    
                    var dialogueSO = ScriptableObject.CreateInstance<TrainAI.SO.Concrete.OpenDialogueInteractionSO>();
                    var npcDef = AssetDatabase.LoadAssetAtPath<TrainAI.SO.Base.NPCSO>("Assets/_Data/NPCs/NPC_DaiDoiTruong.asset");
                    dialogueSO.npc = npcDef;
                    dialogueSO.promptText = "Bam E de noi chuyen";
                    interSO.onInteract = dialogueSO;
                    
                    marker.interactable = interSO;
                    
                    var bc = existingNpc.GetComponent<BoxCollider>();
                    if (bc == null) bc = existingNpc.AddComponent<BoxCollider>();
                    bc.isTrigger = true;
                    bc.size = new Vector3(2, 2, 2);
                    bc.center = new Vector3(0, 1, 0);

                    var view = existingNpc.GetComponent<NpcView>();
                    if (view == null) view = existingNpc.AddComponent<NpcView>();
                    var svc = AssetDatabase.LoadAssetAtPath<TrainAI.Services.ServiceLocatorSO>("Assets/_Data/Config/ServiceLocator.asset");
                    view.Configure(npcDef, svc);
                }
            }

            // Remove from spawner to prevent duplicates
            var spawnerGo = GameObject.Find("_NPCSpawner");
            if (spawnerGo != null)
            {
                var spawner = spawnerGo.GetComponent<NpcStagedSpawner>();
                if (spawner != null)
                {
                    var so = new SerializedObject(spawner);
                    var entries = so.FindProperty("entries");
                    if (entries != null)
                    {
                        for (int i = entries.arraySize - 1; i >= 0; i--)
                        {
                            var label = entries.GetArrayElementAtIndex(i).FindPropertyRelative("label").stringValue;
                            if (label.Contains("DaiDoi"))
                            {
                                entries.DeleteArrayElementAtIndex(i);
                            }
                        }
                    }
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
            }
        }
    }
}
