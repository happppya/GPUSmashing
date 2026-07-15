using System;
using System.Collections.Generic;
using UnityEngine;

public enum UpgradeType
{
    ExplodeChance,
    FreeChance,
    DiscountPercentage,

    GPUBaseEarningAdd,
    GPUBaseEarningMultiplier,
    JackpotChance,
    JackpotEarningMultiplier,
    SuperJackpotChance,
    SuperJackpotEarningMultiplier,

    UnlockedSpikes,
    UnlockedBlades,
}

[Serializable]
public struct BaseValueConfig
{
    public UpgradeType UpgradeType;
    public float BaseValue;
}

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }
    public static event Action<UpgradeType> OnUpgradeTypeChanged;

    [Header("Configuration")]
    [SerializeField] private BaseValueConfig[] baseValues;
    [SerializeField] private ShopDataRegistry shopRegistry;

    // Caches the calculated total bonus from all upgrades for a specific stat
    private readonly Dictionary<UpgradeType, float> statCache = new Dictionary<UpgradeType, float>();

    // Caches the base values
    private readonly Dictionary<UpgradeType, float> baseValueMap = new Dictionary<UpgradeType, float>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        InitializeCache();
    }

    private void InitializeCache()
    {
        baseValueMap.Clear();
        foreach (BaseValueConfig config in baseValues)
        {
            baseValueMap[config.UpgradeType] = config.BaseValue;
        }

        statCache.Clear();

        // Initialize all enum types to 0
        foreach (UpgradeType type in Enum.GetValues(typeof(UpgradeType)))
        {
            statCache[type] = 0f;
        }

        // Calculate initial values from all upgrades
        if (shopRegistry != null && shopRegistry.AllUpgrades != null)
        {
            foreach (var upgrade in shopRegistry.AllUpgrades)
            {
                ApplyUpgradeToCache(upgrade);
            }
        }
    }

    public float GetStat(UpgradeType type)
    {
        float baseValue = baseValueMap.GetValueOrDefault(type, 0.0f);
        float bonusValue = statCache.GetValueOrDefault(type, 0.0f);

        return baseValue + bonusValue;
    }

    public float GetUpgradeContribution(UpgradeSO upgrade, UpgradeType type)
    {
        if (upgrade == null || upgrade.UpgradeModifiers == null) return 0f;

        foreach (var modifier in upgrade.UpgradeModifiers)
        {
            if (modifier.UpgradeType == type)
            {
                return CalculateModifierValue(upgrade.CurrentLevel, modifier);
            }
        }

        // If the upgrade doesn't affect this stat, it contributes 0
        return 0f;
    }

    public void RecalculateStatsOnUpgrade(UpgradeSO leveledUpgrade)
    {
        if (leveledUpgrade == null || leveledUpgrade.UpgradeModifiers == null) return;

        // Identify which stat types need recalculating based on the specific upgrade
        HashSet<UpgradeType> affectedTypes = new HashSet<UpgradeType>();
        foreach (var modifier in leveledUpgrade.UpgradeModifiers)
        {
            affectedTypes.Add(modifier.UpgradeType);
        }

        // Reset those specific stats in the cache back to 0
        foreach (var type in affectedTypes)
        {
            statCache[type] = 0f;
        }

        // Re-sum the contributions from all upgrades, but only for the affected types
        foreach (var upgrade in shopRegistry.AllUpgrades)
        {
            foreach (var modifier in upgrade.UpgradeModifiers)
            {
                if (affectedTypes.Contains(modifier.UpgradeType))
                {
                    float modifierValue = CalculateModifierValue(upgrade.CurrentLevel, modifier);
                    statCache[modifier.UpgradeType] += modifierValue;
                }
            }
        }

        // Notify listeners that these specific stats have changed
        foreach (var type in affectedTypes)
        {
            OnUpgradeTypeChanged?.Invoke(type);
        }
    }

    private void ApplyUpgradeToCache(UpgradeSO upgrade)
    {
        if (upgrade.UpgradeModifiers == null) return;

        foreach (var modifier in upgrade.UpgradeModifiers)
        {
            float modifierValue = CalculateModifierValue(upgrade.CurrentLevel, modifier);
            statCache[modifier.UpgradeType] += modifierValue;
        }
    }

    private float CalculateModifierValue(int level, UpgradeModifier modifier)
    {
        float calculatedValue = level * modifier.IncrementPerLevel;
        return Mathf.Clamp(calculatedValue, modifier.MinValue, modifier.MaxValue);
    }
}