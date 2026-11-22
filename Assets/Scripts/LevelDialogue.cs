using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// A little text box when she first arrives in each dream level, saying where she is.
// Created by itself in any scene with a player (no setup), and finds the level from her x:
// Level 1 below 1000, Level 2 from 1000, Level 3 from 2000. It plays along as she moves; nothing pauses.
public class LevelDialogue : MonoBehaviour
{
    static readonly string[][] Lines =
    {
        new[] { // Level 1: the garden
            "This garden... it's the one at home. Only everything is huge.",
            "I spent every summer out here.",
            "Maybe there's something here worth keeping.",
        },
        new[] { // Level 2: the birthday table
            "A birthday table? This is my party from years ago.",
            "Everyone I love was here that day.",
            "Careful, the candles are still lit.",
        },
        new[] { // Level 3: the empty house
            "It's my house... but everything's gone.",
            "It looks like it did on packing day.",
            "Something's coming out of the dark. Don't stop.",
        },
    };

    BaseMovement player;
    DialogueBox box;
    readonly bool[] shown = new bool[3];
    int current = -1;
    Coroutine talking;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Hook()
    {
        SceneManager.sceneLoaded += (scene, mode) =>
        {
            if (FindFirstObjectByType<BaseMovement>() != null)
                new GameObject("Level Dialogue").AddComponent<LevelDialogue>();
        };
    }

    void Start()
    {
        player = FindFirstObjectByType<BaseMovement>();
        box = DialogueBox.Create(transform, new Vector2(0.2f, 0.05f), new Vector2(0.8f, 0.17f), 34f, 900);
    }

    void Update()
    {
        if (player == null || EyeTransition.Playing) return; // waits until her eyes are open
        float x = player.transform.position.x;
        int level = x < 1000f ? 0 : x < 2000f ? 1 : 2;
        if (level == current) return;
        current = level;
        if (shown[level]) return;
        shown[level] = true;
        if (talking != null) StopCoroutine(talking);
        talking = StartCoroutine(Say(level));
    }

    IEnumerator Say(int level)
    {
        yield return new WaitForSeconds(level == 0 ? 0.3f : 1.2f); // after the door's fade-in
        yield return box.Say(Lines[level], 30f, 2.4f, false);
    }
}
