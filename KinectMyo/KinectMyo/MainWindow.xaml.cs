using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Kinect;
using KinectMyo;
using System.IO;
using Newtonsoft.Json;
using System.Net.Sockets;
using System.Net;

namespace KinectMyo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public List<string> MyoValues;
        public List<string> KinectValues;

        private KinectSensor kinectSensor;
        public InfraredFrameReader frameReader = null;
        //public FaceFrameHandler faceFrameHandler;
        public VolumeHandler volumeHandler;

        /// <summary>
        /// Reader for color frames
        /// </summary>
        private ColorFrameReader colorFrameReader = null;

        /// <summary>
        /// Bitmap to display
        /// </summary>
        private WriteableBitmap colorBitmap = null;

        BodyFrameHandler bodyFrameHandler;

        //MyoManager.MyoManager myoManager = new Myo.Myo();
        //public ConnectorHub.ConnectorHub myConnector;
        public bool start = true;
        //public ConnectorHub.FeedbackHub myFeedback;
        public static bool isRecording = true;
        public static bool sendingData = false;

        MyoViewModel myMyoViewModel;

        ScreenCapture myScreenCapture;

        public MainWindow()
        {
            InitializeComponent();
            initMyo();
            initKinect();
            myScreenCapture = new ScreenCapture();
            myScreenCapture.captureStart();
            myMyoViewModel = new MyoViewModel();
            myMyoViewModel.ValuesChanged += MyMyoViewModel_ValuesChanged;

            CreateSockets();
            //initConnectorHub();
            StartRecordingFunction();
        }

        private void MyMyoViewModel_ValuesChanged(object sender, MyoViewModel.ValuesChangedEventArgs e)
        {
            setMyoValues(e.values);
        }

        public DateTime startRecordingTime;
        public RecordingObject myMyoRecordingObject;
        public RecordingObject myKinectRecordingObject;
        private void StartRecordingFunction()
        {
            startRecordingTime = DateTime.Now;

            myMyoRecordingObject = new RecordingObject
            {
                RecordingID = startRecordingTime.ToString("yyyy-MM-dd-HH-mm-sss"),
                ApplicationName = "Myo"
            };
            myKinectRecordingObject = new RecordingObject
            {
                RecordingID = startRecordingTime.ToString("yyyy-MM-dd-HH-mm-sss"),
                ApplicationName = "Kinect"
            };
            myoChunk = new RecordingObject
            {
                RecordingID = startChunkTime.ToString("yyyy-MM-dd-HH-mm-sss"),
                ApplicationName = "Myo"
            };

            setValueNames();

        }




        #region init

        public void initKinect()
        {
            this.kinectSensor = KinectSensor.GetDefault();

            this.colorFrameReader = this.kinectSensor.ColorFrameSource.OpenReader();

            // wire handler for frame arrival
            this.colorFrameReader.FrameArrived += this.Reader_ColorFrameArrived;

            // create the colorFrameDescription from the ColorFrameSource using Bgra format
            FrameDescription colorFrameDescription = this.kinectSensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);

            // create the bitmap to display
            this.colorBitmap = new WriteableBitmap(colorFrameDescription.Width, colorFrameDescription.Height, 96.0, 96.0, PixelFormats.Bgr32, null);


            this.kinectSensor.Open();
            this.frameReader = this.kinectSensor.InfraredFrameSource.OpenReader();

            bodyFrameHandler = new BodyFrameHandler(this.kinectSensor, this);
            // faceFrameHandler = new FaceFrameHandler(this.kinectSensor);
            volumeHandler = new VolumeHandler(this.kinectSensor);
        }

        public void initMyo()
        {
            MyoManagerClass myoManager = new MyoManagerClass();
        }


        #endregion

        #region connectorHub
        List<string> myoNames = new List<string>();
        List<string> kinectNames = new List<string>();

        private void setValueNames()
        {
            string temp;
            // Myo Attribute Names
            myoNames.Add("OrientationW");
            myoNames.Add("OrientationX");
            myoNames.Add("OrientationY");
            myoNames.Add("OrientationZ");
            myoNames.Add("AccelerometerX");
            myoNames.Add("AccelerometerY");
            myoNames.Add("AccelerometerZ");
            myoNames.Add("GyroscopeX");
            myoNames.Add("GyroscopeY");
            myoNames.Add("GyroscopeZ");
            //myoNames.Add("GripPressure");
            myoNames.Add("EMGPod_0");
            myoNames.Add("EMGPod_1");
            myoNames.Add("EMGPod_2");
            myoNames.Add("EMGPod_3");
            myoNames.Add("EMGPod_4");
            myoNames.Add("EMGPod_5");
            myoNames.Add("EMGPod_6");
            myoNames.Add("EMGPod_7");

            kinectNames.Add("0_AnkleRight_X");
            temp = "0_AnkleRight_Y";
            kinectNames.Add(temp);
            temp = "0_AnkleRight_Z";
            kinectNames.Add(temp);
            temp = "0_AnkleLeft_X";
            kinectNames.Add(temp);
            temp = "0_AnkleLeft_Y";
            kinectNames.Add(temp);
            temp = "0_AnkleLeft_Z";
            kinectNames.Add(temp);
            temp = "0_ElbowRight_X";
            kinectNames.Add(temp);
            temp = "0_ElbowRight_Y";
            kinectNames.Add(temp);
            temp = "0_ElbowRight_Z";
            kinectNames.Add(temp);
            temp = "0_ElbowLeft_X";
            kinectNames.Add(temp);
            temp = "0_ElbowLeft_Y";
            kinectNames.Add(temp);
            temp = "0_ElbowLeft_Z";
            kinectNames.Add(temp);
            temp = "0_HandRight_X";
            kinectNames.Add(temp);
            temp = "0_HandRight_Y";
            kinectNames.Add(temp);
            temp = "0_HandRight_Z";
            kinectNames.Add(temp);
            temp = "0_HandLeft_X";
            kinectNames.Add(temp);
            temp = "0_HandLeft_Y";
            kinectNames.Add(temp);
            temp = "0_HandLeft_Z";
            kinectNames.Add(temp);
            temp = "0_HandRightTip_X";
            kinectNames.Add(temp);
            temp = "0_HandRightTip_Y";
            kinectNames.Add(temp);
            temp = "0_HandRightTip_Z";
            kinectNames.Add(temp);
            temp = "0_HandLeftTip_X";
            kinectNames.Add(temp);
            temp = "0_HandLeftTip_Y";
            kinectNames.Add(temp);
            temp = "0_HandLeftTip_Z";
            kinectNames.Add(temp);
            temp = "0_Head_X";
            kinectNames.Add(temp);
            temp = "0_Head_Y";
            kinectNames.Add(temp);
            temp = "0_Head_Z";
            kinectNames.Add(temp);
            temp = "0_HipRight_X";
            kinectNames.Add(temp);
            temp = "0_HipRight_Y";
            kinectNames.Add(temp);
            temp = "0_HipRight_Z";
            kinectNames.Add(temp);
            temp = "0_HipLeft_X";
            kinectNames.Add(temp);
            temp = "0_HipLeft_Y";
            kinectNames.Add(temp);
            temp = "0_HipLeft_Z";
            kinectNames.Add(temp);
            temp = "0_ShoulderRight_X";
            kinectNames.Add(temp);
            temp = "0_ShoulderRight_Y";
            kinectNames.Add(temp);
            temp = "0_ShoulderRight_Z";
            kinectNames.Add(temp);
            temp = "0_ShoulderLeft_X";
            kinectNames.Add(temp);
            temp = "0_ShoulderLeft_Y";
            kinectNames.Add(temp);
            temp = "0_ShoulderLeft_Z";
            kinectNames.Add(temp);
            temp = "0_SpineMid_X";
            kinectNames.Add(temp);
            temp = "0_SpineMid_Y";
            kinectNames.Add(temp);
            temp = "0_SpineMid_Z";
            kinectNames.Add(temp);
            temp = "0_SpineShoulder_X";
            kinectNames.Add(temp);
            temp = "0_SpineShoulder_Y";
            kinectNames.Add(temp);
            temp = "0_SpineShoulder_Z";
            kinectNames.Add(temp);



        }

        List<string> lastValues = new List<string>();

        public void setMyoValues(List<string> values)
        {

            if (isRecording)
            {
                var update = new FrameObject(startRecordingTime, myoNames, values);
                var updateChunk = new FrameObject(startChunkTime, myoNames, values);
                if (!values.SequenceEqual(lastValues))
                {
                    myMyoRecordingObject.Frames.Add(update);
                    lastValues = values;

                    if (!sendingData)
                    {
                        myoChunk.Frames.Add(updateChunk);
                    }


                }



            }
        }

        public void setKinectValues(Body[] bodies)
        {
            if (isRecording)
            {

                int counter = 0;
                List<string> values = new List<string>();
                KinectValues = new List<string>();
                KinectValues.Add(volumeHandler.averageVolume.ToString());
                foreach (Body body in bodies)
                {
                    try
                    {
                        if (body.IsTracked)
                        {
                            detectCompression(body.Joints[JointType.ShoulderRight].Position.Y);
                            KinectValues.Add(body.Joints[JointType.AnkleRight].Position.X + "");
                            KinectValues.Add(body.Joints[JointType.AnkleRight].Position.Y + "");
                            KinectValues.Add(body.Joints[JointType.AnkleRight].Position.Z + "");

                            KinectValues.Add(body.Joints[JointType.AnkleLeft].Position.X + "");
                            KinectValues.Add(body.Joints[JointType.AnkleLeft].Position.Y + "");
                            KinectValues.Add(body.Joints[JointType.AnkleLeft].Position.Z + "");

                            KinectValues.Add(body.Joints[JointType.ElbowRight].Position.X + "");
                            KinectValues.Add(body.Joints[JointType.ElbowRight].Position.Y + "");
                            KinectValues.Add(body.Joints[JointType.ElbowRight].Position.Z + "");

                            KinectValues.Add(body.Joints[JointType.ElbowLeft].Position.X + "");
                            KinectValues.Add(body.Joints[JointType.ElbowLeft].Position.Y + "");
                            KinectValues.Add(body.Joints[JointType.ElbowLeft].Position.Z + "");

                            KinectValues.Add(body.Joints[JointType.HandRight].Position.X + "");
                            KinectValues.Add(body.Joints[JointType.HandRight].Position.Y + "");
                            KinectValues.Add(body.Joints[JointType.HandRight].Position.Z + "");

                            KinectValues.Add(body.Joints[JointType.HandLeft].Position.X + "");
                            KinectValues.Add(body.Joints[JointType.HandLeft].Position.Y + "");
                            KinectValues.Add(body.Joints[JointType.HandLeft].Position.Z + "");

                            KinectValues.Add(body.Joints[JointType.HandTipRight].Position.X + "");
                            KinectValues.Add(body.Joints[JointType.HandTipRight].Position.Y + "");
                            KinectValues.Add(body.Joints[JointType.HandTipRight].Position.Z + "");

                            KinectValues.Add(body.Joints[JointType.HandTipLeft].Position.X + "");
                            KinectValues.Add(body.Joints[JointType.HandTipLeft].Position.Y + "");
                            KinectValues.Add(body.Joints[JointType.HandTipLeft].Position.Z + "");

                            KinectValues.Add(body.Joints[JointType.Head].Position.X + "");
                            KinectValues.Add(body.Joints[JointType.Head].Position.Y + "");
                            KinectValues.Add(body.Joints[JointType.Head].Position.Z + "");

                            KinectValues.Add(body.Joints[JointType.HipRight].Position.X + "");
                            KinectValues.Add(body.Joints[JointType.HipRight].Position.Y + "");
                            KinectValues.Add(body.Joints[JointType.HipRight].Position.Z + "");

                            KinectValues.Add(body.Joints[JointType.HipLeft].Position.X + "");
                            KinectValues.Add(body.Joints[JointType.HipLeft].Position.Y + "");
                            KinectValues.Add(body.Joints[JointType.HipLeft].Position.Z + "");

                            KinectValues.Add(body.Joints[JointType.ShoulderRight].Position.X + "");
                            KinectValues.Add(body.Joints[JointType.ShoulderRight].Position.Y + "");
                            KinectValues.Add(body.Joints[JointType.ShoulderRight].Position.Z + "");

                            KinectValues.Add(body.Joints[JointType.ShoulderLeft].Position.X + "");
                            KinectValues.Add(body.Joints[JointType.ShoulderLeft].Position.Y + "");
                            KinectValues.Add(body.Joints[JointType.ShoulderLeft].Position.Z + "");

                            KinectValues.Add(body.Joints[JointType.SpineMid].Position.X + "");
                            KinectValues.Add(body.Joints[JointType.SpineMid].Position.Y + "");
                            KinectValues.Add(body.Joints[JointType.SpineMid].Position.Z + "");

                            KinectValues.Add(body.Joints[JointType.SpineShoulder].Position.X + "");
                            KinectValues.Add(body.Joints[JointType.SpineShoulder].Position.Y + "");
                            KinectValues.Add(body.Joints[JointType.SpineShoulder].Position.Z + "");


                            if (body.Joints[JointType.ShoulderRight].Position.X != 0)
                            {
                                int xxx = counter;
                            }

                            var update = new FrameObject(startRecordingTime, kinectNames, KinectValues);
                            myKinectRecordingObject.Frames.Add(update);


                            if (compressionCounter > previousKinectCompressionCounter)
                            {
                                sendingData = true;
                                sendChunk();
                                startChunkTime = DateTime.Now;
                                myoChunk = new RecordingObject
                                {
                                    RecordingID = startChunkTime.ToString("yyyy-MM-dd-HH-mm-sss"),
                                    ApplicationName = "Myo"
                                };
                                kinectChunk = new RecordingObject
                                {
                                    RecordingID = startChunkTime.ToString("yyyy-MM-dd-HH-mm-sss"),
                                    ApplicationName = "Kinect"
                                };

                                previousKinectCompressionCounter = compressionCounter;
                            }
                            var newKinectChunk = new FrameObject(startChunkTime, kinectNames, KinectValues);
                            kinectChunk.Frames.Add(newKinectChunk);
                        }


                    }
                    catch
                    {

                    }
                    counter++;
                }



            }




        }


        #region compressionDetection

        

        private System.Net.Sockets.TcpClient tcpSendingSocketKinect;
        private System.Net.Sockets.TcpClient tcpSendingSocketMyo;
        DateTime startChunkTime;
        RecordingObject kinectChunk;
        RecordingObject myoChunk;
        float previousShoulderY = 0;
        bool goingDown = false;
        bool goingUp = false;
        bool CompressionStarted = false;
        int compressionCounter = 0;
        int previousKinectCompressionCounter = -1;
        int previousMyoCompressionCounter = -1;
        float movingThreshold = (float)0.01;


        int TCPKinectSenderPort = 20001;
        int TCPMyoSenderPort = 20001;
        string HupIPAddress = "127.0.0.1";

        private void CreateSockets()
        {
            //udpSendingSocketKinect = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,
            // ProtocolType.Udp);

            IPAddress serverAddr = IPAddress.Parse(HupIPAddress);

            //UDPendPoint = new IPEndPoint(serverAddr, TCPKinectSenderPort);


        }


        private void sendChunk()
        {
            
            sendKinect();
         //   sendMyo();
            
        }

        private async void sendMyo()
        {
            //FeedbackObject f = new FeedbackObject(startRecordingTime, feedback, myRecordingObject.ApplicationName);

            try
            {
                tcpSendingSocketMyo = new TcpClient(HupIPAddress, TCPMyoSenderPort);
                // Translate the passed message into ASCII and store it as a Byte array.

                string json = JsonConvert.SerializeObject(myoChunk, Formatting.Indented);
                byte[] send_buffer = Encoding.ASCII.GetBytes(json);
                //byte[] data = System.Text.Encoding.ASCII.GetBytes(message);

                // Get a client stream for reading and writing.
                NetworkStream stream = tcpSendingSocketMyo.GetStream();

                // Send the message to the connected TcpServer. 
                await stream.WriteAsync(send_buffer, 0, send_buffer.Length);

                //stream.Write(data, 0, data.Length);

                //Console.WriteLine("Sent: {0}", json);

                // Close everything.
                stream.Close();

            }
            catch
            {
                Console.WriteLine("error sending TCP message");
            }


        }

        private async void sendKinect()
        {
            //FeedbackObject f = new FeedbackObject(startRecordingTime, feedback, myRecordingObject.ApplicationName);

            try
            {
                List <RecordingObject> myRecordings = new List<RecordingObject>();
                myRecordings.Add(kinectChunk);
                myRecordings.Add(myoChunk);
                tcpSendingSocketKinect = new TcpClient(HupIPAddress, TCPKinectSenderPort);
                // Translate the passed message into ASCII and store it as a Byte array.

                string json = JsonConvert.SerializeObject(myRecordings, Formatting.Indented);
                sendingData = false;
                byte[] send_buffer = Encoding.ASCII.GetBytes(json);
                //byte[] data = System.Text.Encoding.ASCII.GetBytes(message);

                // Get a client stream for reading and writing.
                NetworkStream stream = tcpSendingSocketKinect.GetStream();

                // Send the message to the connected TcpServer. 
                await stream.WriteAsync(send_buffer, 0, send_buffer.Length);
                //stream.Write(data, 0, data.Length);

                // Console.WriteLine("Sent: {0}", json);

                // Close everything.
                stream.Close();

            }
            catch
            {
                Console.WriteLine("error sending TCP message");
            }


        }

        private void detectCompression(float currentShoulderY)
        {

            if (currentShoulderY > previousShoulderY && currentShoulderY - previousShoulderY > movingThreshold)
            {
                if (goingUp == true)
                {


                    goingUp = false;
                    goingDown = true;
                    compressionCounter++;
                    CompressionStarted = true;

                }
                else
                {

                    CompressionStarted = false;

                }


            }
            else if (currentShoulderY < previousShoulderY)
            {
                goingDown = false;
                goingUp = true;
            }
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(
                                () =>
                                {
                                    if (goingDown == true)
                                        testLabel.Content = "down  " + compressionCounter + " " + currentShoulderY;
                                    else
                                        testLabel.Content = "up   " + compressionCounter + " " + currentShoulderY;
                                }));

            previousShoulderY = currentShoulderY;
        }
        #endregion

        //private void MyConnector_stopRecordingEvent(object sender)
        //{
        //    start = false;

        //}

        //private void MyConnector_startRecordingEvent(object sender)
        //{
        //    start = true;
        //}

        #endregion



        //#region MyoStuff

        //private void MyFeedback_feedbackReceivedEvent(object sender, string feedback)
        //{
        //    Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(
        //                () =>
        //                {
        //                    //  GripLabel.Content = feedback;
        //                }));
        //}





        ///// <summary>
        ///// Method to update the grip textbox and assign the value to gripPressure var
        ///// </summary>
        ///// <param name="g"></param>
        //public void UpdateGripPressure(Int32 g)
        //{
        //    Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(
        //                () =>
        //                {
        //                    //   GripTxt.Text = g.ToString();
        //                }));
        //}

        ///// <summary>
        ///// Method to update the orientation textbox and assign the value of orientation
        ///// </summary>
        ///// <param name="ww"></param>
        ///// <param name="x"></param>
        ///// <param name="y"></param>
        ///// <param name="z"></param>
        //public void UpdateOrientation(float w, float x, float y, float z)
        //{
        //    Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(
        //                () =>
        //                {
        //                    //   OrientationTxt.Text = w.ToString() + " " + x.ToString() + " " + y.ToString() + " " + z.ToString(); ;
        //                }));
        //}

        ///// <summary>
        ///// Method to update the grip textbox and assign the value to gripPressure var
        ///// </summary>
        ///// <param name="g"></param>
        //public void UpdateRoll(double roll)
        //{
        //    Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(
        //                () =>
        //                {
        //                        RollTxt.Text = roll.ToString();
        //                }));
        //}

        ///// <summary>
        ///// Method to update the grip textbox and assign the value to gripPressure var
        ///// </summary>
        ///// <param name="g"></param>
        //public void UpdatePitch(double pitch)
        //{
        //    Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(
        //                () =>
        //                {
        //                    //    PitchTxt.Text = pitch.ToString();
        //                }));
        //}

        ///// <summary>
        ///// Method to update the grip textbox and assign the value to gripPressure var
        ///// </summary>
        ///// <param name="g"></param>
        //public void UpdateYaw(double yaw)
        //{
        //    Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(
        //                () =>
        //                {
        //                    //   YawTxt.Text = yaw.ToString();
        //                }));
        //}

        //public void UpdateGrip(string grip)
        //{
        //    Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(
        //                () =>
        //                {
        //                    //  GripLabel.Content = grip;
        //                }));
        //}

        //#endregion


        #region KinectStuff


        public ImageSource ImageSource
        {
            get
            {
                return this.colorBitmap;
            }
        }

        public IPEndPoint UDPendPoint { get; private set; }


        /// <summary>
        /// Handles the color frame data arriving from the sensor
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Reader_ColorFrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {
            // ColorFrame is IDisposable
            using (ColorFrame colorFrame = e.FrameReference.AcquireFrame())
            {
                if (colorFrame != null)
                {
                    FrameDescription colorFrameDescription = colorFrame.FrameDescription;

                    using (KinectBuffer colorBuffer = colorFrame.LockRawImageBuffer())
                    {
                        this.colorBitmap.Lock();

                        // verify data and write the new color frame data to the display bitmap
                        if ((colorFrameDescription.Width == this.colorBitmap.PixelWidth) && (colorFrameDescription.Height == this.colorBitmap.PixelHeight))
                        {
                            colorFrame.CopyConvertedFrameDataToIntPtr(
                                this.colorBitmap.BackBuffer,
                                (uint)(colorFrameDescription.Width * colorFrameDescription.Height * 4),
                                ColorImageFormat.Bgra);

                            this.colorBitmap.AddDirtyRect(new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight));
                        }

                        this.colorBitmap.Unlock();

                    }
                }

                myImage.Source = ImageSource;
            }
        }






        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            isRecording = false;

            String myoFileName = myMyoRecordingObject.RecordingID + myMyoRecordingObject.ApplicationName + ".json";
            Console.WriteLine(myoFileName);
            using (StreamWriter sw = new StreamWriter(File.Open(myoFileName, System.IO.FileMode.Append)))
            {
                JsonSerializer s1 = new JsonSerializer();
                s1.Formatting = Formatting.Indented;
                s1.Serialize(sw, myMyoRecordingObject);
            }

            String kinectFileName = myKinectRecordingObject.RecordingID + myKinectRecordingObject.ApplicationName + ".json";
            Console.WriteLine(kinectFileName);
            using (StreamWriter sw = new StreamWriter(File.Open(kinectFileName, System.IO.FileMode.Append)))
            {
                JsonSerializer s2 = new JsonSerializer();
                s2.Formatting = Formatting.Indented;
                s2.Serialize(sw, myKinectRecordingObject);
            }

            myScreenCapture.captureStop();

            if (this.colorFrameReader != null)
            {
                // ColorFrameReder is IDisposable
                this.colorFrameReader.Dispose();
                this.colorFrameReader = null;
            }
            if (volumeHandler != null)
            {
                volumeHandler.close();
            }
            if (bodyFrameHandler != null)
            {
                bodyFrameHandler.close();
            }
            if (this.kinectSensor != null)
            {
                this.kinectSensor.Close();
                this.kinectSensor.Close();
                this.kinectSensor = null;
            }
            Environment.Exit(0);

        }


    }


    #endregion
}