using UnityEngine;

public class ShopItemController : MonoBehaviour
{
    [SerializeField] private ShopDataRegistry registry;

    [SerializeField] private Transform gpuDeliveryPoint;
    [SerializeField] private Transform gpuContainer;

    [SerializeField] private GPUItemBuilder gpuItemPrefab;
    [SerializeField] private UpgradeItemBuilder upgradeItemPrefab;

    [SerializeField] private Transform GPUContent;
    [SerializeField] private Transform UpgradeContent;

    void Start()
    {
        foreach (GraphicsCardSO gpuDefinition in registry.AllGraphicsCards)
        {
            GPUItemBuilder gpuItem = Instantiate(gpuItemPrefab, GPUContent);
            gpuItem.Initialize(gpuDefinition, gpuDeliveryPoint, gpuContainer);
        }
        foreach (UpgradeSO upgradeDefinition in registry.AllUpgrades)
        {
            UpgradeItemBuilder upgradeItem = Instantiate(upgradeItemPrefab, UpgradeContent);
            upgradeItem.Initialize(upgradeDefinition);
        }
    }
    
}
