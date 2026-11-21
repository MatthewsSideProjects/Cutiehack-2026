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
    }

    // ponytail: resets every block in the scene, not just the current section; fine while
    // blocks in other sections can't be affected from where she respawns.
    public static void ResetAll()
    {
        foreach (Pushable p in all)
        {
            p.rb.linearVelocity = Vector3.zero;
            p.rb.position = p.start;
            p.transform.position = p.start;
        }
    }
}
