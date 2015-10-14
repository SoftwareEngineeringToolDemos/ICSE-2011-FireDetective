using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FireDetectiveAnalyzer.Java
{
    public class Call
    {
        private int m_RecursiveCount = -1;
        public Method Method { get; set; }
        public Call CallSite { get; set; }
        public int CallSiteLine { get; set; }
        private List<Call> m_SubCalls = new List<Call>();

        public IEnumerable<Call> SubCalls { get { return m_SubCalls; } }
        public int SubCallsCount { get { return m_SubCalls.Count; } }

        public bool IsHighlighted { get; set; }
        public bool IsMasked { get; set; }

        public Call(Method method)
        {
            Method = method;
        }

        public static Call CreateMasked()
        {
            Call result = new Call(null);
            result.IsMasked = true;
            return result;
        }

        public Call Clone()
        {
            Call call = new Call(Method);
            call.CallSite = CallSite;
            call.CallSiteLine = CallSiteLine;
            return call;
        }

        public string DisplayName
        {
            get
            {
                return IsMasked ? "<filtered>" : 
                    (Method == null ? "<java code>" : 
                    (Method.IsJspExecution ? "<executing: " + Method.DefinitionFileJsp.Name + ">" : Method.ClassName + "." + Method.Name));
            }
        }

        public void AddSubCall(Call call, int callSiteLine)
        {
            m_SubCalls.Add(call);
            call.NotifyCallSiteChanged(this, callSiteLine);
        }

        private void NotifyCallSiteChanged(Call callSite, int callSiteLine)
        {
            CallSite = callSite;
            CallSiteLine = callSiteLine;
        }

        public int Count
        {
            get
            {
                if (m_RecursiveCount >= 0) return m_RecursiveCount;
                m_RecursiveCount = 1 + SubCalls.Sum(call => call.Count);
                return m_RecursiveCount;
            }
        }
        
        public void PruneMaskedLeaves()
        {
            m_SubCalls.RemoveAll(sub => sub.IsMasked && sub.SubCallsCount == 0);
            foreach (Call sub in SubCalls) sub.PruneMaskedLeaves();
        }

        public void PruneMaskedJspExecutions()
        {
            if (Method != null && Method.IsJspExecution)
            {
                var indices = m_SubCalls.FindIndices(sub => sub.IsMasked && sub.SubCallsCount == 1 && sub.SubCalls.First().Method != null && sub.SubCalls.First().Method.IsJspExecution);
                foreach (int i in indices)
                {
                    m_SubCalls[i] = m_SubCalls[i].SubCalls.First();
                    m_SubCalls[i].NotifyCallSiteChanged(this, 0);
                }
            }
            foreach (Call sub in SubCalls) sub.PruneMaskedJspExecutions();
        }

        public void PruneEndTraceException()
        {
            var last = m_SubCalls.LastOrDefault();
            if (last != null && last.Method != null && !last.Method.IsSourceAvailable)
                m_SubCalls.Remove(last);
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

        public IEnumerable<Call> Find(Func<Call, bool> func)
        {
            if (func(this))
                yield return this;
            foreach (Call c in SubCalls.SelectMany(sub => sub.Find(func)))
                yield return c;
        }
    }
}
