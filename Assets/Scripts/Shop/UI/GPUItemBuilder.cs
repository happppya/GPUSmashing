using UnityEngine;

public class GPUItemBuilder : ShopItemBuilder
{
    private GraphicsCardSO gpuDefinition;
    public void Initialize(GraphicsCardSO newDefinition)
    {
        gpuDefinition = newDefinition;
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
    }
}
