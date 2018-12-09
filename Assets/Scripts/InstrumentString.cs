using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using E7.Native;

public class InstrumentString : MonoBehaviour
{
    public AudioClip noteToPlay;
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
        asource.PlayOneShot(noteToPlay);
#else
        loadedAudio.Play();
#endif
    }
}
