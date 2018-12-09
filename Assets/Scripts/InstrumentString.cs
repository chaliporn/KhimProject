using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using E7.Native;

public class InstrumentString : MonoBehaviour
{
    //public enum Note
    //{
    //    H1,
    //    H2,
    //    H3,
    //    H4,
    //    H5,
    //    H6,
    //    H7,

    //    M1,
    //    M2,
    //    M3,
    //    M4,
    //    M5,
    //    M6,
    //    M7,

    //    L1,
    //    L2,
    //    L3,
    //    L4,
    //    L5,
    //    L6,
    //    L7,
    //}

    //public Note noteToPlay;

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
