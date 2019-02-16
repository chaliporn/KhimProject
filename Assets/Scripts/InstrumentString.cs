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

    public Image innerMarker;
    public Image markerBorder;

    public Color nonTrillBorderColor;
    public Color trillBorderColor;
    public Color singleMarkerColor;
    public Color doubleMarkerColor;

    NativeAudioPointer loadedAudio;

    public void Awake()
    {
        director.Evaluate();
#if !UNITY_EDITOR
        loadedAudio = NativeAudio.Load(noteToPlay);
#endif
    }

    bool stopBecauseThisString = false;

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

        if (stopBecauseThisString)
        {
            MainLogic searchMainLogic = GameObject.FindObjectOfType<MainLogic>();
            searchMainLogic.Unpause();
            globalSettings.currentSongTime += 0.001f;

            foreach (var instString in GameObject.FindObjectsOfType<InstrumentString>())
            {
                instString.stopBecauseThisString = false;
            }
        }

    }

    public void Update()
    {
        if (globalSettings.selectedMusic == null)
        {
            return;
        }

        SoundPlayingUpdate();
        AppearanceUpdate();
        timePreviousFrame = timeThisFrame;
    }


    float timePreviousFrame = -999;
    float noteTimeNextToPreviousFrameTime;
    float timeThisFrame;
    private void SoundPlayingUpdate()
    {
        var tuple = NextNoteTimeOfThisString(globalSettings.selectedMusic, timePreviousFrame);
        noteTimeNextToPreviousFrameTime = tuple.noteTime;
        timeThisFrame = globalSettings.currentSongTime;
        bool isTrill = tuple.thatNote.trill;

        // if (noteOfString == NoteKind.ซ2)
        // {
        //     Debug.Log($"CHECKING {timePreviousFrame} < {noteTimeNextToPreviousFrameTime} && {timeThisFrame} > {noteTimeNextToPreviousFrameTime}");
        // }

        // Passed a note
        if (timePreviousFrame < noteTimeNextToPreviousFrameTime && timeThisFrame >= noteTimeNextToPreviousFrameTime)
        {
            if (globalSettings.waitMode)
            {
                MainLogic searchMainLogic = GameObject.FindObjectOfType<MainLogic>();
                searchMainLogic.Pause();
                stopBecauseThisString = true;
                //Debug.Log("Stop on string " + noteOfString);
            }
            else
            {

                if (isTrill)
                {
                    // Debug.Log("TRILL string " + noteOfString);
                    float tailLength = tuple.thatNote.trillLength;
                    float tailDuration = (tailLength * 60.0f) / globalSettings.selectedMusic.bpm;

                    if (tuple.oppositeSideNote.trill)
                    {
                        if (tuple.thatNote.hand == Hand.Left)
                        {
                            StartCoroutine(PlayAndStopRoutine(tailDuration, delayed: false));
                        }
                        else
                        {
                            StartCoroutine(PlayAndStopRoutine(tailDuration, delayed: true));
                        }
                    }
                    else
                    {
                        //Debug.Log("Single trill");
                        StartCoroutine(PlayAndStopRoutine(tailDuration, delayed: false));
                        StartCoroutine(PlayAndStopRoutine(tailDuration, delayed: true));
                    }
                }
                else
                {
                    // Debug.Log("Hitting string " + noteOfString);
                    HitString();
                }
            }
        }

    }

    IEnumerator PlayAndStopRoutine(float tailDuration, bool delayed)
    {
        int timesToPlayPerSecond = 6;
        int timesToPlay = Mathf.CeilToInt(timesToPlayPerSecond * tailDuration);
        float timeToWait = tailDuration / timesToPlay;

        if (delayed == true)
        {
            yield return new WaitForSeconds(timeToWait / 2);
        }

        for (int i = 0; i < timesToPlay; i++)
        {
            //Debug.Log("Hit");
            HitString();
            yield return new WaitForSeconds(timeToWait);
        }
    }

    private void AppearanceUpdate()
    {
        float timeUntil = TimeUntilNextNote(globalSettings.selectedMusic, globalSettings.currentSongTime);
        var tuple = NextNoteTimeOfThisString(globalSettings.selectedMusic, globalSettings.currentSongTime);
        float nextNoteTime = tuple.noteTime;
        float currentSongTime = globalSettings.currentSongTime;

        float visibleThreshold = 1;
        var a = Mathf.InverseLerp(nextNoteTime - visibleThreshold, nextNoteTime, currentSongTime);
        var b = Mathf.Lerp(maxSpacing, 0, a);

        if (stopBecauseThisString)
        {
            //Debug.Log($"Force Zero Original B : {b}");
            b = 0;
        }

        //horizontalLayoutGroup.spacing = b; //0
        //scaler.localScale = HLGToDirector(b);
        director.time = HLGToDirector(b);
        director.Evaluate();

        if (stopBecauseThisString == false)
        {
            if (tuple.thatNote.trill)
            {
                Color copy = markerBorder.color;

                Color colorToSet = trillBorderColor;
                colorToSet.a = copy.a;

                markerBorder.color = colorToSet;
            }
            else
            {
                Color copy = markerBorder.color;

                Color colorToSet = nonTrillBorderColor;
                colorToSet.a = copy.a;

                markerBorder.color = colorToSet;
            }

            if (tuple.oppositeSideNote.noteKind != NoteKind.None)
            {
                Color copy = innerMarker.color;

                Color colorToSet = doubleMarkerColor;
                colorToSet.a = copy.a;

                innerMarker.color = colorToSet;
            }
            else
            {
                Color copy = innerMarker.color;

                Color colorToSet = singleMarkerColor;
                colorToSet.a = copy.a;

                innerMarker.color = colorToSet;
            }
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
        float noteTime = NextNoteTimeOfThisString(currentSong, currentSongTime).noteTime; // 1
        float distanceSeconds = noteTime - currentSongTime; //0.5
        return distanceSeconds;
    }

    private (float noteTime, Note thatNote, Note oppositeSideNote) NextNoteTimeOfThisString(NoteChart currentSong, float currentSongTime) //LJ, 0.5
    {
        float currentBeatFloat = currentSong.bpm * (currentSongTime / 60.0f);
        //Debug.Log($"{noteOfString} Current beat is {currentBeat}");
        for (int i = 0; i < currentSong.beats.Length; i++)
        {
            //     if(noteOfString == NoteKind.ม3)
            //     {
            //    Debug.Log($"{noteOfString} current i {i}");
            //     }
            if (i < 0)
            {
                continue;
            }
            Beat inspectingThisBeat = currentSong.beats[i];
            float percentTime = -1;
            float x = currentBeatFloat;

            float yTail = -1;
            Note foundNote = default;
            Note oppositeSideNote = default;

            if (inspectingThisBeat.noteEvent1.noteL.noteKind == noteOfString && DoNotSkip(x, i + 0, inspectingThisBeat.noteEvent1.noteL.trillLength))
            {
                percentTime = 0;

                foundNote = inspectingThisBeat.noteEvent1.noteL;
                oppositeSideNote = inspectingThisBeat.noteEvent1.noteR;

                yTail = inspectingThisBeat.noteEvent1.noteL.trillLength;
            }
            else if (inspectingThisBeat.noteEvent1.noteR.noteKind == noteOfString && DoNotSkip(x, i + 0, inspectingThisBeat.noteEvent1.noteR.trillLength))
            {
                percentTime = 0;

                foundNote = inspectingThisBeat.noteEvent1.noteR;
                oppositeSideNote = inspectingThisBeat.noteEvent1.noteL;

                yTail = inspectingThisBeat.noteEvent1.noteR.trillLength;
            }

            else if (inspectingThisBeat.noteEvent2.noteL.noteKind == noteOfString && DoNotSkip(x, i + 0.25f, inspectingThisBeat.noteEvent2.noteL.trillLength))
            {
                percentTime = 0.25f;

                foundNote = inspectingThisBeat.noteEvent2.noteL;
                oppositeSideNote = inspectingThisBeat.noteEvent2.noteR;

                yTail = inspectingThisBeat.noteEvent2.noteL.trillLength;
            }
            else if (inspectingThisBeat.noteEvent2.noteR.noteKind == noteOfString && DoNotSkip(x, i + 0.25f, inspectingThisBeat.noteEvent2.noteR.trillLength))
            {
                percentTime = 0.25f;

                foundNote = inspectingThisBeat.noteEvent2.noteR;
                oppositeSideNote = inspectingThisBeat.noteEvent2.noteL;

                yTail = inspectingThisBeat.noteEvent2.noteR.trillLength;
            }

            else if (inspectingThisBeat.noteEvent3.noteL.noteKind == noteOfString && DoNotSkip(x, i + 0.50f, inspectingThisBeat.noteEvent3.noteL.trillLength))
            {
                percentTime = 0.50f;

                foundNote = inspectingThisBeat.noteEvent3.noteL;
                oppositeSideNote = inspectingThisBeat.noteEvent3.noteR;

                yTail = inspectingThisBeat.noteEvent3.noteL.trillLength;
            }
            else if (inspectingThisBeat.noteEvent3.noteR.noteKind == noteOfString && DoNotSkip(x, i + 0.50f, inspectingThisBeat.noteEvent3.noteR.trillLength))
            {
                percentTime = 0.50f;

                foundNote = inspectingThisBeat.noteEvent3.noteR;
                oppositeSideNote = inspectingThisBeat.noteEvent3.noteL;

                yTail = inspectingThisBeat.noteEvent3.noteR.trillLength;
            }

            else if (inspectingThisBeat.noteEvent4.noteL.noteKind == noteOfString && DoNotSkip(x, i + 0.75f, inspectingThisBeat.noteEvent4.noteL.trillLength))
            {
                percentTime = 0.75f;

                foundNote = inspectingThisBeat.noteEvent4.noteL;
                oppositeSideNote = inspectingThisBeat.noteEvent4.noteR;

                yTail = inspectingThisBeat.noteEvent4.noteL.trillLength;
            }
            else if (inspectingThisBeat.noteEvent4.noteR.noteKind == noteOfString && DoNotSkip(x, i + 0.75f, inspectingThisBeat.noteEvent4.noteR.trillLength))
            {
                percentTime = 0.75f;

                foundNote = inspectingThisBeat.noteEvent4.noteR;
                oppositeSideNote = inspectingThisBeat.noteEvent4.noteL;

                yTail = inspectingThisBeat.noteEvent4.noteR.trillLength;
            }

            bool found = percentTime != -1;

            if (found)
            {
                var timeOfThisBeat = 60f * percentTime / (float)currentSong.bpm;
                int howManyPreviousBeats = i;
                float timePerOneBeat = 60f / (float)currentSong.bpm;
                float timeOfAllPreviousBeat = timePerOneBeat * howManyPreviousBeats;

                float answer = timeOfThisBeat + timeOfAllPreviousBeat;
                if (answer < currentSongTime && answer + yTail >= currentSongTime)
                {
                    return (currentSongTime, foundNote, oppositeSideNote);
                }
                else
                {
                    return (answer, foundNote, oppositeSideNote);
                }
            }
        }
        return (99999999, default, default);
    }

    public bool DoNotSkip(float x, float y, float yTail) => !Skip(x, y, yTail);

    public bool Skip(float x, float y, float yTail)
    {
        if (x > y + yTail)
        {
            //     if(noteOfString == NoteKind.ม3)
            //     {
            //    Debug.Log($"{noteOfString} skip {x} {y} {yTail}");
            //     }
            return true;
        }
        else
        {
            // if(noteOfString == NoteKind.ม3)
            // {
            // Debug.Log($"{noteOfString} not skip {x} {y} {yTail}");
            // }
            return false;
        }
    }

    public void ShowActiveMarker() => activeMarker.enabled = true;
    public void HideActiveMarker() => activeMarker.enabled = false;


}
