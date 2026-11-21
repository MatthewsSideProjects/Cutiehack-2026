using UnityEngine;

// Lamppost checkpoint: starts off, lights up the first time the player walks past,
// and from then on is where she respawns.
public class CheckpointDetection : MonoBehaviour
{
    public BaseMovement playerScript;
    public Sprite offSprite;
    public Sprite onSprite;
    private SpriteRenderer sprite;
    private bool lit;

    private void Start()
    {
        sprite = GetComponentInChildren<SpriteRenderer>();
        if (sprite != null && offSprite != null) sprite.sprite = offSprite;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (lit || other.gameObject.tag != "Player") return;
        lit = true;
        if (playerScript == null) playerScript = other.GetComponent<BaseMovement>();
        playerScript.setRespawn(transform.position);
        if (sprite != null && onSprite != null) sprite.sprite = onSprite;
    }
}
