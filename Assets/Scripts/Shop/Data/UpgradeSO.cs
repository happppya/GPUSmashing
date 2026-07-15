using System;
using System.Collections.Generic;
using UnityEngine;

public enum StatFormatType
{
    None,
    Percentage,
    Currency,
}

public enum ValueType
{
    LocalContribution,
    TotalValue,
}

[System.Serializable]
public struct UpgradeModifier
{
    public string Tag; // The tag that will be used to identify where in the description text to replace with
    public UpgradeType UpgradeType;
    public float IncrementPerLevel;
    public float MinValue;
    public float MaxValue;
    public StatFormatType FormatType;
    public bool SignedFormatting;
    public ValueType valueType;
}

[CreateAssetMenu(fileName = "NewUpgrade", menuName = "Upgrades/Upgrade")]
public class UpgradeSO : ScriptableObject
{
    public string DisplayName;
    public string Description;
    public float BasePrice;
    public float PriceIncrement;
    public int MaxLevel;
    public UpgradeModifier[] UpgradeModifiers;

    [Header("Don't change from editor")]
    public float CurrentPrice;
    public int CurrentLevel = 0;

    public void UpdateOnBought()
    {
        CurrentPrice += PriceIncrement;
        CurrentLevel += 1;
    }

    private void OnEnable()
    {
        CurrentPrice = BasePrice;
        CurrentLevel = 0;
    }

}