using System;
using UnityEngine;

public class InterestManager : MonoBehaviour
{
    public static InterestManager Instance { get; private set; }
    public static event Action OnMissesExhausted;

    public bool IsInDebt => isInDebt;

    [SerializeField] private float interestTime = 30.0f;
    [SerializeField] private float interestRate = 1.0f;
    [SerializeField] private int missesUntilGameEnd = 3;

    private float currentInterestTime;
    private int consecutiveMisses = 0;

    bool isInDebt = false;

    public int GetSecondsToNextInterest()
    {
        return Mathf.FloorToInt(currentInterestTime);
    }

    public float GetInterest()
    {
        return Mathf.Abs(CashManager.Instance.Cash * interestRate);
    }

    void Awake()
    {
        Instance = this;
    }
    void Update()
    {
        if (CashManager.Instance.Cash >= 0)
        {
            if (isInDebt)
            {
                isInDebt = false;
                consecutiveMisses = 0;
            }
            return;
        }

        if (!isInDebt)
        {
            isInDebt = true;
            currentInterestTime = interestTime;
        }

        currentInterestTime -= Time.deltaTime;
        if (currentInterestTime <= 0.0f)
        {
            currentInterestTime = interestTime;
            CashManager.Instance.AddCash(GetInterest() * -1.0f);
            consecutiveMisses++;

            if (consecutiveMisses == missesUntilGameEnd)
            {
                OnMissesExhausted?.Invoke();
            }
        }
    }
}
