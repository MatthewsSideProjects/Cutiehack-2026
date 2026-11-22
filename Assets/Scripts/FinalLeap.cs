using UnityEngine;

// The last jump of Level 3: while she is in the air over the final gap, time slows right down,
// so the leap toward the door feels like it lasts forever. Normal speed comes back when she
// leaves the gap (lands, reaches the door, or falls and respawns).
[RequireComponent(typeof(BoxCollider))]
public class FinalLeap : MonoBehaviour
{
    public float slowScale = 0.35f;
    const float BaseFixedStep = 0.02f;

    private void Awake() { GetComponent<BoxCollider>().isTrigger = true; }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) Set(slowScale);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) Set(1f);
    }

    private void OnDisable() { Set(1f); }

    public static void Set(float scale)
    {
        Time.timeScale = scale;
        Time.fixedDeltaTime = BaseFixedStep * scale; // keeps the slowed physics smooth
    }
}
