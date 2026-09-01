using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Singleton that manages the death/respawn menu.
/// Put this on a manager GameObject and assign the UI references in the Inspector.
/// </summary>
public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject deathMenuPanel; // the whole UI panel, disabled by default
    [SerializeField] private Button respawnButton;
    [SerializeField] private TextMeshProUGUI deathMessageText; // optional, e.g. "You Died!"

    [Header("Settings")]
    [SerializeField] private bool pauseOnDeath = true;
    [SerializeField] private string deathMessage = "You Died!";

    private Vector3 currentSpawnPoint;
    private Rigidbody2D playerRb;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (deathMenuPanel != null)
            deathMenuPanel.SetActive(false);

        if (player != null)
        {
            currentSpawnPoint = player.transform.position; // default spawn = wherever player starts
            playerRb = player.GetComponent<Rigidbody2D>();
        }

        if (respawnButton != null)
            respawnButton.onClick.AddListener(Respawn);
    }

    /// <summary>
    /// Call this from a checkpoint trigger to update where the player respawns.
    /// </summary>
    public void SetSpawnPoint(Vector3 newSpawnPoint)
    {
        currentSpawnPoint = newSpawnPoint;
    }

    /// <summary>
    /// Called by DeathPlane when the player falls off the level.
    /// </summary>
    public void PlayerDied()
    {
        if (deathMessageText != null)
            deathMessageText.text = deathMessage;

        if (deathMenuPanel != null)
            deathMenuPanel.SetActive(true);

        if (pauseOnDeath)
            Time.timeScale = 0f;

        // Freeze the player so it doesn't keep falling/moving under the menu
        if (player != null)
            player.SetActive(false);
    }

    /// <summary>
    /// Hooked up to the Respawn button's OnClick in the Inspector (or called automatically).
    /// </summary>
    public void Respawn()
    {
        if (deathMenuPanel != null)
            deathMenuPanel.SetActive(false);

        if (pauseOnDeath)
            Time.timeScale = 1f;

        if (player != null)
        {
            player.transform.position = currentSpawnPoint;

            if (playerRb != null)
                playerRb.linearVelocity = Vector2.zero;

            player.SetActive(true);
        }
    }
}