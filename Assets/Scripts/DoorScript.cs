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
    public float doorHoldTime = 3f;    // how long only the door is left before the light takes over
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
    private Transform beams;  // final door only: light pouring out once it is unlocked
    private Light doorLight;
    private bool Final => !string.IsNullOrEmpty(nextScene);

    private void Start()
    {
        sprite = GetComponentInChildren<SpriteRenderer>();
        if (sprite != null && closedSprite != null) sprite.sprite = closedSprite;
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) player = p.GetComponent<BaseMovement>();
        if (itemsNeeded > 0) counter = MakeCounter();
        if (Final)
        {
            BoxCollider box = GetComponent<BoxCollider>();
            if (box != null) box.size = new Vector3(1.6f, 4f, 2f); // a jump through the doorway counts
            MakeBeams();
        }
    }

    private int Have => player == null ? 0 : player.ItemsIn(transform.root);

    private void Update()
    {
        if (Final) UpdateBeams();
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

        if (Final && player.ItemsIn(transform.root) >= itemsNeeded)
        {
            StartCoroutine(Collapse(player)); // any size, mid-jump: straight in
            return;
        }
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

    // Final door: once she has every item it stands open, glowing, with light beams turning slowly around it.
    private void MakeBeams()
    {
        const int w = 8, h = 32;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
        Color32[] px = new Color32[w * h];
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float across = Mathf.Sin(Mathf.PI * x / (w - 1f));
                float along = 1f - y / (h - 1f); // bright at the door, fading outward
                px[y * w + x] = new Color32(255, 225, 150, (byte)(255 * across * across * along));
            }
        tex.SetPixels32(px);
        tex.Apply();
        Sprite sp = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0f), 4f); // pivot at the door
        Material mat = new Material(Shader.Find("Sprites/Default"));

        beams = new GameObject("Door beams").transform;
        beams.SetParent(transform, false);
        beams.localPosition = new Vector3(0f, 0f, 0.05f); // just behind the door sprite
        for (int i = 0; i < 10; i++)
        {
            GameObject b = new GameObject("Beam");
            b.transform.SetParent(beams, false);
            b.transform.localRotation = Quaternion.Euler(0f, 0f, i * 36f);
            b.transform.localScale = new Vector3(i % 2 == 0 ? 1.2f : 0.7f, i % 2 == 0 ? 1.1f : 0.8f, 1f);
            SpriteRenderer sr = b.AddComponent<SpriteRenderer>();
            sr.sprite = sp;
            sr.sharedMaterial = mat;
        }
        doorLight = new GameObject("Door light").AddComponent<Light>();
        doorLight.transform.SetParent(transform, false);
        doorLight.transform.localPosition = new Vector3(0f, 0f, -1.5f);
        doorLight.type = LightType.Point;
        doorLight.color = new Color(1f, 0.85f, 0.55f);
        doorLight.range = 10f;
        beams.gameObject.SetActive(false);
    }

    private void UpdateBeams()
    {
        bool open = entering || Have >= itemsNeeded;
        beams.gameObject.SetActive(open);
        doorLight.intensity = open ? 3f + 0.5f * Mathf.Sin(Time.time * 2f) : 0f;
        if (sprite != null && openSprite != null && closedSprite != null) sprite.sprite = open ? openSprite : closedSprite;
        if (open) beams.localRotation = Quaternion.Euler(0f, 0f, Time.time * 12f);
        if (open) beams.localScale = Vector3.one * (1f + 0.06f * Mathf.Sin(Time.time * 3f));
    }

    // The finale: she flies straight into the light, then the whole world spirals into the doorway.
    // Things further out swirl more, so it twists like water down a drain. When it is gone only the
    // door is left, glowing in the dark for a few seconds, before the light fills the screen.
    private IEnumerator Collapse(BaseMovement player)
    {
        entering = true;
        FinalLeap.Set(1f);
        CameraScript.Calm = true;
        player.lockControls();
        foreach (CreepingWall w in FindObjectsByType<CreepingWall>(FindObjectsSortMode.None)) w.enabled = false;

        // She is swallowed by the light at once.
        Transform p = player.transform;
        SpriteRenderer playerSprite = player.GetComponentInChildren<SpriteRenderer>();
        Vector3 startPos = p.position, startScale = p.localScale;
        Vector3 doorPos = new Vector3(transform.position.x, transform.position.y, transform.position.z + 0.1f);
        for (float t = 0f; t < 1f; t += Time.deltaTime / 0.3f)
        {
            p.position = Vector3.Lerp(startPos, doorPos, t);
            p.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            if (playerSprite != null) playerSprite.color = new Color(1f, 1f, 1f, 1f - t);
            yield return null;
        }
        p.localScale = Vector3.zero;

        // Everything drawn in this level (and the darkness) gets pulled in; the door and the camera stay.
        Vector3 c = transform.position;
        Transform cam = Camera.main != null ? Camera.main.transform : null;
        List<Transform> parts = new List<Transform>();
        foreach (Renderer r in FindObjectsByType<Renderer>(FindObjectsSortMode.None))
        {
            Transform t = r.transform;
            if (t == transform || t.IsChildOf(transform) || t.IsChildOf(p) || (cam != null && t.IsChildOf(cam))) continue;
            if (!t.IsChildOf(transform.root) && Mathf.Abs(t.position.x - c.x) > 60f) continue;
            if (parts.Contains(t)) continue;
            if (r.TryGetComponent(out Rigidbody rb)) rb.isKinematic = true;
            if (r.TryGetComponent(out Collider col)) col.enabled = false;
            if (r is ParticleSystemRenderer) { r.enabled = false; continue; } // wisps just vanish
            parts.Add(t);
        }
        int n = parts.Count;
        Vector3[] off0 = new Vector3[n], scale0 = new Vector3[n];
        Quaternion[] rot0 = new Quaternion[n];
        float[] swirl = new float[n];
        for (int i = 0; i < n; i++)
        {
            off0[i] = parts[i].position - c;
            scale0[i] = parts[i].localScale;
            rot0[i] = parts[i].rotation;
            swirl[i] = 1f + new Vector2(off0[i].x, off0[i].y).magnitude / 12f; // far things spin more
        }

        for (float t = 0f; t < 1f; t += Time.deltaTime / collapseTime)
        {
            float e = t * t * t; // slow start, then everything rushes in
            for (int i = 0; i < n; i++)
            {
                if (parts[i] == null) continue;
                Quaternion spin = Quaternion.Euler(0f, 0f, 360f * swirl[i] * e);
                Vector3 off = off0[i];
                float z = off.z;
                off.z = 0f;
                Vector3 q = spin * off * (1f - e);
                parts[i].position = c + new Vector3(q.x, q.y, z * (1f - e) + 0.3f); // tucks just behind the door
                parts[i].localScale = scale0[i] * (1f - e);
                parts[i].rotation = spin * rot0[i];
            }
            yield return null;
        }
        foreach (Transform t in parts) if (t != null) t.gameObject.SetActive(false);

        // Just the door, glowing, for a few seconds.
        yield return new WaitForSeconds(doorHoldTime);

        Image fade = MakeFade();
        for (float t = 0f; t < 1f; t += Time.deltaTime / 1.2f)
        {
            fade.color = new Color(1f, 0.93f, 0.82f, t); // warm sunrise white
            if (beams != null) beams.localScale = Vector3.one * (1f + 3f * t); // the light swells to fill the screen
            yield return null;
        }
        fade.color = new Color(1f, 0.93f, 0.82f, 1f);
        yield return new WaitForSeconds(0.4f);
        CameraScript.Calm = false;
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
