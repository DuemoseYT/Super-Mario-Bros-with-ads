using UnityEngine;

/// <summary>
/// Optional: place these around your level. When the player touches one,
/// it updates where RespawnManager will respawn them.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Checkpoint : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint; // if empty, uses this object's position

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        Vector3 point = spawnPoint != null ? spawnPoint.position : transform.position;

        if (RespawnManager.Instance != null)
            RespawnManager.Instance.SetSpawnPoint(point);
    }
}