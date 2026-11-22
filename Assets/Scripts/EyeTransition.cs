using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Her eyes, as black lids over the whole screen. Used for falling asleep into the dream (Intro)
// and for waking up out of it (the last door into the Ending). The object survives scene loads,
// so GoTo can close the eyes in one scene and flicker them open in the next.
public class EyeTransition : MonoBehaviour
{
    public static bool Playing; // true from GoTo until the eyes are fully open again
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetPlaying() => Playing = false; // in case play mode was stopped mid-transition

    Image top, bottom, flash;
    float shut; // 0 = eyes open, 1 = shut

    public static EyeTransition Create(float startShut)
    {
        EyeTransition eyes = new GameObject("Eyes").AddComponent<EyeTransition>();
        DontDestroyOnLoad(eyes.gameObject);
        Canvas canvas = eyes.gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000; // over everything, text boxes included
        // Lids pivot on the screen's outer edges, so shrinking them opens toward the top and bottom.
        eyes.top = Lid(canvas.transform, "Top lid", new Vector2(0f, 0.5f), new Vector2(1f, 1f), new Vector2(0.5f, 1f));
        eyes.bottom = Lid(canvas.transform, "Bottom lid", new Vector2(0f, 0f), new Vector2(1f, 0.5f), new Vector2(0.5f, 0f));
        eyes.flash = Lid(canvas.transform, "Flash", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        eyes.flash.color = Color.clear;
        eyes.SetLids(startShut);
        return eyes;
    }

    static Image Lid(Transform parent, string name, Vector2 min, Vector2 max, Vector2 pivot)
    {
        RectTransform r = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        r.SetParent(parent, false);
        r.anchorMin = min;
        r.anchorMax = max;
        r.offsetMin = r.offsetMax = Vector2.zero;
        r.pivot = pivot;
        Image img = r.gameObject.AddComponent<Image>();
        img.color = Color.black;
        img.raycastTarget = false;
        return img;
    }

    public void SetLids(float s)
    {
        shut = s;
        top.rectTransform.localScale = new Vector3(1f, s, 1f);
        bottom.rectTransform.localScale = new Vector3(1f, s, 1f);
    }

    public IEnumerator Lids(float to, float seconds)
    {
        float from = shut;
        for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / seconds)
        {
            SetLids(Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t)));
            yield return null;
        }
        SetLids(to);
    }

    // A few heavy blinks, each closing further, then shut.
    public IEnumerator DrowsyClose()
    {
        yield return Lids(0.6f, 0.6f);
        yield return Lids(0.15f, 0.4f);
        yield return Lids(0.85f, 0.7f);
        yield return Lids(1f, 0.8f);
    }

    // A few quick half-opens with bright flashes, then wide open as the light washes in.
    public IEnumerator FlickerOpen()
    {
        foreach (float open in new[] { 0.4f, 0.8f, 0.6f })
        {
            yield return Lids(1f - open, 0.12f);
            flash.color = new Color(1f, 0.97f, 0.9f, 0.6f);
            yield return new WaitForSecondsRealtime(0.05f);
            flash.color = Color.clear;
            yield return new WaitForSecondsRealtime(0.08f);
            yield return Lids(1f, 0.08f);
            yield return new WaitForSecondsRealtime(0.12f);
        }
        for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / 0.5f)
        {
            flash.color = new Color(1f, 0.97f, 0.9f, 0.8f * (1f - t));
            SetLids(Mathf.SmoothStep(1f, 0f, t));
            yield return null;
        }
        SetLids(0f);
        flash.color = Color.clear;
    }

    // Close her eyes here, load `scene` behind them, and flicker them open there. A player in the new
    // scene is held still until her eyes are open. Removes itself when done.
    public void GoTo(string scene) => StartCoroutine(GoToRoutine(scene));

    IEnumerator GoToRoutine(string scene)
    {
        Playing = true;
        yield return DrowsyClose();
        yield return new WaitForSecondsRealtime(0.6f);
        Time.timeScale = 1f; // in case the last leap's slow motion was still on
        Time.fixedDeltaTime = 0.02f;
        yield return SceneManager.LoadSceneAsync(scene);
        yield return null; // let the new scene's Start() run before freezing anyone
        BaseMovement player = FindFirstObjectByType<BaseMovement>();
        if (player != null) player.lockControls();
        yield return new WaitForSecondsRealtime(0.25f);
        yield return FlickerOpen();
        if (player != null) player.unlockControls();
        Playing = false;
        Destroy(gameObject);
    }
}
