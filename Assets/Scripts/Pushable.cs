using System.Collections.Generic;
using UnityEngine;

// A box, present or stone she can push sideways, but only in her Big form.
// Any other time it is locked in x (it still falls). Respawning puts every block back
// where it started, so a block pushed into a bad spot can't softlock the level.
[RequireComponent(typeof(Rigidbody))]
public class Pushable : MonoBehaviour
{
    static readonly List<Pushable> all = new List<Pushable>();

    private Rigidbody rb;
    private Vector3 start;
    private BaseMovement player;
    private bool pushed; // she touched it during the last physics step

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        start = transform.position;
        all.Add(this);
    }

    private void OnDestroy()
    {
        all.Remove(this);
    }

    private void Start()
    {
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) player = p.GetComponent<BaseMovement>();
    }

    private void FixedUpdate()
    {
        bool canPush = player != null && player.Size == PlayerSize.Large;
        RigidbodyConstraints locked = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;
        rb.constraints = canPush ? locked : locked | RigidbodyConstraints.FreezePositionX;
        // No ice: the moment she stops pushing, it stops. Without this it kept her momentum and slid into pits.
        if (!pushed) rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        pushed = false;
    }

    private void OnCollisionStay(Collision other)
    {
        if (player != null && other.gameObject == player.gameObject) pushed = true;
    }

    // Puts back only the blocks that start ahead of where she respawns: those belong to the section
    // she is retrying. Blocks behind her stay where she left them, so a bridge she already built
    // (a stone in a trench, a crate in a gap) doesn't vanish when she dies further on.
    public static void ResetAhead(float respawnX)
    {
        foreach (Pushable p in all)
        {
            if (p.start.x < respawnX) continue;
            p.rb.linearVelocity = Vector3.zero;
            p.rb.position = p.start;
            p.transform.position = p.start;
        }
    }
}
