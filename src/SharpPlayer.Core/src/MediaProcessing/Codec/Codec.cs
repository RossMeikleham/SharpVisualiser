using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using SharpPlayer.MediaProcessing.SignalProcessing;

// Dealing with processing media for visualisations
namespace SharpPlayer.MediaProcessing {

    // A sample can either be on a single channel (Mono) or Left and Right channels (Stereo)
    public enum NumChannels { Mono = 1, Stereo = 2 };

    public abstract class Codec {

        public NumChannels NChannels { get; protected set; }
        public uint SampleRate { get; protected set; } // Samples per second
        public List<short> SampleData { get; protected set; }

        private long LastSampleProcessed = 0;
        private const int BufferSize = 256; // Size of buffer to FFT
        private bool IsPlaying = false;
        private Mutex PlayMutex = new Mutex();
        private Task PlayTask;

        public delegate void ProcessFrequencies(double[] buffer);
       

        /* Attempts to start playing the sound samples from
         * where it last left off, returns true if playing is successful,
         * false otherwise (can only play one at a time) */
        public bool Play(ProcessFrequencies pf) {

            PlayMutex.WaitOne();
            bool isPlaying = IsPlaying;
            if (!isPlaying) {
                IsPlaying = true;
            }
            PlayMutex.ReleaseMutex();

            if (!isPlaying) {
                PlayTask =  Task.Run(() => Playing(pf, LastSampleProcessed));

            }

            return !isPlaying;
        }


        /* Plays the loaded media file until it's finished or another thread
         * tells us to stop */ 
        private void Playing(ProcessFrequencies process, long totalSamplesProcessed) {
            
            long currentSamplesProcessed = 0; // Keep track of what samples we've processed so far
            Stopwatch stopWatch = new Stopwatch();
            bool isMono = NChannels == NumChannels.Mono;
            int processLength = isMono ? BufferSize : BufferSize * 2; //Need to process Left + Right channels for Stereo

            stopWatch.Start();

            // While music is playing and all samples haven't been played
            while (totalSamplesProcessed < SampleData.Count()) {
                bool isPlaying;
                PlayMutex.WaitOne();
                isPlaying = IsPlaying;
                PlayMutex.ReleaseMutex();

                if (!isPlaying) {
                    return;
                }

                // Sleep for some time as we don't need to continually send data all the time
                Task.Delay(100).Wait();
                long elapsed = stopWatch.ElapsedMilliseconds;
                stopWatch.Restart();

                // Check how many samples we need to send based on the time elapsed since we last sent samples
                long nextSamplesProcessed = (SampleRate * elapsed) / 1000;
                currentSamplesProcessed += nextSamplesProcessed;

                // Time Elapsed enough to process an entire buffer
                if (currentSamplesProcessed >= processLength) {
                    double[] buffer;
                    if (isMono) {
                        buffer =
                           FFT.Magnitude(FFT.PerformFFT(
                                SampleData.Skip((int)totalSamplesProcessed)
                                          .Take(processLength)
                                          .Select(sample => new Complex(sample , 0))
                                          .ToArray()));
                     // Stereo
                    } else {
                        Complex[] fftBuff = new Complex[BufferSize];
                        int idx = 0;
                        for (int i = (int)totalSamplesProcessed; i < (int)totalSamplesProcessed + processLength; i += 2) {
                            fftBuff[idx++] = (SampleData[i] + SampleData[i + 1]) / 2; // Average left and right channels
                        }
                        buffer = FFT.Magnitude(FFT.PerformFFT(fftBuff));
                    }

                    process(buffer);
                    currentSamplesProcessed -= processLength;
                    totalSamplesProcessed += processLength;


                // Reached the end
                } else if (currentSamplesProcessed + totalSamplesProcessed >= SampleData.Count()) {
                    int toTake = SampleData.Count() - (int)totalSamplesProcessed;
                    double[] buffer;
                    if (isMono) {
                        buffer =
                        FFT.Magnitude(FFT.PerformFFT(
                            SampleData.Skip((int)totalSamplesProcessed)
                                      .Take(toTake)
                                      .Select(sample => new Complex(sample, 0))
                                      .Concat(Enumerable.Repeat(new Complex(0, 0), BufferSize - toTake))
                                      .ToArray()));
                    // Stereo
                    } else {
                        Complex[] fftBuff = new Complex[BufferSize];
                        int idx = 0;
                        for (int i = (int)totalSamplesProcessed; i < (int)totalSamplesProcessed + toTake; i+=2) {
                            fftBuff[idx++] = (SampleData[i] + SampleData[i + 1]) / 2; // Average left and right channels  
                        }
                        // Pad ending with 0s
                        for (int i = toTake; i < BufferSize; i++) {
                            fftBuff[i] = 0;
                        }
                        buffer = FFT.Magnitude(FFT.PerformFFT(fftBuff));
                    }

                    process(buffer);
                    totalSamplesProcessed = SampleData.Count();
                }
            }

            LastSampleProcessed = totalSamplesProcessed;
        }


        // Pauses playing of a currently loaded media file
        public void Pause() {
            PlayMutex.WaitOne();
            if (IsPlaying) {
                IsPlaying = false;
                PlayMutex.ReleaseMutex();
                PlayTask.Wait();
            } else {
                PlayMutex.ReleaseMutex();
            }
        }


        // Resets the currently loaded media file to the beginning
        public void Reset() {
            Pause();
            LastSampleProcessed = 0;
        }

    }

}