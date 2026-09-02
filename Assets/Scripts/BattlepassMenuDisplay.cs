using UnityEngine;
using TMPro;

/// <summary>
/// Put this on your battle pass menu/panel. Assign one TextMeshProUGUI per tier,
/// in the SAME ORDER as the tiers array on BattlePassManager (e.g. your 7 icons
/// showing "0/5", "0/10", "0/20"... in order left to right).
///
/// Automatically refreshes when the menu is opened and whenever progress changes
/// while it's open (e.g. if coins can be collected/gambled while this panel is visible).
/// </summary>
public class BattlePassMenuDisplay : MonoBehaviour
{
    [Tooltip("One text per tier, in the same order as BattlePassManager's Tiers list.")]
    [SerializeField] private TextMeshProUGUI[] tierProgressTexts;

    [Header("Overall Level (optional)")]
    [SerializeField] private TextMeshProUGUI levelText;         // e.g. "Level 12"
    [SerializeField] private TextMeshProUGUI levelProgressText; // e.g. "35 / 50" toward next level

    private void Start()
    {
        RefreshAll();

        if (BattlePassManager.Instance != null)
            BattlePassManager.Instance.OnProgressChanged.AddListener(RefreshAll);
    }

    private void OnDisable()
    {
        if (BattlePassManager.Instance != null)
            BattlePassManager.Instance.OnProgressChanged.RemoveListener(RefreshAll);
    }

    private void RefreshAll()
    {
        if (BattlePassManager.Instance == null)
        {
            Debug.LogWarning("BattlePassMenuDisplay: no BattlePassManager found in the scene (Instance is null).");
            return;
        }

        BattlePassManager.BattlePassTier[] tiers = BattlePassManager.Instance.GetTiers();

        int count = Mathf.Min(tierProgressTexts.Length, tiers.Length);
        for (int i = 0; i < count; i++)
        {
            if (tierProgressTexts[i] == null) continue;
            tierProgressTexts[i].text = BattlePassManager.Instance.GetTierProgressLabel(tiers[i]);
        }

        if (levelText != null)
            levelText.text = $"Level {BattlePassManager.Instance.GetLevel()}";

        if (levelProgressText != null)
            levelProgressText.text = $"{BattlePassManager.Instance.GetCoinsTowardNextLevel()} / {BattlePassManager.Instance.GetCoinsPerLevel()}";
    }
}