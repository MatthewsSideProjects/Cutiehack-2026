using NUnit.Framework;
using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using Unity.VisualScripting;
public enum DeathType {Timeout, Obstacle}
public enum PlayerSize {Small, Medium, Large}
public class BaseMovement : MonoBehaviour
{
    public float speed;
    public float jumpForce;
    public float gravityMod;
    public TimerScript timerScript;
    public GameObject startingCollectableObject;
    public float collectableDisplayOffset;
    public bool spriteFacesLeft; // tick if the rabbit faces the wrong way when running
    public Sprite[] idleFrames;
    public Sprite[] runFrames;
    public Sprite[] deathFrames;
    public float deathFps = 8f;
    public float deathHold = 0.6f; // seconds on the last death frame before she respawns
    private bool dying;
    private bool gameOver; // timer ran out: nothing can kill or respawn her any more
    public float idleFps = 6f;
    public float runFps = 10f;
    // Below this y she counts as fallen into a pit: just under the floors, while she's still on screen,
    // so the death animation shows. (The deepest floor, Level 1's trench, has her at about y -1.2.)
    public float pitDeathY = -2.5f;
    private MovingPlate plate; // the moving plate she is standing on, if any
    private float animTime;
    private bool wasRunning;
    public float transformOffset;
    public float speedScalar;
    public float largeJumpScalar;
    public float smallJumpScalar;
    private Rigidbody rb;
    private SpriteRenderer sprite;
    private bool canJump;
    private bool canMove = true;
    private Vector3 currentRespawn;
    private List<CollectableScript> collectables = new List<CollectableScript>();
    private List<GameObject> collectableObjects = new List<GameObject>();
    private PlayerSize size = PlayerSize.Medium;
    private Vector3 baseScale; // Regular-size scale and speed, restored on respawn
    private float baseSpeed, baseJump;
    private PlayerSize maxSize = PlayerSize.Large;
    public PlayerSize Size => size;
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        sprite = GetComponentInChildren<SpriteRenderer>();
        Physics.gravity = new Vector3(0, -1f * gravityMod, 0);
        currentRespawn = transform.position;
        baseScale = transform.localScale;
        baseSpeed = speed;
        baseJump = jumpForce;
        if (deathFrames == null || deathFrames.Length == 0) // not set in the Inspector: use Assets/Resources/RabbitDeath
        {
            deathFrames = Resources.LoadAll<Sprite>("RabbitDeath");
            Array.Sort(deathFrames, (a, b) => string.CompareOrdinal(a.name, b.name));
        }
    }
    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        if (canMove && transform.position.y < pitDeathY) die(); // fell into a pit
        if (canMove)
        {
            // Move through the Rigidbody so walls and ceilings block her.
            // Setting transform.position teleported her into colliders and she squeezed through gaps.
            // On a moving plate, add its sideways speed so she rides along.
            float carry = plate != null ? plate.Velocity.x : 0f;
            rb.linearVelocity = new Vector3(x * speed + carry, rb.linearVelocity.y, 0f);
            if (sprite != null && x != 0) sprite.flipX = (x < 0) != spriteFacesLeft;
        }
        if (!dying) animate(canMove && x != 0f);
        if (canMove && canJump && y > 0)
        {
            jump();
        }
        if (canMove && Input.GetKeyDown(KeyCode.Q) && size != PlayerSize.Small) 
        {
            transform.localScale /= 2;
            if (size == PlayerSize.Medium)
            {
                speed /= speedScalar;
                jumpForce /= smallJumpScalar;
                size = PlayerSize.Small;
            }
            if (size == PlayerSize.Large)
            {
                speed *= speedScalar;
                jumpForce /= largeJumpScalar;
                size = PlayerSize.Medium;
            }
        }
        if (canMove && Input.GetKeyDown(KeyCode.E) && size != PlayerSize.Large)
        {
            if (size == PlayerSize.Medium && maxSize == PlayerSize.Large)
            {
                speed /= speedScalar;
                jumpForce *= largeJumpScalar;
                size = PlayerSize.Large;
                transform.position = new Vector3(transform.position.x, transform.position.y + transformOffset, transform.position.z);
                transform.localScale *= 2;
            }
            if (size == PlayerSize.Small && maxSize != PlayerSize.Small)
            {
                speed *= speedScalar;
                jumpForce *= smallJumpScalar;
                size = PlayerSize.Medium;
                transform.position = new Vector3(transform.position.x, transform.position.y + transformOffset, transform.position.z);
                transform.localScale *= 2;
            }
        }
    }
    private void animate(bool running)
    {
        Sprite[] frames = running ? runFrames : idleFrames;
        if (sprite == null || frames == null || frames.Length == 0) return;
        if (running != wasRunning) animTime = 0f; // start each cycle from its first frame
        wasRunning = running;
        animTime += Time.deltaTime;
        float fps = running ? runFps : idleFps;
        sprite.sprite = frames[(int)(animTime * fps) % frames.Length];
    }
    public void jump()
    {
        canJump = false;
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }
    private void OnCollisionEnter(Collision other)
    {
        // other.collider, not other.gameObject: for parts of a moving object (fork tines) gameObject
        // is the parent that owns the Rigidbody, which isn't tagged.
        if (other.collider.CompareTag("Floor") && other.collider.bounds.min.y < GetComponent<Collider>().bounds.min.y && !canJump)
        {
            canJump = true;
        }
        if (other.collider.CompareTag("Obstacle"))
        {
            die(); // hazards send her back to the last lit lamp
        }
    }
    private void OnCollisionStay(Collision other)
    {
        MovingPlate p = other.gameObject.GetComponent<MovingPlate>();
        if (p != null && transform.position.y > other.transform.position.y) plate = p;
    }
    private void OnCollisionExit(Collision other)
    {
        if (plate != null && other.gameObject == plate.gameObject) plate = null;
    }
    public void endGame(DeathType deathType)
    {
        canMove = false;
        rb.constraints = RigidbodyConstraints.FreezeAll;
        if (deathType == DeathType.Timeout)
        {
            gameOver = true;
            WakeUpScreen.Show(); // she wakes up from the nightmare in her empty room
        }
        if (deathType == DeathType.Obstacle)
        {
            // death animation
        }
    }
    public void setRespawn(Vector3 respawn)
    {
        currentRespawn = respawn;
    }
    public int ItemCount => collectables.Count;
    // Items picked up inside one level (its root object), so each door only counts its own level's memories.
    public int ItemsIn(Transform level) => collectables.FindAll(c => c.transform.IsChildOf(level)).Count;
    // Stop input and physics, e.g. while walking into the door.
    public void lockControls()
    {
        canMove = false;
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;
    }
    public void unlockControls()
    {
        rb.isKinematic = false;
        canMove = true;
    }
    // Death: freeze, play the death frames, then back to the last lit lamp.
    public void die()
    {
        if (dying || gameOver) return;
        StartCoroutine(DeathRoutine());
    }
    private System.Collections.IEnumerator DeathRoutine()
    {
        dying = true;
        lockControls();
        // In a pit, lift her so the whole body sits above the bottom of the ground (y -3), inside the camera's view.
        if (sprite != null && sprite.bounds.min.y < -3f)
            transform.position += Vector3.up * (-3f - sprite.bounds.min.y);
        if (sprite != null && deathFrames != null)
            foreach (Sprite f in deathFrames)
            {
                sprite.sprite = f;
                yield return new WaitForSeconds(1f / deathFps);
            }
        yield return new WaitForSeconds(deathHold);
        if (gameOver) yield break; // the timer ran out mid-death: stay down, the wake-up screen takes over
        unlockControls(); // before respawn: a kinematic body can't have its velocity set
        respawn();
        dying = false;
    }
    public void respawn()
    {
        // Back to Regular size, and the blocks of the section she is retrying back to their starts, so nothing softlocks.
        size = PlayerSize.Medium;
        transform.localScale = baseScale;
        speed = baseSpeed;
        jumpForce = baseJump;
        Pushable.ResetAhead(currentRespawn.x);
        rb.linearVelocity = Vector3.zero;
        rb.position = currentRespawn;
        transform.position = currentRespawn;
        CreepingWall.ResetAll(currentRespawn.x); // Level 3's wall restarts a little behind her
    }
    public void obtainCollectable(CollectableScript collectable)
    {
        collectables.Add(collectable);
        updateCollectableDisplay();
    }
    public void updateCollectableDisplay()
    {
        if (startingCollectableObject == null) return; // no display set up in this scene; the count still works
        while (collectables.Count < collectableObjects.Count)
        {
            Destroy(collectableObjectsBack());
            collectableObjects.RemoveAt(collectableObjects.Count - 1);
        }
        while (collectables.Count > collectableObjects.Count)
        {
            collectableObjects.Add(Instantiate<GameObject>(startingCollectableObject));
            collectableObjectsBack().GetComponent<Image>().sprite = collectables[collectableObjects.Count - 1].displayImage;
            collectableObjectsBack().transform.position = new Vector3(startingCollectableObject.transform.position.x + collectableDisplayOffset * collectableObjects.Count, startingCollectableObject.transform.position.y, startingCollectableObject.transform.position.z);
            collectableObjectsBack().SetActive(true);
        }
    }
    private GameObject collectableObjectsBack()
    {
        return collectableObjects[collectableObjects.Count - 1];
    }
    public void setMaxSize(PlayerSize max)
    {
        maxSize = max;
    }
}
