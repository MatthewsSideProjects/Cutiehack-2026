using UnityEngine;

public class BaseMovement : MonoBehaviour
{
    public float speed;
    public float jumpForce;
    public float gravityMod;
    private Rigidbody rb;
    private bool canJump;
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        Physics.gravity = new Vector3(0, -1f *gravityMod, 0);
    }
    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(x, 0f, 0f);
        transform.position += direction * speed * Time.deltaTime;
        if (canJump && y > 0)
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
}
