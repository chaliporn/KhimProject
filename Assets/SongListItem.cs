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
        gs.selectedMusic = noteChart;
    }
}
