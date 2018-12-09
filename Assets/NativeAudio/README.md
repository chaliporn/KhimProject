Native Audio
Sirawat Pitaksarit / Exceed7 Experiments
Contact : 5argon@exceed7.com
----

# How to use

All of instructions to use this is in `HowToUse > HowToUse.zip`. Please unzip it somewhere not in your project or Unity will import it as one of your game's asset.

It is an offline version of this website : http://exceed7.com/native-audio

# Demo scene

Also there is a demo scene in `Demo` folder. All of the button does not work in editor, you have to test them in the real iOS/Android device.

# Verbose logging at native side

They are left in the code for my own development use, but should you encounter any strange problems and could not afford to wait for me to look at it, enabling verbose logging might reveal something useful to you.

Verbose logging at native side can be enabled from [iOS] NativeAudio.mm `#define LOG_NATIVE_AUDIO` and [Android] NativeAudio.java `private final static boolean enableLogging = true;` + uncomment logging preprocessor at the top part of `.c` file. On Android you will have to use the project in `Extras > AndroidStudioProject`, edit it, and recompile the `.aar` file.

After enabling, [iOS] you can see it from Xcode console [Android] you can see it with `adb logcat`.