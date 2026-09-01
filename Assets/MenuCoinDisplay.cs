using UnityEngine;
using TMPro;

/// <summary>
/// Attach to a GameObject in your Main Menu scene and assign a TextMeshPro label.
/// Reads the coin total saved by CoinCounter (via PlayerPrefs) and displays it —
/// no gameplay logic here, just a read-only display.
/// </summary>
public class MainMenuCoinDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private string format = "Coins: {0}";

    private void Start()
    {
        int savedCoins = PlayerPrefs.GetInt(CoinCounter.SaveKey, 0);

        if (coinText != null)
            coinText.text = string.Format(format, savedCoins);
    }
}