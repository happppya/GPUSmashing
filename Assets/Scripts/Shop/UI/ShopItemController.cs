using UnityEngine;

public class ShopItemController : MonoBehaviour
{
    [SerializeField] private GraphicsCardSO[] graphicsCards;
    [SerializeField] private UpgradeSO[] upgrades;

    [SerializeField] private GPUItemBuilder gpuItemPrefab;
    [SerializeField] private UpgradeItemBuilder upgradeItemPrefab;

    [SerializeField] private Transform GPUContent;
    [SerializeField] private Transform UpgradeContent;

    void Start()
    {
        foreach (GraphicsCardSO gpuDefinition in graphicsCards)
        {
            GPUItemBuilder gpuItem = Instantiate(gpuItemPrefab, GPUContent);
            gpuItem.Initialize(gpuDefinition);
        }
        foreach (UpgradeSO upgradeDefinition in upgrades)
        {
            UpgradeItemBuilder upgradeItem = Instantiate(upgradeItemPrefab, UpgradeContent);
            upgradeItem.Initialize(upgradeDefinition);
        }
    }
    
}
