using UnityEngine;
using UnityEngine.Events;
using TMPro;

/// <summary>
/// Tracks battle pass progress: every N coins collected = 1 level (separate from
/// CoinCounter's spendable balance, so gambling losses never reduce your level).
/// Specific levels grant specific rewards — some unlock a skin, some pay out a coin bonus —
/// matching a fixed tier list you define in the Inspector (e.g. level 5/20/40/60 = skins,
/// level 10/30/50 = coin bonuses).
///
/// Wire your existing battle pass menu to the UnityEvents below, or read the
/// public getters directly (GetLevel(), IsSkinUnlocked(), GetTiers(), etc.) whenever
/// the menu opens/refreshes.
/// </summary>
public class BattlePassManager : MonoBehaviour
{
    public enum RewardType { Skin, Coins }

    [System.Serializable]
    public class BattlePassTier
    {
        public int requiredLevel;
        public RewardType rewardType;
        public int coinAmount;  // used when rewardType == Coins
        public int skinIndex;   // used when rewardType == Skin (matches your menu's skin slot order)
    }

    public static BattlePassManager Instance { get; private set; }

    // Separate save key from CoinCounter.SaveKey — this is lifetime progress, not spendable coins.
    public const string SaveKey = "BattlePassTotalCoins";

    [Header("Progression Settings")]
    [SerializeField] private int coinsPerLevel = 50;

    [Header("Reward Tiers")]
    [Tooltip("One entry per reward tier from your menu. Coin-reward tiers are only paid out once, the moment that level is first reached.")]
    [SerializeField]
    private BattlePassTier[] tiers = new BattlePassTier[]
    {
        new BattlePassTier { requiredLevel = 5,  rewardType = RewardType.Skin,  skinIndex = 0 },
        new BattlePassTier { requiredLevel = 10, rewardType = RewardType.Coins, coinAmount = 10 },
        new BattlePassTier { requiredLevel = 20, rewardType = RewardType.Skin,  skinIndex = 1 },
        new BattlePassTier { requiredLevel = 30, rewardType = RewardType.Coins, coinAmount = 15 },
        new BattlePassTier { requiredLevel = 40, rewardType = RewardType.Skin,  skinIndex = 2 },
        new BattlePassTier { requiredLevel = 50, rewardType = RewardType.Coins, coinAmount = 25 },
        new BattlePassTier { requiredLevel = 60, rewardType = RewardType.Skin,  skinIndex = 3 },
    };

    [Header("Optional Direct UI (leave empty if your menu reads via script instead)")]
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI progressText; // e.g. "35 / 50" toward next level

    [Header("Events (hook these to your Battle Pass menu)")]
    public UnityEvent<int> OnLevelUp;             // passes the new level reached
    public UnityEvent<int> OnSkinUnlocked;        // passes the skin index from the tier
    public UnityEvent<int> OnCoinBonusAwarded;    // passes the coin amount awarded
    public UnityEvent OnProgressChanged;          // fires on any change, for refreshing UI

    private int totalCoins;
    private int currentLevel;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        totalCoins = PlayerPrefs.GetInt(SaveKey, 0);
        currentLevel = CalculateLevel(totalCoins);
        RefreshUI();
    }

    public int GetTotalCoins() => totalCoins;
    public int GetLevel() => currentLevel;
    public int GetCoinsPerLevel() => coinsPerLevel;
    public int GetCoinsTowardNextLevel() => totalCoins % coinsPerLevel;
    public BattlePassTier[] GetTiers() => tiers;

    /// <summary>
    /// True once the player has reached the level required for the given skin index.
    /// Looks up the tier(s) with that skinIndex and checks its requiredLevel.
    /// </summary>
    public bool IsSkinUnlocked(int skinIndex)
    {
        foreach (BattlePassTier tier in tiers)
        {
            if (tier.rewardType == RewardType.Skin && tier.skinIndex == skinIndex)
                return currentLevel >= tier.requiredLevel;
        }
        return false;
    }

    /// <summary>
    /// Handy for UI labels like "12/20" under a locked tier icon.
    /// </summary>
    public string GetTierProgressLabel(BattlePassTier tier)
    {
        int shownLevel = Mathf.Min(currentLevel, tier.requiredLevel);
        return $"{shownLevel}/{tier.requiredLevel}";
    }

    public bool IsTierUnlocked(BattlePassTier tier) => currentLevel >= tier.requiredLevel;

    /// <summary>
    /// Call this whenever the player collects coins that should count toward battle pass
    /// progress (e.g. from Coin.cs on pickup). Don't call this for gambling winnings unless
    /// you want those to count too.
    /// </summary>
    public void AddCoins(int amount)
    {
        if (amount <= 0) return;

        int previousLevel = currentLevel;

        totalCoins += amount;

        PlayerPrefs.SetInt(SaveKey, totalCoins);
        PlayerPrefs.Save();

        currentLevel = CalculateLevel(totalCoins);

        // Fire an event for every level crossed (in case a big pickup skips several levels at once),
        // and grant any tier reward whose requiredLevel falls in that range.
        for (int lvl = previousLevel + 1; lvl <= currentLevel; lvl++)
        {
            OnLevelUp?.Invoke(lvl);
            GrantTierRewardsForLevel(lvl);
        }

        RefreshUI();
        OnProgressChanged?.Invoke();
    }

    private void GrantTierRewardsForLevel(int level)
    {
        foreach (BattlePassTier tier in tiers)
        {
            if (tier.requiredLevel != level) continue;

            if (tier.rewardType == RewardType.Skin)
            {
                OnSkinUnlocked?.Invoke(tier.skinIndex);
            }
            else if (tier.rewardType == RewardType.Coins)
            {
                if (CoinCounter.Instance != null)
                    CoinCounter.Instance.AddCoin(tier.coinAmount);

                OnCoinBonusAwarded?.Invoke(tier.coinAmount);
            }
        }
    }

    private int CalculateLevel(int coins) => coins / coinsPerLevel;

    private void RefreshUI()
    {
        if (levelText != null)
            levelText.text = $"Level {currentLevel}";
        if (progressText != null)
            progressText.text = $"{GetCoinsTowardNextLevel()} / {coinsPerLevel}";
    }

    /// <summary>Testing helper — wipes battle pass progress back to level 0.</summary>
    public void ResetProgress()
    {
        totalCoins = 0;
        currentLevel = 0;
        PlayerPrefs.SetInt(SaveKey, 0);
        PlayerPrefs.Save();
        RefreshUI();
        OnProgressChanged?.Invoke();
    }
}