
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Diagnostics;
using System.IO;


using SharpVisualiser.MediaProcessing.SignalProcessing;
using SharpVisualiser.MediaProcessing;

// dealing with PCM WAV data
namespace SharpVisualiser.MediaProcessing.Codecs {


    public class Wav : Codec {

        public const int RiffChunkSize = 12; // Size of Riff Chunk In Bytes
        public readonly uint ChunkSize;

        public Wav(List<byte> data) {
            ChunkSize = ReadRiffChunk(data);
            data.RemoveRange(0, RiffChunkSize);

            var FMTChunk = new FormatChunk(data);
            NChannels = FMTChunk.NChannels;
            SampleRate = FMTChunk.SampleRate;
            data.RemoveRange(0, FormatChunk.FormatChunkSize);
      

            SampleData = ReadDataChunk(data);
            SampleData.Count();
        }


        /* Attempts to read the riff chunk from a given
         * stream of bytes, returns the ChunkSize if successful */
        //TODO add custom exception
        public static uint ReadRiffChunk(List<byte> lData) {

            if (lData.Count() < RiffChunkSize) {
                throw new Exception("Not enough bytes available to read the Riff Chunk");
            }

            var data = lData.Take(RiffChunkSize).ToArray();

            // Chunk Id should contain the ascii string "RIFF" in Big Endian format
            if (EndianHelper.ToUInt32BE(data, 0) != 0x52494646) {
                throw new Exception("Unable to read WAVE file");
            }

            // Read In Size of Chunk in Little Endian Format
            uint chunkSize = EndianHelper.ToUInt32LE(data, 4);

            // Format should contain the ascii string "WAVE" in Big Endian format
            if (EndianHelper.ToUInt32BE(data, 8) != 0x57415645) {
                throw new Exception("Unable to read WAVE file");
            }

            return chunkSize;
        }


        public static List<short> ReadDataChunk(List<byte> lData) {

            var data = lData.ToArray();

            // Could be a "List" Chunk prepending
            if (EndianHelper.ToUInt32BE(data, 0) == 0x4C495354) {
                data = data.Skip(78).ToArray();
                lData.RemoveRange(0, 78);
            }

            // Chunk ID should contain the letters "data" in Big Endian format
            if (EndianHelper.ToUInt32BE(data, 0) != 0x64617461) {
                throw new Exception("Unable to read Data Chunk, incorrect ID");
            }

            // Number of bytes containing pure sample data 
            uint dataSize = EndianHelper.ToUInt32LE(data, 4);

            /* Check data size is a multiple of 2, as samples are 16bits each. Also
             * check there's enough bytes left in the buffer to extract the sample data */
            if ((dataSize & 0x1) != 0x0 || dataSize > int.MaxValue || dataSize > lData.Count() - 8) {
                Debug.WriteLine((dataSize & 0x1) + " " + dataSize + " " + lData.Count());
                throw new Exception("Unable to read Data Chunk");
            }

            var sampleData = new List<short>();
            lData.RemoveRange(0, 8);

            for (int i = 0; i < dataSize; i += 2) {
                sampleData.Add(BitConverter.IsLittleEndian ? 
                    (short)(lData[i] | lData[i + 1] << 8) : 
                    (short)(lData[i + 1] | lData[i] << 8)); 
            }

            return sampleData;
        }

    }

    // Format Chunk Descriptor
    public struct FormatChunk {

        public const int FormatChunkSize = 24; // Size of the Format Chunk in Bytes

        public readonly NumChannels NChannels;
        public readonly uint SampleRate;
        public readonly uint ByteRate;
        public readonly ushort BlockAlign;
        public readonly ushort BitsPerSample;

        public FormatChunk(List<byte> lData) {

            if (lData.Count() < FormatChunkSize) {
                throw new Exception("Not enough bytes for reading the Format Chunk");
            }

            var data = lData.Take(FormatChunkSize).ToArray();
         
            // SubChunk ID should contain the ascii string "fmt " in Big Endian format
            if (EndianHelper.ToUInt32BE(data, 0) != 0x666D7420) {
                throw new Exception("Unable to read Format Chunk");
            }
            
            /* Check SubChunk Size is 16 for PCM. currently only supporting PCM at the moment,
             * so fail on any other sizes */
            if (EndianHelper.ToUInt32LE(data, 4) != 0x10) {
                throw new Exception("Non PCM format non-supported");
            }
            
            /* Check Audio Format is for PCM (value of 1). Current only supporting PCM at the moment,
             * so fail on any other values */
            if (EndianHelper.ToUInt16LE(data, 8) != 0x1) {
                throw new Exception("Non PCM format non-supported");
            }

            // Check if Mono(1) or Stereo(2) number of channels
            ushort nChannels = EndianHelper.ToUInt16LE(data, 10);
            if (nChannels != 1 && nChannels != 2) {
                throw new Exception("Unable to read Format Chunk");
            }
            NChannels = nChannels == 1 ? NumChannels.Mono : NumChannels.Stereo;

            // Get Sample Rate
            SampleRate = EndianHelper.ToUInt32LE(data, 12);

            /* Get Byte Rate, in PCM this is redundant as it's equal to
             * SampleRate * NumChannels * BitsPerSample/8. */
            ByteRate = EndianHelper.ToUInt32LE(data, 16);
            

            /* Get Block Align, again in PCM this is redundant as it's equal to
             * NumChannels * BitsPerSample/8 */
            BlockAlign = EndianHelper.ToUInt16LE(data, 20);

            // Get Bits Per Sample, in PCM should be rounded up to the next 8 bits
            ushort bitsPerSample = EndianHelper.ToUInt16LE(data, 22);
            bitsPerSample += (ushort)(bitsPerSample % 8);
            BitsPerSample = bitsPerSample;

            /* For validity check ByteRate = SampleRate * nChannels * BitsPerSample/8*/
            if (ByteRate != SampleRate * nChannels * (BitsPerSample / 8)) {
                throw new Exception("Unable to read Format Chunk");
            }

            /* For validity check BlockAlign = nChannels * BitsPerSample/8*/
            if (BlockAlign != nChannels * (BitsPerSample / 8)) {
                throw new Exception("Unable to read Format Chunk");
            }
           
        }
    }



    // Helper class for converting bytes in the specified byte order
    public class EndianHelper {

        /* Generates a 32 bit unsigned integer in Little Endian form
         * from the first 4 bytes starting at the given starting index in the byte array */
        public static uint ToUInt32LE(byte[] data, int start) {

            uint result = BitConverter.ToUInt32(data, start);
            if (!BitConverter.IsLittleEndian) {
                return SwapBytes(result);
            } else {
                return result;
            }
        }

        /* Generates a 32 bit unsigned integer in Big Endian form
         * from the first 4 bytes starting at the given starting index in the byte array */
        public static uint ToUInt32BE(byte[] data, int start) {

            uint result = BitConverter.ToUInt32(data, start);
            if (BitConverter.IsLittleEndian) {
                return SwapBytes(result);
            } else {
                return result;
            }
        }

        /* Generates a 16 bit unsigned integer in Little Endian form
         * from the first 2 bytes starting at the given starting index in the byte array */
        public static ushort ToUInt16LE(byte[] data, int start) {

            ushort result = BitConverter.ToUInt16(data, start);
            if (!BitConverter.IsLittleEndian) {
                return SwapBytes(result);
            } else {
                return result;
            }
        }

        /* Generates a 16 bit unsigned integer in Big Endian form
         * from the first 2 bytes starting at the given starting index in the byte array */
        public static ushort ToUInt16BE(byte[] data, int start) {

            ushort result = BitConverter.ToUInt16(data, start);
            if (BitConverter.IsLittleEndian) {
                return SwapBytes(result);
            } else {
                return result;
            }
        }


        /* Generates a 16 bit signed integer in Little Endian form
         * from the first 2 bytes starting at the given starting index in the byte array */
        public static short ToInt16LE(byte[] data, int start) {

            ushort result = BitConverter.ToUInt16(data, start);
            if (!BitConverter.IsLittleEndian) {
                return (short)SwapBytes(result);
            } else {
                return (short)result;
            }
        }

        /* Generates a 16 bit nsigned integer in Big Endian form
         * from the first 2 bytes starting at the given starting index in the byte array */
        public static short ToInt16BE(byte[] data, int start) {

            ushort result = BitConverter.ToUInt16(data, start);
            if (BitConverter.IsLittleEndian) {
                return (short)SwapBytes(result);
            } else {
                return (short)result;
            }
        }

        private static uint SwapBytes(uint x) {
            return (x << 24) | ((x << 8) & 0x00FF0000) | ((x >> 8) & 0x0000FF00) | (x >> 24); 
        }

        private static ushort SwapBytes(ushort x) {
            return (ushort)((uint)(x << 8) | ((uint)(x >> 8)));
        }

    }

}

