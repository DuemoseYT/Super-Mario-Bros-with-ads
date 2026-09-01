using UnityEngine;

/// <summary>
/// Debug/testing utility: wipes all saved PlayerPrefs (coins, etc.) with one button click.
/// Wire a UI Button's OnClick to ResetAllPlayerPrefs().
/// Consider removing this script (or hiding its button) before shipping a real build.
/// </summary>
public class DebugResetPlayerPrefs : MonoBehaviour
{
    [SerializeField] private bool logToConsole = true;

    /// <summary>
    /// Hook up to a "Reset PlayerPrefs" button's OnClick.
    /// Wipes ALL saved keys, not just coins — use ResetCoinsOnly() if you want to be more targeted.
    /// </summary>
    public void ResetAllPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        // Also reset the in-memory coin count so the UI updates immediately
        // without needing a scene reload.
        if (CoinCounter.Instance != null)
            CoinCounter.Instance.ResetCoins();

        if (BattlePassManager.Instance != null)
            BattlePassManager.Instance.ResetProgress();

        if (logToConsole)
            Debug.Log("All PlayerPrefs have been reset.");
    }

    /// <summary>
    /// Narrower option: only resets the saved coin total, leaving any other
    /// PlayerPrefs keys (settings, unlocks, etc.) untouched.
    /// </summary>
    public void ResetCoinsOnly()
    {
        PlayerPrefs.DeleteKey(CoinCounter.SaveKey);
        PlayerPrefs.Save();

        if (CoinCounter.Instance != null)
            CoinCounter.Instance.ResetCoins();

        if (logToConsole)
            Debug.Log("Coin total has been reset.");
    }
}