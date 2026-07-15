using System;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public struct TutorialPage
{
    public GameObject PageContainer;
    public GameObject[] Steps;
    public bool HidePreviousSteps;
}

public class TutorialController : MonoBehaviour
{
    [SerializeField] private PlaylistManager playlistManager;
    [SerializeField] private ShopViewController shopViewController;

    [SerializeField] private TutorialPage[] pages;
    [SerializeField] private GameObject openShopTrigger;
    [SerializeField] private GameObject closeShopTrigger;
    [SerializeField] private GameObject playlistStartTrigger;

    public static bool IsFinished = false;
    private int pageIndex = 0;
    private int stepIndex = 0;

    void Start()
    {
        Time.timeScale = 0.0f;
        // Deactivate everything initially
        foreach (TutorialPage page in pages)
        {
            page.PageContainer.SetActive(false);
            foreach (GameObject step in page.Steps)
            {
                step.SetActive(false);
            }
        }
        
        ActivateCurrentStep();
    }

    void Update()
    {
        if (IsFinished) return;
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            AdvanceToNextStep();
        }
    }

    private void AdvanceToNextStep()
    {
        // Deactivate current step
        TutorialPage currentPage = pages[pageIndex];
        if (currentPage.HidePreviousSteps)
        {
            currentPage.Steps[stepIndex].SetActive(false);
        }

        stepIndex++;

        // Page wrap
        if (stepIndex >= currentPage.Steps.Length)
        {
            currentPage.PageContainer.SetActive(false);
            pageIndex++;
            stepIndex = 0;
        }

        // Tutorial completion
        if (pageIndex >= pages.Length)
        {
            Time.timeScale = 1.0f;
            IsFinished = true;
            Destroy(gameObject);
            return;
        }

        ActivateCurrentStep();
    }

    private void ActivateCurrentStep()
    {
        TutorialPage currentPage = pages[pageIndex];
        GameObject currentStep = currentPage.Steps[stepIndex];

        currentPage.PageContainer.SetActive(true);
        currentStep.SetActive(true);
        
        if (currentStep == openShopTrigger)
        {
            shopViewController.SetShopState(true);
        }
        else if (currentStep == closeShopTrigger)
        {
            shopViewController.SetShopState(false);
        }
        else if (currentStep == playlistStartTrigger)
        {
            playlistManager.BeginPlaylist();
        }
    }
}