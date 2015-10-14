using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FireDetectiveAnalyzer.Java
{
    public class TopLevelTrace
    {
        private Dictionary<int, Method> m_Methods = new Dictionary<int, Method>();
        private Call m_Current;

        public int RequestId { get; private set; }
        public JspFile JspFile { get; private set; }
        public Firefox.ContentItem Request { get; private set; }

        public Call Root { get; private set; }
        public Call FilteredRoot { get; private set; }

        public HashSet<JavaFile> JavaFilesHit { get; private set; }

        public bool IsHighlighted { get; set; }

        public TopLevelTrace(int requestId)
        {
            RequestId = requestId;
            m_Current = new Call(null);
            JavaFilesHit = new HashSet<JavaFile>();
        }

        public void SetJspFile(JspFile jsp)
        {
            JspFile = jsp;
            JavaFilesHit.Add(jsp);
        }

        public void AddMethod(int id, Method m)
        {
            m_Methods.Add(id, m);
            if (m.DefinitionFile != null)
                JavaFilesHit.Add(m.DefinitionFile);
            else if (m.DefinitionFileJsp != null)
                JavaFilesHit.Add(m.DefinitionFileJsp);
        }

        public void AddCall(int id, int callerLine)
        {
            Call c = id > 0 ? new Call(m_Methods[id]) : Call.CreateMasked();
            m_Current.AddSubCall(c, callerLine);
            m_Current = c;
        }

        public void PopCall(int line)
        {
            m_Current = m_Current.CallSite;
        }

        public void MarkAsCompleted()
        {
            m_Current.PruneMaskedLeaves();
            m_Current.PruneMaskedJspExecutions();
            m_Current.PruneEndTraceException();
            Root = m_Current;
            FilteredRoot = m_Current; // m_Current.FilterJavaInternalsRoot();
        }

        public void ResolveTo(Firefox.ContentItem request, HashSet<JavaFile> allFiles)
        {
            Request = request;
            request.ServerTrace = this;
            allFiles.UnionWith(JavaFilesHit);
        }

        public Call GetRoot(bool filter)
        {
            return filter ? FilteredRoot : Root;
        }

        public bool IsCompleted { get { return Root != null; } }
    }
}
