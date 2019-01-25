using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainLogic : MonoBehaviour
{
    public NoteChart[] allNoteCharts;
    public GameObject songPrefab;
    public Transform songListTransform;
    public Animator sideMenuAnimator;
    public GlobalSettings globalSettings;

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
            globalSettings.currentSongTime = 0;
        }
    }

}
