// Native Audio
// 5argon - Exceed7 Experiments
// Problems/suggestions : 5argon@exceed7.com

// BONUS ! For owner of Native Touch (http://exceed7.com/native-touch/)
// If you uncomment this + add reference to E7.NativeTouch on the NativeAudio's asmdef you can see how they work together!
// The red "Stop latest play" button will instead activate NativeTouch and disable all Unity touch. 
// From this point, any touch on the screen will be equal to touching the right side. (To reset you have to restart the app)
// But the touch has been speed up! So we can get even less perceived latency.
// #define NATIVE_TOUCH_INTEGRATION

using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Runtime.InteropServices;

using E7.Native;

public class NativeAudioDemo : MonoBehaviour {

    [SerializeField] private AudioSource audioSource;

    [Header("Native Audio now works with AudioClip from Unity's importer!")]
    [SerializeField] private AudioClip nativeClip1;
    [SerializeField] private AudioClip nativeClip2;

    [SerializeField] private Text fpsText;
    [SerializeField] private Text playbackTimeText;

    [Tooltip("Repeatedly play native audio every frame after the scene start without any input. Useful for remote testing on Firebase Test Lab, etc.")]
    [SerializeField] private bool autoRepeat;
    private NativeAudioPointer nativeAudioPointer;
    private NativeAudioController nativeAudioController;

    public void Start()
    {
        Application.targetFrameRate = 60;
        Debug.Log("Unity output sample rate : " + AudioSettings.outputSampleRate);
        if(autoRepeat)
        {
            Initialize();
            LoadAudio1();
            StartCoroutine(RepeatedPlayRoutine());
        }
    }

    IEnumerator RepeatedPlayRoutine()
    {
        while (true)
        {
            PlayAudio1();
            //PlayUnityAudioSource();
            yield return null;
        }
    }

    public void Update()
    {
        fpsText.text = (1/Time.deltaTime).ToString("0.00");
    }

    public void Initialize()
    {
#if UNITY_ANDROID
        var audioInfo = NativeAudio.GetDeviceAudioInformation();
        Debug.Log(audioInfo);
#endif

#if UNITY_EDITOR
        //You should have a fallback to normal AudioSource playing in your game so you can also hear sounds while developing.
        Debug.Log("Please try this in a real device!");
#else
        NativeAudio.Initialize(new NativeAudio.InitializationOptions { androidAudioTrackCount = 2 });
#endif
    }

    public void PlayUnityAudioSource()
    {
        audioSource.Play();
    }

    public void LoadAudio1()
    {
        LoadAudio(nativeClip1);
    }

    public void LoadAudio2()
    {
        LoadAudio(nativeClip2);
    }

#if NATIVE_TOUCH_INTEGRATION
    /// <summary>
    /// On Android this is on a different thread, but because Native Audio does not care about thread it works beautifully! Fast!
    /// </summary>
    public static void NTCallback(NativeTouchData ntd)
    {
        if(ntd.WarmUpTouch)
        {
            Debug.Log("Warm up");
            return;
        }
        if(ntd.Phase == TouchPhase.Began)
        {
            staticPointer.Play();
        }
    }
    public static NativeAudioPointer staticPointer;
#endif

    public bool NotRealDevice()
    {
#if UNITY_EDITOR
        //You should have a fallback to normal AudioSource playing in your game so you can also hear sounds while developing.
        Debug.Log("Please try this in a real device!");
        return true;
#else
        return false;
#endif
    }

    public void StopLatestPlay()
    {
#if NATIVE_TOUCH_INTEGRATION
        Debug.Log("Native touch started!!");
        staticPointer = nativeAudioPointer; 
        NativeTouch.RegisterCallback(NTCallback);
        NativeTouch.Start(new NativeTouch.StartOption { disableUnityTouch = true });
        NativeTouch.WarmUp();
        return;
#endif
        if (NotRealDevice()) return;
        if(nativeAudioController != null)
        {
            nativeAudioController.Stop();
        }
    }

    private void LoadAudio(AudioClip ac)
    {
        if (NotRealDevice()) return;
		nativeAudioPointer = NativeAudio.Load(ac);
        Debug.Log("Loaded audio of length "  + nativeAudioPointer.Length);
    }

    public void Prepare()
    {
        if (NotRealDevice()) return;
		nativeAudioPointer.Prepare();
    }

    public void PlayAudio1()
    {
        if (NotRealDevice()) return;
        nativeAudioController = nativeAudioPointer.Play();
    }

    public void PlayAudio2()
    {
        if (NotRealDevice()) return;
        var options = NativeAudio.PlayOptions.defaultOptions;
        options.volume = 0.3f;
        options.pan = 1f;
        nativeAudioController = nativeAudioPointer.Play(options);
    }

    public void PlayAudio3()
    {
        if (NotRealDevice()) return;
        var options = NativeAudio.PlayOptions.defaultOptions;
        options.volume = 0.5f;
        options.trackLoop = true;
        nativeAudioController = nativeAudioPointer.Play(options);

    }

    public void TrackPause()
    {
        if(nativeAudioController != null)
        {
            nativeAudioController.TrackPause();
        }
    }

    /// <summary>
    /// It can fail to resume if an underlying track has already been replaced with 
    /// other audio.
    /// </summary>
    public void TrackResume()
    {
        if (NotRealDevice()) return;
        if(nativeAudioController != null)
        {
            nativeAudioController.TrackResume();
        }
    }

    float rememberedTime;

    /// <summary>
    /// An another strategy to do pause/resume. This one is resistant to
    /// a track being replaced by other audio.
    /// </summary>
    public void RememberPause()
    {
        if (NotRealDevice()) return;
        if (nativeAudioController != null)
        {
            rememberedTime = nativeAudioController.GetPlaybackTime();
            nativeAudioController.Stop();
            Debug.Log("Pause and remembered time " + rememberedTime);
        }
    }

    /// <summary>
    /// An another strategy to do pause/resume. This one is resistant to
    /// a track being replaced by other audio.
    /// </summary>
    public void RememberResume()
    {
        if (NotRealDevice()) return;
        if (nativeAudioPointer != null)
        {
            var options = NativeAudio.PlayOptions.defaultOptions;
            options.offsetSeconds = rememberedTime;
            nativeAudioPointer.Play(options);
            Debug.Log("Resume from time " + rememberedTime);
        }
    }

    public void Unload()
    {
        if (NotRealDevice()) return;
        nativeAudioPointer.Unload();
    }

    public void GetPlaybackTime()
    {
        if (NotRealDevice()) return;
        if (nativeAudioController != null)
        {
            playbackTimeText.text = nativeAudioController.GetPlaybackTime().ToString();
        }
    }

}
