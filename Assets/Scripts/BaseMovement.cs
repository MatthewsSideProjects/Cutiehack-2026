using NUnit.Framework;
using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
public enum DeathType {Timeout, Obstacle}
public class BaseMovement : MonoBehaviour
{
    public float speed;
    public float jumpForce;
    public float gravityMod;
    public TimerScript timerScript;
    public GameObject startingCollectableObject;
    public float collectableDisplayOffset;
    public bool spriteFacesLeft; // tick if the rabbit faces the wrong way when walking
    private Rigidbody rb;
    private SpriteRenderer sprite;
    private bool canJump;
    private bool canMove = true;
    private Vector3 currentRespawn;
    private List<CollectableScript> collectables = new List<CollectableScript>();
    private List<GameObject> collectableObjects = new List<GameObject>();
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        sprite = GetComponentInChildren<SpriteRenderer>();
        Physics.gravity = new Vector3(0, -1f * gravityMod, 0);
        currentRespawn = transform.position;
    }
    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        if (canMove)
        {
            Vector3 direction = new Vector3(x, 0f, 0f);
            transform.position += direction * speed * Time.deltaTime;
            if (sprite != null && x != 0) sprite.flipX = (x < 0) != spriteFacesLeft;
        }
        if (canMove && canJump && y > 0)
        {
            jump();
        }
    }
    public void jump()
    {
        canJump = false;
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Floor" && transform.position.y > other.transform.position.y)
        {
            canJump = true;
        }
        if (other.gameObject.tag == "Obstacle")
        {
            endGame(DeathType.Obstacle);
        }
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
    public void obtainCollectable(CollectableScript collectable)
    {
        collectables.Add(collectable);
        updateCollectableDisplay();
    }
    public void updateCollectableDisplay()
    {
        //while (collectables.Count < collectableObjects.Count)
        //{
        //    Destroy(collectableObjectsBack());
        //}
    }
    private GameObject collectableObjectsBack()
    {
        return collectableObjects[collectableObjects.Count - 1];
    }
}
