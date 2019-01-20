using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GlobalSettings : MonoBehaviour
{
    public Slider volumeSlider;
    [Range(0,5)]
    public float currentSongTime;
    public float noteSpeed = 306;
    public NoteChart selectedMusic;

}
