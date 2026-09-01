using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

/// <summary>
/// Place on a trigger zone positioned above a pipe.
/// While the player is standing in the zone and presses the crouch key (S),
/// the player sinks down into the pipe, then a video plays, then the next scene loads.
///
/// Requires:
/// - A Collider2D on this GameObject set to "Is Trigger"
/// - A VideoPlayer + RawImage/Canvas set up in the scene (see setup notes)
/// - PlayerMovement2D and PlayerCrouch on the player (referenced automatically via trigger)
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class PipeTransition : MonoBehaviour
{
    [Header("Pipe")]
    [SerializeField] private KeyCode enterKey = KeyCode.S;
    [SerializeField] private Transform pipeBottom;   // where the player sinks to
    [SerializeField] private float sinkSpeed = 2f;    // units per second while sinking

    [Header("Video")]
    [SerializeField] private VideoPlayer videoPlayer;       // assign the VideoPlayer in your scene
    [SerializeField] private GameObject videoCanvasObject;  // the Canvas/panel holding the video display, disabled by default

    [Header("Next Scene")]
    [SerializeField] private string nextSceneName;

    private bool playerInZone;
    private bool isTransitioning;

    private GameObject player;
    private PlayerMovement2D playerMovement;
    private Rigidbody2D playerRb;

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void Awake()
    {
        if (videoCanvasObject != null)
            videoCanvasObject.SetActive(false);
    }

    private bool IsPlayer(Collider2D other)
    {
        // Checking via attachedRigidbody means a dedicated child "detector" collider
        // on the player (separate from the crouch-resizable body collider) still counts,
        // since it shares the same Rigidbody2D as the root player object.
        if (other.attachedRigidbody != null)
            return other.attachedRigidbody.CompareTag("Player");
        return other.CompareTag("Player");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsPlayer(other)) return;

        playerInZone = true;
        player = other.attachedRigidbody != null ? other.attachedRigidbody.gameObject : other.gameObject;
        playerMovement = player.GetComponent<PlayerMovement2D>();
        playerRb = player.GetComponent<Rigidbody2D>();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsPlayer(other)) return;

        playerInZone = false;
    }

    private void Update()
    {
        if (playerInZone && !isTransitioning && Input.GetKey(enterKey))
        {
            StartCoroutine(EnterPipeSequence());
        }
    }

    private IEnumerator EnterPipeSequence()
    {
        isTransitioning = true;

        // Lock out normal control
        if (playerMovement != null)
            playerMovement.enabled = false;
        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
            // Kinematic so physics doesn't resolve/push us back out while we
            // manually move the player into the pipe's solid collider.
            playerRb.bodyType = RigidbodyType2D.Kinematic;
        }

        // Sink the player down into the pipe
        if (pipeBottom != null && player != null)
        {
            while (Vector2.Distance(player.transform.position, pipeBottom.position) > 0.02f)
            {
                player.transform.position = Vector2.MoveTowards(
                    player.transform.position,
                    pipeBottom.position,
                    sinkSpeed * Time.deltaTime
                );
                yield return null;
            }
        }

        // Hide the player once fully in the pipe
        if (player != null)
            player.SetActive(false);

        // Play the video
        if (videoPlayer != null)
        {
            if (videoCanvasObject != null)
                videoCanvasObject.SetActive(true);

            videoPlayer.Prepare();
            while (!videoPlayer.isPrepared)
                yield return null;

            videoPlayer.Play();

            // Wait until the video finishes
            while (videoPlayer.isPlaying)
                yield return null;
        }

        // Load the next scene
        SceneManager.LoadScene(nextSceneName);
    }
}