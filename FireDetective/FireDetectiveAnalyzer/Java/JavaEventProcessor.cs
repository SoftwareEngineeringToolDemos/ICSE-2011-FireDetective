using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace FireDetectiveAnalyzer.Java
{
    public class JavaEventProcessor : IEventProcessor
    {
        private JavaWorkspace m_Workspace;
        private Dictionary<int, TopLevelTrace> m_Requests = new Dictionary<int, TopLevelTrace>();
        private Dictionary<string, Method> m_Methods = new Dictionary<string, Method>();
        private volatile bool m_HasOutstandingRequests = false;

        public event EventHandler<CustomEventArgs<TopLevelTrace>> ContentItemStarted;
        public event EventHandler<CustomEventArgs<TopLevelTrace>> ContentItemCompleted;

        public int Messages = 0;
        public int Bytes = 0;

        public JavaEventProcessor(JavaWorkspace workspace)
        {
            m_Workspace = workspace;
        }

        public void HandleMessage(string message, Func<string> getNextMessage, Func<int, byte[]> getBinaryData)
        {
            Messages++;
            Bytes += message.Length;

            if (message.StartsWith("C,"))
            {
                HandleCall(message);
            }
            else if (message.StartsWith("R,"))
            {
                HandleReturn(message);
            }
            else if (message.StartsWith("METHOD,"))
            {
                HandleMethod(message);
            }
            else if (message.StartsWith("REQUEST,"))
            {
                HandleRequest(message);
            }
            else if (message.StartsWith("REQUEST-DONE,"))
            {
                HandleRequestDone(message);
            }
            else
                throw new ApplicationException("Java: unexpected message '" + message + "'.");
        }

        public bool HasOutstandingRequests { get { return m_HasOutstandingRequests; } }

        private void HandleRequest(string message)
        {
            string[] parts = message.Split(new char[] { ',' }, 3);
            int requestId = int.Parse(parts[1]);
            string jspFile = parts[2];
            m_Requests.Add(requestId, new TopLevelTrace(requestId));
            JspFile jsp = m_Workspace.GetJspFile(jspFile);
            if (jsp != null)
                m_Requests[requestId].SetJspFile(jsp);
            m_HasOutstandingRequests = true;

            if (ContentItemStarted != null)
                ContentItemStarted(this, new CustomEventArgs<TopLevelTrace>(m_Requests[requestId]));
        }

        private void HandleRequestDone(string message)
        {
            string[] parts = message.Split(new char[] { ',' });
            int requestId = int.Parse(parts[1]);
            TopLevelTrace item = m_Requests[requestId];
            item.MarkAsCompleted();
            m_Requests.Remove(requestId);
            m_HasOutstandingRequests = m_Requests.Count > 0;

            if (ContentItemCompleted != null)
                ContentItemCompleted(this, new CustomEventArgs<TopLevelTrace>(item));
        }

        private void HandleMethod(string message)
        {
            string[] parts = message.Split(new char[] { ',' }, 9);
            int requestId = int.Parse(parts[1]);
            int id = int.Parse(parts[2]);
            string packageName = parts[3];
            string className = parts[4];
            string name = parts[5];
            int beginLine = int.Parse(parts[6]) - 1;
            int endLine = int.Parse(parts[7]) - 1;
            string filename = parts[8];
            JavaPackage package = m_Workspace.GetPackage(packageName);
            JavaCodeFile file = package != null ? package.GetFile(filename) : null;
            string methodName = packageName + "." + className + "." + name;
            Method m;
            if (!m_Methods.TryGetValue(methodName, out m))
            {
                // Link execution of JSP file
                JspFile jsp = null;
                if (packageName == "org.apache.jsp" && name == "_jspService")
                    jsp = m_Workspace.GetJspFile(className.Replace("_", "."));

                if (jsp != null)
                {
                    m = new Method(name, className, packageName, jsp);
                    jsp.Document.ResolvedMethod = m;
                }
                else
                {
                    m = new Method(name, className, packageName, file, beginLine);
                }
                m_Methods.Add(methodName, m);
            }
            m_Requests[requestId].AddMethod(id, m);
        }

        private void HandleCall(string message)
        {
            string[] parts = message.Split(new char[] { ',' });
            int requestId = int.Parse(parts[1]);
            int id = int.Parse(parts[2]);
            int callerLine = int.Parse(parts[3]) - 1;
            m_Requests[requestId].AddCall(id, callerLine);
        }

        private void HandleReturn(string message)
        {
            string[] parts = message.Split(new char[] { ',' });
            int requestId = int.Parse(parts[1]);
            int line = int.Parse(parts[2]) - 1;
            m_Requests[requestId].PopCall(line);
        }
    }
}
