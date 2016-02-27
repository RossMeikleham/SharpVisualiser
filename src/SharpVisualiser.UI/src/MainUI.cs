using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using SharpPlayer.MediaProcessing;
using SharpPlayer.MediaProcessing.Codecs;


// Main Visualiser UI

namespace SharpPlayer.UI {
    public partial class SharpVisualiser : Form {

        private bool soundLoaded; 
        private VisualiserState VState; 
        private Codec codec; // Currently Loaded Media File

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

                codec = wav;

                // Enable Media Control Buttons
                playToolStripMenuItem.Enabled = true;
                resetToolStripMenuItem.Enabled  = true;
                soundLoaded = true;         
            }
        }

        // "Quit" Button Pressed
        private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
            Application.Exit();
        }

        // Paint event, send details to the Visualiser to update the screen
        private void visualiserCanvas_Paint(object sender, PaintEventArgs e) {
            if (soundLoaded) {
                VState.drawEqualiserBars(sender, e);
            }
        }

        // Redraw the Visualiser to scale if the screen is resized
        private void visualiserCanvas_Resize(object sender, EventArgs e) {
            if (soundLoaded) {
                visualiserCanvas.Invalidate();
            }
        }

        // Starts playing the loaded media file
        private void playToolStripMenuItem_Click(object sender, EventArgs e) {
            
            // Give a delegate callback to the player for passing sample data to our visualiser 
            if (codec.Play(VState.drawVisualiserData)) { 
                playToolStripMenuItem.Enabled = false; // Can't play if we're already playing
                pauseToolStripMenuItem.Enabled = true; // Allow for pausing in the middle of playing
                
            } else {
                MessageBox.Show("Music Already Playing", "Playing Music", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Pauses a currently playing media file
        private void pauseToolStripMenuItem_Click(object sender, EventArgs e) {
            codec.Pause();
            playToolStripMenuItem.Enabled = true; // If we're paused, allow us the option to start playing again
            pauseToolStripMenuItem.Enabled = false; // Can't pause if we're already paused
        }

        // Resets a currently playing media file to the beginning of the track 
        private void resetToolStripMenuItem_Click(object sender, EventArgs e) {
            codec.Reset();
            playToolStripMenuItem.Enabled = true; // If reset, allow for starting to play again 
            pauseToolStripMenuItem.Enabled = false; // Not currently playing so pausing makes no sense
        }
    }



    // Stores the state of the Visualiser for drawing graphics on screen
    public class VisualiserState {
        public const int pixelSpace = 5; // Space between bars
        public const int barPixelWidth = 15; // Width of bars
        public readonly double maxMagnitude = 20 * Math.Log10(short.MaxValue); // Max magnitude in decibels ~90db

        private PictureBox Canvas;
        private double[] Data; // Processed sample data to visualise


        public VisualiserState(PictureBox canvas) {
            Canvas = canvas;
        }

        public void drawVisualiserData(double[] data) {
            Data = data;
            Canvas.Invalidate();
        }

        public void drawEqualiserBars(object sender, PaintEventArgs e) {
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
                Brush color = Brushes.Aqua;
                double barHeight = (20 * Math.Log10(avg) * height) / maxMagnitude;
                if (barHeight < 1.0) {
                    barHeight = 1.0;
                } 
                
                RectangleF rect = new RectangleF(curX, height - (float)barHeight, barPixelWidth, (float)barHeight);
                graphics.FillRectangle(color, rect);

                curX += (pixelSpace + barPixelWidth);
                i += groupN;
            }
 
        }

    } 
}
