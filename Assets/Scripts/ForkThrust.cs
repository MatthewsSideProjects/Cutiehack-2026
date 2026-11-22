using UnityEngine;

// A fork that stabs into the cake on a fixed loop (the stuck-clock rule):
// wait hidden, shake for a moment as a warning, stab in fast, hold, pull back slowly.
// The fork's child colliders are tagged "Obstacle", so touching any part sends her to the last lamp.
[RequireComponent(typeof(Rigidbody))]
public class ForkThrust : MonoBehaviour
{
    public Vector3 stab = new Vector3(8f, 0f, 0f); // how far it travels when it stabs (local to its parent)
    public float waitTime = 1.5f;
    public float shakeTime = 0.5f;
    public float stabTime = 0.2f;
    public float holdTime = 0.4f;
    public float pullTime = 0.8f;
    public float shakeAmount = 0.08f;
    public float peek = 0.06f; // how far out (0-1 of stab) the tips show while shaking
    [Range(0f, 1f)] public float phase;
    public Vector3 blockCheckSize = new Vector3(1.2f, 1.2f, 0.6f); // area a box must cover to block it

    private Rigidbody rb;
    private Vector3 home;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        home = transform.localPosition;
    }

    private void FixedUpdate()
    {
        float loop = waitTime + shakeTime + stabTime + holdTime + pullTime;
        float t = Mathf.Repeat(Time.time + phase * loop, loop);
        float out01;
        Vector3 jitter = Vector3.zero;

        if ((t -= waitTime) < 0f) out01 = 0f;
        else if ((t -= shakeTime) < 0f)
        {
            out01 = peek; // tips peek out so she can see where it will come from
            jitter = Random.insideUnitSphere * shakeAmount;
            jitter.z = 0f;
        }
        else if ((t -= stabTime) < 0f) out01 = Mathf.Lerp(peek, 1f, 1f + t / stabTime);
        else if ((t -= holdTime) < 0f) out01 = 1f;
        else out01 = Mathf.SmoothStep(1f, 0f, 1f + (t - pullTime) / pullTime);

        // A pushable block standing in front of the fork blocks it, so it stays in.
        if (Blocked()) { out01 = 0f; jitter = Vector3.zero; }

        Vector3 local = home + stab * out01 + jitter;
        Vector3 world = transform.parent != null ? transform.parent.TransformPoint(local) : local;
        rb.MovePosition(world);
    }

    // Looks for a Pushable in the play plane (z = 0) right where the fork comes out.
    private bool Blocked()
    {
        Vector3 spot = new Vector3(home.x, home.y, 0f);
        if (transform.parent != null) spot = transform.parent.TransformPoint(spot);
        foreach (Collider c in Physics.OverlapBox(spot, blockCheckSize / 2f))
            if (c.GetComponentInParent<Pushable>() != null) return true;
        return false;
    }
}
