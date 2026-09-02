using UnityEngine;

/// <summary>
/// Put this on a small child GameObject positioned just below (and slightly
/// overlapping) the lucky block's bottom edge. Give it a small BoxCollider2D
/// marked "Is Trigger", sized to span the block's width and only a thin sliver
/// of height. When the player enters it while moving upward, it tells the
/// parent LuckyBlock to trigger.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class LuckyBlockTrigger : MonoBehaviour
{
    [SerializeField] private LuckyBlock luckyBlock; // auto-found on parent if left empty
    [SerializeField] private float minUpwardVelocity = 0.1f; // ignore the player just falling past this zone

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void Awake()
    {
        if (luckyBlock == null)
            luckyBlock = GetComponentInParent<LuckyBlock>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        Rigidbody2D playerRb = other.attachedRigidbody;

        // Require upward motion so falling back down past this zone
        // (e.g. after bouncing off the block) doesn't double-trigger it.
        if (playerRb != null && playerRb.linearVelocity.y <= minUpwardVelocity)
            return;

        if (luckyBlock != null)
            luckyBlock.TriggerHit();
    }
}