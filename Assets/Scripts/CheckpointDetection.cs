using UnityEngine;

public class CheckpointDetection : MonoBehaviour
{
    public BaseMovement playerScript;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            playerScript.setRespawn(transform.position);
        }
    }
}
