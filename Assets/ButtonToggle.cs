using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[ExecuteAlways]
public class ButtonToggle : MonoBehaviour
{
    public bool pressed = false;

    [Space]

    public Image image;
    public Sprite upImage;
    public Sprite downImage;

    [Space]

    public UnityEvent downEvent;
    public UnityEvent upEvent;

    public void Start()
    {
        image.sprite = upImage;
    }

    public void Update()
    {
        if(pressed)
        {
            image.sprite = downImage;
        }
        else
        {
            image.sprite = upImage;
        }
    }

    public void Press()
    {
        if(pressed)
        {
            pressed = false;
            upEvent.Invoke();
        }
        else
        {
            pressed = true;
            downEvent.Invoke();
        }
    }
}
