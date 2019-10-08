using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace CPRTutor
{
   
    public class MyoViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #region Variables

        MyoManagerClass myoManager = new MyoManagerClass();
        
        private bool _activateMyo = false;
        public bool ActivateMyo
        {
            get { return _activateMyo; }
            set
            {
                _activateMyo = value;
                NotifyPropertyChanged();
            }
        }
        private string _debugText = " ";
        /// <summary>
        /// Text messages that need to be displayed in tge view
        /// </summary>
        public string DebugText
        {
            get { return _debugText; }
            set
            {
                    _debugText = value;
                    NotifyPropertyChanged();
                    //UpdateDebug(_debugText);
            }
        }

        /*Orientation TempValues of the Myo*/
        private float _orientationW = 0;
        public float OrientationW
        {
            get { return _orientationW; }
            set
            {
                _orientationW = value;
                NotifyPropertyChanged();
            }
        }
        private float _orientationX = 0;
        public float OrientationX
        {
            get { return _orientationX; }
            set
            {
                _orientationX = value;
                NotifyPropertyChanged();
            }
        }
        private float _orientationY = 0;
        public float OrientationY
        {
            get { return _orientationY; }
            set
            {
                _orientationY = value;
                NotifyPropertyChanged();
            }
        }
        private float _orientationZ = 0;
        public float OrientationZ
        {
            get { return _orientationZ; }
            set
            {
                _orientationZ = value;
                NotifyPropertyChanged();
            }
        }
        /*Accelerometer TempValues of the Myo*/
        private float _accelerometerX = 0;
        public float AccelerometerX
        {
            get { return _accelerometerX; }
            set
            {
                _accelerometerX = value;
                NotifyPropertyChanged();
            }
        }
        private float _accelerometerY = 0;
        public float AccelerometerY
        {
            get { return _accelerometerY; }
            set
            {
                _accelerometerY = value;
                NotifyPropertyChanged();
            }
        }
        private float _accelerometerZ = 0;
        public float AccelerometerZ
        {
            get { return _accelerometerZ; }
            set
            {
                _accelerometerZ = value;
                NotifyPropertyChanged();
            }
        }

        /*Gryroscope TempValues of the Myo*/
        private float _gyroscopeX = 0;
        public float GyroscopeX
        {
            get { return _gyroscopeX; }
            set
            {
                _gyroscopeX = value;
                NotifyPropertyChanged();
            }
        }
        private float _gyroscopeY = 0;
        public float GyroscopeY
        {
            get { return _gyroscopeY; }
            set
            {
                _gyroscopeY = value;
                NotifyPropertyChanged();
            }
        }
        private float _gyroscopeZ = 0;
        public float GyroscopeZ
        {
            get { return _gyroscopeZ; }
            set
            {
                _gyroscopeZ = value;
                NotifyPropertyChanged();
            }
        }

        
        //private int _gripPressure = 0;
        ///// <summary>
        ///// Pressure with which the user holds the pen. The value is calculated using EMG and returns
        ///// a value between 1-7, which represents the state of each EMG pod. 
        ///// </summary>
        //public int GripPressure
        //{
        //    get { return _gripPressure; }
        //    set
        //    {
        //        if (value != this._gripPressure)
        //        {
        //            _gripPressure = value;
        //            NotifyPropertyChanged();
        //            //UpdateGripPressure(_gripPressure);
        //        }
        //    }
        //}

        //private string _buttonText = "Start Recording";
        //public string ButtonText
        //{
        //    get { return _buttonText; }
        //    set
        //    {
        //        if (value != this._buttonText)
        //        {
        //            _buttonText = value;
        //            NotifyPropertyChanged();
        //        }
        //    }
        //}
        //private SolidColorBrush _buttonBrush =new SolidColorBrush(Colors.White);
        //public SolidColorBrush ButtonBrush
        //{
        //    get { return _buttonBrush; }
        //    set
        //    {
        //        if (value != this._buttonBrush)
        //        {
        //            _buttonBrush = value;
        //            NotifyPropertyChanged();
        //        }
        //    }
        //}

        /// <summary>
        /// Property which holds the time for the timer
        /// </summary>
        private DateTime TimerStart { get; set; }
        #endregion

        //ConnectorHub.ConnectorHub myConnector;
        //ConnectorHub.FeedbackHub myFeedback;
        public MyoViewModel()
        {
            this.TimerStart = DateTime.Now;
            //myConnector = new ConnectorHub.ConnectorHub();
            //myFeedback = new ConnectorHub.FeedbackHub();
            //myConnector.init();
            //myFeedback.init();
            //myConnector.sendReady();
            //myConnector.startRecordingEvent += MyConnector_startRecordingEvent;
            //myConnector.stopRecordingEvent += MyConnector_stopRecordingEvent;
            //myFeedback.feedbackReceivedEvent += MyFeedback_feedbackReceivedEvent;

            myoManager.AccelerometerChanged += UpdateAccelerometer;
            myoManager.GyroscopeChanged += UpdateGyroscope;
            myoManager.OrientationChanged += UpdateOrientation;
            myoManager.EMGChanged += UpdateEMG;

            //setValueNames();

        }

        #region Events handlers

        /// <summary>
        /// When message is recieved from the learning hub
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="feedback"></param>
        //private void MyFeedback_feedbackReceivedEvent(object sender, string feedback)
        //{
        //    ReadStream(feedback);
        //}

        /// <summary>
        /// Stop recording message received
        /// </summary>
        /// <param name="sender"></param>
        private void MyConnector_stopRecordingEvent(object sender)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(
                        () =>
                        {
                            MainWindow.isRecording = true;
                            this.StartRecordingData();
                        }));
        }

        /// <summary>
        /// start recording message received
        /// </summary>
        /// <param name="sender"></param>
        private void MyConnector_startRecordingEvent(object sender)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(
                        () =>
                        {
                            MainWindow.isRecording = false;
                            this.StartRecordingData();
                        }));
        }

        /// <summary>
        /// Read the string message received
        /// </summary>
        /// <param name="s"></param>
        //private void ReadStream(String s)
        //{
        //    //if the string contains Myo vibrate myo
        //    if (s.Contains("Myo"))
        //    {
        //        MyoManagerClass.pingMyo();

        //    }

        //}

        //private ICommand _buttonClicked;

        /// <summary>
        /// When the record button is manually clicked
        /// </summary>
        //public ICommand ButtonClicked
        //{
        //    get
        //    {
        //        if (_buttonClicked == null)
        //        {
        //            _buttonClicked = new RelayCommand(
        //                param => this.StartRecordingData(),
        //                null
        //                );
        //        }

        //        return _buttonClicked;
        //    }
        //}

        /// <summary>
        /// Eventhandler for when Orientation data is received
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="o"></param>
        private void UpdateOrientation(object sender, MyoManagerClass.OrientationChangedEventArgs o)
        {
            OrientationW = o.OrientationW;
            OrientationX = o.OrientationX;
            OrientationY = o.OrientationY;
            OrientationZ = o.OrientationZ;
            if (MainWindow.isRecording == true)
            {
                SendDataAsync();
            }
        }

        /// <summary>
        /// Eventhandler for when the Gyroscope data is received
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateGyroscope(object sender, MyoManagerClass.GyroscopeChangedEventArgs e)
        {
            GyroscopeX = e.gyroscopeX;
            GyroscopeY = e.gyroscopeY;
            GyroscopeZ = e.gyroscopeZ;
            if (MainWindow.isRecording == true)
            {
                SendDataAsync();
            }
        }

        /// <summary>
        /// Eventhandler for when the Accelerometer data is received
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="a"></param>
        private void UpdateAccelerometer(object sender, MyoManagerClass.AccelerometerChangedEventArgs a)
        {
            AccelerometerX = a.accelerometerX;
            AccelerometerY = a.accelerometerY;
            AccelerometerZ = a.accelerometerZ;
            if (MainWindow.isRecording == true)
            {
                SendDataAsync();
            }
        }

        /// <summary>
        /// Temporary holders for emg data to be stored 
        /// </summary>
        //List<double> EMGPod0data = new List<double>();
        //List<double> EMGPod1data = new List<double>();
        //List<double> EMGPod2data = new List<double>();
        //List<double> EMGPod3data = new List<double>();
        //List<double> EMGPod4data = new List<double>();
        //List<double> EMGPod5data = new List<double>();
        //List<double> EMGPod6data = new List<double>();
        //List<double> EMGPod7data = new List<double>();
        /// <summary>
        /// holder for average emg data of each second
        /// </summary>
        double[] EMGdata = new double[8];

        /// <summary>
        /// Eventhandler for when the EMG data is received
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateEMG(object sender, MyoManagerClass.EMGChangedEventArgs e)
        {
            //when the timepsan is more than 0.5 secs
            //if((DateTime.Now - TimerStart).Seconds > 0.5)
            //{
                //take the root mean square of whole array and then store it into the master emg data holder
                EMGdata[0] = e.EMGPod_0;
                EMGdata[1] = e.EMGPod_1;
                EMGdata[2] = e.EMGPod_2;
                EMGdata[3] = e.EMGPod_3;
                EMGdata[4] = e.EMGPod_4;
                EMGdata[5] = e.EMGPod_5;
                EMGdata[6] = e.EMGPod_6;
                EMGdata[7] = e.EMGPod_7;
                //calculate grippressure with the master emg data holder
                //CalculateGripPressureAsync(EMGdata, 15);
                //debud all the emd data
                //for(int i = 0; i < EMGdata.Count()-1; i++)
                //{
                //    //Debug.WriteLine("EMGdata " + i + " = " + EMGdata[i]);
                //}

                if (MainWindow.isRecording == true)
                {
                    SendDataAsync();
                }
                ///clear the list that were holding the previous data
                //TimerStart = DateTime.Now;
                //EMGPod0data.Clear();
                //EMGPod1data.Clear();
                //EMGPod2data.Clear();
                //EMGPod3data.Clear();
                //EMGPod4data.Clear();
                //EMGPod5data.Clear();
                //EMGPod6data.Clear();
                //EMGPod7data.Clear();

        }

        /// <summary>
        /// calculate the RootMeanSquare from the array
        /// </summary>
        /// <param name="doubleList"></param>
        /// <returns></returns>
        //private double RootMeanSquare(double[] doubleList)
        //{
        //    double sum = 0;
        //    for (int i = 0; i < doubleList.Length; i++)
        //    {
        //        sum += (doubleList[i] * doubleList[i]);
        //    }
        //    return Math.Sqrt(sum / doubleList.Length);
        //}

        #endregion

        #region event
        public event EventHandler<ValuesChangedEventArgs> ValuesChanged;
        protected virtual void OnValuesChanged(ValuesChangedEventArgs ValuesEvent)
        {
            EventHandler<ValuesChangedEventArgs> handler = ValuesChanged;
            if (handler != null)
            {
                handler(this, ValuesEvent);
            }
        }

        public class ValuesChangedEventArgs : EventArgs
        {
            public List<string> values {get; set;}
        }

        #endregion

        #region UI
        /// <summary>
        /// Method called when recording is started
        /// </summary>
        public void StartRecordingData()
        {
            Debug.WriteLine("ButtonClicked" + MainWindow.isRecording);
            if (MainWindow.isRecording == false)
            {
                MainWindow.isRecording = true;
                //ButtonText = "Stop Recoding";
                //ButtonBrush = new SolidColorBrush(Colors.Green);

            }
            else if (MainWindow.isRecording == true)
            {
                MainWindow.isRecording = false;
                //ButtonText = "Start Recoding";
                //ButtonBrush = new SolidColorBrush(Colors.White);
            }
        }

        #endregion

        #region Send data
        /// <summary>
        /// set the names of the TempValues that needs to recorded in the learning hub
        /// </summary>
        private void setValueNames()
        {
                try
                {
                    List<string> names = new List<string>();
                    names.Add("OrientationW");
                    names.Add("OrientationX");
                    names.Add("OrientationY");
                    names.Add("OrientationZ");
                    names.Add("AccelerometerX");
                    names.Add("AccelerometerY");
                    names.Add("AccelerometerZ");
                    names.Add("GyroscopeX");
                    names.Add("GyroscopeY");
                    names.Add("GyroscopeZ");
                    names.Add("GripPressure");
                    names.Add("EMGPod_0");
                    names.Add("EMGPod_1");
                    names.Add("EMGPod_2");
                    names.Add("EMGPod_3");
                    names.Add("EMGPod_4");
                    names.Add("EMGPod_5");
                    names.Add("EMGPod_6");
                    names.Add("EMGPod_7");

                    //myConnector.setValuesName(names);
                }
                catch (Exception ex)
                {
                    if (DebugText != ex.ToString())
                    {
                        DebugText = ex.ToString();
                    }
                }

        }
        /// <summary>
        /// method for sending data Async
        /// </summary>
        public async void SendDataAsync()
        {
            await Task.Run(()=> SendData());
        }
        /// <summary>
        /// Method to send the data to the learning hub
        /// </summary>
        private void SendData()
        {
            try
            {
                List<string> TempValues = new List<string>();
                TempValues.Add(OrientationW.ToString());
                TempValues.Add(OrientationX.ToString());
                TempValues.Add(OrientationY.ToString());
                TempValues.Add(OrientationZ.ToString());
                TempValues.Add(AccelerometerX.ToString());
                TempValues.Add(AccelerometerY.ToString());
                TempValues.Add(AccelerometerZ.ToString());
                TempValues.Add(GyroscopeX.ToString());
                TempValues.Add(GyroscopeY.ToString());
                TempValues.Add(GyroscopeZ.ToString());
                //TempValues.Add(GripPressure.ToString());
                TempValues.Add(EMGdata[0].ToString());
                TempValues.Add(EMGdata[1].ToString());
                TempValues.Add(EMGdata[2].ToString());
                TempValues.Add(EMGdata[3].ToString());
                TempValues.Add(EMGdata[4].ToString());
                TempValues.Add(EMGdata[5].ToString());
                TempValues.Add(EMGdata[6].ToString());
                TempValues.Add(EMGdata[7].ToString());

                ValuesChangedEventArgs e = new ValuesChangedEventArgs();
                e.values = TempValues;
                OnValuesChanged(e);
            }
            catch (Exception ex)
            {
                //Debug.WriteLine(ex.StackTrace);
                if (DebugText != ex.ToString())
                {
                    DebugText = ex.ToString();
                }
            }

        }
        #endregion

        /// <summary>
        /// Calculate the GripPressure async
        /// </summary>
        /// <param name="currentEmgValues"></param>
        /// <param name="emgThreshold"></param>
        //private async void CalculateGripPressureAsync(double[] currentEmgValues, double emgThreshold)
        //{
        //    await Task.Run(() => CalculateGripPressure(currentEmgValues, emgThreshold));
        //}

        /// <summary>
        /// Method for calculating the GripPressure. Iterate through each emg sensor pod in myo and assign 1 if RMS value is more than the threshold,
        /// else assign 0. When the RMS value of the Emg pod is more than threshold it means that the the emg recorded higher musvle potential. 
        /// </summary>
        /// <param name="e"></param>
        //private void CalculateGripPressure(double[] currentEmgValues, double emgThreshold)
        //{
        //    //temp array that holds 1 or 0 for each of the myo pod to be added at the last
        //    int[] emgTension = new int[8];
        //    //temporary value that holds the value of grip pressure 
        //    int gripEMG = 0;

        //    //iterate through all the emg pods and store 1/0  in emgTension[] depending on 
        //    //if the RMS is more or less than threshold
        //    // 0 meaning no tension and 100 meaning lots of tension
        //    for (int i = 0; i <= 7; i++)
        //    {
        //        try
        //        {
        //            if (currentEmgValues[i] >= emgThreshold)
        //            {
        //                emgTension[i] = 1;

        //            }
        //            else
        //            {
        //                emgTension[i] = 0;
        //            }

        //        }
        //        catch
        //        {
        //            Debug.WriteLine("Error Calculating GripPressure");
        //        }
        //    }

        //    //add all value from emgTension and assign it to gripEmg
        //    Array.ForEach(emgTension, delegate (int i) { gripEMG += i; });
        //    //assign it to grippressure
        //    GripPressure = gripEMG;
        //    //if the grip pressure is more than 5, or more than 5 pods return high potential readings
        //    if (gripEMG >=4)
        //    {
        //        //vibrate myo
        //        if (ActivateMyo)
        //        {
        //            MyoManagerClass.pingMyo();
        //        }
        //        Debug.WriteLine("GripPressure = " + gripEMG);
        //    }
        //}

    }
}
