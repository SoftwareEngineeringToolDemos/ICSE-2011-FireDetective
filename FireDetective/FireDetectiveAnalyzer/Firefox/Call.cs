using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FireDetectiveAnalyzer.Firefox
{
    public class Call
    {
        private int m_RecursiveCount = -1;
        private List<Call> m_SubCalls = new List<Call>();
        public Script Script { get; set; }
        public Call CallSite { get; private set; }
        public int CallSiteLine { get; private set; }
        public int ReturnLine { get; set; }

        public IEnumerable<Call> SubCalls { get { return m_SubCalls; } }
        public int SubCallsCount { get { return m_SubCalls.Count; } }
        public bool IsFiltered { get; set; }

        public List<Xhr> Xhrs { get; set; }
        public List<IntervalTimeout> IntervalTimeouts { get; set; }

        public Xhr XhrInfo { get; set; }
        public IntervalTimeout IntervalTimeoutInfo { get; set; }

        public bool IsHighlighted { get; set; }

        public Call(Script script)
        {
            Script = script;
        }

        public static Call CreateTopLevel(Script script)
        {
            Call call = new Call(script);
            call.Xhrs = new List<Xhr>();
            call.IntervalTimeouts = new List<IntervalTimeout>();
            return call;
        }

        public Call Clone()
        {
            Call call = new Call(Script);
            call.CallSite = CallSite;
            call.CallSiteLine = CallSiteLine;
            call.ReturnLine = ReturnLine;
            call.IsFiltered = IsFiltered;
            call.Xhrs = Xhrs;
            call.IntervalTimeouts = IntervalTimeouts;
            call.XhrInfo = XhrInfo;
            call.IntervalTimeoutInfo = IntervalTimeoutInfo;
            return call;
        }

        public int RecursiveCount
        {
            get
            {
                if (m_RecursiveCount >= 0) return m_RecursiveCount;
                m_RecursiveCount = 1 + SubCalls.Sum(call => call.RecursiveCount);
                return m_RecursiveCount;
            }
        }

        public void AddSubCall(Call call, int callSiteLine)
        {
            m_SubCalls.Add(call);
            call.NotifyCallSiteChanged(this, callSiteLine);
        }

        public int IndexOfSubCall(Call call)
        {
            return m_SubCalls.IndexOf(call);
        }

        public IEnumerable<Call> Find(Func<Call, bool> func)
        {
            if (func(this))
                yield return this;
            else
                foreach (Call c in m_SubCalls.SelectMany(c => c.Find(func)))
                    yield return c;
        }

        private void NotifyCallSiteChanged(Call callSite, int callSiteLine)
        {
            CallSite = callSite;
            CallSiteLine = callSiteLine;
        }

        public string DisplayName 
        { 
            get 
            {
                return IsFiltered ? "<filtered>" : Script.Name;
            }
        }

        public string TopLevelDisplayName
        {
            get
            {
                return string.Format("{0} [{1}]", DisplayName, RecursiveCount) + 
                    (!Script.IsNative && !IsFiltered ? string.Format(" ({0})", Script.DefinitionContentItem.ShortUrl) : "");
            }
        }

        public Call GetTopLevel()
        {
            Call call = this;
            while (call.CallSite != null) call = call.CallSite;
            return call;
        }

        public List<Call> GetStack()
        {
            List<Call> result = new List<Call>();
            Call call = this;
            do
            {
                result.Add(call);
                call = call.CallSite;
            }
            while (call != null);
            result.Reverse();
            return result;
        }

        public IEnumerable<Call> GetScriptCalls(Script s)
        {
            if (Script == s)
                yield return this;
            foreach (Call c in SubCalls.SelectMany(sub => sub.GetScriptCalls(s)))
                yield return c;
        }

        public bool ShouldKeep()
        {
            return !(m_SubCalls.Count > 0 && m_SubCalls.First().Script.Name == "wf_beginOverride");
        }

        public Call FilterDojoJQuery(bool fromOk, Call parent)
        {
            bool ok = !IsDojoJQuery();
            
            Call c, p;
            if (ok)
            {
                c = this.Clone();
                p = c;
            }
            else if (fromOk)
            {
                c = this.Clone();
                p = null;
            }
            else if (parent == null)
            {
                c = this.Clone();
                c.IsFiltered = true;
                p = c;
            }
            else
            {
                c = parent;
                p = parent;
            }

            foreach (Call z in this.SubCalls.Select(call => call.FilterDojoJQuery(ok, p)).Where(call => call != null))
                if (c != z)
                    c.AddSubCall(z, c.IsFiltered ? -1 : z.CallSiteLine);

            if (c.IsFiltered && c.SubCallsCount == 0)
                return null;
            else
                return c;
        }

        private bool IsDojoJQuery()
        {
            return Script != null && Script.IsDojoJQuery;
        }
    }
}
