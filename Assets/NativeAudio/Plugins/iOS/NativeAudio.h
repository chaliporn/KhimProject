// Native Audio
// 5argon - Exceed7 Experiments
// Problems/suggestions : 5argon@exceed7.com

#import <AVFoundation/AVFoundation.h>
#import <OpenAl/al.h>
#import <OpenAl/alc.h>
#import "libsamplerate-0.1.9/src/samplerate.h"
#include <AudioToolbox/AudioToolbox.h>

@interface NativeAudio : NSObject
{
}

typedef struct
{
    ALuint left;
    ALuint right;
    int channels;
    int bitDepth;
    float lengthSeconds;
} NativeAudioBufferIdPair;

typedef struct
{
    ALuint left;
    ALuint right;
} NativeAudioSourceIdPair;

typedef struct
{
    int audioPlayerIndex;
    float volume;
    float pan;
    float offsetSeconds;
    bool trackLoop;
} NativeAudioPlayAdjustment;

+ (int)Initialize;
+ (int)LoadAudio:(const char *)soundUrl resamplingQuality:(int)resamplingQuality;
+ (int)SendByteArray:(const char *)audioData audioSize:(int)audioSize channels:(int)channel samplingRate:(int)samplingRate resamplingQuality:(int)resamplingQuality;
+ (int)PrepareAudio:(int)alBufferIndex IntoSourceCycle:(int)sourceCycle;
+ (int)PlayAudio:(int)alBufferIndex SourceCycle:(int)sourceCycle Adjustment:(NativeAudioPlayAdjustment)playAdjustment;
+ (void)PlayAudioWithSourceCycle:(int)sourceCycle Adjustment:(NativeAudioPlayAdjustment)playAdjustment;
+ (void)UnloadAudio:(int)index;
+ (float)LengthBySource:(int)index;
+ (void)StopAudio:(int)sourceCycle;

+ (void)SetVolume:(float)volume OnSourceCycle:(int)sourceCycle;
+ (void)SetPan:(float)pan OnSourceCycle:(int)sourceCycle;

+ (float)GetPlaybackTimeOfSourceCycle:(int)sourceCycle;
+ (void)SetPlaybackTimeOfSourceCycle:(int)sourceCycle Offset:(float)offsetSeconds;
+ (void)TrackPause:(int)sourceCycle;
+ (void)TrackResume:(int)sourceCycle;

@end
