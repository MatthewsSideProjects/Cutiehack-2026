using System.Collections;
using UnityEngine;

// Prologue in her room at night (Intro scene, built by CutieHack > Build Intro Scene).
// Her eyes open on the room, she thinks out loud about moving out (Space / Enter / click skips),
// then gets sleepy and EyeTransition carries her into the dream (GameLevels).
public class IntroCutscene : MonoBehaviour
{
    public string nextScene = "GameLevels";
    public float charsPerSecond = 32f;
    public float linePause = 2.2f; // seconds a finished line stays before moving on by itself
    public Vector2 lampPosition = new Vector2(3.8f, -1.2f); // warm pool of lamplight over the bed
    public float lampSize = 7f;
    public float cricketVolume = 0.4f; // Assets/Resources/cricketIntro.mp3, looped quietly under the scene
    [TextArea]
    public string[] lines =
    {
        "Three more weeks. Then I move out for school.",
        "Everything I own fits in a few boxes now.",
        "The room is so empty... I didn't know it could echo like this.",
        "What if I don't like it there? What if I miss home too much?",
        "It's kind of scary, honestly.",
        "...",
        "*yawn* I'll worry about it tomorrow.",
    };

    IEnumerator Start()
    {
        MakeLampPool();
        AudioSource crickets = gameObject.AddComponent<AudioSource>();
        crickets.clip = Resources.Load<AudioClip>("cricketIntro");
        crickets.loop = true;
        crickets.volume = cricketVolume;
        if (crickets.clip != null) crickets.Play();
        DialogueBox box = DialogueBox.Create(transform, new Vector2(0.1f, 0.04f), new Vector2(0.9f, 0.22f), 44f, 999);
        EyeTransition eyes = EyeTransition.Create(1f);
        yield return eyes.Lids(0f, 1f); // open on her room
        yield return box.Say(lines, charsPerSecond, linePause, true);
        eyes.GoTo(nextScene);
    }

    // Soft round lamp-yellow glow drawn over the room art (which is otherwise moonlit navy).
    void MakeLampPool()
    {
        const int n = 64;
        Texture2D tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
        Color lamp = DialogueBox.LampYellow;
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(n / 2f, n / 2f)) / (n / 2f);
                float a = Mathf.Clamp01(1f - d);
                tex.SetPixel(x, y, new Color(lamp.r, lamp.g, lamp.b, a * a * 0.45f));
            }
        tex.Apply();
        GameObject go = new GameObject("Lamp pool");
        go.transform.position = new Vector3(lampPosition.x, lampPosition.y, -1f);
        go.transform.localScale = Vector3.one * lampSize;
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Sprite.Create(tex, new UnityEngine.Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
        sr.sortingOrder = 10;
    }
}
