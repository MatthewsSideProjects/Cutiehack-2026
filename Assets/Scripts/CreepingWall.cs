using System.Collections.Generic;
using UnityEngine;

// The house closing in (Level 3): a dark wall that slides right from the start of the level to the end.
// Its colliders are tagged "Obstacle", so touching it sends her to the last lit lamp.
// It moves only while she is inside its stretch, and when she respawns it restarts `behind` units
// to her left, so she always gets a few seconds before it catches up again.
[RequireComponent(typeof(Rigidbody))]
public class CreepingWall : MonoBehaviour
{
    static readonly List<CreepingWall> all = new List<CreepingWall>();

    public float speed = 1.5f;  // units per second; she runs at 7
    public float startX;        // world x of the wall's leading (right) edge at the start
    public float endX;          // it stops when the leading edge gets here
    public float behind = 8f;   // after a respawn, the leading edge restarts this far left of her

    private Rigidbody rb;
    private Transform player;
    private float edge;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        all.Add(this);
        edge = startX;
        Place();
    }

    private void OnDestroy()
    {
        all.Remove(this);
    }

    private void Start()
    {
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) player = p.transform;
    }

    private void FixedUpdate()
    {
        if (player == null) return;
        float px = player.position.x;
        if (px < startX || px > endX + 40f) return; // she is in another level
        edge = Mathf.Min(edge + speed * Time.fixedDeltaTime, endX);
        rb.MovePosition(new Vector3(edge - transform.localScale.x / 2f, rb.position.y, rb.position.z));
    }

    // Called from BaseMovement.respawn().
    public static void ResetAll(float respawnX)
    {
        foreach (CreepingWall w in all)
        {
            w.edge = Mathf.Clamp(respawnX - w.behind, w.startX, w.endX);
            w.Place();
        }
    }

    private void Place()
    {
        Vector3 p = new Vector3(edge - transform.localScale.x / 2f, transform.position.y, transform.position.z);
        rb.position = p;
        transform.position = p;
    }
}
