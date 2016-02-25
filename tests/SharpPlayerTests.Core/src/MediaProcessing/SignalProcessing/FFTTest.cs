using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SharpPlayer.MediaProcessing.SignalProcessing;

// Tests For Fast Fourier Transform
namespace SharpPlayerTests {

    [TestClass]
    public class FFTTest {

        /* Test FFT with no points */
        [TestMethod]
        public void FFTEmpty() {
            var emptyArr = new Complex[0];
            Assert.AreEqual(emptyArr.Length, FFT.PerformFFT(emptyArr).Length);
        }


        /* Test FFT with single point, should return the same point */
        [TestMethod]
        public void FFT1() {

            Random randNum = new Random();
            Complex[] points =
                Enumerable.Repeat(0, 20)
                          .Select(i => new Complex(randNum.NextDouble() * (10 ^ 6),
                                                     randNum.NextDouble() * (10 ^ 6)))
                          .ToArray();

            var singlePoint = new Complex[1];
            foreach (Complex point in points) {
                singlePoint[0] = point;
                Assert.AreEqual(point, FFT.PerformFFT(singlePoint)[0]);
            }
        }


        /* From Manual Calculation, FFT for N = 2 of points {x0, x1}
         * should result in {x0 + x1, x0 - x1} */
        [TestMethod]
        public void FFT2() {
            double[] realPoints = { 1, 2.34, 78.94, 432987746, 3.14159, -32, -99932, 1, 0, 0 };
            double[] imgPoints = { 0, 0, 0, 323, -32, 43.32234, 3.163, 832, -0.003, 0 };

            var fValues = new Complex[realPoints.Length / 2][];
            var fResult = new Complex[realPoints.Length / 2][];

            for (int i = 0; i < fValues.GetLength(0); i++) {
                fValues[i] = new Complex[2];
                fValues[i][0] = new Complex(realPoints[2 * i], imgPoints[2 * i]);
                fValues[i][1] = new Complex(realPoints[2 * i + 1], imgPoints[2 * i + 1]);

                fResult[i] = FFT.PerformFFT(fValues[i]);
            }

            for (int i = 0; i < fValues.Length; i++) {
                Assert.AreEqual(fValues[i][0] + fValues[i][1], fResult[i][0]);
                Assert.AreEqual(fValues[i][0] - fValues[i][1], fResult[i][1]);
            }
        }


        /* Test FFT with 4 Real points */
        [TestMethod]
        public void RealFFT4() {
            var values = new Complex[4];
            for (int i = 1; i < values.Length + 1; i++) {
                values[i - 1] = new Complex(i, 0);
            }

            double[] expectedReal = { 10, -2, -2, -2, -2 };
            double[] expectedImg = { 0, 2, 0, -2 };

            var result = FFT.PerformFFT(values);
            for (int i = 0; i < result.Length; i++) {
                Assert.AreEqual(expectedReal[i], result[i].Real, delta: Math.Pow(10, -5));
                Assert.AreEqual(expectedImg[i], result[i].Imaginary, delta: Math.Pow(10, -5));
            }

        }


        /* Test FFT with 8 Real points */
        [TestMethod]
        public void RealFFT8() {
            var values = new Complex[8];
            for (int i = 0; i < values.Length; i++) {
                values[i] = new Complex(i, 0);
            }

            double[] expectedReal =
                Enumerable.Repeat(28.0, 1)
                          .Concat(Enumerable.Repeat(-4.0, values.Length - 1))
                          .ToArray();

            double[] expectedImg = { 0, 9.656854, 4, 1.656854, 0, -1.656854, -4, -9.656854 };

            var result = FFT.PerformFFT(values);
            for (int i = 0; i < result.Length; i++) {
                Assert.AreEqual(expectedReal[i], result[i].Real, delta: Math.Pow(10, -5));
                Assert.AreEqual(expectedImg[i], result[i].Imaginary, delta: Math.Pow(10, -5));
            }

        }

        /* Test FFT with a fair amount of points */
        [TestMethod]
        public void FFTMedium() {
            Complex[] values = Enumerable.Repeat(0, 8192).Select(i => new Complex(0, 0)).ToArray();
            var result = FFT.PerformFFT(values);
            var expected = new Complex(0, 0);

            for (int i = 0; i < values.Length; i++) {
                Assert.AreEqual(expected, result[i]);
            }
        }


        [TestMethod]
        public void Normalize1() {
            var value = new Complex(3, 4);
            var arr = new Complex[] { value };

            var result = FFT.Normalize(FFT.PerformFFT(arr));
            Assert.AreEqual(5, result[0]);
        }

      
        

    }
}