using UnityEngine;

// A platform that loops between its start position and start + offset, easing at both ends.
// The player reads Velocity to ride along (see BaseMovement).
[RequireComponent(typeof(Rigidbody))]
public class MovingPlate : MonoBehaviour
{
    public Vector3 offset = new Vector3(5f, 0f, 0f);
    public float period = 4f;            // seconds for a full there-and-back loop
    [Range(0f, 1f)] public float phase;  // offsets plates that share a period
    public Vector3 Velocity { get; private set; }

    private Rigidbody rb;
    private Vector3 start;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        start = rb.position;
    }

    private void FixedUpdate()
    {
        float t = Time.time / period + phase;
        float k = 0.5f - 0.5f * Mathf.Cos(t * 2f * Mathf.PI);
        Vector3 next = start + offset * k;
        Velocity = (next - rb.position) / Time.fixedDeltaTime;
        rb.MovePosition(next);
    }
}
