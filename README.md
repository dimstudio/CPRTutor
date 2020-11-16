# The CPR Tutor
The CPR Tutor is real-time multimodal feedback system for cardiopulmonary resuscitation (CPR) training. The CPR Tutor detects mistakes using recurrent neural networks for real-time time-series classification. 

The CPR Tutor is a Kinect-based application which works in conjunction with the Myo armband (https://developerblog.myo.com/).

From a multimodal data stream consisting of kinematic and electromyographic data, the CPR Tutor system automatically detects the chest compressions, which are then classified and assessed according to five performance indicators. 

Based on this assessment, the CPR Tutor provides audio feedback to correct the most critical mistakes and improve the CPR performance. 

The CPR Tutor was trained using Laerdal ResusciAnne QCPR manikin, but can be used with any manikin. 

# Connected software
- *SharpFlow* https://github.com/dimstudio/SharpFlow for the LSTM model training and for the Real-Time classification of the chest compressions 
- *Visual Inspection Tool* https://github.com/dimstudio/visual-inspection-tool for the visualisation and annotation of the sessions 

# Study design
![Study design of the CPR Tutor](https://i.imgur.com/JNbb6d3.jpg)

# Cite this 
- Di Mitri, D., Schneider, J., Trebing, K., Sopka, S., Specht, M., & Drachsler, H. (2020). Real-Time Multimodal Feedback with the CPR Tutor. In I. I. Bittencourt, M. Cukurova, & K. Muldner (Eds.), Artificial Intelligence in Education (AIED’2020) (pp. 141–152). Cham, Switzerland: Springer, Cham. https://doi.org/10.1007/978-3-030-52237-7{\_}12

## Other resources
- Di Mitri, D., Schneider, J., Specht, M., & Drachsler, H. (2019). Detecting mistakes in CPR training with multimodal data and neural networks. Sensors (Switzerland), 19(14), 1–20. https://doi.org/10.3390/s19143099
- Slideshare
https://www2.slideshare.net/dnldimitri/realtime-multimodal-feedback-with-the-cpr-tutor
