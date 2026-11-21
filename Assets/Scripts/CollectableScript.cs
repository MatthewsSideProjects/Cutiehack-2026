using UnityEngine;

public class CollectableScript : MonoBehaviour
{
    public BaseMovement playerScript;
    public string name;
    public Sprite displayImage;
    private MeshRenderer renderer;
    private CollectableScript thisScript;
    private void Start()
    {
        renderer = GetComponent<MeshRenderer>();
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
