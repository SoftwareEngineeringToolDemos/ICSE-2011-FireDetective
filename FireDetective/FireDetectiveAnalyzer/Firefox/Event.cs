using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FireDetectiveAnalyzer.Firefox
{
    public class Event
    {
        public string Name { get; set; }
        public string TargetType { get; set; }
        public Xhr Xhr { get; set; }
        public IntervalTimeout IntervalTimeout { get; set; }

        public string DisplayName { get { return TargetType != "" ? TargetType + "." + Name : Name; } }

        private List<TopLevelCall> m_TopLevelCallsUnSafe = new List<TopLevelCall>();
        private volatile int m_TopLevelCallsCountAll = 0;
        private volatile int m_TopLevelCallsCountFiltered = 0;

        public Event(string name, string targetType)
        {
            Name = name;
            TargetType = targetType;
        }

        public Event(string name, Xhr xhr)
            : this(name, "")
        {
            Xhr = xhr;
        }

        public Event(string name, IntervalTimeout it)
            : this(name, "")
        {
            IntervalTimeout = it;
        }

        public bool HasTopLevelCallsSafe(bool filter)
        {
            return (filter ? m_TopLevelCallsCountFiltered : m_TopLevelCallsCountAll) > 0;
        }

        public int GetTopLevelCallsCountSafe(bool filter)
        {
            return filter ? m_TopLevelCallsCountFiltered : m_TopLevelCallsCountAll;
        }

        public List<TopLevelCall> GetTopLevelCallsSafe(bool filter)
        {
            lock (m_TopLevelCallsUnSafe)
            {
                return new List<TopLevelCall>(filter ? m_TopLevelCallsUnSafe.Where(top => top.HasFilteredVersion) : m_TopLevelCallsUnSafe);
            }
        }

        public void AddTopLevelCallSafe(Call call)
        {
            lock (m_TopLevelCallsUnSafe)
            {
                var top = new TopLevelCall(call);
                m_TopLevelCallsUnSafe.Add(top);
                m_TopLevelCallsCountAll++;
                m_TopLevelCallsCountFiltered += top.HasFilteredVersion ? 1 : 0;
            }
        }
    }
}
