using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

// CutieHack > Build Intro Scene: makes Assets/Scenes/Intro.unity, the prologue in her room at night
// (Assets/Art/game_start_room_frame.jpg), and puts it first in the build list so the game starts there.
// IntroCutscene plays the monologue, the sleepy blinks, and the flicker into GameLevels.
public static class IntroBuilder
{
    const string ScenePath = "Assets/Scenes/Intro.unity";

    [MenuItem("CutieHack/Build Intro Scene")]
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
        cam.backgroundColor = new Color32(18, 20, 48, 255); // navy, past the picture's edges on wide screens
        camGo.transform.position = new Vector3(0f, 0f, -10f);
        camGo.AddComponent<AudioListener>();
        cam.GetUniversalAdditionalCameraData().renderPostProcessing = true; // for the sleepy blur

        // The room, dimmed to moonlit navy and dusty purple. The lamp pool is added at runtime.
        Sprite room = Level1Builder.LoadSprite("Assets/Art/game_start_room_frame.jpg", 768f / 10f);
        if (room != null)
        {
            SpriteRenderer sr = new GameObject("Room").AddComponent<SpriteRenderer>();
            sr.sprite = room;
            sr.color = new Color32(150, 145, 190, 255);
        }

        new GameObject("Intro").AddComponent<IntroCutscene>();

        EditorSceneManager.SaveScene(scene, ScenePath);
        List<EditorBuildSettingsScene> list = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        list.RemoveAll(s => s.path == ScenePath);
        list.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
        EditorBuildSettings.scenes = list.ToArray();
        Debug.Log("Intro scene saved to " + ScenePath + " and set as the first scene in the build.");
    }
}
