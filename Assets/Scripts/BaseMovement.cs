using NUnit.Framework;
using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEditor.AssetImporters;
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
    public float transformOffset;
    public float speedScalar;
    public float largeJumpScalar;
    public float smallJumpScalar;
    private Rigidbody rb;
    private bool canJump;
    private bool canMove = true;
    private Vector3 currentRespawn;
    private List<CollectableScript> collectables = new List<CollectableScript>();
    private List<GameObject> collectableObjects = new List<GameObject>();
    private PlayerSize size = PlayerSize.Medium;
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
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
        }
        if (canMove && canJump && y > 0)
        {
            jump();
        }
        if (Input.GetKeyDown(KeyCode.Q) && size != PlayerSize.Small) 
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
        if (Input.GetKeyDown(KeyCode.E) && size != PlayerSize.Large)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y + transformOffset, transform.position.z);
            transform.localScale *= 2;
            if (size == PlayerSize.Medium)
            {
                speed /= speedScalar;
                jumpForce *= largeJumpScalar;
                size = PlayerSize.Large;
            }
            if (size == PlayerSize.Small)
            {
                speed *= speedScalar;
                jumpForce *= smallJumpScalar;
                size = PlayerSize.Medium;
            }
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
