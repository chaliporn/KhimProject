using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(Note))]
public class NoteChartDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty noteKind = property.FindPropertyRelative("noteKind");
        SerializedProperty trill = property.FindPropertyRelative("trill");
        SerializedProperty trillLength = property.FindPropertyRelative("trillLength");

        var r = position;
        r.width = r.width / 2;
        EditorGUI.PropertyField(r, noteKind, new GUIContent(""));

        bool isChecked = trill.boolValue;

        if (isChecked)
        {
            r.width = r.width / 2f;
            r.x = r.x + (r.width * 2);
            EditorGUI.PropertyField(r, trill, new GUIContent(""));
            r.x = r.x + (r.width );
            EditorGUI.PropertyField(r, trillLength, new GUIContent(""));
        }
        else
        {
            r.x = r.x + r.width;
            EditorGUI.PropertyField(r, trill, new GUIContent(""));
        }

    }
}

[CustomPropertyDrawer(typeof(NoteEvent))]
public class NoteEventDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty handL = property.FindPropertyRelative("noteL");
        SerializedProperty handR = property.FindPropertyRelative("noteR");
        var r = position;
        r.width = r.width / 2;
        EditorGUI.PropertyField(r, handL, new GUIContent(""));
        r.x = r.x + r.width;
        EditorGUI.PropertyField(r, handR, new GUIContent(""));
    }
}

// [CustomPropertyDrawer(typeof(Measure))]
// public class MeasureDrawer : PropertyDrawer
// {
//     // public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//     // {
//     // }
// }