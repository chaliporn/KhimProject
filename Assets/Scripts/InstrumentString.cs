using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using E7.Native;
using UnityEngine.UI;
using UnityEngine.Playables;

public class InstrumentString : MonoBehaviour
{
    public AudioClip noteToPlay;
    public NoteKind noteOfString;
    public GlobalSettings globalSettings;
    public Image activeMarker;

    public HorizontalLayoutGroup horizontalLayoutGroup;
    public PlayableDirector director;
    public int maxSpacing = 156;

    NativeAudioPointer loadedAudio;

    public void Awake()
    {
        director.Evaluate();
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

    private float spacingOfLastFrame = 0;

    public void Update()
    {
        if(globalSettings.selectedMusic == null)
        {
            return;
        }

        float timeUntil = TimeUntilNextNote(globalSettings.selectedMusic, globalSettings.currentSongTime);
        float nextNoteTime = NextNoteTimeOfThisString(globalSettings.selectedMusic, globalSettings.currentSongTime);
        float currentSongTime = globalSettings.currentSongTime;

        float visibleThreshold = 1;
        if(timeUntil > visibleThreshold)
        {
            //horizontalLayoutGroup.spacing = maxSpacing;
            //scaler.localScale = HLGToDirector(maxSpacing);
            director.time = HLGToDirector(maxSpacing);
            director.Evaluate();

            if(maxSpacing > spacingOfLastFrame)
            {
                HitString();
            }

            spacingOfLastFrame = maxSpacing;
        }
        else
        {
            var a = Mathf.InverseLerp(nextNoteTime - visibleThreshold, nextNoteTime,  currentSongTime);
            var b = Mathf.Lerp(maxSpacing, 0, a);

            //horizontalLayoutGroup.spacing = b; //0
            //scaler.localScale = HLGToDirector(b);
            director.time = HLGToDirector(b);
            director.Evaluate();

            if(b > spacingOfLastFrame)
            {
                HitString();
            }
            
            spacingOfLastFrame = b;
        }
    }

    public float HLGToDirector(float hlg)
    {
        float inverseLerp = Mathf.InverseLerp(maxSpacing, 0, hlg); // 0 - 1
        //Debug.Log($"{inverseLerp} , {(float)director.playableAsset.duration}");
        return Mathf.Lerp(0, (float)director.playableAsset.duration, inverseLerp);
    }

    public float TimeUntilNextNote(NoteChart currentSong, float currentSongTime)
    {
        float noteTime =  NextNoteTimeOfThisString(currentSong, currentSongTime); // 1
        float distanceSeconds = noteTime - currentSongTime; //0.5
        return distanceSeconds;
    }

    private float NextNoteTimeOfThisString(NoteChart currentSong, float currentSongTime) //LJ, 0.5
    {
        float currentBeat = currentSong.bpm * (currentSongTime / 60.0f);
        int currentBeatInteger = Mathf.FloorToInt(currentBeat);
            //Debug.Log($"Current beat is {currentBeat}");
        for (int i = currentBeatInteger; i < currentSong.beats.Length; i++)
        {
            if(i < 0)
            {
                continue;
            }
            Beat inspectingThisBeat = currentSong.beats[i];
            bool thisIsFirstBeat = i == currentBeatInteger;
            float percentTime = -1;
            float x = currentBeat - currentBeatInteger;
            if (inspectingThisBeat.noteEvent1.noteL.noteKind == noteOfString && DoNotSkip(x, 0, thisIsFirstBeat))
            {
                percentTime = 0;
            }
            else if (inspectingThisBeat.noteEvent1.noteR.noteKind == noteOfString && DoNotSkip(x, 0, thisIsFirstBeat))
            {
                percentTime = 0;
            }

            else if (inspectingThisBeat.noteEvent2.noteL.noteKind == noteOfString && DoNotSkip(x, 0.25f, thisIsFirstBeat))
            {
                percentTime = 0.25f;
            }
            else if (inspectingThisBeat.noteEvent2.noteR.noteKind == noteOfString && DoNotSkip(x, 0.25f, thisIsFirstBeat))
            {
                percentTime = 0.25f;
            }

            else if (inspectingThisBeat.noteEvent3.noteL.noteKind == noteOfString && DoNotSkip(x, 0.50f, thisIsFirstBeat))
            {
                percentTime = 0.50f;
            }
            else if (inspectingThisBeat.noteEvent3.noteR.noteKind == noteOfString && DoNotSkip(x, 0.50f, thisIsFirstBeat))
            {
                percentTime = 0.50f;
            }

            else if (inspectingThisBeat.noteEvent4.noteL.noteKind == noteOfString && DoNotSkip(x, 0.75f, thisIsFirstBeat))
            {
                percentTime = 0.75f;
            }
            else if (inspectingThisBeat.noteEvent4.noteR.noteKind == noteOfString && DoNotSkip(x, 0.75f, thisIsFirstBeat))
            {
                percentTime = 0.75f;
            }

            bool found = percentTime != -1;

            if (found)
            {
                var timeOfThisBeat = 60f * percentTime / (float)currentSong.bpm;
                int howManyPreviousBeats = i;
                float timePerOneBeat = 60f / (float)currentSong.bpm;
                float timeOfAllPreviousBeat = timePerOneBeat * howManyPreviousBeats;

                float answer = timeOfThisBeat + timeOfAllPreviousBeat;
                return answer;
            }
        }
        return 99999999;
    }

    public bool DoNotSkip(float x, float y, bool isThisFirstBeat) => !Skip(x, y, isThisFirstBeat);

    public bool Skip(float x, float y, bool isThisFirstBeat)
    {
        if(!isThisFirstBeat)
        {
            return false;
        }

        if (x > y)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void ShowActiveMarker() => activeMarker.enabled = true;
    public void HideActiveMarker() => activeMarker.enabled = false;

    
}
