using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Numerics;
using System.Diagnostics;

using SharpPlayer.MediaProcessing.Codecs;
using SharpPlayer.MediaProcessing.SignalProcessing;


namespace SharpPlayer.UI {
    public partial class SharpVisualiser : Form {

        private bool soundLoaded;
        private VisualiserState VState;

        public SharpVisualiser() {
            InitializeComponent();
            soundLoaded = false;
            VState = new VisualiserState(visualiserCanvas);
        }

        // "Open" Button Pressed, attempt to select a music file
        private void openToolStripMenuItem_Click(object sender, EventArgs e) {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() == DialogResult.OK) {
                List<byte> fileData = File.ReadAllBytes(openFileDialog.FileName).ToList();

                Wav wav;
                try {
                    wav = new Wav(fileData);

                } catch (Exception exc) {
                    MessageBox.Show(exc.Message, "Loading Wav File", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Test
                double[] buffer = FFT.Normalize(FFT.PerformFFT(
                            wav.SampleData.Take(8192)
                               .Select(sample => new Complex(sample, 0))
                               .ToArray()));

                soundLoaded = true;
                VState.drawVisualiserData(buffer);

            }
        }

        // "Quit" Button Pressed
        private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
            Application.Exit();
        }


        private void visualiserCanvas_Paint(object sender, PaintEventArgs e) {
            if (soundLoaded) {
                VState.drawEquiliserBars(sender, e);
            }
        }

        private void visualiserCanvas_Resize(object sender, EventArgs e) {
            if (soundLoaded) {
                Debug.WriteLine("Resizing");
                visualiserCanvas.Invalidate();
            }
        }

    }


    public class VisualiserState {
        public const int pixelSpace = 10; // Space between bars
        public const int barPixelWidth = 20;
        public readonly double maxMagnitude = 20 * Math.Log10(short.MaxValue);

        private PictureBox Canvas;
        private double[] Data;


        public VisualiserState(PictureBox canvas) {
            Canvas = canvas;
        }

        public void drawVisualiserData(double[] data) {
            Data = data;
            Canvas.Invalidate();
        }

        public void drawEquiliserBars(object sender, PaintEventArgs e) {
            int nData = Data.Length;
            int width = Canvas.Width;
            int height = Canvas.Height;

            // Count how many bars we can fit on Screen
            int maxBars = (width + pixelSpace) / (barPixelWidth + pixelSpace);

            // Check how many bars it's possible to draw with the given data
            int totalBars = maxBars > Data.Count() ? Data.Count() : maxBars;

            int grouping = nData / totalBars;
            int remainder = nData % totalBars;

            Graphics graphics = e.Graphics;

            int i = 0;
            int curX = 0;

            while (i < nData) {
                int groupN = grouping;

                if (remainder > 0) {
                    groupN += 1;
                    remainder--;
                }

                // Get average of data to plot as single bar
                double avg = 0;
                for (int j = 0; j < groupN; j++) {
                    avg += Data[i + j];
                }
                avg /= groupN;

                // Draw The Bar
                Brush color = Brushes.Aquamarine;
                double barHeight = (20 * Math.Log10(avg) * height) / maxMagnitude;
                RectangleF rect = new RectangleF(curX, height - (float)barHeight, barPixelWidth, (float)barHeight);
                graphics.FillRectangle(color, rect);

                curX += (pixelSpace + barPixelWidth);
                i += groupN;
            }
 
        }

    } 
}
