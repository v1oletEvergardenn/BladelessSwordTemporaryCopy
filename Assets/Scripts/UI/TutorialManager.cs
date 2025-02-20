using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    private InputMaster inputManager;
    public TextMeshProUGUI title;
    public Image image;
    public TextMeshProUGUI context;
    public TextMeshProUGUI pageNumber;
    public GameObject leftPage;
    public GameObject rightPage;

    public float maxHeight;
    public float maxWidth;

    public int currentPageIndex = 0;
    public List<TutorialPage> currentTutorialPages = new List<TutorialPage>();

    private void Start()
    {
        inputManager = InputMaster.instance;
        inputManager.uiActions.FlipPageLeft.performed += ctx => ShowPageAtIndex(currentPageIndex -= 1);
        inputManager.uiActions.FlipPageRight.performed += ctx => ShowPageAtIndex(currentPageIndex += 1);
    }

    private void OnEnable()
    {
        ShowPageAtIndex(0);
    }

    public void UpdateInformation(string _title, List<TutorialPage> pages)
    {
        title.text = _title;
        currentTutorialPages = pages;
        currentPageIndex = 0;
        ShowPageAtIndex(currentPageIndex);
    }

    public void ShowPageAtIndex(int i)
    {
        if (i <= 0) { i = 0; }
        if (i >= currentTutorialPages.Count) { i = currentTutorialPages.Count - 1; }
        currentPageIndex = i;
        if (currentTutorialPages != null && i >= 0 && i < currentTutorialPages.Count)
        {
            if (currentTutorialPages[i].image == null) { image.gameObject.SetActive(false); }
            else
            {
                image.gameObject.SetActive(true);
                image.sprite = currentTutorialPages[i].image;
                image.SetNativeSize();
                float r_x = 0;
                float r_y = 0;
                float r = 0f;
                r_x = maxWidth / image.rectTransform.sizeDelta.x;
                r_y = maxHeight / image.rectTransform.sizeDelta.y;
                r = r_y;
                if (r_x <= r_y) { r = r_x; }
                image.rectTransform.sizeDelta *= r;
            }
            context.text = currentTutorialPages[i].context;

            leftPage.SetActive(true);
            rightPage.SetActive(true);
            if (i == 0) { leftPage.SetActive(false); }
            if (i == currentTutorialPages.Count - 1) { rightPage.SetActive(false); }
            pageNumber.text = $"{i + 1}/{currentTutorialPages.Count}";
        }
    }
}