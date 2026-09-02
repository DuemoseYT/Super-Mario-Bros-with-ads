using System.Collections;
using UnityEngine;

/// <summary>
/// Classic "question block" — hit it from below and it swaps to an empty sprite,
/// does a small bump animation, and pops an item out the top.
/// Requires a solid (non-trigger) Collider2D and a SpriteRenderer.
/// </summary>
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class LuckyBlock : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private Sprite emptySprite; // shown after the block has been hit
    // "full" sprite is auto-cached from whatever's already on the SpriteRenderer

    [Header("Item")]
    [SerializeField] private GameObject itemPrefab; // e.g. a Coin prefab, or a power-up
    [SerializeField] private float popHeight = 1f;    // how far up the item rises out of the block
    [SerializeField] private float popDuration = 0.4f;

    [Header("Bump Animation")]
    [SerializeField] private float bumpHeight = 0.15f;
    [SerializeField] private float bumpDuration = 0.1f;

    [Header("Behavior")]
    [SerializeField] private bool oneTimeUse = true;

    private SpriteRenderer spriteRenderer;
    private Sprite fullSprite;
    private bool isUsed;
    private Vector3 blockStartPos;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        fullSprite = spriteRenderer.sprite;
        blockStartPos = transform.position;
    }

    /// <summary>
    /// Call this from a small trigger zone positioned just below the block
    /// (see LuckyBlockTrigger.cs) when the player enters it moving upward.
    /// </summary>
    public void TriggerHit()
    {
        if (isUsed && oneTimeUse) return;

        if (oneTimeUse)
            isUsed = true;

        if (emptySprite != null)
            spriteRenderer.sprite = emptySprite;

        StartCoroutine(BumpAnimation());
        SpawnItem();
    }

    private IEnumerator BumpAnimation()
    {
        Vector3 bumpedPos = blockStartPos + Vector3.up * bumpHeight;

        float t = 0f;
        while (t < bumpDuration)
        {
            transform.position = Vector3.Lerp(blockStartPos, bumpedPos, t / bumpDuration);
            t += Time.deltaTime;
            yield return null;
        }

        t = 0f;
        while (t < bumpDuration)
        {
            transform.position = Vector3.Lerp(bumpedPos, blockStartPos, t / bumpDuration);
            t += Time.deltaTime;
            yield return null;
        }

        transform.position = blockStartPos;
    }

    private void SpawnItem()
    {
        if (itemPrefab == null) return;

        GameObject item = Instantiate(itemPrefab, transform.position, Quaternion.identity);

        // If the item has its own physics or floating behavior, pause it during
        // the pop-up animation so our manual position lerp isn't fought over.
        Rigidbody2D itemRb = item.GetComponent<Rigidbody2D>();
        RigidbodyType2D originalBodyType = RigidbodyType2D.Dynamic;
        if (itemRb != null)
        {
            originalBodyType = itemRb.bodyType;
            itemRb.bodyType = RigidbodyType2D.Kinematic;
        }

        CoinFloat coinFloat = item.GetComponent<CoinFloat>();
        if (coinFloat != null)
            coinFloat.enabled = false;

        StartCoroutine(PopItem(item, itemRb, originalBodyType, coinFloat));
    }

    private IEnumerator PopItem(GameObject item, Rigidbody2D itemRb, RigidbodyType2D originalBodyType, CoinFloat coinFloat)
    {
        if (item == null) yield break;

        Vector3 start = item.transform.position;
        Vector3 target = start + Vector3.up * popHeight;

        float t = 0f;
        while (t < popDuration)
        {
            if (item == null) yield break;
            item.transform.position = Vector3.Lerp(start, target, t / popDuration);
            t += Time.deltaTime;
            yield return null;
        }

        if (item == null) yield break;
        item.transform.position = target;

        // Hand control back to the item's own components now that it's fully popped out
        if (itemRb != null)
            itemRb.bodyType = originalBodyType;

        if (coinFloat != null)
        {
            coinFloat.ResetStartPosition(); // bob around the new popped-up position, not the old spawn point
            coinFloat.enabled = true;
        }
    }
}