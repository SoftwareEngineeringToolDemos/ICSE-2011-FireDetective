using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FireDetectiveAnalyzer.Firefox
{
    public class Section
    {
        private List<Event> m_EventsUnSafe = new List<Event>();
        private Event m_CurrentEvent = null;
        private Event m_EventForCurrentTopLevelCall = null;
        public bool Marked { get; set; }

        public Section(bool marked)
        {
            Marked = marked;
        }

        public bool IsEmptySafe
        {
            get
            {
                lock (m_EventsUnSafe)
                {
                    return m_EventsUnSafe.Count == 0;
                }
            }
        }

        public void StartEventSafe(Event e)
        {
            lock (m_EventsUnSafe)
            {
                m_CurrentEvent = e;
                m_EventsUnSafe.Add(e);
            }
        }

        public IEnumerable<Event> EventsSafe
        {
            get
            {
                lock (m_EventsUnSafe)
                {
                    return new List<Event>(m_EventsUnSafe);
                }
            }
        }

        public void BeginTopLevelCallSafe()
        {
            lock (m_EventsUnSafe)
            {
                if (m_CurrentEvent == null)
                {
                    // No need to watch out, acquires same lock we already have, no problem.
                    StartEventSafe(new Event("<unknown>", "<unknown>"));
                }

                m_EventForCurrentTopLevelCall = m_CurrentEvent;                
            }
        }

        public void FinishTopLevelCallSafe(Call topLevelCall)
        {
            lock (m_EventsUnSafe)
            {
                // Careful here, calling "Safe" function which means another lock is being acquired inside the current lock.
                m_EventForCurrentTopLevelCall.AddTopLevelCallSafe(topLevelCall);
                m_EventForCurrentTopLevelCall = null;
            }
        }

    }
}
