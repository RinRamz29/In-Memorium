using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Transform player;
    public Vector3 offset = new Vector3(0, 1, 0);

    private void Update()
    {
        if (player != null)
        {
            transform.position = player.position + offset;
        }
    }
}
