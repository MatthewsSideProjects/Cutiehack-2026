using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public GameObject player;
    // Vertical dead zone: the camera only moves up or down once she gets this far from its center,
    // so normal jumps don't bob the view but climbing the cake scrolls it.
    public float aboveMargin = 3f;
    public float belowMargin = 2f;
    private float minY;

    void Start()
    {
        if (player == null) player = GameObject.FindWithTag("Player");
        minY = transform.position.y; // never look lower than the starting view
    }

    void LateUpdate()
    {
        Vector3 p = player.transform.position;
        float y = transform.position.y;
        if (p.y > y + aboveMargin) y = p.y - aboveMargin;
        if (p.y < y - belowMargin) y = p.y + belowMargin;
        transform.position = new Vector3(p.x, Mathf.Max(minY, y), transform.position.z);
    }
}
