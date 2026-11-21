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
    public float idleFps = 6f;
    public float runFps = 10f;
    public float fallLimit = -8f; // below this y she counts as fallen into a pit
    private MovingPlate plate; // the moving plate she is standing on, if any
    private float animTime;
    private bool wasRunning;
    public float transformOffset;
    public float speedNerf;
    private Rigidbody rb;
    private SpriteRenderer sprite;
    private bool canJump;
    private bool canMove = true;
    private Vector3 currentRespawn;
    private List<CollectableScript> collectables = new List<CollectableScript>();
    private List<GameObject> collectableObjects = new List<GameObject>();
    private PlayerSize size = PlayerSize.Medium;
    private Vector3 baseScale; // Regular-size scale and speed, restored on respawn
    private float baseSpeed;
    public PlayerSize Size => size;
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        sprite = GetComponentInChildren<SpriteRenderer>();
        Physics.gravity = new Vector3(0, -1f * gravityMod, 0);
        currentRespawn = transform.position;
        baseScale = transform.localScale;
        baseSpeed = speed;
    }
    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        if (canMove && transform.position.y < fallLimit) respawn(); // fell into a pit
        if (canMove)
        {
            // Move through the Rigidbody so walls and ceilings block her.
            // Setting transform.position teleported her into colliders and she squeezed through gaps.
            // On a moving plate, add its sideways speed so she rides along.
            float carry = plate != null ? plate.Velocity.x : 0f;
            rb.linearVelocity = new Vector3(x * speed + carry, rb.linearVelocity.y, 0f);
            if (sprite != null && x != 0) sprite.flipX = (x < 0) != spriteFacesLeft;
        }
        animate(canMove && x != 0f);
        if (canMove && canJump && y > 0)
        {
            jump();
        }
        if (canMove && Input.GetKeyDown(KeyCode.Q) && size != PlayerSize.Small) 
        {
            transform.localScale /= 2;
            if (size == PlayerSize.Medium)
            {
                speed -= speedNerf;
                size = PlayerSize.Small;
            }
            if (size == PlayerSize.Large)
            {
                speed += speedNerf;
                size = PlayerSize.Medium;
            }
        }
        if (canMove && Input.GetKeyDown(KeyCode.E) && size != PlayerSize.Large)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y + transformOffset, transform.position.z);
            transform.localScale *= 2;
            if (size == PlayerSize.Medium)
            {
                speed -= speedNerf;
                size = PlayerSize.Large;
            }
            if (size == PlayerSize.Small)
            {
                speed += speedNerf;
                size = PlayerSize.Medium;
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
        if (other.collider.CompareTag("Floor") && transform.position.y > other.transform.position.y)
        {
            canJump = true;
        }
        if (other.collider.CompareTag("Obstacle"))
        {
            respawn(); // hazards send her back to the last lit lamp
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
            // timeout cutscene or death
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
    public void respawn()
    {
        // Back to Regular size, and every pushable block back to its start, so nothing softlocks.
        size = PlayerSize.Medium;
        transform.localScale = baseScale;
        speed = baseSpeed;
        Pushable.ResetAll();
        rb.linearVelocity = Vector3.zero;
        rb.position = currentRespawn;
        transform.position = currentRespawn;
    }
    public void obtainCollectable(CollectableScript collectable)
    {
        collectables.Add(collectable);
        updateCollectableDisplay();
    }
    public void updateCollectableDisplay()
    {
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
}
