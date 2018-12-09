// Native Audio
// 5argon - Exceed7 Experiments
// Problems/suggestions : 5argon@exceed7.com

// Special thanks to Con for written this wonderful OpenAL tutorial : http://ohno789.blogspot.com/2013/08/playing-audio-samples-using-openal-on.html

#import "NativeAudio.h"

//#define LOG_NATIVE_AUDIO

@implementation NativeAudio

static ALCdevice *openALDevice;
static ALCcontext *openALContext;

//OpenAL sources starts at number 2400
#define kMaxConcurrentSources 32
#define kHalfMaxConcurrentSources 16

//OpenAL buffer index starts at number 2432. (Now you know then source limit is implicitly 32)
//This number will be remembered in NativeAudioPointer at managed side.
//As far as I know there is no limit. I can allocate way over 500 sounds and it does not seems to cause any bad things.
//But of course every sound will cost memory and that should be your real limit.
//This is limit for just in case someday we discover a real hard limit, then Native Audio could warn us.
#define kMaxBuffers 1024

#define fixedMixingRate 48000

//Error when this goes to max
static int bufferAllocationCount = 0;
//Never reset
static int runningBufferAllocationNumber = 0;

static NativeAudioSourceIdPair* nasips;
static NativeAudioBufferIdPair* nabips;

//Currently "amount" is disregarded on iOS. It is always 32 but shared for all sounds played with Native Audio.

//OpenAL can play at most 32 concurrent sounds while having separated audio buffer loaded. (at most kMaxBuffers buffers)
//Unlike Android's AudioTrack or iOS's AVAudioPlayer where each buffer has its own sound channel.

//We will cycle through these 32 channels when we play any sounds.

+ (AudioFileID) openAudioFile:(NSString *)audioFilePathAsString
{
    NSURL *audioFileURL = [NSURL fileURLWithPath:audioFilePathAsString];
    
    AudioFileID afid;
    OSStatus openAudioFileResult = AudioFileOpenURL((__bridge CFURLRef)audioFileURL, kAudioFileReadPermission, 0, &afid);
    if (0 != openAudioFileResult)
    {
        NSLog(@"An error occurred when attempting to open the audio file %@: %d", audioFilePathAsString, (int)openAudioFileResult);
    }
    
    return afid;
}

+ (UInt32) getSizeOfAudioComponent:(AudioFileID)afid
{
    UInt64 audioDataSize = 0;
    UInt32 propertySize = sizeof(UInt64);
    
    OSStatus getSizeResult = AudioFileGetProperty(afid, kAudioFilePropertyAudioDataByteCount, &propertySize, &audioDataSize);
    
    if (0 != getSizeResult)
    {
        NSLog(@"An error occurred when attempting to determine the size of audio file.");
    }
    
    return (UInt32)audioDataSize;
}

+ (AudioStreamBasicDescription) getDescription:(AudioFileID)afid
{
    AudioStreamBasicDescription desc;
    UInt32 propertySize = sizeof(desc);
    
    OSStatus getSizeResult = AudioFileGetProperty(afid, kAudioFilePropertyDataFormat, &propertySize, &desc);
    
    if (0 != getSizeResult)
    {
        NSLog(@"An error occurred when attempting to determine the property of audio file.");
    }
    
    return desc;
}

+ (int) Initialize
{
    openALDevice = alcOpenDevice(NULL);
    
    /*
    ALCint attributes[] =
    {
        ALC_FREQUENCY, fixedMixingRate
    };
     */
    
    openALContext = alcCreateContext(openALDevice, NULL); //,attributes);
    alcMakeContextCurrent(openALContext);
    
    nasips = (NativeAudioSourceIdPair*) malloc(sizeof(NativeAudioSourceIdPair) * kHalfMaxConcurrentSources);
    
    //"nabip" is for that just a single number can maps to 2 number (L and R buffer)
    //The upper limit of buffers is a whopping 1024, this will take 4096 bytes = 0.0041MB
    //I tried the realloc way, but it strangely realloc something related to text display Unity is using and crash the game (bug?)
    //Might be related to that the memory area is in the heap (static)
    nabips = (NativeAudioBufferIdPair*) malloc(sizeof(NativeAudioBufferIdPair*) * kMaxBuffers);
    
    ALuint sourceIDL;
    ALuint sourceIDR;
    for (int i = 0; i < kHalfMaxConcurrentSources; i++) {
        alGenSources(1, &sourceIDL);
        alSourcei(sourceIDL, AL_SOURCE_RELATIVE, AL_TRUE);
        alSourcef(sourceIDL, AL_REFERENCE_DISTANCE, 1.0f);
        alSourcef(sourceIDL, AL_MAX_DISTANCE, 2.0f);
        alGenSources(1, &sourceIDR);
        alSourcei(sourceIDR, AL_SOURCE_RELATIVE, AL_TRUE);
        alSourcef(sourceIDR, AL_REFERENCE_DISTANCE, 1.0f);
        alSourcef(sourceIDR, AL_MAX_DISTANCE, 2.0f);
        
        NativeAudioSourceIdPair nasip;
        nasip.left = sourceIDL;
        nasip.right = sourceIDR;
        nasips[i] = nasip;
        
        //roll off factor is default to 1.0
    }
    
    alDistanceModel(AL_LINEAR_DISTANCE_CLAMPED);
    
#ifdef LOG_NATIVE_AUDIO
    NSLog(@"Initialized OpenAL");
#endif
    
    return 0; //0 = success
}

+ (void) UnloadAudio: (int) index
{
    ALuint bufferIdL = (ALuint)nabips[index].left;
    ALuint bufferIdR = (ALuint)nabips[index].right;
    alDeleteBuffers(1, &bufferIdL);
    alDeleteBuffers(1, &bufferIdR);
    bufferAllocationCount -= 2;
}

+ (int) LoadAudio:(const char*) soundUrl resamplingQuality:(int) resamplingQuality
{
    if (bufferAllocationCount > kMaxBuffers) {
        NSLog(@"Fail to load because OpenAL reaches the maximum sound buffers limit. Raise the limit or use unloading to free up the quota.");
        return -1;
    }
    
    if(openALDevice == nil)
    {
        [NativeAudio Initialize];
    }
    
    NSString *audioFilePath = [NSString stringWithFormat:@"%@/Data/Raw/%@", [[NSBundle mainBundle] resourcePath], [NSString stringWithUTF8String:soundUrl] ];
    
    AudioFileID afid = [NativeAudio openAudioFile:audioFilePath];
    AudioStreamBasicDescription loadingAudioDescription = [NativeAudio getDescription:afid];
    
#ifdef LOG_NATIVE_AUDIO
    NSLog(@"Input description : Flags %u Bits/channel %u FormatID %u SampleRate %f Bytes/Frame %u Bytes/Packet %u Channels/Frame %u Frames/Packet %u",
          (unsigned int)loadingAudioDescription.mFormatFlags,
          (unsigned int)loadingAudioDescription.mBitsPerChannel,
          (unsigned int)loadingAudioDescription.mFormatID,
          loadingAudioDescription.mSampleRate,
          (unsigned int)loadingAudioDescription.mBytesPerFrame,
          (unsigned int)loadingAudioDescription.mBytesPerPacket,
          (unsigned int)loadingAudioDescription.mChannelsPerFrame,
          (unsigned int)loadingAudioDescription.mFramesPerPacket
          );
#endif
    
    //UInt32 bytesPerFrame = loadingAudioDescription.mBytesPerFrame;
    UInt32 channel = loadingAudioDescription.mChannelsPerFrame;
    
    //This is originally float?
    //NSLog(@"LOADED RATE : %f", loadingAudioDescription.mSampleRate);
    //NSLog(@"CHANN : %u", (unsigned int)loadingAudioDescription.mChannelsPerFrame);
    //NSLog(@"BPF : %u", (unsigned int)loadingAudioDescription.mBytesPerFrame);
    UInt32 samplingRate = (UInt32) loadingAudioDescription.mSampleRate;
    
    //Next, load the original audio
    UInt32 audioSize = [NativeAudio getSizeOfAudioComponent:afid];
    char *audioData = (char*)malloc(audioSize);
    
    OSStatus readBytesResult = AudioFileReadBytes(afid, false, 0, &audioSize, audioData);
    
    if (0 != readBytesResult)
    {
        NSLog(@"ERROR : AudioFileReadBytes %@: %d", audioFilePath, (int)readBytesResult);
    }
    AudioFileClose(afid);
    
    int loadedIndex = [NativeAudio SendByteArray:audioData audioSize:audioSize channels:channel samplingRate:samplingRate resamplingQuality:resamplingQuality];
    
    if (audioData)
    {
        free(audioData);
        audioData = NULL;
    }
    
    return loadedIndex;
}

//Can call from Unity to give Unity-loaded AudioClip!!
+ (int) SendByteArray:(char*) audioData audioSize:(int)audioSize channels:(int)channel samplingRate:(int)samplingRate resamplingQuality:(int)resamplingQuality
{
    //double preferredRate = [[AVAudioSession sharedInstance]preferredSampleRate];
    //double bufferDuration = [ [AVAudioSession sharedInstance] IOBufferDuration];
    //double preferredBufferDuration = [ [AVAudioSession sharedInstance] preferredIOBufferDuration];
    
    //I don't know if an "optimal rate" exist on Apple device or not.
    //int rate = fixedMixingRate;
    int rate = samplingRate;
    
    //Anyways, this is always 24000 for all Unity games as far as I tried. (why?)
    //int rate = (int)[[AVAudioSession sharedInstance]sampleRate];
    
    //NSLog(@"DEVICE RATE : %d", rate);
    
    if(samplingRate != rate)
    {
        float ratio = rate / ((float) samplingRate);
        
        //byte -> short
        size_t shortLength = audioSize / 2;

        size_t resampledArrayShortLength = (size_t)floor(shortLength * ratio);
        resampledArrayShortLength += resampledArrayShortLength % 2;
        
        //NSLog(@"Resampling! Ratio %f / Length %zu -> %zu", ratio, shortLength, resampledArrayShortLength);
        
        float *floatArrayForSRCIn = (float*)calloc(shortLength, sizeof(float *));
        float *floatArrayForSRCOut = (float*)calloc(resampledArrayShortLength, sizeof(float *));
        
        //SRC takes float data.
        src_short_to_float_array((short*)audioData, floatArrayForSRCIn, (int)shortLength);
        
        SRC_DATA dataForSRC;
        dataForSRC.data_in = floatArrayForSRCIn;
        dataForSRC.data_out = floatArrayForSRCOut;
        
        dataForSRC.input_frames = shortLength / channel;
        dataForSRC.output_frames = resampledArrayShortLength / channel;
        dataForSRC.src_ratio = ratio; //This is in/out and it is less than 1.0 in the case of upsampling.
        
        //Use the SRC library. Thank you Eric!
        int error = src_simple(&dataForSRC, resamplingQuality, channel);
        if(error != 0)
        {
            [NSException raise:@"Native Audio Error" format:@"Resampling error with code %s", src_strerror(error)];
        }
        
        short* shortData = (short*)calloc(resampledArrayShortLength, sizeof(short *));
        src_float_to_short_array(floatArrayForSRCOut, shortData, (int)resampledArrayShortLength);
        shortLength = resampledArrayShortLength;
        
        //Replace the input argument with a new calloc.
        //We don't release the input argument, but in the case of resample we need to release it too.
        
        audioData = (char*)shortData;
        audioSize = (int)(resampledArrayShortLength * 2);
        
        free(floatArrayForSRCIn);
        free(floatArrayForSRCOut);
    }
    
    //I have a failed attempt to use AudioConverterFillComplexBuffer, a method where an entire internet does not have a single understandable working example.
    //If you want to do the "elegant" conversion, this is a very important read. (terminology, etc.)
    //https://developer.apple.com/documentation/coreaudio/audiostreambasicdescription
    //The deinterleaving conversion below is super noob and ugly... but it works.
    
    UInt32 bytesPerFrame = 2 * channel; // We fixed to 16-bit audio so that's that.
    UInt32 step = bytesPerFrame / channel;
    char *audioDataL = (char*)malloc(audioSize/channel);
    char *audioDataR = (char*)malloc(audioSize/channel);
    
    //NSLog(@"LR Length %d AudioSize %d Channel %d" , audioSize/channel, audioSize, channel );
    
    if(channel == 2)
    {
        BOOL rightInterleave = false;
        // 0 1 2 3 4 5 6 7 8 9 101112131415
        // 0 1 0 1 2 3 2 3 4 5 4 5 6 7 6 7
        // L L R R L L R R L L R R L L R R
        for(int i = 0; i < audioSize; i += step)
        {
            int baseIndex = (i/bytesPerFrame) * step; //the divide will get rid of fractions first
            //NSLog(@"%d %d %u %d",i,baseIndex, (unsigned int)step, rightInterleave);
            for(int j = 0; j < step ; j++)
            {
                if(!rightInterleave)
                {
                    audioDataL[baseIndex + j] = audioData[i + j];
                }
                else
                {
                    audioDataR[baseIndex + j] = audioData[i + j];
                }
            }
            rightInterleave = !rightInterleave;
        }
    }
    else if(channel == 1)
    {
        for(int i = 0; i < audioSize; i++)
        {
            audioDataL[i] = audioData[i];
            audioDataR[i] = audioData[i];
        }
    }
    else
    {
        //throw?
        [NSException raise:@"Native Audio Error" format:@"Your audio is not either 1 or 2 channels!"];
    }
    
    ALuint bufferIdL;
    alGenBuffers(1, &bufferIdL);
    bufferAllocationCount++;
    alBufferData(bufferIdL, AL_FORMAT_MONO16, audioDataL, audioSize/channel, rate);
    
    ALuint bufferIdR;
    alGenBuffers(1, &bufferIdR);
    bufferAllocationCount++;
    alBufferData(bufferIdR, AL_FORMAT_MONO16, audioDataR, audioSize/channel, rate);
    
    if (audioDataL)
    {
        free(audioDataL);
        audioDataL = NULL;
    }
    if (audioDataR)
    {
        free(audioDataR);
        audioDataR = NULL;
    }
    
    if(samplingRate != rate)
    {
        //This is now the new calloc-ed memory from the resampler. We can remove it.
        //The caller is still holding the old pointer which we won't release.
        free(audioData);
    }
    
    runningBufferAllocationNumber++;
    
    NativeAudioBufferIdPair nabip;
    nabip.left = bufferIdL;
    nabip.right = bufferIdR;
    
    //Calculate and cache other data
    nabip.channels = channel;
    nabip.bitDepth = 16;

    //This byte size is already stereo
    nabip.lengthSeconds = audioSize / (float)nabip.channels / (float)(nabip.bitDepth / 8) / (float)rate;
    
    nabips[runningBufferAllocationNumber - 1] = nabip;
    
#ifdef LOG_NATIVE_AUDIO
    NSLog(@"Loaded OpenAL sound: %@ bufferId: L %d R %d size: %u",[NSString stringWithUTF8String:soundUrl], bufferIdL, bufferIdR, (unsigned int)audioSize);
#endif
    
    return runningBufferAllocationNumber - 1;
}


static ALuint sourceCycleIndex = 0;

//Sources are selected sequentially.
//Searching for non-playing source might be a better idea to reduce sound cutoff chance
//(For example, by the time we reach 33rd sound some sound earlier must have finished playing, and we can select that one safely)
//But for performance concern I don't want to run a for...in loop everytime I play sounds.

//The reason of "half" of total available sources is this is only for the left channel. The right channel will be the left's index *2
+ (int) CycleThroughSources
{
    sourceCycleIndex = (sourceCycleIndex + 1) % kHalfMaxConcurrentSources;
    return sourceCycleIndex;
}

+ (int)PrepareAudio:(int) alBufferIndex IntoSourceCycle:(int) sourceCycle
{
    //-1 or invalid source cycle will get a round robin play.
    if(sourceCycle >= kHalfMaxConcurrentSources || sourceCycle < 0)
    {
        sourceCycle = [NativeAudio CycleThroughSources];
    }
    
    alSourcei(nasips[sourceCycle].left, AL_BUFFER, nabips[alBufferIndex].left);
    alSourcei(nasips[sourceCycle].right, AL_BUFFER, nabips[alBufferIndex].right);
#ifdef LOG_NATIVE_AUDIO
    NSLog(@"Pairing OpenAL buffer: L %d R %d with source : L %d R %d", nabips[alBufferIndex].left, nabips[alBufferIndex].right, nasips[sourceCycle].left, nasips[sourceCycle].right);
#endif
    return sourceCycle;
}

+ (int)PlayAudio:(int) alBufferIndex SourceCycle:(int)sourceCycle Adjustment:(NativeAudioPlayAdjustment) playAdjustment
{
    if(sourceCycle == -1)
    {
        sourceCycle = [NativeAudio PrepareAudio:alBufferIndex IntoSourceCycle:-1];
    }
    else
    {
        sourceCycle = [NativeAudio PrepareAudio:alBufferIndex IntoSourceCycle:sourceCycle];
    }
    [NativeAudio PlayAudioWithSourceCycle:sourceCycle Adjustment:playAdjustment];
    return sourceCycle;
}

+ (float)LengthBySource:(int)index
{
    return nabips[index].lengthSeconds;
}

+ (void)StopAudio:(int) sourceCycle
{
    alSourceStop(nasips[sourceCycle].left);
    alSourceStop(nasips[sourceCycle].right);
}

//If you have prepared and have a source index in hand it is possible to call this and play whatever sound that is in a that source. Directly after preparing it should be the sound that you want, but later it might be something else when sources cycle to the same point again.
+ (void)PlayAudioWithSourceCycle:(int) sourceCycle Adjustment:(NativeAudioPlayAdjustment) playAdjustment
{
    NativeAudioSourceIdPair nasip = nasips[sourceCycle];
    
    //If we call play before the adjust, you might hear the pre-adjusted audio.
    //It is THAT fast, even in between lines of code you can hear the audio already.
    [NativeAudio SetVolume:playAdjustment.volume OnSourceCycle:sourceCycle];
    [NativeAudio SetPan:playAdjustment.pan OnSourceCycle:sourceCycle];
    alSourcef(nasips[sourceCycle].left, AL_SEC_OFFSET, playAdjustment.offsetSeconds);
    alSourcef(nasips[sourceCycle].right, AL_SEC_OFFSET, playAdjustment.offsetSeconds);
    alSourcei(nasips[sourceCycle].left, AL_LOOPING, playAdjustment.trackLoop ? AL_TRUE : AL_FALSE);
    alSourcei(nasips[sourceCycle].right, AL_LOOPING, playAdjustment.trackLoop ? AL_TRUE : AL_FALSE);
    
    ALint state;
    //alSourcePlay on a paused source results in RESUME, we need to stop it to start over.
    alGetSourcei(nasips[sourceCycle].left, AL_SOURCE_STATE, &state);
    if(state == AL_PAUSED)
    {
        alSourceStop(nasip.left);
        alSourceStop(nasip.right);
    }
    alSourcePlay(nasip.left);
    alSourcePlay(nasip.right);
    
#ifdef LOG_NATIVE_AUDIO
    NSLog(@"Played OpenAL at source index : L %d R %d", nasip.left, nasip.right);
#endif
}

+ (void)SetVolume:(float) volume OnSourceCycle:(int) sourceCycle
{
#ifdef LOG_NATIVE_AUDIO
    NSLog(@"Set Volume %f L %d R %d", volume, nasips[sourceCycle].left, nasips[sourceCycle].right);
#endif
    alSourcef(nasips[sourceCycle].left, AL_GAIN, volume);
    alSourcef(nasips[sourceCycle].right, AL_GAIN, volume);
}

//With OpenAL's 3D design, to achieve 2D panning we have deinterleaved the stereo file
//into 2 separated mono sources positioned left and right of the listener. This achieve the same stereo effect.
//Gain is already used in SetVolume, we will use a linear attenuation for panning.
+ (void)SetPan:(float) pan OnSourceCycle:(int) sourceCycle
{
#ifdef LOG_NATIVE_AUDIO
    NSLog(@"Set Pan %f L %d R %d", pan, nasips[sourceCycle].left, nasips[sourceCycle].right);
#endif
    //Left channel attenuate linearly on right pan
    alSource3f(nasips[sourceCycle].left, AL_POSITION, -1 - (MAX(pan, 0)), 0, 0);
    //Right channel attenuate linearly on left pan
    alSource3f(nasips[sourceCycle].right, AL_POSITION, 1 - (MIN(pan, 0)), 0, 0);
}

//Only one side is enough?
+ (float)GetPlaybackTimeOfSourceCycle:(int) sourceCycle
{
    ALfloat returnValue;
    alGetSourcef(nasips[sourceCycle].left, AL_SEC_OFFSET, &returnValue);
#ifdef LOG_NATIVE_AUDIO
    NSLog(@"Get Playback Time %f", returnValue);
#endif
    return returnValue;
}

+(void)SetPlaybackTimeOfSourceCycle:(int) sourceCycle Offset:(float)offsetSeconds
{
    alSourcef(nasips[sourceCycle].left, AL_SEC_OFFSET, offsetSeconds);
    alSourcef(nasips[sourceCycle].right, AL_SEC_OFFSET, offsetSeconds);
    ALint state;
    alGetSourcei(nasips[sourceCycle].left, AL_SOURCE_STATE, &state);
    if(state == AL_STOPPED)
    {
        alSourcePlay(nasips[sourceCycle].left);
        alSourcePlay(nasips[sourceCycle].right);
    }
#ifdef LOG_NATIVE_AUDIO
    NSLog(@"Set Playback Time %f", offsetSeconds);
#endif
}

+(void)TrackPause:(int)sourceCycle
{
    ALint state;
    alGetSourcei(nasips[sourceCycle].left, AL_SOURCE_STATE, &state);
    if(state == AL_PLAYING)
    {
        alSourcePause(nasips[sourceCycle].left);
        alSourcePause(nasips[sourceCycle].right);
    }
}

+(void) TrackResume:(int)sourceCycle
{
    ALint state;
    alGetSourcei(nasips[sourceCycle].left, AL_SOURCE_STATE, &state);
    if(state == AL_PAUSED)
    {
        alSourcePlay(nasips[sourceCycle].left);
        alSourcePlay(nasips[sourceCycle].right);
    }
}

@end


extern "C" {
    
    int _Initialize() {
        return [NativeAudio Initialize];
    }
    
    int _SendByteArray(char* byteArrayInput, int byteSize, int channels, int samplingRate, int resamplingQuality)
    {
        return [NativeAudio SendByteArray:byteArrayInput audioSize:byteSize channels:channels samplingRate:samplingRate resamplingQuality: resamplingQuality];
    }
    
    int _LoadAudio(const char* soundUrl, int resamplingQuality) {
        return [NativeAudio LoadAudio:soundUrl resamplingQuality: resamplingQuality];
    }
    
    void _PrepareAudio(int bufferIndex) {
        [NativeAudio PrepareAudio:bufferIndex IntoSourceCycle:-1];
    }
    
    int _PlayAudio(int bufferIndex, int sourceCycle, NativeAudioPlayAdjustment playAdjustment) {
        return [NativeAudio PlayAudio:bufferIndex SourceCycle:sourceCycle Adjustment: playAdjustment];
    }
    
    float _LengthBySource(int bufferIndex)
    {
        return [NativeAudio LengthBySource: bufferIndex];
    }
    
    void _StopAudio(int sourceCycle) {
        [NativeAudio StopAudio:sourceCycle];
    }
    
    void _PlayAudioWithSourceCycle(int sourceCycle, NativeAudioPlayAdjustment playAdjustment) {
        [NativeAudio PlayAudioWithSourceCycle:sourceCycle Adjustment: playAdjustment];
    }
    
    void _SetVolume(int sourceCycle, float volume){
        [NativeAudio SetVolume:volume OnSourceCycle:sourceCycle];
    }
    
    void _SetPan(int sourceCycle, float pan){
        [NativeAudio SetPan:pan OnSourceCycle:sourceCycle];
    }
    
    float _GetPlaybackTime(int sourceCycle){
        return [NativeAudio GetPlaybackTimeOfSourceCycle: sourceCycle];
    }
    
    void _SetPlaybackTime(int sourceCycle, float offsetSeconds){
        [NativeAudio SetPlaybackTimeOfSourceCycle:sourceCycle Offset: offsetSeconds];
    }
    
    void _TrackPause(int sourceCycle)
    {
        [NativeAudio TrackPause: sourceCycle];
    }
    
    void _TrackResume(int sourceCycle)
    {
        [NativeAudio TrackResume: sourceCycle];
    }
    
    void _UnloadAudio(int index) {
        [NativeAudio UnloadAudio:index];
    }
}
