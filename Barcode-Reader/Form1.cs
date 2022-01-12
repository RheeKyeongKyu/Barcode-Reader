using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using OpenCvSharp;

namespace Barcode_Reader
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void fromImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string imageFile = ShowFileOpenDialogAndGetImageFile();

            // Display the image if exists
            if (File.Exists(imageFile))
            {
                pb_ImageWebcam.Image = new Bitmap(imageFile);
            }
        }

        public string ShowFileOpenDialogAndGetImageFile()
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
    }
}
