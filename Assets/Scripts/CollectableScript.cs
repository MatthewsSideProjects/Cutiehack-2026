using UnityEngine;

public class CollectableScript : MonoBehaviour
{
    public BaseMovement playerScript;
    public string name;
    public Sprite displayImage;
    private Renderer renderer; // mesh (old spheres) or sprite (pixel-art items)
    private CollectableScript thisScript;
    private void Start()
    {
        renderer = GetComponent<Renderer>();
        thisScript = GetComponent<CollectableScript>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (renderer.enabled && other.gameObject.tag == "Player")
        {
            renderer.enabled = false;
            playerScript.obtainCollectable(thisScript);
        }
    }
}
