// Native Audio
// 5argon - Exceed7 Experiments
// Problems/suggestions : 5argon@exceed7.com

#define USE_NATIVE_COLLECTIONS

using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System;

#if USE_NATIVE_COLLECTIONS
using Unity.Collections;
#endif

namespace E7.Native
{
    public partial class NativeAudio
    {
#if UNITY_IOS
        [DllImport("__Internal")]
        internal static extern int _Initialize();

        [DllImport("__Internal")]
        internal static extern int _SendByteArray(short[] byteArrayInput, int byteSize, int channels, int samplingRate, LoadOptions.ResamplingQuality resamplingQuality);

        [DllImport("__Internal")]
        internal static extern int _LoadAudio(string soundUrl, LoadOptions.ResamplingQuality resamplingQuality);

        [DllImport("__Internal")]
        internal static extern int _PrepareAudio(int bufferIndex);

        [DllImport("__Internal")]
        internal static extern int _PlayAudio(int bufferIndex, int sourceCycle, PlayOptions playOptions);

        [DllImport("__Internal")]
        internal static extern void _PlayAudioWithSourceCycle(int sourceCycle, PlayOptions playOptions);

        [DllImport("__Internal")]
        internal static extern void _UnloadAudio(int bufferIndex);

        [DllImport("__Internal")]
        internal static extern float _LengthBySource(int bufferIndex);

        // -- Operates on sound "source" chosen for a particular audio --
        // ("source" terms of OpenAL is like a speaker, not the "source of data" which is a loaded byte array.)

        [DllImport("__Internal")]
        internal static extern void _StopAudio(int sourceCycle);

        [DllImport("__Internal")]
        internal static extern void _SetVolume(int sourceCycle, float volume);

        [DllImport("__Internal")]
        internal static extern void _SetPan(int sourceCycle, float pan);

        [DllImport("__Internal")]
        internal static extern float _GetPlaybackTime(int sourceCycle);

        [DllImport("__Internal")]
        internal static extern void _SetPlaybackTime(int sourceCycle, float offsetSeconds);

        [DllImport("__Internal")]
        internal static extern void _TrackPause(int sourceCycle);

        [DllImport("__Internal")]
        internal static extern void _TrackResume(int sourceCycle);
#endif

#if UNITY_ANDROID
        private static AndroidJavaClass androidNativeAudio;
        internal static AndroidJavaClass AndroidNativeAudio
        {
            get
            {
                if (androidNativeAudio == null)
                {
                    androidNativeAudio = new AndroidJavaClass("com.Exceed7.NativeAudio.NativeAudio");
                }
                return androidNativeAudio;
            }
        }

        /// <summary>
        /// [Android] Initialize needs to contact Java as it need the device's native sampling rate and native buffer size to get the "fast path" audio.
        /// </summary>
        internal const string AndroidInitialize = "Initialize";

        /// <summary>
        /// [Android] Load needs to contact Java as it needs to read the audio file sent from `StreamingAssets`,
        /// which could end up in either app persistent space or an another OBB package which we will unpack it and get the content.
        /// </summary>
        internal const string AndroidLoadAudio = "LoadAudio";

        internal const string AndroidGetDeviceAudioInformation = "GetDeviceAudioInformation";

        // -- Operates on an audio file ("source" of data) --

        //The lib name is libnativeaudioe7

        [DllImport("nativeaudioe7")]
        internal static extern int sendByteArray(short[] byteArrayInput, int byteSize, int channels, int samplingRate, LoadOptions.ResamplingQuality resamplingQuality);

        [DllImport("nativeaudioe7")]
        internal static extern int playAudio(int sourceIndex, PlayOptions playOptions);
        [DllImport("nativeaudioe7")]
        internal static extern void unloadAudio(int sourceIndex);
        [DllImport("nativeaudioe7")]
        internal static extern float lengthBySource(int sourceIndex);

        // -- Operates on an audio track chosen for a particular audio --

        [DllImport("nativeaudioe7")]
        internal static extern int stopAudio(int playerIndex);
        [DllImport("nativeaudioe7")]
        internal static extern void setVolume(int playerIndex, float volume);
        [DllImport("nativeaudioe7")]
        internal static extern void setPan(int playerIndex, float pan);
        [DllImport("nativeaudioe7")]
        internal static extern float getPlaybackTime(int playerIndex);
        [DllImport("nativeaudioe7")]
        internal static extern void setPlaybackTime(int playerIndex, float offsetSeconds);
        [DllImport("nativeaudioe7")]
        internal static extern void trackPause(int playerIndex);
        [DllImport("nativeaudioe7")]
        internal static extern void trackResume(int playerIndex);

#endif

        private static bool initialized = false;

        /// <summary>
        /// Note that initialization of NativeAudio is currently permanent, unlike loading of audio which can be unloaded.
        /// In other words, if you use `initializationOptions` you won't be able to change your mind later. The resources at native side will be permanently allocated
        /// according to that options. Calling `Initialize()` again is prevented at managed side and will do nothing.
        /// 
        /// [iOS] Initializes OpenAL. Then 16 OpenAL sources will be allocated all at once. You have a maximum of 16 concurrency shared for all sounds.
        /// 
        /// [Android] Initializes OpenSL ES. Then 1 OpenSL ES Engine and a number of AudioPlayer object (and in turn AudioTrack) will be allocated all at once.
        /// See `InitializationOption` overload how to modify this.
        /// (More about this limit : https://developer.android.com/ndk/guides/audio/opensl/opensl-for-android)
        /// (And my own research here : https://gametorrahod.com/androids-native-audio-primer-for-unity-developers-65acf66dd124)
        /// </summary>
        public static void Initialize()
        {
            Initialize(InitializationOptions.defaultOptions);
        }

        public static void Initialize(InitializationOptions initializationOptions)
        {
            if (!initialized)
            {
                int errorCode;
#if UNITY_IOS
                errorCode = _Initialize();
                if (errorCode == -1)
                {
                    throw new System.Exception("There is an error initializing Native Audio.");
                }
                //There is also a check at native side but just to be safe here.
                initialized = true;
#elif UNITY_ANDROID
                errorCode = AndroidNativeAudio.CallStatic<int>(AndroidInitialize, initializationOptions.androidAudioTrackCount, initializationOptions.androidMinimumBufferSize);
                if(errorCode == -1)
                {
                    throw new System.Exception("There is an error initializing Native Audio.");
                }
                //There is no check at the native C side so without this and we initialize again it is a crash!
                initialized = true;
#endif
            }
        }

        /// <summary>
        /// Loads by copying Unity-imported `AudioClip`'s raw audio memory to native side. You are free to unload the `AudioClip`'s audio data without affecting what's loaded at the native side.
        /// 
        /// Hard requirements : 
        /// - Load type MUST be Decompress On Load so Native Audio could read raw PCM byte array from your compressed audio.
        /// - If you use Load In Background, you must call `audioClip.LoadAudioData()` beforehand and ensure that `audioClip.loadState` is `AudioDataLoadState.Loaded` before calling `NativeAudio.Load`. Otherwise it would throw an exception. If you are not using Load In Background but also not using Preload Audio Data, Native Audio can load for you if not yet loaded.
        /// - Must not be ambisonic.
        /// 
        /// It supports all compression format, force to mono, overriding to any sample rate, and quality slider.
        /// 
        /// If this is the first time loading any audio it will call `NativeAudio.Initialize()` automatically which might take a bit more time.
        /// 
        /// [iOS] Loads an audio into OpenAL's output audio buffer. (Max 256) This buffer will be paired to one of 16 OpenAL source when you play it.
        /// 
        /// [Android] Loads an audio into a `short*` array at unmanaged native side. This array will be pushed into one of available `SLAndroidSimpleBufferQueue` when you play it.
        /// The resampling of audio will occur at this moment to match your player's device native rate. The SLES audio player must be created to match the device rate
        /// to enable the special "fast path" audio. What's left is to make our audio compatible with that fast path player, which the resampler will take care of.
        /// 
        /// You can change the sampling quality of SRC (libsamplerate) library per audio basis with the `LoadOptions` overload.
        /// </summary>
        /// <param name="audioClip">
        /// Hard requirements : 
        /// - Load type MUST be Decompress On Load so Native Audio could read raw PCM byte array from your compressed audio.
        /// - If you use Load In Background, you must call `audioClip.LoadAudioData()` beforehand and ensure that `audioClip.loadState` is `AudioDataLoadState.Loaded` before calling `NativeAudio.Load`. Otherwise it would throw an exception. If you are not using Load In Background but also not using Preload Audio Data, Native Audio can load for you if not yet loaded.
        /// - Must not be ambisonic.
        /// </param>
        /// <returns> An object that stores a number. Native side can pair this number with an actual loaded audio data when you want to play it. You can `Play`, `Prepare`, or `Unload` with this object. `Load` returns null on error, for example : wrong name, or calling Load in Editor </returns>
        public static NativeAudioPointer Load(AudioClip audioClip)
        {
            return Load(audioClip, LoadOptions.defaultOptions);
        }

        /// <summary>
        /// Loads by copying Unity-imported `AudioClip`'s raw audio memory to native side. You are free to unload the `AudioClip`'s audio data without affecting what's loaded at the native side.
        /// 
        /// Hard requirements : 
        /// - Load type MUST be Decompress On Load so Native Audio could read raw PCM byte array from your compressed audio.
        /// - If you use Load In Background, you must call `audioClip.LoadAudioData()` beforehand and ensure that `audioClip.loadState` is `AudioDataLoadState.Loaded` before calling `NativeAudio.Load`. Otherwise it would throw an exception. If you are not using Load In Background but also not using Preload Audio Data, Native Audio can load for you if not yet loaded.
        /// - Must not be ambisonic.
        /// 
        /// It supports all compression format, force to mono, overriding to any sample rate, and quality slider.
        /// 
        /// If this is the first time loading any audio it will call `NativeAudio.Initialize()` automatically which might take a bit more time.
        /// 
        /// [iOS] Loads an audio into OpenAL's output audio buffer. (Max 256) This buffer will be paired to one of 16 OpenAL source when you play it.
        /// 
        /// [Android] Loads an audio into a `short*` array at unmanaged native side. This array will be pushed into one of available `SLAndroidSimpleBufferQueue` when you play it.
        /// The resampling of audio will occur at this moment to match your player's device native rate. The SLES audio player must be created to match the device rate
        /// to enable the special "fast path" audio. What's left is to make our audio compatible with that fast path player, which the resampler will take care of.
        /// 
        /// You can change the sampling quality of SRC (libsamplerate) library per audio basis with the `LoadOptions` overload.
        /// </summary>
        /// <param name="audioClip">
        /// Hard requirements : 
        /// - Load type MUST be Decompress On Load so Native Audio could read raw PCM byte array from your compressed audio.
        /// - If you use Load In Background, you must call `audioClip.LoadAudioData()` beforehand and ensure that `audioClip.loadState` is `AudioDataLoadState.Loaded` before calling `NativeAudio.Load`. Otherwise it would throw an exception. If you are not using Load In Background but also not using Preload Audio Data, Native Audio can load for you if not yet loaded.
        /// - Must not be ambisonic.
        /// </param>
        /// <returns> An object that stores a number. Native side can pair this number with an actual loaded audio data when you want to play it. You can `Play`, `Prepare`, or `Unload` with this object. `Load` returns null on error, for example : wrong name, or calling in Editor </returns>
        public static NativeAudioPointer Load(AudioClip audioClip, LoadOptions loadOptions)
        {
            AssertAudioClip(audioClip);
            if (!initialized)
            {
                NativeAudio.Initialize();
            }

            //We have to wait for GC to collect this big array, or you could do `GC.Collect()` immediately after.
            short[] shortArray = AudioClipToShortArray(audioClip);

#if UNITY_IOS
            int startingIndex = _SendByteArray(shortArray, shortArray.Length * 2, audioClip.channels, audioClip.frequency, loadOptions.resamplingQuality);
            if (startingIndex == -1)
            {
                throw new Exception("Error loading NativeAudio with AudioClip named : " + audioClip.name);
            }
            else
            {
                float length = _LengthBySource(startingIndex);
                return new NativeAudioPointer(audioClip.name, startingIndex, length);
            }
#elif UNITY_ANDROID

            //The native side will interpret short array as byte array, thus we double the length.
            int startingIndex = sendByteArray(shortArray, shortArray.Length * 2, audioClip.channels, audioClip.frequency, loadOptions.resamplingQuality);


            if(startingIndex == -1)
            {
                throw new Exception("Error loading NativeAudio with AudioClip named : " + audioClip.name);
            }
            else
            {
                float length = lengthBySource(startingIndex);
                return new NativeAudioPointer(audioClip.name, startingIndex, length);
            }
#else
            //Load is defined on editor so that autocomplete shows up, but it is a stub. If you mistakenly use the pointer in editor instead of forwarding to normal sound playing method you will get a null reference error.
            return null;
#endif
        }

        private static void AssertAudioClip(AudioClip audioClip)
        {
            if(audioClip.loadType != AudioClipLoadType.DecompressOnLoad)
            {
                throw new Exception(string.Format("Your audio clip {0} load type is not Decompress On Load but {1}. Native Audio needs to read the raw PCM data by that import mode.", audioClip.name, audioClip.loadType));
            }
            if(audioClip.channels != 1 && audioClip.channels != 2)
            {
                throw new Exception(string.Format("Native Audio only supports mono or stereo. Your audio {0} has {1} channels", audioClip.name, audioClip.channels));
            }
            if(audioClip.ambisonic)
            {
                throw new Exception("Native Audio does not support ambisonic audio!");
            }
            if(audioClip.loadState != AudioDataLoadState.Loaded && audioClip.loadInBackground)
            {
                throw new Exception("Your audio is not loaded yet while having the import settings Load In Background. Native Audio cannot wait for loading asynchronously for you and it would results in an empty audio. To keep Load In Background import settings, call `audioClip.LoadAudioData()` beforehand and ensure that `audioClip.loadState` is `AudioDataLoadState.Loaded` before calling `NativeAudio.Load`, or remove Load In Background then Native Audio could load it for you.");
            }
        }

        private static short[] AudioClipToShortArray(AudioClip audioClip)
        {
            if (audioClip.loadState != AudioDataLoadState.Loaded)
            {
                if (!audioClip.LoadAudioData())
                {
                    throw new Exception(string.Format("Loading audio {0} failed!", audioClip.name));
                }
            }

            float[] data = new float[audioClip.samples * audioClip.channels];
            audioClip.GetData(data, 0);

            //Convert to 16-bit PCM
            short[] shortArray = new short[audioClip.samples * audioClip.channels];
            for(int i = 0; i < shortArray.Length; i++)
            {
                shortArray[i] = (short)(data[i] * short.MaxValue);
            }
            return shortArray;
        }

        /// <summary>
        /// (**Advanced**) Loads an audio from `StreamingAssets` folder's desination at runtime. Most of the case you should use the `AudioClip` overload instead.
        /// It only supports .wav PCM 16-bit format, stereo or mono, in any sampling rate since it will be resampled to fit the device.
        /// 
        /// If this is the first time loading any audio it will call `NativeAudio.Initialize()` automatically which might take a bit more time.
        /// 
        /// [iOS] Loads an audio into OpenAL's output audio buffer. (Max 256) This buffer will be paired to one of 16 OpenAL source when you play it.
        /// 
        /// [Android] Loads an audio into a `short*` array at unmanaged native side. This array will be pushed into one of available `SLAndroidSimpleBufferQueue` when you play it.
        /// The resampling of audio will occur at this moment to match your player's device native rate. The SLES audio player must be created to match the device rate
        /// to enable the special "fast path" audio. What's left is to make our audio compatible with that fast path player, which the resampler will take care of.
        /// 
        /// You can change the sampling quality of SRC (libsamplerate) library per audio basis with the `LoadOptions` overload.
        /// 
        /// If the audio is not found in the main app's persistent space (the destination of `StreamingAssets`) it will continue to search for the audio 
        /// in all OBB packages you might have. (Often if your game is a split OBB, things in `StreamingAssets` will go there by default even if the main one is not that large.)
        /// </summary>
        /// <param name="streamingAssetsRelativePath">If the file is `SteamingAssets/Hit.wav` use "Hit.wav" (WITH the extension).</param>
        /// <returns> An object that stores a number. Native side can pair this number with an actual loaded audio data when you want to play it. You can `Play`, `Prepare`, or `Unload` with this object. `Load` returns null on error, for example : wrong name, not existing in StreamingAssets, calling in Editor </returns>
        public static NativeAudioPointer Load(string streamingAssetsRelativePath)
        {
            return Load(streamingAssetsRelativePath, LoadOptions.defaultOptions);
        }

        /// <summary>
        /// (**Advanced**) Loads an audio from `StreamingAssets` folder's desination at runtime. Most of the case you should use the `AudioClip` overload instead.
        /// It only supports .wav PCM 16-bit format, stereo or mono, in any sampling rate since it will be resampled to fit the device.
        /// 
        /// If this is the first time loading any audio it will call `NativeAudio.Initialize()` automatically which might take a bit more time.
        /// 
        /// [iOS] Loads an audio into OpenAL's output audio buffer. (Max 256) This buffer will be paired to one of 16 OpenAL source when you play it.
        /// 
        /// [Android] Loads an audio into a `short*` array at unmanaged native side. This array will be pushed into one of available `SLAndroidSimpleBufferQueue` when you play it.
        /// The resampling of audio will occur at this moment to match your player's device native rate. The SLES audio player must be created to match the device rate
        /// to enable the special "fast path" audio. What's left is to make our audio compatible with that fast path player, which the resampler will take care of.
        /// 
        /// You can change the sampling quality of SRC (libsamplerate) library per audio basis with the `LoadOptions` overload.
        /// 
        /// If the audio is not found in the main app's persistent space (the destination of `StreamingAssets`) it will continue to search for the audio 
        /// in all OBB packages you might have. (Often if your game is a split OBB, things in `StreamingAssets` will go there by default even if the main one is not that large.)
        /// </summary>
        /// <param name="streamingAssetsRelativePath">If the file is `SteamingAssets/Hit.wav` use "Hit.wav" (WITH the extension).</param>
        /// <returns> An object that stores a number. Native side can pair this number with an actual loaded audio data when you want to play it. You can `Play`, `Prepare`, or `Unload` with this object. `Load` returns null on error, for example : wrong name, not existing in StreamingAssets, calling in Editor </returns>
        public static NativeAudioPointer Load(string audioPath, LoadOptions loadOptions)
        {
            if (!initialized)
            {
                NativeAudio.Initialize();
            }

            if (System.IO.Path.GetExtension(audioPath).ToLower() == ".ogg")
            {
                throw new Exception("Loading via StreamingAssets does not support OGG. Please use the AudioClip overload and set the import settings to Vorbis.");
            }

#if UNITY_IOS
            int startingIndex = _LoadAudio(audioPath, loadOptions.resamplingQuality);
            if (startingIndex == -1)
            {
                throw new Exception("Error loading audio at path : " + audioPath); 
            }
            else
            {
                float length = _LengthBySource(startingIndex);
                return new NativeAudioPointer(audioPath, startingIndex, length);
            }
#elif UNITY_ANDROID
            int startingIndex = AndroidNativeAudio.CallStatic<int>(AndroidLoadAudio, audioPath, loadOptions.resamplingQuality);

            if(startingIndex == -1)
            {
                throw new Exception("Error loading audio at path : " + audioPath); 
            }
            else
            {
                float length = lengthBySource(startingIndex);
                return new NativeAudioPointer(audioPath, startingIndex, length);
            }
#else
            //Load is defined on editor so that autocomplete shows up, but it is a stub. If you mistakenly use the pointer in editor instead of forwarding to normal sound playing method you will get a null reference error.
            return null;
#endif
        }


        /// <summary>
        /// Native Audio will load a small silent wav and perform various stress test for about 1 second. Your player won't be able to hear anything, but recommended to do it
        /// when there's no other workload running because it will also measure FPS.
        /// 
        /// The test will be asynchronous because it has to wait for frame to play the next audio. Yield wait for the result with the returned `NativeAudioAnalyzer`.
        /// This is a component of a new game object created to run a test coroutine on your scene.
		/// 
		/// If your game is in a yieldable routine, use `yield return new WaitUntil( () => analyzer.Analyzed );' it will wait a frame until that is `true`.
		/// If not, you can do a blocking wait with a `while` loop on `analyzer.Analyzed == false`.
        /// 
        /// You must have initialized Native Audio before doing the analysis or else Native Audio will initialize with default options.
        /// (Remember you cannot initialize twice to fix initialization options)
        /// 
        /// By the analysis result you can see if the frame rate drop while using Native Audio or not. I have fixed most of the frame rate drop problem I found.
        /// But if there are more obscure devices that drop frame rate, this method can check it at runtime and by the returned result you can stop using Native Audio
        /// and return to Unity `AudioSource`.
        /// </summary>
        public static NativeAudioAnalyzer SilentAnalyze()
        {
#if UNITY_ANDROID
            var go = new GameObject("NativeAudioAnalyzer");
            NativeAudioAnalyzer sa = go.AddComponent<NativeAudioAnalyzer>();
            sa.Analyze();
            return sa;
#else
            return null;
#endif
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DeviceAudioInformation
        {
            public DeviceAudioInformation(int samplingRate, int bufferSize, int lowLatencyFeature, int proAudioFeature)
            {
                this.nativeSamplingRate = samplingRate;
                this.optimalBufferSize = bufferSize;
                this.lowLatencyFeature = lowLatencyFeature == 1;
                this.proAudioFeature = proAudioFeature == 1;
            }

            /// <summary>
            /// Only audio matching this sampling rate on a native AudioTrack created with this sampling rate is eligible for fast track playing.
            /// </summary>
            public int nativeSamplingRate;

            /// <summary>
            /// How large of a buffer that your phone wants to work with.
            /// </summary>
            public int optimalBufferSize;
            
            /// <summary>
            /// Indicates a continuous output latency of 45 ms or less.
            /// </summary>
            public bool lowLatencyFeature;

            /// <summary>
            /// Indicates a continuous round-trip latency of 20 ms or less.
            /// </summary>
            public bool proAudioFeature;

            public override string ToString()
            {
                return string.Format("Native Sampling Rate: {0} | Optimal Buffer Size: {1} | Low Latency Feature: {2} | Pro Audio Feature: {3}",
                nativeSamplingRate, optimalBufferSize, lowLatencyFeature, proAudioFeature);
            }
        }

        /// <summary>
        /// [Android] Ask the phone about its audio capability.
        /// 
        /// [iOS] Does not work!
        /// </summary>
        public static DeviceAudioInformation GetDeviceAudioInformation()
        {
#if UNITY_ANDROID
            var jObject = AndroidNativeAudio.CallStatic<AndroidJavaObject>(AndroidGetDeviceAudioInformation);
            return new DeviceAudioInformation
            (
                samplingRate: jObject.Get<int>("nativeSamplingRate"),
                bufferSize: jObject.Get<int>("optimalBufferSize"),
                lowLatencyFeature: jObject.Get<int>("lowLatencyFeature"),
                proAudioFeature: jObject.Get<int>("proAudioFeature")
            );
#else
            return default(DeviceAudioInformation);
#endif
        }
    }
}