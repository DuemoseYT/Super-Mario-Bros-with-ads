using UnityEngine;
using TMPro;

/// <summary>
/// Singleton that tracks total coins and updates a TextMeshPro label.
/// Coins persist across scenes and game restarts via PlayerPrefs.
/// Put this on a manager GameObject (or your Canvas) and assign the TMP text field.
/// </summary>
public class CoinCounter : MonoBehaviour
{
    public static CoinCounter Instance { get; private set; }

    // Shared key so MainMenuCoinDisplay reads the same saved value.
    public const string SaveKey = "TotalCoins";

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

        // Load previously saved total (defaults to 0 if never saved before)
        coinCount = PlayerPrefs.GetInt(SaveKey, 0);

        UpdateText();
    }

    public void AddCoin(int amount = 1)
    {
        coinCount += amount;

        // Save immediately so the total survives scene loads / quitting the game
        PlayerPrefs.SetInt(SaveKey, coinCount);
        PlayerPrefs.Save();

        UpdateText();
    }

    /// <summary>
    /// Attempts to spend coins (e.g. for a bet or purchase).
    /// Returns false and changes nothing if the player doesn't have enough.
    /// </summary>
    public bool SpendCoins(int amount)
    {
        if (amount <= 0) return true;
        if (coinCount < amount) return false;

        coinCount -= amount;
        PlayerPrefs.SetInt(SaveKey, coinCount);
        PlayerPrefs.Save();

        UpdateText();
        return true;
    }

    /// <summary>
    /// Optional: call this to wipe saved coins, e.g. from a "New Game" or debug button.
    /// </summary>
    public void ResetCoins()
    {
        coinCount = 0;
        PlayerPrefs.SetInt(SaveKey, 0);
        PlayerPrefs.Save();
        UpdateText();
    }

    private void UpdateText()
    {
        if (coinText != null)
            coinText.text = string.Format(format, coinCount);
    }

    public int GetCoinCount() => coinCount;
}