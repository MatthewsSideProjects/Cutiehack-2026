using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

// End-of-level door. When the player walks in: the door opens, she shrinks into the light,
// the screen fades to white, she is moved to `destination` (the next level, far away in the
// same scene), and the screen fades back in.
public class DoorScript : MonoBehaviour
{
    public Transform destination;       // where she appears in the next level
    public string nextScene;            // set on the last door: the world collapses into it, then this scene loads
    public float collapseTime = 2.8f;
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
        if (!string.IsNullOrEmpty(nextScene))
        {
            StartCoroutine(Collapse(player));
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

    // The finale: she steps through, the whole house folds and spins into the open door,
    // the doorway flares warm white, and the ending scene loads.
    private IEnumerator Collapse(BaseMovement player)
    {
        entering = true;
        FinalLeap.Set(1f);
        player.lockControls();
        if (sprite != null && openSprite != null) sprite.sprite = openSprite;
        foreach (CreepingWall w in FindObjectsByType<CreepingWall>(FindObjectsSortMode.None)) w.enabled = false;
        yield return new WaitForSeconds(openPause);

        // She walks into the light first.
        Transform p = player.transform;
        SpriteRenderer playerSprite = player.GetComponentInChildren<SpriteRenderer>();
        Vector3 startPos = p.position, startScale = p.localScale;
        Vector3 endPos = new Vector3(transform.position.x, startPos.y, transform.position.z + 0.1f);
        for (float t = 0f; t < 1f; t += Time.deltaTime / walkInTime)
        {
            float e = t * t * (3f - 2f * t);
            p.position = Vector3.Lerp(startPos, endPos, e);
            p.localScale = Vector3.Lerp(startScale, startScale * 0.2f, e);
            if (playerSprite != null) playerSprite.color = new Color(1f, 1f, 1f, 1f - e);
            yield return null;
        }

        // Everything nearby that is drawn (rooms, boxes, the darkness) gets pulled in.
        Vector3 c = transform.position;
        List<Transform> parts = new List<Transform>();
        foreach (Renderer r in FindObjectsByType<Renderer>(FindObjectsSortMode.None))
        {
            Transform t = r.transform;
            if (t == transform || t.IsChildOf(transform) || t.IsChildOf(p) || parts.Contains(t)) continue;
            if (Mathf.Abs(t.position.x - c.x) > 45f) continue;
            if (r.TryGetComponent(out Rigidbody rb)) rb.isKinematic = true;
            if (r.TryGetComponent(out Collider col)) col.enabled = false;
            parts.Add(t);
        }
        Vector3[] pos0 = new Vector3[parts.Count], scale0 = new Vector3[parts.Count]; Quaternion[] rot0 = new Quaternion[parts.Count];
        for (int i = 0; i < parts.Count; i++) { pos0[i] = parts[i].position; scale0[i] = parts[i].localScale; rot0[i] = parts[i].rotation; }

        Image fade = MakeFade();
        for (float t = 0f; t < 1f; t += Time.deltaTime / collapseTime)
        {
            float e = t * t * t; // slow start, then everything rushes in
            Quaternion spin = Quaternion.Euler(0f, 0f, 540f * e);
            for (int i = 0; i < parts.Count; i++)
            {
                if (parts[i] == null) continue;
                Vector3 off = pos0[i] - c;
                off.z = 0f;
                parts[i].position = c + spin * off * (1f - e) + new Vector3(0f, 0f, pos0[i].z);
                parts[i].localScale = scale0[i] * (1f - e);
                parts[i].rotation = spin * rot0[i];
            }
            fade.color = new Color(1f, 0.93f, 0.82f, Mathf.Clamp01((t - 0.6f) / 0.4f)); // warm sunrise white
            yield return null;
        }
        fade.color = new Color(1f, 0.93f, 0.82f, 1f);
        yield return new WaitForSeconds(0.4f);
        SceneManager.LoadScene(nextScene);
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
