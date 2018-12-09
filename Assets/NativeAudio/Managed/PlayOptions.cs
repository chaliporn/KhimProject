// Native Audio
// 5argon - Exceed7 Experiments
// Problems/suggestions : 5argon@exceed7.com

using System.Runtime.InteropServices;

namespace E7.Native
{

    public partial class NativeAudio
    {
        /// <summary>
        /// You can modify volume, pan, etc. on BEFORE play with this struct. On some platforms like iOS, adjusting them after the play with `NativeAudioController` is already too late because you will already hear the audio. (Even in consecutive lines of code)
        /// It has to be a struct since this will be sent to the native side, interop to a matching code in other language. If you want the correct default value before modifying something further, start from `PlayOptions.defaultOptions`
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct PlayOptions
        {
            public static readonly PlayOptions defaultOptions = new PlayOptions
            {
                audioPlayerIndex = -1,
                volume = 1,
                pan = 0,
                offsetSeconds = 0,
                trackLoop = false,
            };

            /// <summary>
			/// - If -1 (`PlayOptions.defaultOptions`) The track/player at native side will be round robin selected for you.
            /// - If any 0+ number, you specify which track you like to use for this play. (This means it will silence the previous sound playing at this track for the new ones)
			/// This is zero indexed. If you specify you want 3 AudioTracks at `Initialization()` you can use 0, 1, and 2.
			/// - If the number is over how many track the hardware actually gave you, it is converted to be like -1 automatically.
			/// 
            /// Positive numbers is an advanced usage. Some ideas :
			/// 
			/// - Force the automatic cutoff even when you have multiple tracks. For example, machine gun sound use exclusively track 0 so the sound is not too crowded.
            /// - Protect the looping sound that is playing at a certain track.
            /// - [Android] You know from https://gametorrahod.com/androids-native-audio-primer-for-unity-developers-65acf66dd124 that we might get less than 
			/// the number of AudioTrack specify when `Instantiate()` You also know that some of the track requested might be fast. Native Audio request AudioTrack
			/// sequentially therefore the using lower track number has more **chance** to be fast. The strategy might be force lower number for important sounds.
			/// - You can use -1 to auto-select AudioTrack first, then force the next sound use that same track by asking `NativeAudioController.InstanceIndex`
			/// and use that as this.
            /// </summary>
            public int audioPlayerIndex;

            /// <summary>
            /// [iOS] Maps to `AL_GAIN`. It is a scalar amplitude multiplier, so the value can go over 1.0 for increasing volume but can be clipped. 
            /// If you put 0.5f, it is attenuated by 6 dB.
            /// 
            /// [Android] Maps to `SLVolumeItf` interface -> `SetVolumeLevel`.
            /// The floating volume parameter will be converted to millibel (20xlog10x100) so that putting 0.5f here results in 6dB attenuation.
            /// </summary>
            public float volume;

            /// <summary>
            /// -1 for full left, 0 for center, 1 for full right. This pan is based on "balance effect" and not a "constant energy pan". That is
            /// at the center you hear each side fully. (Constant energy pan has 3dB attenuation to both on center.)
            /// 
            /// [iOS] 2D panning in iOS will be emulated in OpenAL's 3D audio engine by splitting your stereo sound into a separated mono sounds, 
            /// then position each one on left and right ear of the listener. When panning, instead of adjusting gain we will just move the source 
            /// further from the listener and the distance attenuation will do the work. (Gain is reserved to the setting volume command, 
            /// so we have 2 stage of gain adjustment this way.
            /// 
            /// [Android] Maps to `SLVolumeItf` interface -> `SetStereoPosition`
            /// </summary>
            public float pan;

            /// <summary>
            /// Start playing from other point in the audio. Offset from the beginning in SECONDS unit. Will do nothing if the offset is over the length of audio.
            /// </summary>
            public float offsetSeconds;

            /// <summary>
            /// Apply a looping state on the TRACK. It means that if some newer sound decided to use that track to play, that looping sound is immediately stopped.
            /// To protect the looping sound, you likely have to plan your track number usage manually with`PlayOption.audioPlayerIndex`.
            /// </summary>
            public bool trackLoop;
        }

    }
}