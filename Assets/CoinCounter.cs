using UnityEngine;
using TMPro;

/// <summary>
/// Singleton that tracks total coins and updates a TextMeshPro label.
/// Put this on a manager GameObject (or your Canvas) and assign the TMP text field.
/// </summary>
public class CoinCounter : MonoBehaviour
{
    public static CoinCounter Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private string format = "x {0}"; // e.g. "x 12"

    private int coinCount = 0;

    private void Awake()
    {
        // Simple singleton so Coin.cs can find this from anywhere
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        UpdateText();
    }

    public void AddCoin(int amount = 1)
    {
        coinCount += amount;
        UpdateText();
    }

    private void UpdateText()
    {
        if (coinText != null)
            coinText.text = string.Format(format, coinCount);
    }

    public int GetCoinCount() => coinCount;
}