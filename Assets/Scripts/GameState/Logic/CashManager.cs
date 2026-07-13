using System;
using UnityEngine;

public class CashManager : MonoBehaviour
{
    [SerializeField] private float cash = 312.0f;
    public static CashManager Instance { get; private set; }
    public static event Action<float> OnCashChanged;

    public float Cash => cash;

    public bool CanSpendCash(float amount)
    {
        return cash >= amount;
    }

    public void AddCash(float amount)
    {
        cash += amount;
        OnCashChanged?.Invoke(cash);
    }

    void Awake()
    {
        Instance = this;
    }

}
