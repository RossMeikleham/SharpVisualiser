
using System;
using System.Linq;
using System.Numerics;


using SharpPlayer.MediaProcessing.SignalProcessing;

// dealing with Uncompressed WAV data
namespace SharpPlayer.MediaProcessing.Codec {

    public enum NumChannels { Mono = 1, Stereo = 2 };


    public class Wav {

        public const int RiffChunkSize = 12; // Size of Riff Chunk In Bytes

        private byte[] rawData; //Raw PCM Data
        private uint ChunkSize;


        /* Attempts to read the riff chunk from a given
         * stream of bytes, returns the ChunkSize if successful */
        //TODO add custom exception
        public static uint ReadRiffChunk(byte[] data) {
            if (data.Length < RiffChunkSize) {
                throw new Exception("Not enough bytes available to read the Riff Chunk");
            }

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

    }

    // Format Chunk Descriptor
    public struct FormatChunk {

        public const int FormatChunkSize = 24; // Size of the Format Chunk in Bytes

        public readonly NumChannels NChannels;
        public readonly uint SampleRate;
        public readonly uint ByteRate;
        public readonly ushort BlockAlign;
        public readonly ushort BitsPerSample;

        public FormatChunk(byte[] data) {

            if (data.Length < FormatChunkSize) {
                throw new Exception("Not enough bytes available to read the Format Chunk");
            }

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

            // Get Bits Per Sample, in PCM should be rounded up to the next 8 bits
            ushort bitsPerSample = EndianHelper.ToUInt16LE(data, 22);
            bitsPerSample += (ushort)(bitsPerSample % 8);
            BitsPerSample = bitsPerSample;

            /* Get Byte Rate, in PCM this is redundant as it's equal to
             * SampleRate * NumChannels * BitsPerSample/8. For validity we'll
             * check these match */
            ByteRate = EndianHelper.ToUInt32LE(data, 16);

            if (ByteRate != SampleRate * nChannels * (BitsPerSample / 8)) {
                throw new Exception("Unable to read Format Chunk");
            }

            /* Get Block Align, again in PCM this is redundant as it's equal to
             * NumChannels * BitsPerSample/8 */
            BlockAlign = EndianHelper.ToUInt16LE(data, 20);

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

        private static uint SwapBytes(uint x) {
            return (x << 24) | ((x << 8) & 0x00FF0000) | ((x >> 8) & 0x0000FF00) | (x >> 24); 
        }

        private static ushort SwapBytes(ushort x) {
            return (ushort)((uint)(x << 8) | ((uint)(x >> 8)));
        }

    }

}

