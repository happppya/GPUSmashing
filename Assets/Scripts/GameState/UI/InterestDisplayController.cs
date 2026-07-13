using System.Collections;
using TMPro;
using UnityEngine;

public class InterestDisplayController : MonoBehaviour
{
    [SerializeField] private GameObject container;
    [SerializeField] private TMP_Text timeLabel;

    [SerializeField] private float flashInterval = 0.3f;
    [SerializeField] private float flashDuration = 0.1f;

    private float flashTimer = 0;
    private float previousSeconds;

    void Awake()
    {
        container.SetActive(false);
    }

    void Update()
    {
        if (InterestManager.Instance.IsInDebt == false)
        {
            container.SetActive(false);
            return;
        }

        flashTimer -= Time.deltaTime;
        if (flashTimer < 0)
        {
            flashTimer = flashInterval;
            StartCoroutine(FlashCoroutine());
        }

        int seconds = InterestManager.Instance.GetSecondsToNextInterest();
        if (previousSeconds != seconds)
        {
            previousSeconds = seconds;
            timeLabel.text = seconds.ToString();
        }

    }

    IEnumerator FlashCoroutine()
    {
        container.SetActive(false);
        yield return new WaitForSeconds(flashDuration);
        container.SetActive(true);
    }

}
