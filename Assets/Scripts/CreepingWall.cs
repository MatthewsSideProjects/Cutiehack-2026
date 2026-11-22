using System.Collections.Generic;
using UnityEngine;

// The house closing in (Level 3): darkness that slides right from the start of the level to the end.
// The box the builder makes is only the hit zone now (invisible trigger). The look is built here at runtime:
// two ragged, soft-edged black layers that breathe, plus wisps of shadow reaching out ahead of them.
// It is drawn in front of the room, so as it reaches her it swallows the floor, the walls and her.
// It moves only while she is inside its stretch, and when she respawns it restarts `behind` units
// to her left, so she always gets a few seconds before it catches up again.
[RequireComponent(typeof(Rigidbody))]
public class CreepingWall : MonoBehaviour
{
    static readonly List<CreepingWall> all = new List<CreepingWall>();
    public static CreepingWall Active => all.Count > 0 ? all[0] : null;

    public float speed = 1.5f;  // units per second; she runs at 7
    public float startX;        // world x of the wall's leading (right) edge at the start
    public float endX;          // it stops when the leading edge gets here
    public float behind = 8f;   // after a respawn, the leading edge restarts this far left of her

    private Rigidbody rb;
    private BaseMovement player;
    private float edge;
    private Transform layerA, layerB;
    private ParticleSystem wisps;

    public float LeadingEdge => edge;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        all.Add(this);
        edge = startX;
        // Hit zone only: a trigger, so it can't shove her into walls or park visibly in front of the camera.
        GetComponent<Collider>().isTrigger = true;
        Renderer r = GetComponent<Renderer>();
        if (r != null) r.enabled = false;
        BuildLook();
        Place();
    }

    private void OnDestroy()
    {
        all.Remove(this);
        if (layerA != null) Destroy(layerA.parent.gameObject);
    }

    private void Start()
    {
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) player = p.GetComponent<BaseMovement>();
    }

    private void FixedUpdate()
    {
        if (player == null) return;
        float px = player.transform.position.x;
        if (px < startX || px > endX + 40f) return; // she is in another level
        edge = Mathf.Min(edge + speed * Time.fixedDeltaTime, endX);
        rb.MovePosition(new Vector3(edge - transform.localScale.x / 2f, rb.position.y, rb.position.z));
    }

    private void LateUpdate()
    {
        // Breathe: the edge surges forward and pulls back a little, the two layers out of step.
        float t = Time.time;
        float y = transform.position.y;
        layerA.position = new Vector3(edge + 2f + 0.6f * Mathf.Sin(t * 1.3f), y, -0.6f);
        layerB.position = new Vector3(edge + 3.5f + 0.9f * Mathf.Sin(t * 0.8f + 1f), y + 0.4f * Mathf.Sin(t * 0.5f), -0.62f);
        wisps.transform.position = new Vector3(edge + 1f, y, -0.65f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (player != null && other.gameObject == player.gameObject) player.die();
    }

    // Called from BaseMovement.respawn().
    public static void ResetAll(float respawnX)
    {
        foreach (CreepingWall w in all)
        {
            w.edge = Mathf.Clamp(respawnX - w.behind, w.startX, w.endX);
            w.Place();
            w.wisps.Clear();
        }
    }

    private void Place()
    {
        Vector3 p = new Vector3(edge - transform.localScale.x / 2f, transform.position.y, transform.position.z);
        rb.position = p;
        transform.position = p;
    }

    void BuildLook()
    {
        Transform holder = new GameObject("Darkness (runtime)").transform; // not a child: the hit box is scaled
        Material mat = new Material(Shader.Find("Sprites/Default"));
        layerA = Layer(holder, mat, 7, 1f);
        layerB = Layer(holder, mat, 31, 0.55f);

        GameObject go = new GameObject("Shadow wisps");
        go.transform.SetParent(holder, false);
        wisps = go.AddComponent<ParticleSystem>();
        var main = wisps.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(1.2f, 2.2f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.8f, 2.4f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.3f, 1.1f);
        main.startColor = new Color(0.02f, 0.01f, 0.04f, 0.9f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 300;
        var emission = wisps.emission; emission.rateOverTime = 70f;
        var shape = wisps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(0.5f, 20f, 0.1f);
        shape.rotation = new Vector3(0f, 90f, 0f); // emit along +x, toward her
        var noise = wisps.noise; noise.enabled = true; noise.strength = 0.8f; noise.frequency = 0.6f;
        var fade = wisps.colorOverLifetime; fade.enabled = true;
        Gradient g = new Gradient();
        g.SetKeys(new[] { new GradientColorKey(Color.black, 0f), new GradientColorKey(Color.black, 1f) },
                  new[] { new GradientAlphaKey(0.9f, 0f), new GradientAlphaKey(0f, 1f) });
        fade.color = g;
        var size = wisps.sizeOverLifetime; size.enabled = true; size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0.2f));
        go.GetComponent<ParticleSystemRenderer>().material = mat;
    }

    // A 40 x 24 slab of black whose right edge is ragged and fades out over the last few units.
    static Transform Layer(Transform holder, Material mat, int seed, float alpha)
    {
        const int w = 128, h = 64;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        Color32[] px = new Color32[w * h];
        for (int y = 0; y < h; y++)
        {
            // Where this row's edge sits: a few sine waves, so it looks like reaching fingers.
            float reach = 12f + 6f * Mathf.Sin(y * 0.31f + seed) + 3f * Mathf.Sin(y * 0.77f + seed * 2f);
            for (int x = 0; x < w; x++)
            {
                float d = (w - 1 - x) - (20f - reach); // > 0 inside the dark
                float a = Mathf.Clamp01(d / 10f);
                a = a * a * (3f - 2f * a);
                px[y * w + x] = new Color32(4, 2, 10, (byte)(255 * a * alpha));
            }
        }
        tex.SetPixels32(px);
        tex.Apply();
        GameObject go = new GameObject("Dark layer");
        go.transform.SetParent(holder, false);
        go.transform.localScale = new Vector3(40f / w, 24f / h, 1f);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(1f, 0.5f), 1f);
        sr.sharedMaterial = mat;
        return go.transform;
    }
}
