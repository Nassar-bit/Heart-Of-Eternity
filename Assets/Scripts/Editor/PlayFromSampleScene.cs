using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class PlayFromSampleScene
{
    private const string SampleScenePath = "Assets/Scenes/SampleScene.unity"; // apna actual path check kar lena

    static PlayFromSampleScene()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                if (EditorSceneManager.GetActiveScene().path != SampleScenePath)
                {
                    EditorSceneManager.OpenScene(SampleScenePath);
                }
            }
            else
            {
                EditorApplication.isPlaying = false;
            }
        }
    }
}