using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public GameObject player;
    void Start()
    {
        if (player == null) player = GameObject.FindWithTag("Player");
    }
    void LateUpdate()
    {
        transform.position = new Vector3(player.transform.position.x, transform.position.y, transform.position.z);
    }
}
