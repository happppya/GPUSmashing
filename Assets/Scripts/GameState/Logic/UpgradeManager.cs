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
}

[Serializable]
struct BaseValueConfig
{
    public UpgradeType UpgradeType;
    public float BaseValue;
}

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }
    public static event Action<UpgradeType> OnUpgradeTypeChanged;

    [SerializeField] private BaseValueConfig[] baseValues;
    [SerializeField] private ShopDataRegistry shopRegistry;
    private Dictionary<UpgradeType, float> statCache = new Dictionary<UpgradeType, float>();
    private Dictionary<UpgradeType, float> baseValueMap = new Dictionary<UpgradeType, float>();

    void Awake()
    {
        Instance = this;
        InitializeCache();
    }

    private void InitializeCache()
    {
        foreach (BaseValueConfig baseValueConfig in baseValues)
        {
            baseValueMap[baseValueConfig.UpgradeType] = baseValueConfig.BaseValue;
        }

        statCache.Clear();

        // Initialize all types to 0
        foreach (UpgradeType type in Enum.GetValues(typeof(UpgradeType)))
        {
            statCache[type] = 0f;
        }

        // Calculate initial values
        foreach (var upgrade in shopRegistry.AllUpgrades)
        {
            ApplyUpgradeToCache(upgrade);
        }
    }
    
    public float GetStat(UpgradeType type)
    {
        if (statCache.TryGetValue(type, out float value))
            return baseValueMap.GetValueOrDefault(type, 0.0f) + value;

        return baseValueMap.GetValueOrDefault(type, 0.0f);
    }

    public void RecalculateStatsOnUpgrade(UpgradeSO leveledUpgrade)
    {
        // Identify which stat types need recalculating
        HashSet<UpgradeType> affectedTypes = new HashSet<UpgradeType>();
        foreach (var modifier in leveledUpgrade.UpgradeModifiers)
        {
            affectedTypes.Add(modifier.UpgradeType);
        }

        // Reset those stats in the cache
        foreach (var type in affectedTypes)
        {
            statCache[type] = 0f;
        }

        // Re-sum only the upgrades that contribute to the affected types
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

        foreach (var type in affectedTypes)
        {
            OnUpgradeTypeChanged?.Invoke(type);
        }

        /*Debug.Log("NEW UPGRADE");
        foreach (var kvp in statCache)
        {
            Debug.Log($"{kvp.Key}: {kvp.Value}");
        }*/
    }

    private void ApplyUpgradeToCache(UpgradeSO upgrade)
    {
        foreach (var modifier in upgrade.UpgradeModifiers)
        {
            float modifierValue = CalculateModifierValue(upgrade.CurrentLevel, modifier);
            statCache[modifier.UpgradeType] += modifierValue;
        }
    }

    private float CalculateModifierValue(int level, UpgradeModifier modifier)
    {
        float calculatedValue = (level * modifier.IncrementPerLevel);
        calculatedValue = Mathf.Clamp(calculatedValue, modifier.MinValue, modifier.MaxValue);

        return calculatedValue;
    }

}
