using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SharpPlayer.MediaProcessing.Codecs;
using SharpPlayer.MediaProcessing.SignalProcessing;

using System.Diagnostics;
using System.IO;
using SharpPlayer.MediaProcessing;


// Test Wav Data 
namespace WavTests {

    [TestClass]
    public class WavTest {


        [TestMethod]
        public void TestWavParsing() {

            byte[] data = {
                0x52, 0x49, 0x46, 0x46, 0x24, 0x08, 0x00, 0x00, 0x57, 0x41, 0x56, 0x45,
                0x66, 0x6D, 0x74, 0x20, 0x10, 0x00, 0x00, 0x00, 0x01, 0x00, 0x02, 0x00,
                0x22, 0x56, 0x00, 0x00, 0x88, 0x58, 0x01, 0x00, 0x04, 0x00, 0x10, 0x00,
                0x64, 0x61, 0x74, 0x61, 0x1C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x24, 0x17, 0x1E, 0xF3, 0x3C, 0x13, 0x3C, 0x14, 0x16, 0xF9, 0x18, 0xF9,
                0x34, 0xE7, 0x23, 0xA6, 0x3C, 0xF2, 0x24, 0xF2, 0x11, 0xCE, 0x1A, 0x0D };

            var wav = new Wav(data.ToList());

            Assert.AreEqual(wav.ChunkSize, 2084u);

            Assert.AreEqual(NumChannels.Stereo, wav.NChannels);
            Assert.AreEqual(22050u, wav.SampleRate);


            short[] expectedSamples;

            unchecked {
                short[] expectedSamples2 = {
                0x0000, 0x0000, 0x1724, (short)0xF31E, 0x133C, 0x143C, (short)0xF916, (short)0xF918,
                (short)0xE734, (short)0xA623, (short)0xF23C, (short)0xF224, (short)0xCE11, 0x0D1A};

                expectedSamples = expectedSamples2;
            }

            for (int i = 0; i < wav.SampleData.Count(); i++) {
                Assert.AreEqual(expectedSamples[i], wav.SampleData[i]);
            }
        }

        [TestMethod]
        public void TestLoadFile() {
            var exampleDir = Directory.GetParent(Directory.GetCurrentDirectory())
                                        .Parent.Parent.Parent.FullName + "/examples/";

            string wavFilePath = exampleDir + "8k16bitpcm.wav";
            Wav wav = new Wav(File.ReadAllBytes(wavFilePath).ToList());

            Debug.WriteLine("Channels:{0}", wav.NChannels);
            Debug.WriteLine("SampleRate:{0}Hz", wav.SampleRate);
            Debug.WriteLine("LengthSamples{0}", wav.SampleData.Count());

            int bufferSize = 8192;
            double[] buffer;

            for (int i = 0; i < wav.SampleData.Count(); i+= bufferSize) {
                if (i + bufferSize > wav.SampleData.Count()) {
                    break;
                }
                buffer = FFT.Normalize(FFT.PerformFFT(
                            wav.SampleData.Skip(i)
                               .Take(bufferSize)
                               .Select(sample => new Complex(sample, 0))
                               .ToArray()));

                Debug.WriteLine(buffer[0]);
            }

        }
    }
}