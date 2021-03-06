﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPRTutor
{
    public class AnnotationObject
    {
        public string RecordingID { get; set; }
        public string ApplicationName { get; set; }

        public List<IntervalObject> Intervals { get; set; }

        public AnnotationObject()
        {
            Intervals = new List<IntervalObject>();
        }
    }

    public class IntervalObject
    {
        public System.TimeSpan start;
        public System.TimeSpan end;
        public Dictionary<string, string> annotations;

        public IntervalObject(System.TimeSpan startInterval, System.TimeSpan endInterval)
        {
            annotations = new Dictionary<string, string>();
            this.start = startInterval;
            this.end = endInterval;

        }


        public void setAnnotations(List<string> annotationNames, List<string> annotationValues)
        {
            for (int i = 0; i < annotationNames.Count; i++)
            {
                annotations.Add(annotationNames[i], annotationValues[i]);
            }
        }

    }
}