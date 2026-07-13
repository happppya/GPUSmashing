using System;
using System.Collections.Generic;
using UnityEngine;

public enum StatFormatType
{
    None,
    Percentage,
    Currency,
}

[System.Serializable]
public struct UpgradeModifier
{
    public string Tag; // The tag that will be used to identify where in the description text to replace with
    public UpgradeType UpgradeType;
    public float BaseValue;
    public float IncrementPerLevel;
    public float MaxValue;
    public StatFormatType FormatType;
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
}