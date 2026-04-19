using UnityEngine;

public class CollectibleObject : MonoBehaviour
{
    [SerializeField] public float objectSizeValue = 1f;
    [SerializeField] private float pushbackForce = 8f;

    private bool isCollected;

    private void OnCollisionEnter(Collision collision)
    {
        if (isCollected)
        {
            return;
        }

        PlayerStats player = collision.gameObject.GetComponent<PlayerStats>();
        if (player == null)
        {
            return;
        }

        if (player.playerCurrentSize > objectSizeValue)
        {
            CollectObject(player);
        }
        else
        {
            ApplyPushback(collision.gameObject.GetComponent<Rigidbody>(), collision.contacts[0].point);
        }
    }

    private void CollectObject(PlayerStats player)
    {
        isCollected = true;
        player.Grow(objectSizeValue);
        Destroy(gameObject);
    }

    private void ApplyPushback(Rigidbody playerRigidbody, Vector3 contactPoint)
    {
        if (playerRigidbody == null)
        {
            return;
        }

        Vector3 direction = (playerRigidbody.position - contactPoint).normalized;
        playerRigidbody.AddForce(direction * pushbackForce, ForceMode.Impulse);
    }
}
