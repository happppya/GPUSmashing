using TMPro;
using UnityEngine;

public class RentDisplayController : MonoBehaviour
{
    [SerializeField] private TMP_Text topLabel;
    [SerializeField] private TMP_Text timeLabel;

    private int previousSecondsLeft;

    void UpdateTopLabel()
    {
        topLabel.text = $"{RentManager.Instance.GetNextRentValue().ToString("C0")} RENT IS DUE IN";
    }

    void UpdateTimeLabel()
    {
        int secondsLeft = RentManager.Instance.GetSecondsToNextPeriod();

        if (secondsLeft == previousSecondsLeft) return;
        previousSecondsLeft = secondsLeft;

        int minutes = secondsLeft / 60;
        int seconds = secondsLeft % 60;
        timeLabel.text = $"{minutes}:{seconds:D2}";
    }

    void Start()
    {
        UpdateTopLabel();
        RentManager.OnPeriodAdvanced += UpdateTopLabel;
    }

    void Update()
    {
        UpdateTimeLabel();
    }
}
