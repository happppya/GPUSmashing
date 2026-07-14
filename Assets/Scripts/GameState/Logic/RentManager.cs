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
    private float[] rentValues;

    [SerializeField]
    private int currentDayIndex = 0;

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
        if (gameOver) return;
        
        currentTime += Time.deltaTime;
        if (currentTime > periodLength)
        {
            currentTime = 0.0f;
            AdvanceToNextPeriod();
            if (currentDayIndex >= rentValues.Length)
            {
                gameOver = true;
                Time.timeScale = 0.0f;
                OnGameOver?.Invoke();
                Debug.Log("Game over");
            }
        }
    }

    private void AdvanceToNextPeriod()
    {
        CashManager.Instance.AddCash(-1.0f * rentValues[currentDayIndex]);
        OnPeriodAdvanced?.Invoke();
        currentDayIndex++;
    }

}
