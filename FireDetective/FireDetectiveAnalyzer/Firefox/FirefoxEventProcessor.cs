using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace FireDetectiveAnalyzer.Firefox
{
    public class FirefoxEventProcessor : IEventProcessor
    {
        public event EventHandler<CustomEventArgs<PageRequest>> PageRequested;

        // Different stages of a page request. First, only initialPage will be set, then the others, one-by-one.
        private PageRequest m_InitialPage = null;
        private PageRequest m_LoadedPage = null;
        private PageRequest m_ScriptPage = null;

        private Dictionary<int, PageRequest> m_PageRequestById = new Dictionary<int, PageRequest>();

        private ScriptResolver m_Resolver = new ScriptResolver();

        private bool m_InMarkedSection = false;

        private ContentItem m_ScriptSourceHolder;

        public FirefoxEventProcessor()
        {
        }

        public void HandleMessage(string message, Func<string> getNextMessage, Func<int, byte[]> getBinaryData)
        {
            //if (m_Page == null && m_NewRequest == null)
            //{
            //    if (!message.StartsWith("REQUEST,") || peekMessage() != "NEW-PAGE-REQUEST")
            //    {
            //        // TODO: Show please refresh message, better: buffer messages in firefox ("attach later" feature).
            //        return;
            //    }
            //}

            if (message.StartsWith("C,"))
            {
                if (m_ScriptPage != null) HandleCall(message);
            }
            else if (message.StartsWith("R,"))
            {
                if (m_ScriptPage != null) HandleReturn(message);
            }
            else if (message.StartsWith("EVENT,"))
            {
                if (m_ScriptPage != null) HandleEvent(message);
            }
            else if (message.StartsWith("SCRIPT,"))
            {
                if (m_ScriptPage != null)
                    HandleScriptCreated(message, getNextMessage);
                else
                    HandleScriptCreatedNoPage(message, getNextMessage);
            }
            /*else if (message.StartsWith("MOD,"))
            {
                if (m_ScriptPage != null) HandleMod(message);
            }*/
            else if (message.StartsWith("CALL-INFO,"))
            {
                if (m_ScriptPage != null) HandleCallInfo(message);
            }
            else if (message.StartsWith("REQUEST,"))
            {
                HandleRequest(message);
            }
            else if (message.StartsWith("RESPONSE,"))
            {
                HandleResponse(message);
            }
            else if (message.StartsWith("RESPONSE-DATA"))
            {
                HandleResponseData(message, getBinaryData);
            }
            else if (message.StartsWith("NEW-PAGE-LOADED,"))
            {
                HandleNewPageLoaded(message);
            }
            else if (message.StartsWith("NEW-PAGE-SCRIPTS,"))
            {
                HandleNewPageScripts();
            }
            else if (message == "MARK-START")
            {
                HandleMarkStart();
            }
            else if (message == "MARK-STOP")
            {
                HandleMarkStop();
            }
            else
                throw new ApplicationException("Firefox: unexpected message '" + message + "'.");
        }

        private void HandleRequest(string message)
        {
            string[] parts = message.Split(new char[] { ',' }, 6);
            int id = int.Parse(parts[1]);
            long startTime = long.Parse(parts[2]);
            bool isInitialRequest = parts[3] == "initial";
            bool isInitialDuplicateRequest = parts[3] == "initial-duplicate";
            bool isXhr = parts[3] == "xhr";
            string method = parts[4];
            string url = parts[5];

            if (isInitialRequest)
            {
                HandleNewPageRequest(url);
            }

            PageRequest page = (isInitialRequest || isInitialDuplicateRequest) ? m_InitialPage : m_LoadedPage;
            m_PageRequestById[id] = page;
            ContentItem ci = new ContentItem(page, new SimpleUri(url));
            ci.RequestMethod = method;
            ci.IsXhr = isXhr;
            ci.RequestStartTime = startTime;

            page.AddContentItemWithoutUrl(id, ci);
            if (isXhr)
            {
                if (m_LoadedPage != m_ScriptPage)
                    throw new ApplicationException("Bad xhr call");
                m_ScriptPage.AddCallInfoSendXhr(ci);
            }
        }

        public void HandleNewPageRequest(string url)
        {
            m_InitialPage = new PageRequest();
            m_InitialPage.Location = new SimpleUri(url);
            m_InitialPage.Date = DateTime.Now;
            if (PageRequested != null)
                PageRequested(this, new CustomEventArgs<PageRequest>(m_InitialPage));
        }

        public void HandleNewPageLoaded(string message)
        {
            // We are unable to trace requests for local pages (like about:*, file:// urls, etc.). So simulate a first initial request when missing.
            if (m_InitialPage == null)
            {
                string[] parts = message.Split(new char[] { ',' }, 2);
                HandleNewPageRequest(parts[1]);
            }

            m_LoadedPage = m_InitialPage;
            m_InitialPage = null; // Not really necessary, but might help with finding bugs
            m_LoadedPage.StartSectionSafe(m_InMarkedSection);
        }

        private void HandleResponse(string message)
        {
            string[] parts = message.Split(new char[] { ',' }, 9);
            int id = int.Parse(parts[1]);
            long responseStartTime = long.Parse(parts[2]);
            bool isXhr = parts[3] == "xhr";
            string status = parts[4];
            bool hasContent = parts[5] == "content";
            string mimeType = parts[6];
            string encoding = parts[7];
            string url = parts[8];

            ContentItem ci = m_PageRequestById[id].GetContentItemById(id);
            ci.Url = new SimpleUri(url);
            ci.MimeType = mimeType;
            ci.ResponseStatus = status;
            ci.ResponseStartTime = responseStartTime;
            if (encoding != "")
                ci.ResponseEncoding = Encoding.GetEncoding(encoding);
            else if (hasContent)
                ci.ResponseEncoding = Encoding.Default;
            else
                ci.ResponseEncoding = null;

            if (isXhr)
            {
                m_PageRequestById[id].NotifyContentItemHasUrl(id, ci);
            }
            else
            {
                m_PageRequestById[id].NotifyContentItemHasUrl(id, ci);
            }
        }

        private void HandleResponseData(string message, Func<int, byte[]> getBinaryData)
        {
            string[] parts = message.Split(new char[] { ',' }, 3);
            int id = int.Parse(parts[1]);
            int length = int.Parse(parts[2]);
            byte[] data = getBinaryData(length);

            ContentItem ci = m_PageRequestById[id].GetContentItemById(id);
            ci.Document = new TextDocument(ci.ResponseEncoding.GetString(data));

            // Try to parse content items (an xhr content item will be parsed only when eval() has been called on its content, regardless of its mime type)
            if (!ci.IsXhr)
            {
                EnqueueParse(ci);
            }
        }

        private void EnqueueParse(ContentItem ci)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback((dummy) =>
            {
                ci.Parse();
                m_Resolver.NotifyContentItemParsed(ci);
            }));
        }

        private void HandleNewPageScripts()
        {
            m_ScriptPage = m_LoadedPage;
            m_ScriptSourceHolder = null;
        }

        private void HandleMarkStart()
        {
            m_InMarkedSection = true;
            if (m_LoadedPage != null)
                m_LoadedPage.StartSectionSafe(true);
        }

        private void HandleMarkStop()
        {
            m_InMarkedSection = false;
            if (m_LoadedPage != null)
                m_LoadedPage.StartSectionSafe(false);
        }
        
        // Scripts

        private void HandleScriptCreated(string message, Func<string> getNextMessage)
        {
            bool parseHolder = false;
            string[] parts = message.Split(new char[] { ',' }, 8);
            int id = int.Parse(parts[1]);
            string name = parts[2];
            string startInfo = parts[3];
            string endInfo = parts[4];
            int deltaLine = int.Parse(parts[5]);
            int xhrId;
            bool hasXhrId = int.TryParse(parts[6], out xhrId);
            string url = parts[7];

            if (m_ScriptPage.HasScriptWithId(id))
                throw new ApplicationException("Cannot define script more than once.");

            string nextMessage = getNextMessage();
            if (!nextMessage.StartsWith("SCRIPT-SOURCE,"))
                throw new ApplicationException("Script source expected.");
            parts = nextMessage.Split(new char[] { ',' }, 3);
            string desc = parts[1];
            string src = parts[2];

            Script s;
            if (url != "" && url != "<eval>" && m_ScriptPage.HasContentItemWithUrl(url))
            {
                // Small TODO: There might have been duplicate requests for the same url, returning different data.
                // Instead of the url, our firefox plugin should give us something more accurate.
                s = new Script(name, m_ScriptPage.GetContentItemByUrl(url));
            }
            else
            {
                bool isEval = url == "<eval>";
                if (m_ScriptSourceHolder == null)
                {
                    if (hasXhrId)
                    {
                        m_ScriptSourceHolder = m_ScriptPage.GetContentItemById(xhrId);
                        m_ScriptSourceHolder.OverrideMimeType("text/javascript");
                    }
                    else
                    {
                        SimpleUri u = isEval ? new SimpleUri("") : new SimpleUri(url);
                        m_ScriptSourceHolder = new ContentItem(m_ScriptPage, null) { Url = u, MimeType = "text/javascript", IsXhr = false };
                    }
                }

                bool isTopLevelEval = desc == "src" && isEval;
                if (desc == "src")
                {
                    if (hasXhrId)
                    {
                        if (new TextDocument(src).FullText != m_ScriptSourceHolder.Document.FullText)
                            throw new ApplicationException("Hash sanity check failed.");
                    }
                    else
                    {
                        m_ScriptSourceHolder.Document = new TextDocument(src);
                    }
                    parseHolder = true;
                }

                if (isEval && name == "")
                    name = "<toplevel>";

                s = new Script(name, isTopLevelEval, m_ScriptSourceHolder);
            }

            if (startInfo == "UNKNOWN")
            {
                s.SetPosInfoUnknown();
            }
            else if (startInfo == "ALL")
            {
                s.SetPosInfoAll(deltaLine);
            }
            else if (startInfo.StartsWith("POS;"))
            {
                string[] posInfo = startInfo.Split(';');
                int startPos = int.Parse(posInfo[1]);
                int endPos = int.Parse(posInfo[2]);
                s.SetPosInfo(startPos, endPos, deltaLine);
            }
            else
            {
                int startLine = int.Parse(startInfo) - 1;
                int endLine = int.Parse(endInfo) - 1;
                s.SetLineInfo(startLine, endLine, deltaLine);
            }

            m_ScriptPage.AddScript(id, s);
            m_Resolver.QueueForResolve(s);

            if (parseHolder)
            {
                EnqueueParse(m_ScriptSourceHolder);
                m_ScriptSourceHolder = null;
            }
        }

        private void HandleScriptCreatedNoPage(string message, Func<string> getNextMessage)
        {
            if (!getNextMessage().StartsWith("SCRIPT-SOURCE,"))
                throw new ApplicationException("Script source expected-");
        }

        private void HandleCall(string message)
        {
            string[] parts = message.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            int depth = int.Parse(parts[1]);
            int id = int.Parse(parts[2]);
            int line = int.Parse(parts[3]) - 1;

            if (!m_ScriptPage.HasScriptWithId(id))
                throw new ApplicationException("Can't find script for this id.");

            if (parts.Length > 4)
            {
                int callerId = int.Parse(parts[4]);
                int callerLine = int.Parse(parts[5]) - 1;
                m_ScriptPage.AddCallApplyDelta(id, line, callerId, callerLine);
            }
            else
                m_ScriptPage.AddCallApplyDelta(id, line, -1, -1);
        }

        private void HandleCallInfo(string message)
        {
            string[] parts = message.Split(new char[] { ',' }, 4);
            string desc = parts[1];
            int xhrId;
            bool hasXhrId = int.TryParse(parts[2], out xhrId);
            int itId;
            bool hasItId = int.TryParse(parts[3], out itId);
            if (hasXhrId)
            {
                m_ScriptPage.AddCallInfoAccessXhr(xhrId);
            }
            else if (hasItId)
            {
                if (desc.StartsWith("set-"))
                {
                    IntervalTimeout it = new IntervalTimeout(desc == "set-interval");
                    m_ScriptPage.AddCallInfoSetIntervalTimeout(itId, it);
                }
                else if (desc.StartsWith("clear-"))
                {
                    m_ScriptPage.AddCallInfoClearIntervalTimeout(itId);
                }
                else
                {
                    throw new ApplicationException("set- or clear- expected");
                }
            }
            else
            {
                throw new ApplicationException("Xhr/IT id expected");
            }
        }

        private void HandleReturn(string message)
        {
            string[] parts = message.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            int id = int.Parse(parts[1]);
            int line = int.Parse(parts[2]) - 1;
            m_ScriptPage.PopCallApplyDelta(id, line);
        }

        /*private void HandleMod(string message)
        {
            string[] parts = message.Split(',');
            int id = int.Parse(parts[1]);
            m_ScriptPage.MarkDomNodeModification(id);
        }*/

        // Events

        private void HandleEvent(string message)
        {
            string[] parts = message.Split(new char[] { ',' }, 4);
            int id;
            bool hasId = int.TryParse(parts[3], out id);
            bool xhrOrIt = hasId && parts[2].Trim() == "";
            if (xhrOrIt && (parts[1] == "<xhr.readystatechange>"))
                m_ScriptPage.StartXhrEventSafe(parts[1], id);
            else if (xhrOrIt && (parts[1] == "<timeout>" || parts[1] == "<interval>"))
                m_ScriptPage.StartIntervalTimeoutEventSafe(parts[1], id);
            else
                m_ScriptPage.StartEventSafe(parts[1], parts[2]);
        }
    }
}