using UnityEngine;

/// <summary>
/// Attach to a coin GameObject with a Collider2D set to "Is Trigger".
/// The player object must have a Rigidbody2D and a Collider2D, and be tagged "Player"
/// (or adjust the tag check below to match your setup).
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Coin : MonoBehaviour
{
    [SerializeField] private int value = 1;
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private GameObject pickupEffect; // optional particle/vfx prefab

    private void Reset()
    {
        // Auto-set the collider to trigger mode when the component is added
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Add to the counter
        if (CoinCounter.Instance != null)
            CoinCounter.Instance.AddCoin(value);
        else
            Debug.LogWarning("Coin collected but no CoinCounter found in scene.");

        // Optional feedback
        if (pickupSound != null)
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);

        if (pickupEffect != null)
            Instantiate(pickupEffect, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}