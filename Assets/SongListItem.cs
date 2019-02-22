using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[ExecuteAlways]
public class SongListItem : MonoBehaviour
{
    public NoteChart noteChart;
    public TextMeshProUGUI songNameText;
    public TextMeshProUGUI rhythmText;



    public void popUp () {
        SheetMusicView gs = (SheetMusicView)GameObject.FindObjectsOfTypeAll(typeof(SheetMusicView))[0];
       gs.gameObject.SetActive(true);
       gs.SongViewNote = noteChart ;
       gs.ActivePage = 0 ;
       gs.ActiveImage () ;

    } 

    public void Update()
    {
        if (noteChart != null)
        {
            songNameText.text = noteChart.songName;
            rhythmText.text = "| อัตราจังหวะ " + noteChart.rhythm + " ชั้น";
        }
    }

    public void PressCdButton()
    {
        GlobalSettings gs = GameObject.FindObjectOfType<GlobalSettings>();
        gs.waitMode = false;
        gs.selectedMusic = noteChart;
        gs.ResetToPrerollTime();
    }

    public void PressWaitButton()
    {
        GlobalSettings gs = GameObject.FindObjectOfType<GlobalSettings>();
        gs.waitMode = true;
        gs.selectedMusic = noteChart;
        gs.ResetToPrerollTime();
    }
}
