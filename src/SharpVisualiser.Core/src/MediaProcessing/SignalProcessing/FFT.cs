using System;
using System.Numerics;
using System.Linq;
using System.Diagnostics;

// Fast Fourier Transform Implementation
namespace SharpPlayer.MediaProcessing.SignalProcessing {


    public class FFT {

         public static double[] Magnitude(Complex[] data) {
            return data.Take((data.Length/ 2) + 1).Select(x => Complex.Abs(x)/data.Length).ToArray();
        }

        // Normalizes short between -1 and 1
        public static double Normalize(short s) {
            return (((s + short.MaxValue + 1.0) * 2) / ushort.MaxValue) - 1;
        }

        /* Uses Cooley–Tukey algorithm with radix-2 DIT case
         * assumes no of points are a power of 2 */
        public static Complex[] PerformFFT(Complex[] buffer) {
            var result = new Complex[buffer.Length];
            Array.Copy(buffer, result, buffer.Length);

            /* After Processing, alternate odd/even pairs are out of order, e.g.
             * after first iteration for 8 points: [x0,x2,x1,x3,x4,x6,x5,x7]
             * The correct positions can be
             * calculated by reversing the bits for the current position */

            int bits = (int)Math.Log(buffer.Length, 2);
            for (int j = 1; j < buffer.Length / 2; j++) {

                int swapPos = BitReverse(j, bits);
                var temp = result[j];
                result[j] = result[swapPos];
                result[swapPos] = temp;
            }

            for (int N = 2; N <= buffer.Length; N <<= 1) {

                /* Process all split splices of points; (TotalNoPoints / 2^n) splices at iteration n 
                 * e.g. for points [1,2,3,4,5,6,7,8]:
                 *
                 * n : 1 -> 4 splices [1,5],[3,7],[2,6],[4,8]
                 * n : 2 -> 2 splices [1,3,5,7],[2,4,6,8]
                 * n : 3 -> 1 splice [1,2,3,4,5,6,7,8] */

                for (int i = 0; i < buffer.Length; i += N) {

                    for (int k = 0; k < N / 2; k++) {

                        int evenIndex = i + k;
                        int oddIndex = i + k + (N / 2);
                        var even = result[evenIndex];
                        var odd = result[oddIndex];

                        double term = -2 * Math.PI * k / (double)N;
                        Complex exp = new Complex(Math.Cos(term), Math.Sin(term)) * odd;

                        result[evenIndex] = even + exp;
                        result[oddIndex] = even - exp;

                    }
                }
            }

            return result;
        }

        /* Performs a Bit Reversal Algorithm on a postive integer 
         * for given number of bits
         * e.g. 011 with 3 bits is reversed to 110 */
        private static int BitReverse(int n, int bits) {
            int reversedN = n;
            int count = bits - 1;

            n >>= 1;
            while (n > 0) {
                reversedN = (reversedN << 1) | (n & 1);
                count--;
                n >>= 1;
            }

            return ((reversedN << count) & ((1 << bits) - 1));
        }

        
       
    }

}
