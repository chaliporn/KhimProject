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
        return 0;
    }
}
