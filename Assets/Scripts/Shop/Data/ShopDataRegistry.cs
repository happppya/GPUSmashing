using UnityEngine;

[CreateAssetMenu(menuName = "Shop/ShopDataRegistry")]
public class ShopDataRegistry : ScriptableObject
{
    public GraphicsCardSO[] AllGraphicsCards;
    public UpgradeSO[] AllUpgrades;
}
