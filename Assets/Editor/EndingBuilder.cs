using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// CutieHack > Build Ending Scene: makes Assets/Scenes/Ending.unity, the scene the last door of Level 3
// wakes her up into: her room at sunrise (Assets/Art/frame_4.jpg), with EndingScene's closing lines.
// It also puts GameLevels and Ending in the build list, which SceneManager.LoadScene needs.
public static class EndingBuilder
{
    const string ScenePath = "Assets/Scenes/Ending.unity";
    const string LevelsPath = "Assets/Scenes/GameLevels.unity";

    [MenuItem("CutieHack/Build Ending Scene")]
    public static void Build()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Flat camera framing the picture: 10 units tall.
        GameObject camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        Camera cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color32(240, 190, 170, 255); // peach, past the picture's edges on wide screens
        camGo.transform.position = new Vector3(0f, 0f, -10f);
        camGo.AddComponent<AudioListener>();

        Sprite room = Level1Builder.LoadSprite("Assets/Art/frame_4.jpg", 768f / 10f);
        if (room != null) new GameObject("Room").AddComponent<SpriteRenderer>().sprite = room;

        new GameObject("Ending").AddComponent<EndingScene>();

        EditorSceneManager.SaveScene(scene, ScenePath);
        List<EditorBuildSettingsScene> list = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        foreach (string p in new[] { LevelsPath, ScenePath })
            if (!list.Exists(s => s.path == p)) list.Add(new EditorBuildSettingsScene(p, true));
        EditorBuildSettings.scenes = list.ToArray();
        Debug.Log("Ending scene saved to " + ScenePath + " and added to the build list with GameLevels.");
    }
}
