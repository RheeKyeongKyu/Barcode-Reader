using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using OpenCvSharp;


namespace Barcode_Reader
{
    public partial class MainForm : Form
    {
        // Declare class-level variables for webcam operations
        VideoCapture capture;
        private BackgroundWorker workerCamera;
        bool isWebcamRunning = false;

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
                BarcodeInfo barcodeInfo = ImageProcessing.GetBarcodeRegion(input: new Mat(imageFile));
                DisposePictureBoxAndSetImage(barcodeInfo: barcodeInfo);
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

                workerCamera.CancelAsync();
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

                capture = new VideoCapture();
                capture.Open(index: 0);
                if (!capture.IsOpened())
                {
                    return;
                }

                workerCamera = new BackgroundWorker();
                workerCamera.WorkerReportsProgress = true;
                workerCamera.WorkerSupportsCancellation = true;

                workerCamera.DoWork += new DoWorkEventHandler(Worker_DoWork);

                workerCamera.ProgressChanged += new ProgressChangedEventHandler(Worker_ProgressChanged);
                workerCamera.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Worker_RunWorkerCompleted);

                // Start Worker Thread
                workerCamera.RunWorkerAsync();
            }
        }


        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (capture != null)
            {
                using (Mat frame = capture.RetrieveMat())
                {
                    if (workerCamera.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }

                    // Process image for retrieving barcode region and show it in PictureBox
                    BarcodeInfo barcodeInfo = ImageProcessing.GetBarcodeRegion(input: frame);
                    workerCamera.ReportProgress(percentProgress: 0, userState: barcodeInfo);

                    if (barcodeInfo.BarcodeFound)
                    {
                        // Store barcode information in database
                        //System.Threading.Thread.Sleep(3000);
                    }
                }
            }
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // Do not update PictureBox when BackgroundWorker is in Cancellation
            if (!workerCamera.CancellationPending)
            {
                BarcodeInfo barcodeInfo = (BarcodeInfo)e.UserState;
                DisposePictureBoxAndSetImage(barcodeInfo: barcodeInfo);
            }
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                // Commented code below because disposing of PictureBox is already done in the Main Thread
                //DisposePictureBoxAndSetImage();
            }
        }

        private void ReleaseCamera()
        {
            try
            {
                capture?.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
            }
        }

        private void DisposePictureBoxAndSetImage(BarcodeInfo barcodeInfo = null)
        {
            // Dispose PictureBox if it's not null then set Image
            pb_ImageWebcam.Image?.Dispose();
            pb_barcodeRegion.Image?.Dispose();
            pb_decodeResult.Image?.Dispose();

            if (barcodeInfo != null)
            {
                pb_ImageWebcam.Image = barcodeInfo.ProcessedImage;
                pb_barcodeRegion.Image = barcodeInfo.BarcodeImage;
                pb_decodeResult.Image = barcodeInfo.BarcodeDecodeResult;
            }
            else
            {
                pb_ImageWebcam.Image = null;
                pb_barcodeRegion.Image = null;
                pb_decodeResult.Image = null;
            }
        }

    }
}
