using UnityEditor;
using TrainAI.Services;
using Unity.InferenceEngine;
using TrainAI.SO.Base;
using UnityEngine;

namespace TrainAI.Editor
{
    public static class AutoFixServiceLocator
    {
        [InitializeOnLoadMethod]
        public static void Fix()
        {
            var locator = AssetDatabase.LoadAssetAtPath<ServiceLocatorSO>("Assets/_Data/Config/ServiceLocator.asset");
            if (locator == null) return;
            
            bool changed = false;
            
            if (locator.intentModel == null)
            {
                locator.intentModel = AssetDatabase.LoadAssetAtPath<ModelAsset>("Assets/_Models/intent_classifier.onnx");
                changed = true;
            }
            if (locator.soldierModel == null)
            {
                locator.soldierModel = AssetDatabase.LoadAssetAtPath<ModelAsset>("Assets/_Models/soldier.onnx");
                changed = true;
            }
            if (locator.intentMetaJson == null)
            {
                locator.intentMetaJson = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/_Models/intent_classifier_meta.json");
                changed = true;
            }
            if (locator.responsesJson == null)
            {
                locator.responsesJson = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/_Models/responses.json");
                changed = true;
            }
            
            if (changed)
            {
                EditorUtility.SetDirty(locator);
                AssetDatabase.SaveAssets();
                Debug.Log("[AutoFix] Fixed ServiceLocator models/templates");
            }
            
            FixNpcDialogues();
        }

        static void FixNpcDialogues()
        {
            var keywordDialogue = AssetDatabase.LoadAssetAtPath<TrainAI.SO.Concrete.KeywordDialogueSO>("Assets/_Data/Config/Dialogue_Keyword.asset");
            if (keywordDialogue == null)
            {
                keywordDialogue = ScriptableObject.CreateInstance<TrainAI.SO.Concrete.KeywordDialogueSO>();
                AssetDatabase.CreateAsset(keywordDialogue, "Assets/_Data/Config/Dialogue_Keyword.asset");
                AssetDatabase.SaveAssets();
            }

            string[] guids = AssetDatabase.FindAssets("t:NPCSO", new[] { "Assets/_Data/NPCs" });
            bool npcChanged = false;
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var npc = AssetDatabase.LoadAssetAtPath<TrainAI.SO.Base.NPCSO>(path);
                if (npc != null && npc.dialogue != keywordDialogue)
                {
                    npc.dialogue = keywordDialogue;
                    EditorUtility.SetDirty(npc);
                    npcChanged = true;
                }
            }

            if (npcChanged)
            {
                AssetDatabase.SaveAssets();
                Debug.Log("[AutoFix] Assigned KeywordDialogueSO to all NPCs!");
            }
        }
    }
}
