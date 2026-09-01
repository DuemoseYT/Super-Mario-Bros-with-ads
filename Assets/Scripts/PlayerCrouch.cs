using UnityEngine;

/// <summary>
/// Attach to the same GameObject as PlayerMovement2D (or a component that has both).
/// Holding S (or Down Arrow) swaps to a crouch sprite and shrinks the collider.
/// Requires a SpriteRenderer and a BoxCollider2D on the player.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class PlayerCrouch : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private Sprite standSprite;
    [SerializeField] private Sprite crouchSprite;

    [Header("Collider Sizing")]
    [SerializeField] private Vector2 standColliderSize = new Vector2(1f, 2f);
    [SerializeField] private Vector2 standColliderOffset = new Vector2(0f, 0f);
    [SerializeField] private Vector2 crouchColliderSize = new Vector2(1f, 1f);
    [SerializeField] private Vector2 crouchColliderOffset = new Vector2(0f, -0.5f);

    [Header("Input")]
    [SerializeField] private KeyCode crouchKey = KeyCode.S;

    [Header("Optional Link")]
    [SerializeField] private PlayerMovement2D movement; // auto-found if left empty

    private SpriteRenderer spriteRenderer;
    private BoxCollider2D bodyCollider;
    private bool isCrouching;

    public bool IsCrouching => isCrouching;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        bodyCollider = GetComponent<BoxCollider2D>();

        if (movement == null)
            movement = GetComponent<PlayerMovement2D>();

        // Cache stand sprite from whatever is already assigned, if not set explicitly
        if (standSprite == null)
            standSprite = spriteRenderer.sprite;
    }

    private void Update()
    {
        bool wantsCrouch = Input.GetKey(crouchKey);

        if (wantsCrouch != isCrouching)
            SetCrouch(wantsCrouch);
    }

    private void SetCrouch(bool state)
    {
        isCrouching = state;

        // Swap sprite
        spriteRenderer.sprite = state ? crouchSprite : standSprite;

        // Resize collider
        bodyCollider.size = state ? crouchColliderSize : standColliderSize;
        bodyCollider.offset = state ? crouchColliderOffset : standColliderOffset;

        // Tell movement script so it can slow down / block jump
        if (movement != null)
            movement.SetCrouching(state);
    }
}