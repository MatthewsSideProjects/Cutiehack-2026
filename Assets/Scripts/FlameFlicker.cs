using UnityEngine;

// Candle flame hazard that turns on and off in a fixed loop (the stuck-clock rule).
// Tag the object "Obstacle"; while it's off it can't hurt her.
public class FlameFlicker : MonoBehaviour
{
    public float onTime = 1.2f;
    public float offTime = 1.0f;
    [Range(0f, 1f)] public float phase;

    private Collider col;
    private Renderer rend;

    private void Start()
    {
        col = GetComponent<Collider>();
        rend = GetComponent<Renderer>();
    }

    private void Update()
    {
        float loop = onTime + offTime;
        bool on = Mathf.Repeat(Time.time + phase * loop, loop) < onTime;
        if (col != null) col.enabled = on;
        if (rend != null) rend.enabled = on;
    }
}
