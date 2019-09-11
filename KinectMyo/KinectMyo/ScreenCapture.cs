using Accord.Video;
using Accord.Video.FFMPEG;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Drawing;


namespace KinectMyo
{
    class ScreenCapture 
    {
        System.Windows.Threading.DispatcherTimer timer1;
        VideoFileWriter vf;
        DateTime startCaptureTime;
        string filename;
        int screenWidth = 2560;
        int screenHeight = 1440;
        Bitmap bmpScreenShot;
        int i;


        public ScreenCapture() {
            timer1 = new System.Windows.Threading.DispatcherTimer();
            timer1.Interval = TimeSpan.FromMilliseconds(20);
            timer1.Tick += timer1_Tick;
            vf = new VideoFileWriter();
            screenWidth =  (int)System.Windows.SystemParameters.PrimaryScreenWidth;
            screenHeight = (int)System.Windows.SystemParameters.PrimaryScreenHeight;
            bmpScreenShot = new Bitmap(screenWidth, screenHeight);
        }


        private void timer1_Tick(object sender, EventArgs e) {
            captureFunction();
        }

        private void captureFunction()
        {
            try
            {
               // Bitmap bmpScreenShot = new Bitmap(screenWidth, screenHeight);
                Graphics gfx = Graphics.FromImage((System.Drawing.Image)bmpScreenShot);
                gfx.CopyFromScreen(0, 0, 0, 0, new System.Drawing.Size(screenWidth, screenHeight));

                TimeSpan elapse = DateTime.Now.Subtract(startCaptureTime);
                Console.WriteLine(i.ToString() +' ' + elapse);
                vf.WriteVideoFrame(bmpScreenShot);
            }
            catch (Exception e)
            {
                //int x = i;

            }
            i++;
        }

        public void captureStart()
        {
            vf = new VideoFileWriter();
            startCaptureTime = DateTime.Now;
            string time = DateTime.Now.Hour.ToString();
            time = time + "H" + DateTime.Now.Minute.ToString() + "M" + DateTime.Now.Second.ToString() + "S";
            time = time + ".mp4";
            filename = time;
            vf.Width = screenWidth;
            vf.Height = screenHeight;
            vf.FrameSize = 25;
            vf.VideoCodec = VideoCodec.Default;
            vf.BitRate = 1000000;
            vf.Open(filename);
            timer1.Start();
        }

        public void captureStop()
        {
            timer1.Stop();
            vf.Close();
        }
    }
}
