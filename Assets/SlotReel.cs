using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// One reel of the slot machine. Attach to a UI Image representing a single reel slot.
/// SlotMachine.cs drives this by calling Spin().
/// </summary>
public class SlotReel : MonoBehaviour
{
    [SerializeField] private Image reelImage;
    [SerializeField] private Sprite[] symbols; // all possible symbols this reel can land on

    public int CurrentSymbolIndex { get; private set; }

    private void Reset()
    {
        reelImage = GetComponent<Image>();
    }

    /// <summary>
    /// Rapidly flickers through random symbols for the given duration, then settles
    /// on a final (also random) symbol.
    /// </summary>
    public IEnumerator Spin(float duration, float flickerInterval)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            SetRandomSymbol();
            yield return new WaitForSeconds(flickerInterval);
            elapsed += flickerInterval;
        }

        // Final settle (the last flicker above already counts as the result,
        // but this makes the "landing" symbol explicit/readable in code).
        SetRandomSymbol();
    }

    private void SetRandomSymbol()
    {
        if (symbols == null || symbols.Length == 0) return;

        CurrentSymbolIndex = Random.Range(0, symbols.Length);

        if (reelImage != null)
            reelImage.sprite = symbols[CurrentSymbolIndex];
    }
}