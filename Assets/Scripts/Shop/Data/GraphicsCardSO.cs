using System;
using UnityEngine;

[Serializable]
public struct IntRange
{
    public int Min;
    public int Max;
    public int GetRandomValue()
    {
        return UnityEngine.Random.Range(Min, Max);
    }
}

[CreateAssetMenu(fileName = "NewCard", menuName = "GraphicsCards/GraphicsCard")]
public class GraphicsCardSO : ScriptableObject
{
    public GameObject Prefab;
    public string DisplayName;
    public string Description;
    public float Price;

    public IntRange BaseEarnings;
}
