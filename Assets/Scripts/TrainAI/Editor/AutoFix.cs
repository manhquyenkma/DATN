using UnityEditor;
using UnityEngine;

namespace TrainAI.Editor
{
    [InitializeOnLoad]
    public static class AutoFixMissingWorldElements
    {
        const string PrefKey = "AutoFixWorldElements_Ran_v3";

        static AutoFixMissingWorldElements()
        {
            if (EditorPrefs.GetBool(PrefKey, false)) return;
            
            EditorApplication.delayCall += () => 
            {
                if (EditorPrefs.GetBool(PrefKey, false)) return;
                EditorPrefs.SetBool(PrefKey, true);
                
                FixMissingWorldElements.Fix();
                Debug.Log("[AutoFix] Ran missing elements fix automatically.");
            };
        }
    }
}
