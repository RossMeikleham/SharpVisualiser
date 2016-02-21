using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SharpPlayer.MediaProcessing.Codec;
using System.Diagnostics;

// Test Wav Data 
namespace WavTests {

    [TestClass]
    public class WavTest {

     
        [TestMethod]
        public void CheckHeaderParsing() {

            byte[] riffHeader = { 0x52, 0x49, 0x46, 0x46, 0x24, 0x08, 0x00, 0x00, 0x57, 0x41, 0x56, 0x45 };
            byte[] fmtHeader = {
                0x66, 0x6D, 0x74, 0x20, 0x10, 0x00, 0x00, 0x00, 0x01, 0x00, 0x02, 0x00,
                0x22, 0x56, 0x00, 0x00, 0x88, 0x58, 0x01, 0x00, 0x04, 0x00, 0x10, 0x00,
                0x64, 0x61, 0x74, 0x61, 0x00, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x24, 0x17, 0x1E, 0xF3, 0x3C, 0x13, 0x3C, 0x14, 0x16, 0xF9, 0x18, 0xF9,
                0x34, 0xE7, 0x23, 0xA6, 0x3C, 0xF2, 0x24, 0xF2, 0x11, 0xCE, 0x1A, 0x0D };

            uint chunkSize = Wav.ReadRiffChunk(riffHeader);
            //Debug.WriteLine("Chunk Size: {0}", chunkSize);
            Assert.AreEqual(chunkSize, 2084u);

            FormatChunk fChunk = new FormatChunk(fmtHeader);
            Assert.AreEqual(NumChannels.Stereo, fChunk.NChannels);
            Assert.AreEqual(22050u, fChunk.SampleRate);
            Assert.AreEqual(88200u, fChunk.ByteRate);
            Assert.AreEqual(4u, fChunk.BlockAlign);
            Assert.AreEqual(16u, fChunk.BitsPerSample);

            /*
            Debug.WriteLine("Mono/Stereo: {0}", fChunk.NChannels);
            Debug.WriteLine("Sample Rate: {0}hz", fChunk.SampleRate);
            Debug.WriteLine("Byte Rate: {0}", fChunk.ByteRate);
            Debug.WriteLine("Block Align: {0}", fChunk.BlockAlign);
            Debug.WriteLine("Bits Per Sample: {0}", fChunk.BitsPerSample);
            */
        }
    }
}