using UnityEngine;

/// <summary>
/// Purely code-driven floating/bobbing animation. No Animator or Animation clips needed.
/// Attach this alongside Coin.cs on the same coin GameObject.
/// Uses a sine wave for smooth up-down motion, with optional gentle rotation.
/// </summary>
public class CoinFloat : MonoBehaviour
{
    [Header("Bobbing")]
    [SerializeField] private float bobHeight = 0.25f;   // how far up/down it moves
    [SerializeField] private float bobSpeed = 2f;        // cycles per second-ish

    [Header("Rotation (optional)")]
    [SerializeField] private bool rotate = true;
    [SerializeField] private float rotationSpeed = 90f;  // degrees per second

    private Vector3 startPos;
    private float randomOffset; // desyncs multiple coins so they don't bob in unison

    private void Awake()
    {
        startPos = transform.position;
        randomOffset = Random.Range(0f, Mathf.PI * 2f); // random phase
    }

    private void Update()
    {
        // Sine wave bob around the starting position
        float newY = startPos.y + Mathf.Sin((Time.time * bobSpeed) + randomOffset) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        if (rotate)
            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime, Space.Self);
        // For a 3D-looking spin on a 2D sprite (like classic Mario coins), use:
        // transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f, Space.Self);
    }
}