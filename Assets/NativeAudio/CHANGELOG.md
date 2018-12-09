# Native Audio Changelog
Sirawat Pitaksarit / Exceed7 Experiments (Contact : 5argon@exceed7.com)

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

I am keeping the **Unreleased** section at the bottom of this document. You can look at it to learn about what's sorely missing and would be coming next.

# [4.0.0] - 2018-12-24

## Added

### [All Platforms] New load API : `NativeAudio.Load(AudioClip)`

You are now freed from `StreamingAssets` folder, because you can give data to NativeAudio via Unity-loaded `AudioClip`.

Here's how loading this way works, it is quite costly but convenient nonetheless :

- It uses `audioClip.GetData` to get a float array of PCM data.
- That float array is converted to a byte array which represent 16-bit per sample PCM audio.
- The byte array is sent to native side. NativeAudio **copy** those bytes and keep at native side. You are then safe to release the bytes at Unity side without affecting native data.

This is now the recommeded way of loading audio, it allows a platform like PC which Native Audio does not support to use the same imported audio file as Android and iOS. Also for the tech-savvy you can use the newest Addressables Asset System to load audio from anywhere (local or remote) and use it with Native Audio once you get a hold of that as an `AudioClip`.

Hard requirements : 

- Load type MUST be Decompress On Load so Native Audio could read raw PCM byte array from your compressed audio.
- If you use Load In Background, you must call `audioClip.LoadAudioData()` beforehand and ensure that `audioClip.loadState` is `AudioDataLoadState.Loaded` before calling `NativeAudio.Load`. Otherwise it would throw an exception. If you are not using Load In Background but also not using Preload Audio Data, Native Audio can load for you if not yet loaded.
- Must not be ambisonic.

In the Unity's importer, it works with all compression formats, force to mono, overriding to any sample rate, and quality slider.

The old `NativeAudio.Load(string audioPath)` is now documented as an advanced use method. You should not require it anymore in most cases.

### [All Platforms] OGG support added via `NativeAudio.Load(AudioClip)`

From the previous point, being able to send data from Unity meaning that we can now use OGG. I don't even have to write my own native OGG decoder!

The load type must be **Decompress on Load** to enable decompressed raw PCM data to be read before sending to Native Audio. This means on the moment you load, it will consume full PCM data in Unity on the read **and** also full PCM data again in native side, resulting in double uncompressed memory cost. You can call `audioClip.UnloadAudioData` afterwards to free up memory of managed side leaving just the uncompressed native memory.

OGG support is not implemented for the old `NativeAudio.Load(string audioPath)`. An error has been added to throw when you use a string path with ".ogg" to prevent misuse.

### [iOS] Resampler added, but not enabled yet

I have added `libsamplerate` integration to the native side but not activate it yet.

Now you can load an audio of any sampling rate. Currently I don't have an information what is the best sampling rate (latency-wise) for each iOS device, now I left the audio alone at imported rate.

Combined with the previous points, you are free to use any sampling rate override import settings specified in Unity.

### [All Platforms] Mono support added

- When you loads a 1 channel audio, it will be duplicated into 2 channels (stereo) in the memory. Mono saves space only on the device and not in-memory.
- Combined with the previous points, you are free to use the `Force To Mono` Unity importer checkbox. 

### [Android] NativeAudio.GetDeviceAudioInformation()

It returns audio feature information of an Android phone. [Superpowered is hosting a nice database of these information of various phones.](https://superpowered.com/latency).

Native Audio is already instantiating a good Audio Track based on these information, but you could use it in other way such as enforing your Unity DSP buffer size to be in line with the phone, etc. There is a case that Unity's "Best Latency" results in a buffer size guess that is too low it made Unity-played audio slow down and glitches out.

## Changed

### `LoadOptions.androidResamplingQuality` renamed to `LoadOptions.resamplingQuality`

Because now iOS can also resample your audio.

## Removed

### [EXPERIMENTAL] Native Audio Generator removed

It just here for 1 version but now that the recommended way is to load via Unity's importer this is not worth it to maintain anymore. (That's why I marked it as experimental!)

# [3.0.0] - 2018-11-01

## Added

### [All Platforms] Track's playhead manipulation methods added

- `NativeAudio.Play(playOptions)` : Able to specify play offset in seconds in the `PlayOptions` argument.
- `NativeAudioController` : Added track-based pause, resume, get playback time, and set playback time even while the track is playing. Ways to pause and resume include using this track-based pause/resume, or use get playback time and store it for a new `Play(playOptions)` later and at the same time `Stop()` it immediately, if you fear that the track's audio content might be overwritten before you can resume.
- `NativeAudioPointer` : Added `Length` property. It contains a cached audio's length in seconds calculated after loading.

### [All Platforms] Track Looping

A new `PlayOptions` applies a looping state on the TRACK. It means that if some newer sound decided to use that track to play, that looping sound is immediately stopped.

To protect the looping sound, you likely have to plan your track number usage manually with `PlayOptions.audioPlayerIndex`.

- If you pause a looping track, it will resume in a looping state.
- `nativeAudioController.GetPlaybackTime()` on a looping track will returns a playback time that resets every loop, not an accumulated playback time over multiple loops.

### [iOS] Specifying a track index

Previously only Android can do it. Now you can specify index 0 ~ 15 on iOS to use precisely which track for your audio. It is especially important for the new looping function.

### [EXPERIMENTAL] Native Audio Generator

When you have tons of sound in `StreamingAssets` folder it is getting difficult to manage string paths to load them.

The "Native Audio Generator" will use a script generation to create a static access point like this : `NativeAudioLibrary.Action.Attack`, this is of a new type `NativeAudioObject` which manages the usual `NativeAudioPointer` inside. You can call `.Play()` on it directly among other things. You even get a neat per-sound mixer in your `Resources` folder which will be applied to the `.Play()` via `NativeAudioObject` automatically.

Use `Assets > Native Audio > Generate or update NativeAudioLibrary` menu, then you can point the pop-up dialog to any folder inside your `StreamingAssets` folder. It must contain one more layer of folder as a group name before finally arriving at the audio files. Try this on the `StreamingAssets` folder example that comes with the package.

This is still not documented anywhere in the website yet, but I think it is quite ready for use now. EXPERIMENTAL means it might be removed in the future if I found it is not good enough.

## Removed

### `PlayAdjustment` inside the `PlayOptions` is no more.

Having 2 layers of configuration is not a good API design, but initially I did that because we need a struct for interop and we need a class for its default value ability.

I decided to make it 1 layer. The entire `PlayOptions` is now used to interop with the native side.

Everything is moved to the `PlayOptions`, and also `PlayOptions` is now a struct. Previously the `PlayAdjustment` inside is the struct. Not a class anymore, now to get the default `PlayOptions` you have to use `PlayOptions.defaultOptions` then you can modify things from there. If you use `new PlayOptions()` the default value of the struct is not a good one. (For example volume's default is supposed to be 1, not int-default 0)

# [2.1.0] - 2018-09-12

## Added

### [IOS] 2D Panning

The backend `OpenAL` of iOS is a 3D positional audio engine. 2D panning is emulated by deinterleaving a stereo source audio into 2 mono sources, then adjust the distance from the listener so that it sounds like 2D panning.

### [ALL PLATFORMS] Play Adjustment

There is a new member `playAdjustment` in `PlayOptions` that you can use on each `nativeAudioPointer.Play()`. You can adjust volume and pan right away BEFORE play. This is because I discorvered on iOS it is too late to adjust volume immediately after play with `NativeAudioController` without hearing the full volume briefly.

## Fixed

- Previously the Android panning that is supposed to work had no effect. Now it works alongside with the new iOS 2D panning.

# [2.0.0] - 2018-09-08

## Added

### [Android] Big migration from Java-based AudioTrack to OpenSL ES

Unlike Java AudioTrack, (which built on top of OpenSL ES with similar latency from my test) OpenSL ES is one of the officially mentioned "high performance audio" way of playing audio right here. It will be awesome. And being in C language part Unity can invoke method via extern as opposed to via AndroidJavaClass like what we have previously. (speed!) Only audio loading has to go through Java because the code have to look for StramingAssets folder or OBB packages, but loading is only a one-time thing so that's fine.

I am proud to present that unlike v1.0, Native Audio v2.0 follows everything the official Android high-performance audio guidelines specified. For details of everything I did for this new "back end" of Native Audio, please [read this research](https://gametorrahod.com/androids-native-audio-primer-for-unity-developers-65acf66dd124). 

### [Android] Resampler

Additionally I will go as far as resampling the audio file on the fly (we don't know which device the player will use, but we can only prepare 1 sampling rate of audio practically) to match each device differing native sampling rate (today it is mainly either 44100Hz or 48000Hz) so that the special "fast path" audio is enabled. The previous version does not enable fast path if device is not 44100Hz native because we hard-fixed everything to 44100Hz. This will be awesome for any music games out there. (But it adds some load time if a resampling is required, it is the price to pay)

About resampling quality do not worry, as instead of writing my own which would be super slow and sounds bad I will incorporate the impressive libsamplerate (Secret Rabbit Code) http://www.mega-nerd.com/SRC/ and it has a very permissive BSD license that just require you to put some attributions, not to open source your game or anything. You are required to do your part in the open source software intiative.

### [Android] Double buffering

The previous version not only it use Java it also push one big chunk of audio in the buffer. In this version, with double buffering technique we put just a small bit of audio and we are ready to play way faster. While this one is playing the next bit will be prepared on the other buffer. It switch back and forth like this until we used up all the sameples. The size of this "audio bit" is set to be as small as possible.

### [Android] Native buffer size aligned zero-padded audio

Even more I will intentionally zero pad the audio buffer so that it is a multiple of "native buffer size" of each device further reducing jitter when pushing data to the output stream. High-end device has smaller native buffer size and require less zero-pad. Combined with double buffering mentioned earlier the play will ends exactly without any remainders buffer.

### [Android] Keep alive the audio track

Unlike naive implementation of playing a native audio on Android, Native Audio use a hack which keep the track constantly **playing silence** even if nothing is playing.

This is to counter the costly audio policy that triggers on a transition between play and stopped state on some badly programmed Android phone. It makes rapidly playing audio lags some phones badly.

Big thanks to PuzzledBoy for helping me investigating into this problem.

## Changed

### Volume/pan/etc. adjustments on sound moved to `NativeAudioController`, a new class type.

It is returned from the new `Play()`, previously returns nothing. Please use it in the line immediately after `Play()` to adjust volume instead of as an argument on `Play()`.

### Requiring an open source attribution (BSD) to `libsamplerate`

Native Audio is now powered by `libsamplerate`, required for the minimum latency without noticable compromise on audio quality!
Please visit the [how to use](http://exceed7.com/native-audio/how-to-use.html) page for details. 

### Initialize, Load, Play now has an option argument.

It provides various customization which you can read in the website or in code comment/Intellisense.

### Completely new Android underlying program : OpenSL ES and the AudioTrack customization.

Now it is crucial to know that Android requests 3 AudioTracks by default and you can change this with the initialization options.
Increasing this number increases concurrency but with consequence. Please visit [the homepage](http://exceed7.com/native_audio) and read carefully.

### Audio format requirement relaxed only on Android

In Android thanks to `libsamplerate` you can use any rate now, but it is not on iOS yet. For now, stick with **16-bit PCM stereo 44100Hz .wav** file.

# [1.0.0] - 2018-04-13

The first release.

# Unreleased

### Mono support

I admit I was lazy and hard coded that the .wav file has to be in stereo. With this you can potentially halves the space used.

### OGG support

Did you know if you are to make a piano game with 4 seconds long for each keys for 88 keys, those would take 60MB from your game following the current format limitations??

OGG should greatly reduce the file size while cost you some decoding time. The decoding will be like "Decompress on load" in Unity which it turns in to PCM at runtime. (not "compressed in memory")

### [iOS] Resampler

Similar to the current Android side, we will resampling all audio to match the device's preferred sampling rate so we can reduce the hardware's work as much as possible. It will also use the libsamplerate.

Currently we still require 44100Hz audio because even at Android side it is able to resample to any rate in iOS we still have a fixed sample rate player. When this feature arrives, we are finally able to use any sampling rate of the .wav file. Then from that point onwards it is recommended to go for 48000Hz since phones in the future is likely to use this rate, and with that those players will not have to spend time resampling + get the true quality audio file.

### [Android] AAudio support via Oboe

Big news! (https://www.youtube.com/watch?v=csfHAbr5ilI) 

It seems that "Oboe" is now out of developer preview. This is Google-made C++ audio library for Android that seems to do exactly what Native Audio wants to do.

Including the AAudio and OpenSL ES fallback thing I intended to do. Anyways, it needs to pass my latency comparison with the current way first and I will let you know the result when that's done. If it is equal or lower I will switch to Oboe implementation. Then we all will get automatic **AAudio** support. Wow!

### Nintendo Switch support

Depending on how successful I as a game developer can be after finishing the current game, the next game I want to make a Nintendo Switch exclusive game. And I will definitely take Native Audio with me. Let's see what Switch API offers and how much latency Unity adds to it.

But this is not a guarantee because my current game is the last try, if I can't make a living with it I will go to day job and likely cannot continue making that game on Switch. And I will likely not try to support other platforms that cannot be field-tested by my own game.
