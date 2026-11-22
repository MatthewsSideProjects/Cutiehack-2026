using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Follows her, and sets each level's mood from where she is (Level 1 at x 0, Level 2 at 1000, Level 3 at 2000):
//   Level 1  garden: bright golden sunlight, glow, warm and lush, soft light shafts across the screen.
//   Level 2  birthday: the picture swims a little (lens wobble, colour fringing), pink tint.
//   Level 3  empty house: very dark, desaturated, tight vignette, the camera moves in closer
//            and shakes harder the further she gets, and harder still when the darkness is close.
// Everything blends over about a second, so walking through a door fades between moods.
public class CameraScript : MonoBehaviour
{
    public GameObject player;
    // Vertical dead zone: the camera only moves up or down once she gets this far from its center,
    // so normal jumps don't bob the view but climbing the cake scrolls it.
    public float aboveMargin = 3f;
    public float belowMargin = 2f;
    // Bottom of the 3-block ground in Levels 1 and 2: the view's bottom edge never dips below it.
    public float groundBottom = -3f;
    // Outer edges of the green hedges at the ends of Level 1: the view never goes past them.
    public float level1Left = -2f;
    public float level1Right = 72f;
    // Level 2's ends (it's built at x 1000, floor runs to local x 142).
    public float level2Left = 1000f;
    public float level2Right = 1142f;
    // Level 1's flat (orthographic) view: half the view height in units.
    public float viewHalfHeight = 6f;

    [Header("Level 3")]
    public float level3Length = 165f;   // x from the start of Level 3 to its door, for "towards the end"
    public float closeZoomStart = 0.8f; // camera distance multiplier entering Level 3
    public float closeZoomEnd = 0.6f;   // ... and at the door
    public float maxShake = 0.25f;      // units of shake at the door

    public static bool Calm; // the finale: no shake while the world spirals into the door
    private float minY, baseY, baseZ;
    private Camera cam;
    private Light sun;
    private float sunIntensity;
    private Color sunColor;
    private Light lantern; // a small glow on her in Level 3, so the dark has something to eat
    private Vignette vignette;
    private ColorAdjustments color;
    private LensDistortion lens;
    private ChromaticAberration fringe;
    private Bloom bloom;
    private SpriteRenderer[] beams;
    [Header("Music")]
    public float level1MusicVolume = 0.4f; // Assets/Resources/gardenAudio.mp3, kept low
    public float level2MusicVolume = 0.6f; // Assets/Resources/bitCrushAudio.mp3, a bit under full
    public float level3MusicVolume = 0.4f; // Assets/Resources/level3Audio.mp3, kept low
    private AudioSource level1Music, level2Music, level3Music;
    private float calmW;
    private float w1 = 1f, w2, w3; // how much each level's mood is on, 0..1

    void Start()
    {
        if (player == null) player = GameObject.FindWithTag("Player");
        Calm = false; // left on by the last door when the game is played again
        minY = baseY = transform.position.y; // never look lower than the starting view
        baseZ = transform.position.z;

        foreach (Light l in FindObjectsByType<Light>(FindObjectsSortMode.None))
            if (l.type == LightType.Directional) { sun = l; break; }
        if (sun != null) { sunIntensity = sun.intensity; sunColor = sun.color; }

        lantern = new GameObject("Lantern").AddComponent<Light>();
        lantern.type = LightType.Point;
        lantern.color = new Color(1f, 0.8f, 0.55f);
        lantern.range = 7f;
        lantern.intensity = 0f;

        cam = GetComponent<Camera>();
        cam.GetUniversalAdditionalCameraData().renderPostProcessing = true;
        Volume v = gameObject.AddComponent<Volume>();
        v.isGlobal = true;
        v.priority = 10f;
        VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
        v.profile = profile;
        vignette = profile.Add<Vignette>(true);
        color = profile.Add<ColorAdjustments>(true);
        lens = profile.Add<LensDistortion>(true);
        fringe = profile.Add<ChromaticAberration>(true);
        bloom = profile.Add<Bloom>(true);
        beams = MakeSunBeams();
        level1Music = MakeMusic("gardenAudio");
        level2Music = MakeMusic("bitCrushAudio");
        level3Music = MakeMusic("level3Audio");
    }

    AudioSource MakeMusic(string clip)
    {
        AudioSource a = gameObject.AddComponent<AudioSource>();
        a.clip = Resources.Load<AudioClip>(clip);
        a.loop = true;
        a.playOnAwake = false;
        a.volume = 0f;
        return a;
    }

    static void Fade(AudioSource a, float volume)
    {
        a.volume = volume;
        if (volume > 0.01f && !a.isPlaying && a.clip != null) a.Play();
        else if (volume <= 0.01f && a.isPlaying) a.Stop();
    }

    // Long soft diagonal stripes of light, fixed to the camera: sun pouring through the leaves.
    SpriteRenderer[] MakeSunBeams()
    {
        const int w = 32, h = 4;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
        Color32[] px = new Color32[w * h];
        for (int x = 0; x < w; x++)
        {
            float a = Mathf.Sin(Mathf.PI * x / (w - 1f)); // soft at both sides
            for (int y = 0; y < h; y++) px[y * w + x] = new Color32(255, 236, 170, (byte)(255 * a * a));
        }
        tex.SetPixels32(px);
        tex.Apply();
        Sprite sp = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), w);
        Material mat = new Material(Shader.Find("Sprites/Default"));
        float[] xs = { -7f, -2.5f, 3f, 8f }, widths = { 2.5f, 1.4f, 3.2f, 1.8f };
        SpriteRenderer[] list = new SpriteRenderer[xs.Length];
        for (int i = 0; i < xs.Length; i++)
        {
            GameObject go = new GameObject("Sun beam");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(xs[i], 1f, -baseZ - 1.2f); // just in front of the play layer
            go.transform.localRotation = Quaternion.Euler(0f, 0f, -28f);
            go.transform.localScale = new Vector3(widths[i], 40f, 1f);
            list[i] = go.AddComponent<SpriteRenderer>();
            list[i].sprite = sp;
            list[i].sharedMaterial = mat;
        }
        return list;
    }

    void LateUpdate()
    {
        Vector3 p = player.transform.position;
        if (p.y > baseY + aboveMargin) baseY = p.y - aboveMargin;
        if (p.y < baseY - belowMargin) baseY = p.y + belowMargin;
        baseY = Mathf.Max(minY, baseY);

        // Which level she is in, eased so moods cross-fade.
        float k = 1f - Mathf.Exp(-2f * Time.deltaTime);
        w1 = Mathf.Lerp(w1, p.x < 1000f ? 1f : 0f, k);
        w2 = Mathf.Lerp(w2, p.x >= 1000f && p.x < 2000f ? 1f : 0f, k);
        calmW = Mathf.Lerp(calmW, Calm ? 1f : 0f, k); // the dark lifts so the door shines
        w3 = Mathf.Lerp(w3, p.x >= 2000f ? 1f : 0f, k);

        // Each level's music fades in with its mood and out again when she leaves it.
        Fade(level1Music, level1MusicVolume * w1);
        Fade(level2Music, level2MusicVolume * w2);
        Fade(level3Music, level3MusicVolume * w3);
        float progress = Mathf.Clamp01((p.x - 2000f) / level3Length); // 0 at the start of Level 3, 1 at its door
        float near = 0f; // 1 when the darkness is on her heels
        CreepingWall dark = CreepingWall.Active;
        if (dark != null && w3 > 0.01f) near = 1f - Mathf.Clamp01((p.x - dark.LeadingEdge) / 10f);
        float t = Time.time;

        // Light
        if (sun != null)
        {
            sun.intensity = sunIntensity * Mathf.Lerp(1f, 0.12f, w3) * (1f + 0.5f * w1);
            sun.color = Color.Lerp(Color.Lerp(Color.Lerp(sunColor, new Color(1f, 0.9f, 0.7f), w1), new Color(1f, 0.82f, 0.86f), w2), new Color(0.55f, 0.6f, 0.85f), w3);
        }
        lantern.transform.position = p + new Vector3(0f, 0.5f, -1.5f);
        lantern.intensity = w3 * (2.2f - 0.8f * progress) * (0.92f + 0.08f * Mathf.PerlinNoise(t * 6f, 0f)); // flickers, dims near the end

        // Post effects
        color.postExposure.Override((0.35f * w1 - 1.1f * w3 - 0.4f * w3 * progress) * (1f - calmW));
        color.saturation.Override(18f * w1 + 12f * w2 - 55f * w3);
        color.colorFilter.Override(Color.Lerp(Color.Lerp(Color.white, new Color(1f, 0.96f, 0.86f), w1), new Color(1f, 0.9f, 0.95f), w2));
        bloom.intensity.Override(0.8f * w1 + 0.3f * w2 + 1.5f * calmW);
        bloom.threshold.Override(0.85f);
        for (int i = 0; i < beams.Length; i++) // each shaft breathes slowly, like leaves moving across the sun
            beams[i].color = new Color(1f, 1f, 1f, w1 * (0.06f + 0.05f * Mathf.Sin(t * 0.6f + i * 1.9f)));
        lens.intensity.Override(w2 * (-0.18f + 0.1f * Mathf.Sin(t * 0.7f)) + w3 * 0.15f * near);
        fringe.intensity.Override(0.3f * w2 + w3 * (0.15f + 0.6f * near));
        vignette.intensity.Override(w3 * (0.4f + 0.15f * progress + 0.2f * near));
        vignette.smoothness.Override(0.5f);

        // Closer and shakier through Level 3
        float zoom = Mathf.Lerp(1f, Mathf.Lerp(closeZoomStart, closeZoomEnd, progress), w3);
        float shake = Calm ? 0f : w3 * maxShake * (progress * progress + 0.6f * near);
        Vector2 jitter = shake * new Vector2(Mathf.PerlinNoise(t * 18f, 1f) - 0.5f, Mathf.PerlinNoise(1f, t * 18f) - 0.5f) * 2f;
        // Level 1: flat (orthographic) view. Levels 2 and 3: the original perspective camera.
        // Levels 1 and 2 keep the view between their ends and above the ground; Level 3 is unclamped.
        cam.orthographic = p.x < 1000f;
        float z = baseZ * zoom;
        float halfH = cam.orthographic
            ? (cam.orthographicSize = viewHalfHeight)
            : -z * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad); // half the view height at the play layer (z 0)
        float halfW = halfH * cam.aspect;
        float x = p.x + jitter.x, y = baseY + jitter.y;
        if (p.x < 2000f)
        {
            bool l1 = p.x < 1000f;
            x = Mathf.Clamp(x, (l1 ? level1Left : level2Left) + halfW, (l1 ? level1Right : level2Right) - halfW);
            y = Mathf.Max(y, groundBottom + halfH);
        }
        transform.position = new Vector3(x, y, z);
    }
}
