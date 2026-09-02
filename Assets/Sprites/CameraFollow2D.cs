using UnityEngine;

/// <summary>
/// Side-scrolling camera, classic Mario style. Attach to your Main Camera.
/// Follows the player horizontally with smoothing; vertical follow and
/// "no backtracking" (camera never scrolls left, like NES Mario) are both optional.
/// </summary>
public class CameraFollow2D : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target; // usually the player
    [SerializeField] private Vector2 offset = new Vector2(0f, 1f);

    [Header("Smoothing")]
    [SerializeField] private float smoothTime = 0.15f;

    [Header("Horizontal Behavior")]
    [Tooltip("Classic NES Mario: the camera only ever moves right, never scrolls back left even if the player walks backward.")]
    [SerializeField] private bool preventBacktrackX = true;

    [Header("Vertical Behavior")]
    [Tooltip("If true, the camera's Y position stays fixed (classic side-scroller feel). If false, it follows the player's height too.")]
    [SerializeField] private bool lockVertical = true;
    [SerializeField] private float fixedY = 0f; // used only if lockVertical is true; set to your level's camera height

    [Header("Level Bounds (optional)")]
    [SerializeField] private bool useBounds = false;
    [SerializeField] private float minX;
    [SerializeField] private float maxX;
    [SerializeField] private float minY;
    [SerializeField] private float maxY;

    private Vector3 velocity = Vector3.zero;
    private float maxReachedX; // tracks the furthest right the camera has gone, for no-backtrack mode

    private void Start()
    {
        if (target != null)
            maxReachedX = target.position.x + offset.x;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        float targetX = target.position.x + offset.x;
        float targetY = lockVertical ? fixedY : target.position.y + offset.y;

        if (preventBacktrackX)
        {
            // Only ever allow the camera to move further right, never back left
            maxReachedX = Mathf.Max(maxReachedX, targetX);
            targetX = maxReachedX;
        }

        Vector3 desiredPosition = new Vector3(targetX, targetY, transform.position.z);

        if (useBounds)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);
        }

        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
    }
}