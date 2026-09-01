using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Slot machine gambling minigame. Bets coins through CoinCounter, spins a set of
/// SlotReel objects, and pays out based on how many reels match.
/// Works with any number of reels (2, 3, 5...) — just assign them in the Inspector.
/// </summary>
public class SlotMachine : MonoBehaviour
{
    [Header("Reels")]
    [SerializeField] private SlotReel[] reels;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI betText;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private Button spinButton;
    [SerializeField] private Button increaseBetButton;
    [SerializeField] private Button decreaseBetButton;

    [Header("Betting")]
    [SerializeField] private int betAmount = 10;
    [SerializeField] private int betStep = 5;
    [SerializeField] private int minBet = 5;
    [SerializeField] private int maxBet = 100;

    [Header("Spin Feel")]
    [SerializeField] private float baseSpinDuration = 1f;   // how long the first reel spins
    [SerializeField] private float reelStaggerDelay = 0.3f;  // each subsequent reel spins a bit longer, so they stop in sequence
    [SerializeField] private float flickerInterval = 0.08f;  // how fast symbols cycle while spinning

    [Header("Payouts (multiplier applied to bet)")]
    [SerializeField] private float allMatchMultiplier = 5f;  // every reel lands on the same symbol
    [SerializeField] private float twoMatchMultiplier = 1.5f; // at least two reels match (only used if 3+ reels)

    private bool isSpinning;

    private void Start()
    {
        UpdateBetText();

        if (increaseBetButton != null)
            increaseBetButton.onClick.AddListener(IncreaseBet);
        if (decreaseBetButton != null)
            decreaseBetButton.onClick.AddListener(DecreaseBet);
        if (spinButton != null)
            spinButton.onClick.AddListener(Spin);
    }

    public void IncreaseBet()
    {
        betAmount = Mathf.Min(betAmount + betStep, maxBet);
        UpdateBetText();
    }

    public void DecreaseBet()
    {
        betAmount = Mathf.Max(betAmount - betStep, minBet);
        UpdateBetText();
    }

    private void UpdateBetText()
    {
        if (betText != null)
            betText.text = $"Bet: {betAmount}";
    }

    /// <summary>
    /// Hook up to your "Spin" button's OnClick (also auto-hooked in Start()).
    /// </summary>
    public void Spin()
    {
        if (isSpinning) return;

        if (CoinCounter.Instance == null)
        {
            Debug.LogWarning("SlotMachine: no CoinCounter found in scene.");
            return;
        }

        if (!CoinCounter.Instance.SpendCoins(betAmount))
        {
            if (resultText != null)
                resultText.text = "Not enough coins!";
            return;
        }

        StartCoroutine(SpinRoutine());
    }

    private IEnumerator SpinRoutine()
    {
        isSpinning = true;
        SetInteractable(false);

        if (resultText != null)
            resultText.text = "";

        // Kick off every reel; each one spins slightly longer than the last so they
        // stop one at a time instead of all snapping still simultaneously.
        Coroutine[] spins = new Coroutine[reels.Length];
        for (int i = 0; i < reels.Length; i++)
        {
            float duration = baseSpinDuration + (i * reelStaggerDelay);
            spins[i] = StartCoroutine(reels[i].Spin(duration, flickerInterval));
        }

        for (int i = 0; i < reels.Length; i++)
            yield return spins[i];

        EvaluateResult();

        isSpinning = false;
        SetInteractable(true);
    }

    private void EvaluateResult()
    {
        // Count how many reels landed on each symbol index
        Dictionary<int, int> frequency = new Dictionary<int, int>();
        foreach (SlotReel reel in reels)
        {
            int symbol = reel.CurrentSymbolIndex;
            if (!frequency.ContainsKey(symbol))
                frequency[symbol] = 0;
            frequency[symbol]++;
        }

        int maxMatches = 0;
        foreach (int count in frequency.Values)
            if (count > maxMatches) maxMatches = count;

        int winnings = 0;
        string message;

        if (maxMatches >= reels.Length)
        {
            // Every reel matches
            winnings = Mathf.RoundToInt(betAmount * allMatchMultiplier);
            message = $"JACKPOT! +{winnings} coins";
        }
        else if (maxMatches >= 2)
        {
            winnings = Mathf.RoundToInt(betAmount * twoMatchMultiplier);
            message = $"Winner! +{winnings} coins";
        }
        else
        {
            message = "No match — try again!";
        }

        if (winnings > 0 && CoinCounter.Instance != null)
            CoinCounter.Instance.AddCoin(winnings);

        if (resultText != null)
            resultText.text = message;
    }

    private void SetInteractable(bool value)
    {
        if (spinButton != null) spinButton.interactable = value;
        if (increaseBetButton != null) increaseBetButton.interactable = value;
        if (decreaseBetButton != null) decreaseBetButton.interactable = value;
    }
}