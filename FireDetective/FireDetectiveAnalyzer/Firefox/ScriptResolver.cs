using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace FireDetectiveAnalyzer.Firefox
{
    public class ScriptResolver
    {
        private Thread m_Thread;

        private List<Script> m_ScriptQueue = new List<Script>();
        private Dictionary<ContentItem, List<Script>> m_Waiting = new Dictionary<ContentItem, List<Script>>();
        private Dictionary<ContentItem, bool> m_AlreadyDone = new Dictionary<ContentItem, bool>();
        
        private Queue<ContentItem> m_IsReady = new Queue<ContentItem>();
        private AutoResetEvent m_SignalReady = new AutoResetEvent(false);

        public ScriptResolver()
        {
            m_Thread = new Thread(new ThreadStart(() =>
            {
                while (true)
                {
                    m_SignalReady.WaitOne();

                    // Retrieve scripts
                    List<Script> scripts = new List<Script>();
                    lock (this)
                    {
                        // Get ready scripts
                        while (m_IsReady.Count > 0)
                        {
                            ContentItem ci = m_IsReady.Dequeue();
                            m_AlreadyDone.Add(ci, true);
                            if (m_Waiting.ContainsKey(ci))
                            {
                                List<Script> toAdd = m_Waiting[ci];
                                m_Waiting.Remove(ci);
                                scripts.AddRange(toAdd);
                            }
                        }

                        // Get ordinary scripts
                        scripts.AddRange(m_ScriptQueue);
                        m_ScriptQueue.Clear();
                    }

                    // Important to maintain script order!
                    foreach (Script s in scripts)
                    {
                        s.Resolve();
                    }
                } 
            }));
            m_Thread.IsBackground = true;
            m_Thread.Start();
        }

        public void QueueForResolve(Script s)
        {
            lock (this)
            {
                if (m_AlreadyDone.ContainsKey(s.DefinitionContentItem))
                {
                    m_ScriptQueue.Add(s);
                    m_SignalReady.Set();
                }
                else
                {
                    if (!m_Waiting.ContainsKey(s.DefinitionContentItem))
                        m_Waiting.Add(s.DefinitionContentItem, new List<Script>());
                    m_Waiting[s.DefinitionContentItem].Add(s);
                }
            }
        }

        public void NotifyContentItemParsed(ContentItem ci)
        {
            lock (this)
            {
                m_IsReady.Enqueue(ci);
                m_SignalReady.Set();
            }
        }
    }
}
