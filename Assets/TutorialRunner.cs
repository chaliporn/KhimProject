using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialRunner : MonoBehaviour
{
    public Sprite[] tutorialImages;
    public int currentPage = 0;
    public Image imageTarget;
    public MainLogic mainLogic;
    public GlobalSettings globalSettings;
    public Animator animator;
    public Image block;

    private NoteChart songAfterTutorial;

    public void StartTutorial(TutorialPage tutorialToStart)
    {
        tutorialImages = tutorialToStart.tutorialImages;
        songAfterTutorial = tutorialToStart.tutorialChart;
        UsePage();
        CheckEnding();
    }

    public void NextPage()
    {
        currentPage++;
        UsePage();
        CheckEnding();
    }

    private void UsePage()
    {
        if(currentPage >= tutorialImages.Length)
        {
            return;
        }
        imageTarget.sprite = tutorialImages[currentPage];
        //animator.SetTrigger("Change");
    }

    private void CheckEnding()
    {
        if(currentPage == tutorialImages.Length)
        {
            globalSettings.selectedMusic = songAfterTutorial;
            mainLogic.Play();
            animator.SetTrigger("Hide");
            block.raycastTarget = false;
        }
    }

}
