using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FireDetectiveAnalyzer.Firefox
{
    public class IntervalTimeout
    {
        public bool IsInterval { get; set; }

        public Call SetCall { get; set; }
        public Call ClearCall { get; set; }
        public List<Event> Events { get; private set; }

        public IntervalTimeout(bool isInterval)
        {
            IsInterval = isInterval;
            Events = new List<Event>();
        }
    }
}
