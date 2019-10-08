using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPRTutor
{
    public class RecordingObject
    {
        public string RecordingID { get; set; }
        public string ApplicationName { get; set; }
        public string OenName { get; set; }

        public List<FrameObject> Frames { get; set; }

        public RecordingObject()
        {
            Frames = new List<FrameObject>();
        }
    }
}