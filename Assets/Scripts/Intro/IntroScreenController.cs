using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IntroScreenController : MonoBehaviour
{

    [SerializeField] private Button startButtonEasy;
    [SerializeField] private Button startButtonMedium;
    [SerializeField] private Button startButtonHard;
    [SerializeField] private Button quitButton;
    void Awake()
    {
        startButtonEasy.onClick.AddListener(() =>
        {
            StartGame(90);
        });
        startButtonMedium.onClick.AddListener(() =>
        {
            StartGame(50);
        });
        startButtonHard.onClick.AddListener(() =>
        {
            StartGame(30);
        });
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

    private void StartGame(float time)
    {
        Debug.Log($"firing with time {time}");
        RentManager.SetPeriodLength(time);
        SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
    }
}
