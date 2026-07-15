using System;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public struct TutorialPage
{
    public GameObject PageContainer;
    public GameObject[] Steps;
}

public class TutorialController : MonoBehaviour
{
    [SerializeField] private PlaylistManager playlistManager;
    [SerializeField] private ShopViewController shopViewController;

    [SerializeField] private TutorialPage[] pages;
    [SerializeField] private GameObject openShopTrigger;
    [SerializeField] private GameObject closeShopTrigger;
    [SerializeField] private GameObject playlistStartTrigger;

    private bool isFinished = false;
    private int pageIndex = 0;
    private int stepIndex = 0;

    void Start()
    {
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
        if (isFinished) return;
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            AdvanceToNextStep();
        }
    }

    private void AdvanceToNextStep()
    {
        // Deactivate current step
        TutorialPage currentPage = pages[pageIndex];
        currentPage.Steps[stepIndex].SetActive(false);

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
            isFinished = true;
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