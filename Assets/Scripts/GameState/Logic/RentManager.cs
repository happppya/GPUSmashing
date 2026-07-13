using System;
using UnityEngine;

public class RentManager : MonoBehaviour
{
    public static RentManager Instance { get; private set; }
    public static event Action OnPeriodAdvanced;
    public static event Action OnGameOver;

    private bool gameOver = false;

    [SerializeField] private float periodLength = 90.0f;
    private float currentTime = 0.0f;

    [SerializeField]
    private float[] rentValues =
    {
        1000.0f,
        1200.0f,
        2000.0f,
        3000.0f,
        6000.0f,
        12000.0f,
        25000.0f,
    };

    [SerializeField]
    private int currentDayIndex = 0;

    public int GetSecondsToNextPeriod()
    {
        return Mathf.FloorToInt(periodLength - currentTime);
    }

    public float GetNextRentValue()
    {
        return rentValues[currentDayIndex + 1];
    }

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (gameOver) return;
        
        currentTime += Time.deltaTime;
        if (currentTime > periodLength)
        {
            if (currentDayIndex == rentValues.Length - 1)
            {
                gameOver = true;
                Time.timeScale = 0.0f;
                OnGameOver?.Invoke();
                Debug.Log("Game over");
            }
            currentTime = 0.0f;
            AdvanceToNextPeriod();
        }
    }

    private void AdvanceToNextPeriod()
    {
        currentDayIndex++;
        CashManager.Instance.AddCash(-1.0f * rentValues[currentDayIndex]);
        OnPeriodAdvanced?.Invoke();
    }

}
