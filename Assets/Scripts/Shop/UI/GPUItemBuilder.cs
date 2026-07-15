using UnityEngine;

public class GPUItemBuilder : ShopItemBuilder
{
    private Transform gpuDeliveryPoint;
    private Transform instanceContainer;
    private GraphicsCardSO gpuDefinition;
    public void Initialize(GraphicsCardSO newDefinition, Transform deliveryPoint, Transform container)
    {
        gpuDefinition = newDefinition;
        gpuDeliveryPoint = deliveryPoint;
        instanceContainer = container;
        base.Initialize(gpuDefinition.DisplayName, gpuDefinition.Description, GetPrice());

        UpgradeManager.OnUpgradeTypeChanged += (UpgradeType type) => {
            if (type == UpgradeType.DiscountPercentage)
            {
                PriceChanged();
            }
        };

    }

    protected override bool CanBeBought()
    {
        if (gpuDefinition.Price <= 0) { return true; }
        return CashManager.Instance.CanSpendCash(gpuDefinition.Price);
    }

    protected override void BuyButtonPressed()
    {
        if (!CanBeBought()) return;
        
        if (UnityEngine.Random.Range(0.0f, 1.0f) > UpgradeManager.Instance.GetStat(UpgradeType.FreeChance))
        {
            CashManager.Instance.AddCash(GetPrice() * -1.0f);
        }

        GameObject graphicsCard = Instantiate(gpuDefinition.Prefab, instanceContainer);
        graphicsCard.transform.position = gpuDeliveryPoint.position;
    }

    private void PriceChanged()
    {
        base.SetCost(GetPrice());
    }

    private float GetPrice()
    {
        return gpuDefinition.Price * (1 - UpgradeManager.Instance.GetStat(UpgradeType.DiscountPercentage));
    }
}
