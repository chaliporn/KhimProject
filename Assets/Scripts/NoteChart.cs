using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class NoteChart : ScriptableObject 
{
    public int bpm = 120;
    public Measure[] measures;
}

[System.Serializable]
public struct Measure
{
    public NoteEvent noteEvent1;
    public NoteEvent noteEvent2;
    public NoteEvent noteEvent3;
    public NoteEvent noteEvent4;
    public NoteEvent noteEvent5;
    public NoteEvent noteEvent6;
    public NoteEvent noteEvent7;
    public NoteEvent noteEvent8;
}

[System.Serializable]
public struct NoteEvent
{
    public Note noteL;
    public Note noteR;
}

[System.Serializable]
public struct Note
{
    public NoteKind noteKind;
    public bool trill;
}

public enum NoteKind
{
    None,

    ล1, //สูง
    ซ1,
    ฟ1,
    ม1,
    ร1,
    ด1,
    ท1,

    ร2,//กลาง
    ด2,
    ท2,
    ล2,
    ซ2,
    ฟ2,
    ม2,

    ซ3, //ต่ำ
    ฟ3,
    ม3,
    ร3,
    ด3,
    ท3,
    ล3,




}