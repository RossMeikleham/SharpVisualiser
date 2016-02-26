using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Diagnostics;
using System.IO;


using SharpPlayer.MediaProcessing.SignalProcessing;

// dealing with PCM WAV data
namespace SharpPlayer.MediaProcessing {

    public enum NumChannels { Mono = 1, Stereo = 2 };

    public interface Codec {
        NumChannels NChannels { get; }
        uint SampleRate { get; }
        List<short> SampleData { get; }

        //public ThreadStaticAttribute 
    }
}