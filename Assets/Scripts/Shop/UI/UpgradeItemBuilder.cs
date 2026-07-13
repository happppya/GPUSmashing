using UnityEngine;

public class UpgradeItemBuilder : ShopItemBuilder
{
    private UpgradeSO upgradeDefinition;
    private bool isMax = false;

    public void Initialize(UpgradeSO newDefinition)
    {
        upgradeDefinition = newDefinition;
        base.Initialize(upgradeDefinition.DisplayName + " [1]", upgradeDefinition.Description, upgradeDefinition.BasePrice);
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

        if (upgradeDefinition.CurrentLevel == upgradeDefinition.MaxLevel)
        {
            base.SetCostRaw("MAX");
            isMax = true;
        }

        CashManager.Instance.AddCash(upgradeDefinition.CurrentPrice * -1.0f);
    }
}
