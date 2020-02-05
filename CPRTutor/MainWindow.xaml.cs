using Microsoft.Kinect;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WMPLib;


namespace CPRTutor
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
        public VolumeHandler volumeHandler;

        private string sessionPath = "sessions/";
        private string sessionName;
        /// <summary>
        /// Reader for color frames
        /// </summary>
        private ColorFrameReader colorFrameReader = null;

        /// <summary>
        /// Bitmap to display
        /// </summary>
        private WriteableBitmap colorBitmap = null;

        BodyFrameHandler bodyFrameHandler;

        public bool start = true;
        public static bool isRecording = false;
        public static bool sendingData = false;

        MyoViewModel myMyoViewModel;

        ScreenCapture myScreenCapture;

        public System.TimeSpan startCompression;
        public System.TimeSpan endCompression;


        public MainWindow()
        {
            InitializeComponent();
            initMyo();
            initKinect();
            initFeedback();
        }

        private void MyMyoViewModel_ValuesChanged(object sender, MyoViewModel.ValuesChangedEventArgs e)
        {
            setMyoValues(e.values);
        }

        public DateTime startRecordingTime;
        public RecordingObject myMyoRecordingObject;
        public RecordingObject myKinectRecordingObject;

        public AnnotationObject detectedCompressions;

        public Dictionary<string, List<int>> targetList;

        public IntervalObject lastCompression;

        public Dictionary<string, string> feedbackAudioFiles;
        public String feedbackAudioDirectory;


        public void Start_Recording(object sender, System.EventArgs e)
        {

            sessionName = DateTime.Now.ToString("yyyy-MM-dd-") + DateTime.Now.Hour.ToString() + "H" + DateTime.Now.Minute.ToString() + "M" + DateTime.Now.Second.ToString() + "S";

            isRecording = true;
            isRecordingLabel.Content = "Recording...";

            // if session folder doesn't exists, it creates it
            System.IO.Directory.CreateDirectory(sessionPath + sessionName + "/");

            myScreenCapture = new ScreenCapture();
            myScreenCapture.captureStart(sessionPath + sessionName);
            myMyoViewModel = new MyoViewModel();
            myMyoViewModel.ValuesChanged += MyMyoViewModel_ValuesChanged;

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
            //myoChunk = new RecordingObject
            //{
            //    RecordingID = startChunkTime.ToString("yyyy-MM-dd-HH-mm-sss"),
            //    ApplicationName = "Myo"
            //};
            feedbackObject = new RecordingObject
            {
                RecordingID = startRecordingTime.ToString("yyyy-MM-dd-HH-mm-sss"),
                ApplicationName = "Feedback"
            };
            detectedCompressions = new AnnotationObject
            {
                RecordingID = startRecordingTime.ToString("yyyy-MM-dd-HH-mm-sss"),
                ApplicationName = "CPRTutor_annotations"
            };

            targetList = new Dictionary<string, List<int>>();

            setValueNames();
            CreateSockets();
        }




        #region init

        public void Stop_Recording(object sender, System.EventArgs e)
        {

            if (isRecording)
            {
                isRecording = false;
                isRecordingLabel.Content = "";

                String myoFileName = sessionPath + sessionName + "/" + myMyoRecordingObject.RecordingID + "_" + myMyoRecordingObject.ApplicationName + ".json";
                Console.WriteLine(myoFileName);
                using (StreamWriter sw = new StreamWriter(File.Open(myoFileName, System.IO.FileMode.Append)))
                {
                    JsonSerializer s1 = new JsonSerializer();
                    s1.Formatting = Formatting.Indented;
                    s1.Serialize(sw, myMyoRecordingObject);
                }

                String kinectFileName = sessionPath + sessionName + "/" + myKinectRecordingObject.RecordingID + "_" + myKinectRecordingObject.ApplicationName + ".json";
                Console.WriteLine(kinectFileName);
                using (StreamWriter sw = new StreamWriter(File.Open(kinectFileName, System.IO.FileMode.Append)))
                {
                    JsonSerializer s2 = new JsonSerializer();
                    s2.Formatting = Formatting.Indented;
                    s2.Serialize(sw, myKinectRecordingObject);
                }

                String feedbackFileName = sessionPath + sessionName + "/" + feedbackObject.RecordingID + "_" + feedbackObject.ApplicationName + ".json";
                Console.WriteLine(feedbackFileName);
                using (StreamWriter sw = new StreamWriter(File.Open(feedbackFileName, System.IO.FileMode.Append)))
                {
                    JsonSerializer s3 = new JsonSerializer();
                    s3.Formatting = Formatting.Indented;
                    s3.Serialize(sw, feedbackObject);
                }

                String annotationFileName = sessionPath + sessionName + "/" + detectedCompressions.RecordingID + "_" + detectedCompressions.ApplicationName + ".json";

                Console.WriteLine(annotationFileName);
                using (StreamWriter sw = new StreamWriter(File.Open(annotationFileName, System.IO.FileMode.Append)))
                {
                    JsonSerializer s3 = new JsonSerializer();
                    s3.Formatting = Formatting.Indented;
                    s3.Serialize(sw, detectedCompressions);
                }

                myScreenCapture.captureStop();

                //string startPath = sessionPath + sessionName;//folder to add
                //string zipPath = sessionPath + sessionName+".zip";//URL for your ZIP file
                //ZipFile.CreateFromDirectory(startPath, zipPath, CompressionLevel.Fastest, true);

            }
        }



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


        public void initScreenRecorder()
        {

        }

        public void initFeedback()
        {
            feedbackAudioDirectory = "CPRTutor.Common.";//Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Common\\");
            feedbackAudioFiles = new Dictionary<string, string>();
            feedbackAudioFiles.Add("classRate", "feedback_classRate.mp3");
            feedbackAudioFiles.Add("classRelease", "feedback_classRelease.mp3");
            feedbackAudioFiles.Add("classDepth", "feedback_classDepth.mp3");
            feedbackAudioFiles.Add("armsLocked", "feedback_armsLocked.mp3");
            feedbackAudioFiles.Add("bodyWeight", "feedback_bodyWeight.mp3");

        }

        #endregion

        #region setting_datavalues
        List<string> myoNames = new List<string>();
        List<string> kinectNames = new List<string>();

        private void setValueNames()
        {
            // Myo Attribute Names
            myoNames.Add("Orientation_W");
            myoNames.Add("Orientation_X");
            myoNames.Add("Orientation_Y");
            myoNames.Add("Orientation_Z");
            myoNames.Add("Accelerometer_X");
            myoNames.Add("Accelerometer_Y");
            myoNames.Add("Accelerometer_Z");
            myoNames.Add("Gyroscope_X");
            myoNames.Add("Gyroscope_Y");
            myoNames.Add("Gyroscope_Z");
            //myoNames.Add("GripPressure");
            myoNames.Add("EMGPod_0");
            myoNames.Add("EMGPod_1");
            myoNames.Add("EMGPod_2");
            myoNames.Add("EMGPod_3");
            myoNames.Add("EMGPod_4");
            myoNames.Add("EMGPod_5");
            myoNames.Add("EMGPod_6");
            myoNames.Add("EMGPod_7");

            kinectNames.Add("Ankle_Right_X");
            kinectNames.Add("Ankle_Right_Y");
            kinectNames.Add("Ankle_Right_Z");

            kinectNames.Add("Ankle_Left_X");
            kinectNames.Add("Ankle_Left_Y");
            kinectNames.Add("Ankle_Left_Z");

            kinectNames.Add("Elbow_Right_X");
            kinectNames.Add("Elbow_Right_Y");
            kinectNames.Add("Elbow_Right_Z");

            kinectNames.Add("Elbow_Left_X");
            kinectNames.Add("Elbow_Left_Y");
            kinectNames.Add("Elbow_Left_Z");

            kinectNames.Add("Hand_Right_X");
            kinectNames.Add("Hand_Right_Y");
            kinectNames.Add("Hand_Right_Z");

            kinectNames.Add("Hand_Left_X");
            kinectNames.Add("Hand_Left_Y");
            kinectNames.Add("Hand_Left_Z");

            kinectNames.Add("Hand_Right_Tip_X");
            kinectNames.Add("Hand_Right_Tip_Y");
            kinectNames.Add("Hand_Right_Tip_Z");

            kinectNames.Add("Hand_Left_Tip_X");
            kinectNames.Add("Hand_Left_Tip_Y");
            kinectNames.Add("Hand_Left_Tip_Z");

            kinectNames.Add("Head_X");
            kinectNames.Add("Head_Y");
            kinectNames.Add("Head_Z");

            kinectNames.Add("Hip_Right_X");
            kinectNames.Add("Hip_Right_Y");
            kinectNames.Add("Hip_Right_Z");

            kinectNames.Add("Hip_Left_X");
            kinectNames.Add("Hip_Left_Y");
            kinectNames.Add("Hip_Left_Z");

            kinectNames.Add("Shoulder_Right_X");
            kinectNames.Add("Shoulder_Right_Y");
            kinectNames.Add("Shoulder_Right_Z");

            kinectNames.Add("Shoulder_Left_X");
            kinectNames.Add("Shoulder_Left_Y");
            kinectNames.Add("Shoulder_Left_Z");

            kinectNames.Add("Spine_Mid_X");
            kinectNames.Add("Spine_Mid_Y");
            kinectNames.Add("Spine_Mid_Z");

            kinectNames.Add("Spine_Shoulder_X");
            kinectNames.Add("Spine_Shoulder_Y");
            kinectNames.Add("Spine_Shoulder_Z");

            kinectNames.Add("Volume");

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
                            KinectValues.Add(volumeHandler.averageVolume.ToString());

                            if (body.Joints[JointType.ShoulderRight].Position.X != 0)
                            {
                                int xxx = counter;
                            }

                            var update = new FrameObject(startRecordingTime, kinectNames, KinectValues);
                            myKinectRecordingObject.Frames.Add(update);


                            if (compressionCounter > previousKinectCompressionCounter)
                            {
                                sendingData = true;
                                String responseData;
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
                                sendChunk();
                                



                                previousKinectCompressionCounter = compressionCounter;

                                string gaugeImage = @"C:\Users\Daniele-WIN10\Documents\GitHub\SharpFlow\gauge.png";
                                if (File.Exists(gaugeImage))
                                {
                                    BitmapImage image = new BitmapImage();
                                    image.BeginInit();
                                    image.CacheOption = BitmapCacheOption.OnLoad;
                                    image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                                    image.UriSource = new Uri(gaugeImage);
                                    image.EndInit();
                                    cprGauge.Source = image;
                                    cprGauge.Width = 500;

                                }

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

        #endregion

        public void setFeedbackValues(List<string> feedbackNames, List<string> feedbackValues)
        {
            if (isRecording)
            {
                var update = new FrameObject(startRecordingTime, feedbackNames, feedbackValues);
                feedbackObject.Frames.Add(update);
            }
        }

        #region compressionDetection


        private System.Net.Sockets.TcpClient tcpSendingSocketKinect;
        private System.Net.Sockets.TcpClient tcpSendingSocketMyo;
        DateTime startChunkTime;
        RecordingObject kinectChunk;
        RecordingObject myoChunk;
        RecordingObject feedbackObject;
        float previousShoulderY = 0;
        bool goingDown = false;
        bool goingUp = false;
        bool CompressionStarted = false;
        int compressionCounter = 0;
        int previousKinectCompressionCounter = -1;
        //int previousMyoCompressionCounter = -1;
        float movingThreshold = (float)0.003;
        //float ccMinDuration = (float)0.3;
        float ccMaxDuration = (float)0.85;
        public DateTime lastFeedbackPrompted = DateTime.Now;

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



        private async void sendChunk()
        {
            if (compressionCounter > 0)
            {

                try
                {
                    List<RecordingObject> myRecordings = new List<RecordingObject>();
                    myRecordings.Add(kinectChunk);
                    myRecordings.Add(myoChunk);

                    tcpSendingSocketKinect = new TcpClient(HupIPAddress, TCPKinectSenderPort);
                    // Translate the passed message into ASCII and store it as a Byte array.

                    string json = JsonConvert.SerializeObject(myRecordings, Formatting.Indented);
                    sendingData = false;
                    byte[] send_buffer = Encoding.ASCII.GetBytes(json);
                    byte[] data;

                    // Get a client stream for reading and writing
                    NetworkStream stream = tcpSendingSocketKinect.GetStream();

                    // Send the message to the connected TcpServer. 
                    await stream.WriteAsync(send_buffer, 0, send_buffer.Length);

                    //Console.WriteLine("Sent: {0}", json);

                    // Receive the TcpServer.response.

                    // Buffer to store the response bytes.
                    data = new Byte[256];

                    // String to store the response ASCII representation.
                    String responseData = String.Empty;

                    // Read the first batch of the TcpServer response bytes.
                    Int32 bytes = stream.Read(data, 0, data.Length);
                    responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                    Console.WriteLine("Received: {0}", responseData);
                    //feedbackOutput.Content = responseData;
                    processFeedback(responseData);
                    // Close everything.
                    stream.Close();

                }
                catch
                {
                    Console.WriteLine("error sending TCP message");
                }
            }
        }


        private void processFeedback(String feedbackString)
        {

            int noLastCC = 10;
            int correctCCs = 0;
            double ratio = 0;
            double errorRate = 0;
            List<string> annotationNames = new List<string>();
            List<string> annotationValues = new List<string>();
            List<int> errorRates = new List<int>();
            TimeSpan deltaFeedback;
            TimeSpan feedbackTimestamp;
            List<string> feedbackNames = new List<string>();
            List<string> feedbackValues = new List<string>();
            String worstTargetName = "";
            Double worstTargetValue = 0;

            try
            {
                dynamic jsonFeedback = JsonConvert.DeserializeObject(feedbackString);
                foreach (var property in jsonFeedback)
                {
                    if (!targetList.ContainsKey(property.Name))
                    {
                        targetList.Add(property.Name, new List<int>());
                    }
                    targetList[property.Name].Add((int)property.Value.Value);

                    // save the property names and values in the two lists 
                    annotationNames.Add(property.Name);
                    annotationValues.Add((property.Value.Value).ToString());
                }
                feedbackOutput.Content = "";
                // set the annoation names and values in the annotation object 
                lastCompression.setAnnotations(annotationNames, annotationValues);
                foreach (KeyValuePair<string, List<int>> target in targetList)
                {
                    if (target.Value.Count() > noLastCC)
                    {
                        List<int> lastNCCs = Enumerable.Reverse(target.Value).Take(noLastCC).Reverse().ToList(); ;
                        correctCCs = (from temp in lastNCCs where temp.Equals(1) select temp).Count();
                        ratio = (double)(correctCCs) / noLastCC * 100;
                    }
                    else
                    {
                        correctCCs = (from temp in target.Value where temp.Equals(1) select temp).Count();
                        ratio = (double)(correctCCs) / target.Value.Count();
                        errorRate = 100 - Math.Round((ratio * 100), 1);
                    }

                    feedbackOutput.Content += target.Key + " (correct CC rate " + (errorRate.ToString()) + "%): " + String.Join(", ", target.Value) + "\n";

                    if (errorRate > worstTargetValue) {
                        worstTargetValue = errorRate;
                        worstTargetName = target.Key;
                    }

              
                }
                // check if 10s are passed      
                deltaFeedback = DateTime.Now.Subtract(lastFeedbackPrompted);
                if (deltaFeedback > TimeSpan.FromSeconds(10) && worstTargetValue>50)
                {
                    Console.WriteLine("10 seconds are passed: "+deltaFeedback.TotalSeconds.ToString()
                        +" worst target: "+worstTargetName+" target value: "+worstTargetValue.ToString());
                    feedbackTimestamp = DateTime.Now.Subtract(startRecordingTime);

                    feedbackNames.Add("errorRate");
                    feedbackValues.Add((worstTargetValue / 100).ToString());
                    feedbackNames.Add("feedbackDevice");
                    feedbackValues.Add("audio");
                    feedbackNames.Add("targetClass");
                    feedbackValues.Add(worstTargetName);
                    feedbackNames.Add("feedbackMessage");
                    feedbackValues.Add(feedbackAudioFiles[worstTargetName]);

                    var update = new FrameObject(startRecordingTime, feedbackNames, feedbackValues);
                    feedbackObject.Frames.Add(update);
                    lastFeedbackPrompted = DateTime.Now;
                    promptFeedback(worstTargetName,"audio");
                }
                else
                {
                    Console.WriteLine("10 seconds are NOT passed");
                }

            }
            catch
            {
                Console.WriteLine("Failed to process the feedback");
            }

        }

        private void promptFeedback(String target, String channel)
        {
            if (channel == "audio"){

                WindowsMediaPlayer wmp = new WindowsMediaPlayer();
                Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    feedbackAudioDirectory+feedbackAudioFiles[target]);
                var files = Assembly.GetExecutingAssembly().GetManifestResourceNames();
                Console.WriteLine(files.ToString());
                string temppath = Path.GetTempPath() + "\\temp.mp3";
                using (Stream output = new FileStream(temppath, FileMode.Create))
                {
                    byte[] buffer = new byte[32 * 1024];
                    int read;

                    while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        output.Write(buffer, 0, read);
                    }
                }
                wmp.URL = temppath;
                wmp.controls.play();
            }
        }

        private void detectCompression(float currentShoulderY)
        {
            IntervalObject tmpAnnotation;

            if (currentShoulderY < previousShoulderY && Math.Abs(currentShoulderY - previousShoulderY) > movingThreshold)
            {
                if (goingDown == false)
                {
                    goingDown = true;
                    goingUp = false;
                    CompressionStarted = true;
                    startCompression = DateTime.Now.Subtract(startRecordingTime).Subtract(TimeSpan.FromMilliseconds(35));
                }
            }
            else if (currentShoulderY > previousShoulderY && Math.Abs(currentShoulderY - previousShoulderY) > movingThreshold)
            {
                goingUp = true;
                goingDown = false;
            }
            else if (currentShoulderY > previousShoulderY && Math.Abs(currentShoulderY - previousShoulderY) < movingThreshold)
            {
                if (goingUp && CompressionStarted)
                {
                    endCompression = DateTime.Now.Subtract(startRecordingTime);
                    double timeDifference = endCompression.Subtract(startCompression).TotalSeconds;
                    //Console.WriteLine("timeDifference: " + timeDifference);

                    goingUp = false;
                    goingDown = false;
                    CompressionStarted = false;
                    endCompression = DateTime.Now.Subtract(startRecordingTime);
                    tmpAnnotation = new IntervalObject(startCompression, endCompression);
                    if (timeDifference < ccMaxDuration)
                    {
                        // if it doesn't contain the item tem
                        if (!detectedCompressions.Intervals.Contains(tmpAnnotation))
                        {
                            detectedCompressions.Intervals.Add(tmpAnnotation);
                            compressionCounter++;
                            lastCompression = tmpAnnotation;
                        }
                    }
                }
            }

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                if (goingDown == true)
                    compressionLabel.Content = "c.c. #" + compressionCounter + " down";
                else
                    compressionLabel.Content = "c.c. #" + compressionCounter + " up";
            }));

            previousShoulderY = currentShoulderY;
        }
        #endregion


        #region KinectStuff
        public ImageSource ImageSource
        {
            get
            {
                return this.colorBitmap;
            }
        }

        public IPEndPoint UDPendPoint { get; private set; }
        public string SessionName { get => sessionName; set => sessionName = value; }


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

            // delete temp file
            File.Delete(Path.GetTempPath() + "\\temp.mp3");

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