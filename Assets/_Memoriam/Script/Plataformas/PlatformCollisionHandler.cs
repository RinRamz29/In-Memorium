using UnityEngine;

public class PlatformCollisionHandler : MonoBehaviour
{ 
    Transform plaform;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("MovingPlatform"))
        {
            ContactPoint2D contact = collision.GetContact(0);
            if (contact.normal.y < 0.5f) return;

            plaform = collision.transform;
            transform.SetParent(plaform);
        }
    }


    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("MovingPlatform"))
        {
            transform.SetParent(null);
            plaform = null;
        }
    }
}
