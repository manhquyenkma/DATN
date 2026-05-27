using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TrainAI.Presentation;

namespace TrainAI.Editor
{
    public static class AutoFixLighting
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
                    FixLightingInScene(scene);
                }
            };
        }

        static void FixLightingInScene(UnityEngine.SceneManagement.Scene scene)
        {
            var lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            Light dirLight = null;
            foreach (var l in lights)
            {
                if (l.type == LightType.Directional && l.gameObject.scene == scene)
                {
                    dirLight = l;
                    break;
                }
            }

            if (dirLight != null)
            {
                var comp = dirLight.GetComponent<DayNightLighting>();
                if (comp == null)
                {
                    comp = dirLight.gameObject.AddComponent<DayNightLighting>();
                    
                    var so = AssetDatabase.LoadAssetAtPath<TrainAI.Services.ServiceLocatorSO>("Assets/_Data/Config/ServiceLocator.asset");
                    
                    comp.services = so;
                    
                    comp.lightColor = new Gradient();
                    comp.lightColor.SetKeys(
                        new GradientColorKey[] {
                            new GradientColorKey(new Color(0.1f, 0.1f, 0.2f), 0.0f),
                            new GradientColorKey(new Color(0.1f, 0.1f, 0.2f), 0.2f),
                            new GradientColorKey(new Color(1.0f, 0.6f, 0.4f), 0.25f),
                            new GradientColorKey(new Color(1.0f, 0.9f, 0.8f), 0.4f),
                            new GradientColorKey(new Color(1.0f, 1.0f, 1.0f), 0.5f),
                            new GradientColorKey(new Color(1.0f, 0.9f, 0.8f), 0.6f),
                            new GradientColorKey(new Color(1.0f, 0.5f, 0.3f), 0.75f),
                            new GradientColorKey(new Color(0.1f, 0.1f, 0.2f), 0.8f),
                            new GradientColorKey(new Color(0.1f, 0.1f, 0.2f), 1.0f)
                        },
                        new GradientAlphaKey[] {
                            new GradientAlphaKey(1f, 0f),
                            new GradientAlphaKey(1f, 1f)
                        }
                    );
                    
                    comp.lightIntensity = new AnimationCurve(
                        new Keyframe(0.0f, 0.2f),
                        new Keyframe(0.2f, 0.2f),
                        new Keyframe(0.25f, 0.8f),
                        new Keyframe(0.5f, 1.2f),
                        new Keyframe(0.75f, 0.8f),
                        new Keyframe(0.8f, 0.2f),
                        new Keyframe(1.0f, 0.2f)
                    );

                    EditorUtility.SetDirty(comp);
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                    Debug.Log("[AutoFixLighting] Added DayNightLighting to Directional Light and saved scene.");
                }
            }
        }
    }
}
