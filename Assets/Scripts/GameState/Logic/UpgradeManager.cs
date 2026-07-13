using UnityEngine;

public enum UpgradeType
{
    ExplodeChance,
    FreeChance,
    DiscountPercentage,
}

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }
    void Awake()
    {
        Instance = this;
    }
}
