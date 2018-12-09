// Native Audio
// 5argon - Exceed7 Experiments
// Problems/suggestions : 5argon@exceed7.com


namespace E7.Native
{

    public partial class NativeAudio
    {
        public class InitializationOptions
        {
            public static readonly InitializationOptions defaultOptions = new InitializationOptions();
            /// <summary>
            /// The no argument overload of `Play` round robin select audio player (AudioTrack) for you. 
            /// So this translate to the most concurrent audio amount you can have.
            /// 
            /// If you would like to learn about this AudioTrack limit problem please read !
            /// https://gametorrahod.com/androids-native-audio-primer-for-unity-developers-65acf66dd124
            /// 
            /// The gist :
            /// - You might get less than this number.
            /// - Even if you get this number of AudioTrack, not all of them might be granted fast path since the limit seems to be 7 fast AT per device.
            /// (You phone might took more than one at home screen, Unity take 1 by default, there is an other Unity game open, etc.)
            /// - There is an advanced Play overload that instead of round robin select the player, you directly specify which one you want.
            /// That number of course cannot be more than this number - 1.
            /// - You might not need that much concurrency and letting new sound cut of the old one is actually great for some rapid sounds. Even 1 AT might sound nicer than more.
            /// /// </summary>
            public int androidAudioTrackCount = 3;

            /// <summary>
            /// If -1 it uses buffer size in the buffer queue equal to device's native buffer size.
            /// Smaller buffer size means better latency, and buffer size multiple of device's native buffer size enables FAST TRACK AudioTrack.
            /// Therefore -1 means it is the best latency-wise. (Do not modify the buffer size asked from the device)
            /// 
            /// But if you experiences audio glitches, it might be that the device could not write in time 
            /// when the first buffer runs out of data. (Native Audio uses double buffering)
            /// So you can put any positive number here and the device's native buffer size will keep increasing by a factor of itself until over this number.
            /// But you will experience longer latency as a tradeoff.
            /// 
            /// Example : 256
            /// - Xperia Z5 : Native buffer size : 192 -> 384
            /// - Lenovo A..something : Native buffer size : 620 -> 620
            /// </summary>
            public int androidMinimumBufferSize = -1;
        }

    }
}