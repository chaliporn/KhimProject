using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[ExecuteAlways]
public class TutorialScroller : MonoBehaviour
{
    public static int rememberPage;
    public static NoteChart selectedTutorial;

    [Range(0,4)]
    public int page;

    [Space]

    public RectTransform[] allPages;
    public TutorialPage[] allTutorials;
    public RectTransform mainRectTransform;

    public GraphicRaycaster graphicRaycaster;

    [Space]

    public RectTransform rectToScroll;

    public void BackToTitle()
    {
        SceneManager.LoadScene("Title");
    }

    public void Awake()
    {
        page = rememberPage;
    }

    public void GoToPlay()
    {
        rememberPage = page;
        selectedTutorial = allTutorials[page].tutorialChart;
        SceneManager.LoadScene("Main");
    }

    public void DisableInput() => graphicRaycaster.enabled = false;
    public void EnableInput() => graphicRaycaster.enabled = true;

    public void NextPage()
    {
        page++;
        page = Mathf.Clamp(page, 0 , allPages.Length-1);
    }

    public void PreviousPage() 
    {
        page--;
        page = Mathf.Clamp(page, 0 , allPages.Length-1);
    }

    public void UpdateToPage()
    {
        int clampedPage = Mathf.Clamp(page, 0 , allPages.Length);
        foreach(var page in allPages)
        {
            page.gameObject.SetActive(false);
        }
        RectTransform pageToMoveTo = allPages[clampedPage];
        pageToMoveTo.gameObject.SetActive(true);
    }
}
