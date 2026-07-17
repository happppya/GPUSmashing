using System;
using UnityEngine;
using UnityEngine.Audio;

public class UpgradeItemBuilder : ShopItemBuilder
{
    [SerializeField] private SoundCollection upgradeBuySound;

    private UpgradeSO upgradeDefinition;
    private bool isMax = false;
    public void Initialize(UpgradeSO newDefinition)
    {
        upgradeDefinition = newDefinition;
        base.Initialize(upgradeDefinition.DisplayName + " [0]", upgradeDefinition.Description, upgradeDefinition.BasePrice);
        UpdateDescription();
    }

    protected override bool CanBeBought()
    {
        if (isMax) return false;
        return CashManager.Instance.CanSpendCash(upgradeDefinition.CurrentPrice);
    }

    protected override void BuyButtonPressed()
    {
        if (!CanBeBought()) return;

        float originalPrice = upgradeDefinition.CurrentPrice;

        upgradeDefinition.UpdateOnBought();

        base.SetCost(upgradeDefinition.CurrentPrice);
        base.SetName(upgradeDefinition.DisplayName + $" [{upgradeDefinition.CurrentLevel}]");

        if (upgradeDefinition.CurrentLevel >= upgradeDefinition.MaxLevel)
        {
            base.SetCostRaw("MAX");
            isMax = true;
        }

        CashManager.Instance.AddCash(originalPrice * -1.0f);
        UpgradeManager.Instance.RecalculateStatsOnUpgrade(upgradeDefinition);
        
        UpdateDescription();

        SoundUtility.PlayRandomSound(upgradeBuySound, null, false);
    }

    private void UpdateDescription()
    {
        string newDescription = upgradeDefinition.Description;
        foreach (UpgradeModifier modifier in upgradeDefinition.UpgradeModifiers)
        {
            if (modifier.Tag == null || modifier.Tag.Length == 0) continue;

            float statValue;
            if (modifier.valueType == ValueType.LocalContribution)
            {
                statValue = UpgradeManager.Instance.GetUpgradeContribution(upgradeDefinition, modifier.UpgradeType);
            } else if (modifier.valueType == ValueType.TotalValue)
            {
                statValue = UpgradeManager.Instance.GetStat(modifier.UpgradeType);
            } else
            {
                throw new Exception();
            }

            string statFormatted = modifier.FormatType switch
            {
                StatFormatType.None => Mathf.Abs(statValue).ToString(),
                StatFormatType.Percentage => Mathf.Abs(statValue).ToString("P"),
                StatFormatType.Currency => Mathf.Abs(statValue).ToString("C0"),
                _ => throw new System.ArgumentException($"Invalid format type {modifier.FormatType}"),
            };
            
            if (modifier.SignedFormatting)
            {
                string sign = statValue >= 0 ? "+" : "-";
                statFormatted = sign + statFormatted;
            }

            newDescription = newDescription.Replace(modifier.Tag, statFormatted);
        }

        base.SetDescription(newDescription);
    }
}
