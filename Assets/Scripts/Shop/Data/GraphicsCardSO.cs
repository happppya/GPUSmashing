using UnityEngine;

public struct IntRange
{
    public int Min;
    public int Max;
    public IntRange(int min, int max)
    {
        Min = min;
        Max = max;
    }
}

[CreateAssetMenu(fileName = "NewCard", menuName = "GraphicsCards/GraphicsCard")]
public class GraphicsCardSO : ScriptableObject
{
    public GameObject Prefab;
    public string DisplayName;
    public string Description;
    public float Price;

    public float JackpotChance;
    public float SuperJackpotChance;

    public IntRange NormalEarnings;
    public IntRange JackpotEarnings;
    public IntRange SuperJackpotEarnings;
}
