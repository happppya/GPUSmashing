using System;
using UnityEngine;

public class UpgradeItemBuilder : ShopItemBuilder
{
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
        
        upgradeDefinition.UpdateOnBought();
        base.SetCost(upgradeDefinition.CurrentPrice);
        base.SetName(upgradeDefinition.DisplayName + $" [{upgradeDefinition.CurrentLevel}]");

        if (upgradeDefinition.CurrentLevel >= upgradeDefinition.MaxLevel)
        {
            base.SetCostRaw("MAX");
            isMax = true;
        }

        UpgradeManager.Instance.RecalculateStatsOnUpgrade(upgradeDefinition);
        CashManager.Instance.AddCash(upgradeDefinition.CurrentPrice * -1.0f);

        UpdateDescription();
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
                StatFormatType.None => statValue.ToString(),
                StatFormatType.Percentage => statValue.ToString("P"),
                StatFormatType.Currency => statValue.ToString("C0"),
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
