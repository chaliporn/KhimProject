using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using E7.Native;
using UnityEngine.UI;

public class InstrumentString : MonoBehaviour
{
    public AudioClip noteToPlay;
    public NoteKind noteOfString;
    public GlobalSettings globalSettings;
    public Image activeMarker;

    NativeAudioPointer loadedAudio;

    public void Awake()
    {
#if !UNITY_EDITOR
        loadedAudio = NativeAudio.Load(noteToPlay);
#endif
    }

    public void HitString()
    {
#if UNITY_EDITOR
        AudioSource asource = gameObject.GetComponent<AudioSource>();
        asource.volume = globalSettings.volumeSlider.value;
        asource.PlayOneShot(noteToPlay);
#else
        var option = NativeAudio.PlayOptions.defaultOptions;
        option.volume = globalSettings.volumeSlider.value;
        loadedAudio.Play(option);
#endif
    }

    public void ShowActiveMarker() => activeMarker.enabled = true;
    public void HideActiveMarker() => activeMarker.enabled = false;

    public float WhereAmI(NoteChart currentSong, float currentTime)
    {
        float noteTime =  NextNoteTimeOfThisString(currentSong, currentTime); // 1
        float distanceSeconds = noteTime - currentTime; //0.5
        return distanceSeconds * globalSettings.noteSpeed;
    }

    private float NextNoteTimeOfThisString(NoteChart currentSong, float currentTime) //LJ, 0.5
    {
        return 0; //TODO 

        float whatBeat = (currentSong.bpm / 60f) * currentTime; 
        int whatBeatInteger = Mathf.FloorToInt(whatBeat);

        float foundAt = -1;

        for(int i = whatBeatInteger; i < currentSong.beats.Length; i++)
        {
            Beat checkingBeat = currentSong.beats[i];
            if(checkingBeat.noteEvent1.noteL.noteKind == noteOfString) foundAt = i + 0;
            if(checkingBeat.noteEvent1.noteR.noteKind == noteOfString) foundAt = i + 0;
            if(checkingBeat.noteEvent2.noteL.noteKind == noteOfString) foundAt = i + 0.25f;
            if(checkingBeat.noteEvent2.noteR.noteKind == noteOfString) foundAt = i + 0.25f;
            if(checkingBeat.noteEvent1.noteL.noteKind == noteOfString) foundAt = i + 0;
            if(checkingBeat.noteEvent1.noteL.noteKind == noteOfString) foundAt = i + 0;
            if(checkingBeat.noteEvent1.noteL.noteKind == noteOfString) foundAt = i + 0;
            if(checkingBeat.noteEvent1.noteL.noteKind == noteOfString) foundAt = i + 0;
            if(checkingBeat.noteEvent1.noteL.noteKind == noteOfString) foundAt = i + 0;
            if(checkingBeat.noteEvent1.noteL.noteKind == noteOfString) foundAt = i + 0;
        }
    }
}
