using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainLogic : MonoBehaviour
{
    public NoteChart[] allNoteCharts;
    public GameObject songPrefab;
    public Transform songListTransform;
    public Animator sideMenuAnimator;

    void Start()
    {
        PrepareAllSongs();
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

    public void OpenSideMenu()
    {
        sideMenuAnimator.SetTrigger("SlideIn");
    }
    
    public void CloseSideMenu()
    {
        sideMenuAnimator.SetTrigger("SlideOut");
    }
}
