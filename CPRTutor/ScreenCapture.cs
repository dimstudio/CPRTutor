using System;
using System.Drawing;
using System.Threading;
using Accord.Video.FFMPEG;
using System.IO.Compression;

namespace CPRTutor
{
    class ScreenCapture
    {
        VideoFileWriter vf;
        DateTime startCaptureTime;
        string filename;
        Bitmap bmpScreenShot;
        private Thread myCaptureThread;
        bool isRecording = false;
        string filePath;

        public ScreenCapture(){ }


        private void timer1_Tick(object sender, EventArgs e)
        {
            captureFunction();
        }

        private void captureFunction()
        {
            while (isRecording == true)
            {
                try
                {
                    int screenWidth = (int)System.Windows.SystemParameters.PrimaryScreenWidth * 2;
                    int screenHeight = (int)System.Windows.SystemParameters.PrimaryScreenHeight * 2;

                    Graphics gfx = Graphics.FromImage((System.Drawing.Image)bmpScreenShot);
                    gfx.CopyFromScreen(0, 0, 0, 0, new System.Drawing.Size(screenWidth, screenHeight));
                    System.TimeSpan diff1 = DateTime.Now.Subtract(startCaptureTime);
                    vf.WriteVideoFrame(bmpScreenShot, diff1);

                }
                catch
                {

                }

                Thread.Sleep(40);
            }
            vf.Close();
            //string startPath = this.filePath;//folder to add
            string zipPath = this.filePath + ".zip";//URL for your ZIP file
            ZipFile.CreateFromDirectory(filePath, zipPath, CompressionLevel.Fastest, true);
        }

        public void captureStart(String filePath)
        {
            isRecording = true;
            this.filePath = filePath;
            vf = new VideoFileWriter();
            startCaptureTime = DateTime.Now;
            filename = filePath + "/" + DateTime.Now.ToString("yyyy-MM-dd-") + DateTime.Now.Hour.ToString() + "H" + DateTime.Now.Minute.ToString() + "M" + DateTime.Now.Second.ToString() + "S_video.mp4";

            int screenWidth = (int)System.Windows.SystemParameters.PrimaryScreenWidth * 2;
            int screenHeight = (int)System.Windows.SystemParameters.PrimaryScreenHeight * 2;
            bmpScreenShot = new Bitmap(screenWidth, screenHeight);

            //vf.Width = screenWidth;
            //vf.Height = screenHeight;
            //vf.FrameSize = 25;
            //vf.VideoCodec = VideoCodec.Default;
            //vf.BitRate = 1000000;
            //vf.Open(filename);

            vf.Open(filename, screenWidth, screenHeight, 25, VideoCodec.Default, 500000);


            myCaptureThread = new Thread(new ThreadStart(captureFunction));
            myCaptureThread.Start();

        }

        public void captureStop()
        {
            isRecording = false;
        }
    }
}