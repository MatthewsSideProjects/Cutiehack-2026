using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public GameObject player;
    void LateUpdate()
    {
        transform.position = new Vector3(player.transform.position.x, transform.position.y, transform.position.z);
    }
}
