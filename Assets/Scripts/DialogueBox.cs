using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// A navy text box with a lamp-yellow edge that types lines out one by one.
// Used by the intro monologue and the little box at the start of each dream level.
public class DialogueBox : MonoBehaviour
{
    public static readonly Color Navy = new Color32(18, 20, 48, 255);
    public static readonly Color LampYellow = new Color32(255, 214, 120, 255);

    TextMeshProUGUI text;
    CanvasGroup group;
    AudioSource typing; // Assets/Resources/textType.mp3, looped while letters appear

    // Makes a screen-space canvas (sorted above most UI) holding one box between the given anchors.
    public static DialogueBox Create(Transform parent, Vector2 anchorMin, Vector2 anchorMax, float fontSize, int sortingOrder)
    {
        Canvas canvas = new GameObject("Dialogue Canvas", typeof(Canvas), typeof(CanvasScaler)).GetComponent<Canvas>();
        canvas.transform.SetParent(parent, false);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        RectTransform box = new GameObject("Dialogue", typeof(RectTransform)).GetComponent<RectTransform>();
        box.SetParent(canvas.transform, false);
        box.anchorMin = anchorMin;
        box.anchorMax = anchorMax;
        box.offsetMin = box.offsetMax = Vector2.zero;
        Image bg = box.gameObject.AddComponent<Image>();
        bg.color = new Color(Navy.r, Navy.g, Navy.b, 0.85f);
        bg.raycastTarget = false;
        Outline edge = box.gameObject.AddComponent<Outline>();
        edge.effectColor = LampYellow;
        edge.effectDistance = new Vector2(3f, -3f);

        RectTransform tr = new GameObject("Text", typeof(RectTransform)).GetComponent<RectTransform>();
        tr.SetParent(box, false);
        tr.anchorMin = Vector2.zero;
        tr.anchorMax = Vector2.one;
        tr.offsetMin = new Vector2(36f, 18f);
        tr.offsetMax = new Vector2(-36f, -18f);

        DialogueBox d = box.gameObject.AddComponent<DialogueBox>();
        d.group = box.gameObject.AddComponent<CanvasGroup>();
        d.group.alpha = 0f;
        d.text = tr.gameObject.AddComponent<TextMeshProUGUI>();
        d.text.fontSize = fontSize;
        d.text.color = new Color(1f, 0.95f, 0.85f);
        d.text.alignment = TextAlignmentOptions.MidlineLeft;
        d.text.raycastTarget = false;
        d.text.text = "";
        d.typing = box.gameObject.AddComponent<AudioSource>();
        d.typing.clip = Resources.Load<AudioClip>("textType");
        d.typing.loop = true;
        d.typing.playOnAwake = false;
        return d;
    }

    static bool Pressed() =>
        Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0);

    // Fades in, types each line, fades out. With skippable, Space / Enter / click finishes or skips a line.
    public IEnumerator Say(string[] lines, float charsPerSecond, float linePause, bool skippable)
    {
        typing.Stop(); // in case a previous Say was cut off mid-line
        yield return Fade(0f, 1f);
        foreach (string line in lines)
        {
            text.text = line;
            text.maxVisibleCharacters = 0;
            yield return null; // the key that ended the last line doesn't skip this one
            if (typing.clip != null) typing.Play();
            for (float shown = 0f; shown < line.Length; shown += Time.deltaTime * charsPerSecond)
            {
                if (skippable && Pressed()) break;
                text.maxVisibleCharacters = (int)shown;
                yield return null;
            }
            text.maxVisibleCharacters = line.Length;
            typing.Stop();
            yield return null;
            for (float t = 0f; t < linePause && !(skippable && Pressed()); t += Time.deltaTime) yield return null;
        }
        yield return Fade(1f, 0f);
    }

    // Shows one line at once and leaves it up (a prompt).
    public void Show(string line)
    {
        text.text = line;
        text.maxVisibleCharacters = line.Length;
        StartCoroutine(Fade(group.alpha, 1f));
    }

    IEnumerator Fade(float from, float to)
    {
        for (float t = 0f; t < 1f; t += Time.deltaTime / 0.35f)
        {
            group.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }
        group.alpha = to;
    }
}
