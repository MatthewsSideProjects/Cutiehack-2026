using System;
using UnityEngine;
public enum DeathType {Timeout, Obstacle}
public class BaseMovement : MonoBehaviour
{
    public float speed;
    public float jumpForce;
    public float gravityMod;
    public TimerScript timerScript;
    private Rigidbody rb;
    private bool canJump;
    private bool canMove = true;
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        Physics.gravity = new Vector3(0, -1f *gravityMod, 0);
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
    }
    public void endGame(DeathType deathType)
    {
        canMove = false;
        if (deathType == DeathType.Timeout)
        {
            // timeout cutscene or death
        }
        if (deathType == DeathType.Obstacle)
        {
            // death animation
        }
    }
}
