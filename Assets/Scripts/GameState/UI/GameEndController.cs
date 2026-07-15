using TMPro;
using UnityEngine;
using UnityEngine.UI;

enum GameEndSource
{
    Millionaire,
    PeriodsExhausted,
    MissedInterest,
}

public class GameEndController : MonoBehaviour
{
    public static GameEndController Instance;
    public bool IsGameOver => isGameOver;

    [SerializeField] private GameObject background;
    [SerializeField] private TMP_Text titleLabel;
    [SerializeField] private TMP_Text subtitleLabel;
    [SerializeField] private TMP_Text descriptionLabel;
    [SerializeField] private TMP_Text statsLabel;
    [SerializeField] private Button quitButton;
    [SerializeField] private float millionaireEndingThreshold = 100_000_000;

    private bool isGameOver;

    void Awake()
    {
        Instance = this;

        background.SetActive(false);

        RentManager.OnPeriodsExhausted += () =>
        {
            GameEnded(GameEndSource.PeriodsExhausted);
        };

        CashManager.OnCashChanged += (float cash) =>
        {
            if (cash >= millionaireEndingThreshold)
            {
                GameEnded(GameEndSource.Millionaire);
            }
        };

        InterestManager.OnMissesExhausted += () =>
        {
            GameEnded(GameEndSource.MissedInterest);
        };

        quitButton.onClick.AddListener(Quit);
    }

    private void Quit()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    private void GameEnded(GameEndSource source)
    {
        if (isGameOver) return;

        Debug.Log("GAME OVER");
        Time.timeScale = 0.0f;
        isGameOver = true;

        background.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        float cash = CashManager.Instance.Cash;

        float totalSeconds = Time.time;
        int minutes = (int)(totalSeconds / 60);
        int seconds = (int)(totalSeconds % 60);
        string cashFormatted = (cash >= 0) ? cash.ToString("C0") : Mathf.Abs(cash).ToString("C0") + " debt";
        statsLabel.text = $"Time: {minutes} minutes {seconds} seconds\nCash: {cashFormatted}";

        if (source == GameEndSource.Millionaire)
        {
            titleLabel.text = "Millionaire";
            subtitleLabel.text = "ending 5 / 5";
            descriptionLabel.text = "You escaped the landlord's wrath and became a multi-millionaire in the process! You bought out his properties, turned his factory into a luxury resort, and now Mr. Landlord works for you. An absolute masterclass in finance... Great job!";
        } 
        else if (source == GameEndSource.MissedInterest || cash < -5000.0f)
        {
            titleLabel.text = "Debt Spiral";
            subtitleLabel.text = "ending 1 / 5";
            descriptionLabel.text = "Forever unable to pay off your debts, you and your descendants will have to work in Mr. Landlord's grueling factory for centuries. The interest compounded much faster than your hopes. A tragic financial ruin... Better luck next time.";
        }
        else if (source == GameEndSource.PeriodsExhausted)
        {
            if (cash <= 0)
            {
                titleLabel.text = "Barely Survived";
                subtitleLabel.text = "ending 2 / 5";
                descriptionLabel.text = "You narrowly manage to avoid going into debt forever, escaping with basically zero to your name. You're finally free from the landlord, but you'll be eating instant ramen for the foreseeable future. There's plenty of room for improvement.";
            }
            else if (cash > 10_000)
            {
                titleLabel.text = "Comfortable Living";
                subtitleLabel.text = "ending 4 / 5";
                descriptionLabel.text = "You survived the landlord's relentless schemes and walked away with a very comfortable sum. You bought a nice house in the suburbs, started a small business, and never have to worry about rent again. Well done!";
            } else
            {
                titleLabel.text = "New Beginnings";
                subtitleLabel.text = "ending 3 / 5";
                descriptionLabel.text = "You paid off the landlord and managed to save just a little extra on the side. It's certainly not a massive fortune, but it's enough to pack your bags and start a new, quiet life far away from this greedy town. A solid, honest effort.";
            }
        }
    }
}
