using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// End-of-level door. When the player walks in: the door opens, she shrinks into the light,
// the screen fades to white, she is moved to `destination` (the next level, far away in the
// same scene), and the screen fades back in.
public class DoorScript : MonoBehaviour
{
    public Transform destination;       // where she appears in the next level
    public int itemsNeeded = 0;         // GDD says 2; raise once the memory items are in the level
    public Sprite closedSprite;
    public Sprite openSprite;
    public float openPause = 0.35f;
    public float walkInTime = 0.8f;
    public float fadeTime = 0.6f;

    private SpriteRenderer sprite;
    private bool entering;

    private void Start()
    {
        sprite = GetComponentInChildren<SpriteRenderer>();
        if (sprite != null && closedSprite != null) sprite.sprite = closedSprite;
    }

    // Enter logs why the door won't open; Stay lets her change size while standing in the doorway.
    private void OnTriggerEnter(Collider other) { TryEnter(other, true); }
    private void OnTriggerStay(Collider other) { TryEnter(other, false); }

    private void TryEnter(Collider other, bool explain)
    {
        if (entering || other.gameObject.tag != "Player") return;
        BaseMovement player = other.GetComponent<BaseMovement>();
        if (player == null) return;

        if (player.Size != PlayerSize.Medium)
        {
            if (explain) Debug.Log("The door only opens at regular size.");
            return;
        }
        if (player.ItemCount < itemsNeeded)
        {
            if (explain) Debug.Log("The door is locked. Items: " + player.ItemCount + "/" + itemsNeeded);
            return;
        }
        if (destination == null)
        {
            if (explain) Debug.LogError("Door has no Destination set.");
            return;
        }
        StartCoroutine(Enter(player));
    }

    private IEnumerator Enter(BaseMovement player)
    {
        entering = true;
        player.lockControls();
        if (sprite != null && openSprite != null) sprite.sprite = openSprite;
        yield return new WaitForSeconds(openPause);

        // Slide into the doorway while shrinking and fading.
        Transform p = player.transform;
        Vector3 startPos = p.position;
        Vector3 endPos = new Vector3(transform.position.x, startPos.y, transform.position.z + 0.1f);
        Vector3 startScale = p.localScale;
        SpriteRenderer playerSprite = player.GetComponentInChildren<SpriteRenderer>();
        for (float t = 0f; t < 1f; t += Time.deltaTime / walkInTime)
        {
            float e = t * t * (3f - 2f * t); // smoothstep
            p.position = Vector3.Lerp(startPos, endPos, e);
            p.localScale = Vector3.Lerp(startScale, startScale * 0.2f, e);
            if (playerSprite != null) playerSprite.color = new Color(1f, 1f, 1f, 1f - e);
            yield return null;
        }

        // Fade the whole screen to white (the GDD's "bright light").
        Image fade = MakeFade();
        for (float t = 0f; t < 1f; t += Time.deltaTime / fadeTime)
        {
            fade.color = new Color(1f, 1f, 1f, t);
            yield return null;
        }
        fade.color = Color.white;

        // Move to the next level while the screen is white, and restore her.
        p.localScale = startScale;
        if (playerSprite != null) playerSprite.color = Color.white;
        player.unlockControls(); // before respawn: a kinematic body can't have its velocity set
        player.setRespawn(destination.position);
        player.respawn();
        if (sprite != null && closedSprite != null) sprite.sprite = closedSprite;
        yield return null; // let the camera catch up before fading in

        for (float t = 1f; t > 0f; t -= Time.deltaTime / fadeTime)
        {
            fade.color = new Color(1f, 1f, 1f, t);
            yield return null;
        }
        Destroy(fade.canvas.gameObject);
        entering = false;
    }

    private static Image MakeFade()
    {
        GameObject canvasGo = new GameObject("DoorFade", typeof(Canvas));
        Canvas canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        GameObject imageGo = new GameObject("Fade", typeof(Image));
        imageGo.transform.SetParent(canvasGo.transform, false);
        RectTransform rt = imageGo.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        Image image = imageGo.GetComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0f);
        return image;
    }
}
