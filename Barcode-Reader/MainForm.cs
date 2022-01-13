using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Extensions;


namespace Barcode_Reader
{
    public partial class MainForm : Form
    {
        // Declare class-level variables for webcam operations
        VideoCapture capture;
        Mat frame;
        private Thread camera;
        bool isWebcamRunning = false;
        Bitmap image;

        public MainForm()
        {
            InitializeComponent();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            ReleaseCamera();
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void OpenImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string imageFile = ShowFileOpenDialogAndGetImageFile();

            // Display the image in PictureBox if the file exists
            if (File.Exists(imageFile))
            {
                StopRecording();

                // Process image for retrieving barcode region and show it in PictureBox
                //frame = GetBarcodeRegion(new Mat(imageFile));
                //image = BitmapConverter.ToBitmap(frame);

                image = new Bitmap(imageFile);

                DisposePictureBoxAndSetImage(image);
            }
        }

        private void StartRecordingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StartRecording();
        }

        private void StopRecordingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StopRecording();
        }

        private string ShowFileOpenDialogAndGetImageFile()
        {
            string imageFile = string.Empty;

            // Allow user to select a image file from local directory
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Title = "Select an image file containing barcode";
            fileDialog.Filter = "Image File (*.jpg; *.jpeg; *.png; *.bmp; *.gif; *.tiff; *.raw) | *.jpg; *.jpeg; *.png; *.bmp; *.gif; *.tiff; *.raw";

            DialogResult dialogResult = fileDialog.ShowDialog();
            if (dialogResult == DialogResult.OK)
            {
                imageFile = fileDialog.FileName;
            }
            
            return imageFile;
        }

        private void StopRecording()
        {
            if (isWebcamRunning)
            {
                isWebcamRunning = false;
                startRecordingToolStripMenuItem.Enabled = true;
                stopRecordingToolStripMenuItem.Enabled = false;

                ReleaseCamera();
                DisposePictureBoxAndSetImage();
            }
        }

        private void StartRecording()
        {
            if (!isWebcamRunning)
            {
                isWebcamRunning = true;
                startRecordingToolStripMenuItem.Enabled = false;
                stopRecordingToolStripMenuItem.Enabled = true;

                camera = new Thread(new ThreadStart(CaptureCameraCallback));
                camera.Start();
            }
        }

        private void ReleaseCamera()
        {
            try
            {
                if (capture != null && frame != null && camera != null)
                {
                    capture.Release();
                    frame.Release();
                    camera.Abort();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.ToString()}");
            }
        }

        private void CaptureCameraCallback()
        {
            frame = new Mat();
            capture = new VideoCapture(index: 0);
            capture.Open(index: 0);

            if (capture.IsOpened())
            {
                while (isWebcamRunning)
                {
                    capture.Read(image: frame);

                    // Process image for retrieving barcode region and show it in PictureBox
                    //frame = GetBarcodeRegion(frame);
                    image = BitmapConverter.ToBitmap(frame);

                    DisposePictureBoxAndSetImage(image);
                }
            }
        }

        private void DisposePictureBoxAndSetImage(Bitmap image = null)
        {
            // Dispose PictureBox and set Image
            if (pb_ImageWebcam.Image != null)
            {
                pb_ImageWebcam.Image.Dispose();
            }

            pb_ImageWebcam.Image = image;
        }


    }
}
