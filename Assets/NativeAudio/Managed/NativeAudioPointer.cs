// Native Audio
// 5argon - Exceed7 Experiments
// Problems/suggestions : 5argon@exceed7.com

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace E7.Native
{
	/// <summary>
	/// An ID that will be used at native side to get pointer to loaded audio memory area.
	/// Please do not create an instance of this class by yourself!
	/// Call NativeAudio.Load to get it.
	/// </summary>
    public class NativeAudioPointer 
    {
		private string soundPath;
		private int startingIndex;

		/// <summary>
		/// Some implementation in the future may need you to specify concurrent amount for each audio upfront so I prepared this field.
		/// But it is always 1 for now. It has no effect because both iOS and Android implementation automatically rotate players on play.
		/// and so you get the concurrent amount equal to amount of players shared for all sounds, not just this one sound.
		/// </summary>
		private int amount;
		private bool isUnloaded;

		/// <summary>
		/// Cached length in SECONDS of a loaded audio calculated from PCM byte size and specifications.
		/// </summary>
		public float Length { get; private set; }

		//[iOS] When preparing to play, OpenAL need you to remember some information so that the next Play will use the prepared things correctly.
#if UNITY_IOS
		private int prepareIndex;
		private bool prepared;
#endif
		private int currentIndex;

        /// <summary>
        /// This will automatically cycles for you if the amount is not 1.
        /// </summary>
        public int NextIndex
		{
            get
            {
				int toReturn = currentIndex;
				currentIndex = currentIndex + 1;
                if (currentIndex > startingIndex + amount - 1)
				{
					currentIndex = startingIndex;
				}
				return toReturn;
			}
		}

        /// <param name="amount">Right now amount is not used anywhere yet.</param>
        public NativeAudioPointer(string soundPath, int index, float length, int amount = 1)
		{
            this.soundPath = soundPath;
			this.startingIndex = index;
			this.amount = amount;
			this.Length = length;

			this.currentIndex = index;
		}

        /// <summary>
        /// Plays an audio using the underlying OS's native method. A number stored in this object will be used to determine which loaded audio data at native side that we would like to play. If you previously call `Prepare()` it will take effect here.
		/// You can adjust volume and others immediately via `PlayOptions.PlayAdjustment` or adjust later via the returned `NativeAudioController`.

		/// [iOS] (Unprepared) Native Audio remembered which of the total of 16 OpenAL source that we just played. It will get the next one (wraps to the first instance when already at 16th instance), find the correct sound buffer and assign it to that source, then play it.
		/// [iOS] (Prepared) Instead of using buffer index (the sound, not the sound player) it will play the source at the time you call `Prepare` immediately without caring if the sound in that source is currently the same as when you call `Prepare` or not. After calling this `Play`, the prepare status will be reset to unprepared, and the next `Play` will use buffer index as usual.

		/// [Android] Use the index stored in this NativeAudioPointer as an index to unmanaged audio data array loaded at OpenSL ES part. (C part) The code remembered which OpenSL ES AudioPlayer was used the last time and this will get you the next one. Loop back to the first player when already at the final player.

        /// </summary>
		/// <returns>With the controller you can further set the volume, panning, stop, etc. to the audio player that was just chosen to play your audio.</returns>
        public NativeAudioController Play()
		{
			return Play(NativeAudio.PlayOptions.defaultOptions);
		}

        public NativeAudioController Play(NativeAudio.PlayOptions playOptions)
        {
			if(isUnloaded)
			{
				throw new System.Exception("You cannot play an unloaded NativeAudio.");
			}

            int playedSourceIndex = -1;
#if UNITY_IOS
            if (prepared)
            {
				//This is using source index. It means we have already loaded our sound to that source with Prepare.
                NativeAudio._PlayAudioWithSourceCycle(this.prepareIndex, playOptions);
				playedSourceIndex = this.prepareIndex;
            }
            else
            {
				//-1 audioPlayerIndex results in round-robin, 0~15 results in hard-specifying the track.
				playedSourceIndex = NativeAudio._PlayAudio(this.NextIndex, playOptions.audioPlayerIndex, playOptions);
            }
            prepared = false;
#elif UNITY_ANDROID
            playedSourceIndex = NativeAudio.playAudio(this.NextIndex, playOptions);
#endif
            return new NativeAudioController(playedSourceIndex);
        }


        /// <summary>
        /// Shave off as much start up time as possible to play a sound. The majority of load time is already in `Load` but `Prepare` might help a bit more, or not at all. You can also call `Play()` without calling this first. The effectiveness depends on platform's audio library's approach :

        /// [iOS] Assigns OpenAL audio buffer to a source. `NativeAudioPointer` then remembers this source index. The next `Play()` you call will immediately play this remembered source without caring what sound is in it instead of using a buffer index to get sound to pair with the next available source. This means if in between `Prepare()` and `Play()` you have played 16 sounds, the resulting sound will be something else as other sound has already put their buffer into the source you have remembered.

        /// [Android] No effect as OpenSL ES play audio by pushing data into `SLAndroidSimpleBufferQueueItf`. All the prepare is already at the `Load()`.

        /// </summary>
        public void Prepare()
        {
#if UNITY_IOS
            prepareIndex = NativeAudio._PrepareAudio(NextIndex);
			prepared = true;
#elif UNITY_ANDROID
			//There is no possible preparation for OpenSL ES at the moment..
#endif
        }

		public override string ToString()
		{
			return soundPath;
		}

        /// <summary>
        /// You cannot call `Play` anymore after unloading. It will throw an exception if you do so.
		/// 
		/// [iOS] Unload OpenAL buffer. The total number of 16 OpenAL source does not change. Immediately stop the sound if it is playing.
		/// 
		/// [Android] `free` the unmanaged audio data array at C code part of OpenSL ES code.
		/// 
		/// It is HIGHLY recommended to stop those audio player via NativeAudioController before unloading because the play head will continue 
		/// running into oblivion if you unload data while it is still reading. I have seen 2 cases : 
		/// 
		/// - The game immediately crash with signal 11 (SIGSEGV), code 1 (SEGV_MAPERR) on my 36$ low end phone. Probably it does not permit freed memory reading.
		/// - In some device it produce scary noisehead sound if you load something new, `malloc` decided to use the same memory area you just freed,
		/// and the still running playhead pick that up.
        /// </summary>
        public void Unload()
        {
#if UNITY_IOS
			if(!isUnloaded)
			{
				NativeAudio._UnloadAudio(startingIndex);
				isUnloaded = true;
			}
#elif UNITY_ANDROID
			if(!isUnloaded)
			{
				for(int i = startingIndex; i < startingIndex + amount; i++)
				{
					NativeAudio.unloadAudio(i);
				}
				isUnloaded = true;
			}
#endif
        }

    }
}