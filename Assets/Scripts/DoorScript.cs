using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// End-of-level door. When the player walks in: the door opens, she shrinks into the light,
// the screen fades to white, she is moved to `destination` (the next level, far away in the
// same scene), and the screen fades back in.
public class DoorScript : MonoBehaviour
{
    public Transform destination;       // where she appears in the next level
    public int itemsNeeded = 0;         // memory items from this level she must carry before it opens
    public Sprite closedSprite;
    public Sprite openSprite;
    public float openPause = 0.35f;
    public float walkInTime = 0.8f;
    public float fadeTime = 0.6f;

    private SpriteRenderer sprite;
    private bool entering;
    private BaseMovement player;
    private TextMesh counter; // "1/2" above a locked door

    private void Start()
    {
        sprite = GetComponentInChildren<SpriteRenderer>();
        if (sprite != null && closedSprite != null) sprite.sprite = closedSprite;
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) player = p.GetComponent<BaseMovement>();
        if (itemsNeeded > 0) counter = MakeCounter();
    }

    private int Have => player == null ? 0 : player.ItemsIn(transform.root);

    private void Update()
    {
        if (counter == null) return;
        int have = Mathf.Min(Have, itemsNeeded);
        counter.text = have + "/" + itemsNeeded;
        counter.color = have >= itemsNeeded ? new Color32(250, 210, 110, 255) : new Color32(255, 240, 230, 255);
    }

    private TextMesh MakeCounter()
    {
        GameObject go = new GameObject("Item counter");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(0f, 1.7f, -0.1f);
        TextMesh tm = go.AddComponent<TextMesh>();
        tm.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        go.GetComponent<MeshRenderer>().sharedMaterial = tm.font.material;
        tm.fontSize = 48;
        tm.characterSize = 0.07f;
        tm.anchor = TextAnchor.MiddleCenter;
        return tm;
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
        if (player.ItemsIn(transform.root) < itemsNeeded)
        {
            if (explain) Debug.Log("The door is locked. Items: " + player.ItemsIn(transform.root) + "/" + itemsNeeded);
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
