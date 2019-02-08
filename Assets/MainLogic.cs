using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum MainState
{
    FreePlay,
    InTutorial,
}

public class MainLogic : MonoBehaviour
{
    public MainState state = MainState.FreePlay;

    [Space]
    public NoteChart[] allNoteCharts;
    public GameObject songPrefab;
    public Transform songListTransform;
    public Animator sideMenuAnimator;
    public GlobalSettings globalSettings;

    [Space]

    public Sprite visibleGuideKim;
    public Sprite invisibleGuideKim;
    public Image kimImage;

    [Space]
    public float leftEdge = 100;
    public float rightEdge = -420;
    public float clampRange = 10;
    public float currentSlide = 0;
    public RectTransform kim;

    public bool isPlaying = false;
    public bool isPausing = false;

    [Space]
    public ButtonToggle playButton;
    public ButtonToggle pauseButton;

    void Start()
    {
        PrepareAllSongs();
    }

    void Update()
    {
        if(isPlaying && !isPausing)
        {
            globalSettings.currentSongTime += Time.deltaTime;
        }
    }

    public void Back()
    {
        if(state == MainState.FreePlay)
        {
            SceneManager.LoadScene("Title");
        }
        else if(state == MainState.InTutorial)
        {
            SceneManager.LoadScene("Learn");
        }
    }

    public void VisibleGuide()
    {
        kimImage.sprite = visibleGuideKim;
    }

    public void InvisibleGuide()
    {
        kimImage.sprite = invisibleGuideKim;
    }

    private void PrepareAllSongs()
    {
        foreach (NoteChart nc in allNoteCharts)
        {
            GameObject createdSong = GameObject.Instantiate(songPrefab);

            createdSong.transform.SetParent(songListTransform);
            createdSong.transform.localScale = new Vector3(1, 1, 1);

            SongListItem sli = createdSong.GetComponent<SongListItem>();
            sli.noteChart = nc;
        }
    }

    public void OpenSideMenu() => sideMenuAnimator.SetTrigger("SlideIn");
    public void CloseSideMenu() => sideMenuAnimator.SetTrigger("SlideOut");

    public void Play() 
    {
        isPlaying = true;
    }

    public void Pause() 
    {
        isPausing = true;
    }

    public void Unpause() 
    {
        isPausing = false;
    }

    public void Stop()
    {
        if(isPlaying)
        {
            playButton.ForceUnpress();
            pauseButton.ForceUnpress();
            isPlaying = false;
            isPausing = false;
            globalSettings.ResetToPrerollTime();
        }
    }

    public void Slide(BaseEventData baseEventData)
    {
        PointerEventData ped = (PointerEventData) baseEventData;
        float draggedX = ped.delta.x;
        currentSlide += draggedX;
        currentSlide = Mathf.Clamp(currentSlide, rightEdge, leftEdge);

        if(Mathf.Abs(currentSlide) < clampRange)
        {
            currentSlide = 0;
        }

        kim.anchoredPosition = new Vector2(currentSlide, kim.anchoredPosition.y);
    }

}
