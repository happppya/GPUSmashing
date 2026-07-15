using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IntroScreenController : MonoBehaviour
{
    public static event Action<float> OnTimeSelected;

    [SerializeField] private Button startButtonEasy;
    [SerializeField] private Button startButtonMedium;
    [SerializeField] private Button startButtonHard;
    [SerializeField] private Button quitButton;
    void Awake()
    {
        startButtonEasy.onClick.AddListener(() =>
        {
            StartGame(70);
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
        OnTimeSelected?.Invoke(time);
        SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
    }
}
