using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Shown when the dream timer runs out: she jolts awake from a nightmare, back in her empty room
// (Assets/Resources/wake_room.jpg), thinks it over, and Space / Enter / click restarts the dream.
public class WakeUpScreen : MonoBehaviour
{
    static readonly string[] Lines =
    {
        "*gasp*",
        "...It was just a dream.",
        "No. A nightmare. Everything kept slipping away, and I couldn't hold on to any of it.",
        "The room is still empty. The boxes are still packed.",
        "Maybe I'm not ready to let go yet.",
    };

    RawImage room;
    Image black;
    DialogueBox box;

    public static void Show()
    {
        if (FindFirstObjectByType<WakeUpScreen>() == null) new GameObject("Wake Up Screen").AddComponent<WakeUpScreen>();
    }

    void Start()
    {
        Canvas canvas = new GameObject("Wake Canvas", typeof(Canvas), typeof(CanvasScaler)).GetComponent<Canvas>();
        canvas.transform.SetParent(transform, false);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 950; // above the level's UI, below the text box
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        room = Full(canvas.transform, "Room").gameObject.AddComponent<RawImage>();
        room.texture = Resources.Load<Texture2D>("wake_room");
        room.color = new Color(1f, 1f, 1f, 0f);
        AspectRatioFitter fit = room.gameObject.AddComponent<AspectRatioFitter>(); // fill the screen, crop the rest
        fit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        if (room.texture != null) fit.aspectRatio = (float)room.texture.width / room.texture.height;
        black = Full(canvas.transform, "Black").gameObject.AddComponent<Image>();
        black.color = new Color(0f, 0f, 0f, 0f);

        box = DialogueBox.Create(transform, new Vector2(0.1f, 0.04f), new Vector2(0.9f, 0.22f), 44f, 999);
        StartCoroutine(Play());
    }

    static RectTransform Full(Transform parent, string name)
    {
        RectTransform r = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        r.SetParent(parent, false);
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.offsetMin = r.offsetMax = Vector2.zero;
        return r;
    }

    static bool Pressed() =>
        Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0);

    IEnumerator Play()
    {
        // The dream cuts to black.
        for (float t = 0f; t < 1f; t += Time.deltaTime / 0.5f)
        {
            black.color = new Color(0f, 0f, 0f, t);
            yield return null;
        }
        black.color = Color.black;
        room.color = Color.white;
        yield return new WaitForSeconds(0.8f);

        // Eyes snap open on her room, with a jolt.
        RectTransform r = room.rectTransform;
        for (float t = 0f; t < 1f; t += Time.deltaTime / 0.6f)
        {
            black.color = new Color(0f, 0f, 0f, 1f - t);
            float shake = 30f * (1f - t);
            r.anchoredPosition = new Vector2(Random.Range(-shake, shake), Random.Range(-shake, shake));
            yield return null;
        }
        r.anchoredPosition = Vector2.zero;
        black.color = Color.clear;

        yield return box.Say(Lines, 32f, 2.2f, true);

        box.Show("Press Space to try again.");
        yield return null;
        while (!Pressed()) yield return null;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
