using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FireDetectiveAnalyzer.Firefox
{
    public class PageRequest
    {
        public SimpleUri Location { get; set; }
        public DateTime Date { get; set; }

        private Dictionary<int, ContentItem> m_ContentItems = new Dictionary<int, ContentItem>();
        private Dictionary<string, ContentItem> m_ContentItemsByUrl = new Dictionary<string, ContentItem>();
        private Dictionary<int, Script> m_Scripts = new Dictionary<int, Script>();
        private Dictionary<int, IntervalTimeout> m_IntervalTimeouts = new Dictionary<int, IntervalTimeout>();
        
        private Dictionary<int, ContentItem> m_ContentItemsUnSafe = new Dictionary<int, ContentItem>();
        private List<Section> m_SectionsUnSafe = new List<Section>();

        public HashSet<Java.JavaFile> JavaFilesHit { get; private set; }

        private int m_FirstContentItemId = -1;

        public PageRequest()
        {
            JavaFilesHit = new HashSet<Java.JavaFile>();
        }

        public void AddContentItemWithoutUrl(int id, ContentItem ci)
        {
            m_ContentItems.Add(id, ci);
            if (m_FirstContentItemId < 0)
                m_FirstContentItemId = id;

            // Also update thread safe list
            AddContentItemSafe(id, ci);
        }

        public void NotifyContentItemHasUrl(int id, ContentItem ci)
        {
            if (!m_ContentItems.ContainsKey(id))
                throw new ApplicationException("There is no content item for this id.");
            
            // We know the final url, add the content item by name as well.
            // There may be duplicates, ignore these in this case.
            if (!m_ContentItemsByUrl.ContainsKey(ci.Url.ToString()))
                m_ContentItemsByUrl.Add(ci.Url.ToString(), ci); 
        }

        public void AddScript(int id, Script script)
        {
            m_Scripts.Add(id, script);
        }

        public Script GetScriptById(int id)
        {
            return m_Scripts[id];
        }

        public bool HasScriptWithId(int id)
        {
            return m_Scripts.ContainsKey(id);
        }

        public ContentItem GetContentItemById(int id)
        {
            return m_ContentItems[id];
        }

        public bool HasContentItemWithId(int id)
        {
            return m_ContentItems.ContainsKey(id);
        }

        public ContentItem GetContentItemByUrl(string url)
        {
            return m_ContentItemsByUrl[url];
        }

        public bool HasContentItemWithUrl(string url)
        {
            return m_ContentItemsByUrl.ContainsKey(url);
        }

        // Safe content item access

        private void AddContentItemSafe(int id, ContentItem ci)
        {
            lock (m_ContentItemsUnSafe)
            {
                m_ContentItemsUnSafe.Add(id, ci);
            }
        }

        public IEnumerable<ContentItem> ContentItemsSafe
        {
            get
            {
                lock (m_ContentItemsUnSafe)
                {
                    return new List<ContentItem>(m_ContentItemsUnSafe.Values);
                }
            }
        }

        public MatchContentItemServerTraceResult MatchContentItemServerTrace(Java.TopLevelTrace st)
        {
            if (m_FirstContentItemId < 0) return MatchContentItemServerTraceResult.Unknown;
            if (st.RequestId < m_FirstContentItemId) return MatchContentItemServerTraceResult.IsHistory;
            lock (m_ContentItemsUnSafe)
            {
                if (m_ContentItemsUnSafe.ContainsKey(st.RequestId))
                {
                    st.ResolveTo(m_ContentItemsUnSafe[st.RequestId], JavaFilesHit);
                    return MatchContentItemServerTraceResult.Matched;
                }
                else
                {
                    return MatchContentItemServerTraceResult.IsFuture;
                }
            }
        }

        // Safe section access
        
        private Section m_CurrentSection = null;

        public void StartSectionSafe(bool marked)
        {
            lock (m_SectionsUnSafe)
            {
                m_CurrentSection = new Section(marked);
                m_SectionsUnSafe.Add(m_CurrentSection);
            }
        }

        public IEnumerable<Section> SectionsSafe
        {
            get
            {
                lock (m_SectionsUnSafe)
                {
                    return new List<Section>(m_SectionsUnSafe);
                }
            }
        }

        // Safe event access

        public void StartEventSafe(string name, string targetType)
        {
            m_CurrentSection.StartEventSafe(new Event(name, targetType));
        }

        public void StartXhrEventSafe(string name, int id)
        {
            Xhr xhr = m_ContentItems[id].Xhr;
            Event e = new Event(name, xhr);
            xhr.AddReadyStateChangeEvent(e);
            m_CurrentSection.StartEventSafe(e);
        }

        public void StartIntervalTimeoutEventSafe(string name, int id)
        {
            IntervalTimeout it = m_IntervalTimeouts[id];
            Event e = new Event(name, it);
            it.Events.Add(e);
            m_CurrentSection.StartEventSafe(e);
        }

        public IEnumerable<Event> EventsSafe
        {
            get
            {
                lock(m_SectionsUnSafe)
                {
                    // Careful here, calling "Safe" functions which means other locks are being acquired inside the current lock.
                    return m_SectionsUnSafe.SelectMany(section => section.EventsSafe);
                }
            }
        }

        private void TopLevelCallFinished(Call topLevelCall)
        {
            m_CurrentSection.FinishTopLevelCallSafe(topLevelCall);
        }

        // Building top-level calls

        private Call m_CurrentTopLevel;
        private Call m_Current;

        public void AddCallApplyDelta(int id, int line, int callerId, int callerLine)
        {
            Script s = m_Scripts[id];
            if (m_CurrentTopLevel != null)
            {
                Call call = new Call(s);
                if (callerId >= 0)
                {
                    Script caller = m_Scripts[callerId];
                    if (caller != m_Current.Script)
                        throw new ApplicationException("Firefox corrupted call stack.");
                    /*{
                        Call stub = Call.CreateStub("...", false);
                        Call nextStub = Call.CreateStub("", false);
                        AddCall(stub, -1);
                        nextStub.Script = caller;
                        AddCall(nextStub, -1);
                    }*/
                    AddCall(call, callerLine + caller.LineDelta);
                }
                else
                    AddCall(call, -1);
            }
            else
            {
                m_CurrentTopLevel = Call.CreateTopLevel(s);
                m_Current = m_CurrentTopLevel;
                m_CurrentSection.BeginTopLevelCallSafe();
            }

            if (s.IsTopLevelEval && s.DefinitionContentItem.Xhr != null)
            {
                AddCallInfoEvalXhr(s.DefinitionContentItem.Xhr);
            }
        }

        public void PopCallApplyDelta(int id, int line)
        {
            if (m_Current.Script != m_Scripts[id]) throw new ApplicationException("Wrong return script.");
            m_Current.ReturnLine = line + m_Current.Script.LineDelta;
            PopCall();
        }

        private void AddCall(Call call, int line)
        {
            if (call.CallSite != null) throw new ApplicationException("Callsite already set");
            m_Current.AddSubCall(call, line);
            m_Current = call;
        }

        private void PopCall()
        {
            m_Current = m_Current.CallSite;
            //while (m_Current != null && m_Current.IsStub) m_Current = m_Current.CallSite;
            if (m_Current == null)
            {
                if (m_CurrentTopLevel.ShouldKeep())
                    TopLevelCallFinished(m_CurrentTopLevel);
                m_CurrentTopLevel = null;
            }
        }

        /*public void MarkXhrCall(ContentItem ci)
        {
            m_CurrentTopLevel.Xhrs.Add(ci);
            /*m_CurrentRoot.AddXhr(ci);
            ci.XhrCall = Call.CreateXhr(ci);
            AddCall(ci.XhrCall, -1);
            PopCall();
        }*/

        /*public void MarkDomNodeModification(int id)
        {
            if (m_CurrentRoot == null) return; // TODO! Do not ignore in between dom mods
            AddCall(Call.CreateNodeModification(id), -1);
            PopCall();
        }*/

        public void AddCallInfoSetIntervalTimeout(int id, IntervalTimeout it)
        {
            m_Current.IntervalTimeoutInfo = it;
            it.SetCall = m_Current;
            m_IntervalTimeouts.Add(id, it);
            m_CurrentTopLevel.IntervalTimeouts.Add(it);
        }

        public void AddCallInfoClearIntervalTimeout(int id)
        {
            IntervalTimeout it = m_IntervalTimeouts[id];
            m_Current.IntervalTimeoutInfo = it;
            it.ClearCall = m_Current;
        }

        public void AddCallInfoSendXhr(ContentItem ci)
        {
            Xhr xhr = new Xhr(ci);
            m_Current.XhrInfo = xhr;
            xhr.SetSend(m_Current);
            m_CurrentTopLevel.Xhrs.Add(xhr);
            ci.Xhr = xhr;
        }

        public void AddCallInfoAccessXhr(int id)
        {
            Xhr xhr = m_ContentItems[id].Xhr;
            m_Current.XhrInfo = xhr;
            xhr.AddAccess(m_Current);            
        }

        public void AddCallInfoEvalXhr(Xhr xhr)
        {
            m_Current.XhrInfo = xhr;
            xhr.AddEval(m_Current);
        }
    }

    public class PageRequestCollection : List<PageRequest>
    {
        public bool MatchContentItemServerTrace(Java.TopLevelTrace st)
        {
            MatchContentItemServerTraceResult result = MatchContentItemServerTraceResult.IsFuture;
            for (int i = Count - 1; i >= 0; i--)
            {
                result = this[i].MatchContentItemServerTrace(st);
                if (result == MatchContentItemServerTraceResult.Matched || result == MatchContentItemServerTraceResult.IsFuture)
                    break;
            }
            return result != MatchContentItemServerTraceResult.IsFuture;  // Should retry in the future
        }
    }

    public enum MatchContentItemServerTraceResult
    {
        Unknown, Matched, IsHistory, IsFuture
    }
}
