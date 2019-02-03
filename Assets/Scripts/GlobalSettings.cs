using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GlobalSettings : MonoBehaviour
{
    public Slider volumeSlider;
    [Range(-2,5)]
    public float currentSongTime;
    public float prerollTime = 2;
    public float noteSpeed = 306;
    public NoteChart selectedMusic;

    public void ResetToPrerollTime()
    {
        currentSongTime = -prerollTime;
    }

}
