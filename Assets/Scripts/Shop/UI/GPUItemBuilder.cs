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
        base.Initialize(gpuDefinition.DisplayName, gpuDefinition.Description, gpuDefinition.Price);
    }

    protected override bool CanBeBought()
    {
        return CashManager.Instance.CanSpendCash(gpuDefinition.Price);
    }

    protected override void BuyButtonPressed()
    {
        if (!CanBeBought()) return;
        CashManager.Instance.AddCash(gpuDefinition.Price * -1.0f);

        GameObject graphicsCard = Instantiate(gpuDefinition.Prefab, instanceContainer);
        graphicsCard.transform.position = gpuDeliveryPoint.position;
    }
}
