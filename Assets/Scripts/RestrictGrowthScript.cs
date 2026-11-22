using UnityEngine;

public class RestrictGrowthScript : MonoBehaviour
{
    public PlayerSize growthLimit;
    public BaseMovement playerScript;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            playerScript.setMaxSize(growthLimit);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            playerScript.setMaxSize(PlayerSize.Large);
        }
    }
}
