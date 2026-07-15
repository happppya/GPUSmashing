using System;
using UnityEngine;

public class RentManager : MonoBehaviour
{

    private static float periodLength = 5;

    public static RentManager Instance { get; private set; }
    public static event Action OnPeriodAdvanced;
    public static event Action OnPeriodsExhausted;
    
    private float currentTime = 0.0f;

    [SerializeField]
    private float[] rentValues;

    [SerializeField]
    private int currentDayIndex = 0;

    public static void SetPeriodLength(float time)
    {
        periodLength = time;
    }

    public int GetSecondsToNextPeriod()
    {
        return Mathf.FloorToInt(periodLength - currentTime);
    }

    public float GetNextRentValue()
    {
        if (currentDayIndex >= rentValues.Length) return 0f;
        return rentValues[currentDayIndex];
    }

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (GameEndController.Instance.IsGameOver) return;
        
        currentTime += Time.deltaTime;
        if (currentTime > periodLength)
        {
            currentTime = 0.0f;
            AdvanceToNextPeriod();
            if (currentDayIndex >= rentValues.Length)
            {
                OnPeriodsExhausted?.Invoke();
            }
        }
    }

    private void AdvanceToNextPeriod()
    {
        CashManager.Instance.AddCash(-1.0f * rentValues[currentDayIndex]);
        currentDayIndex++;
        OnPeriodAdvanced?.Invoke();
    }

}
