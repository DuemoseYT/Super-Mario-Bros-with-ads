using UnityEngine;

/// <summary>
/// Attach to a wide, thin trigger collider placed below the bottom of your level.
/// When the player falls into it, it tells RespawnManager to show the death menu.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class DeathPlane : MonoBehaviour
{
    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (RespawnManager.Instance != null)
            RespawnManager.Instance.PlayerDied();
        else
            Debug.LogWarning("Player hit DeathPlane but no RespawnManager found in scene.");
    }
}