using UnityEngine;

[CreateAssetMenu(fileName = "NewUpgrade", menuName = "Upgrades/Upgrade")]
public class UpgradeSO : ScriptableObject
{
    public string DisplayName;
    public string Description;
    public float BasePrice;
    public float PriceIncrement;
    public int MaxLevel;
}
