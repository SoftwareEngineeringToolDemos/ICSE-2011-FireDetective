using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FireDetectiveAnalyzer.Firefox
{
    public class Xhr
    {
        public ContentItem ContentItem { get; private set; }

        public List<XhrEvent> XhrEvents { get; private set; }

        public Xhr(ContentItem ci)
        {
            ContentItem = ci;
            XhrEvents = new List<XhrEvent>();
        }

        public bool IsCompleted { get { return ContentItem.IsCompleted; } }

        public string ShortOriginalUrl { get { return ContentItem.ShortOriginalUrl; } }

        public string ShortUrl { get { return ContentItem.ShortUrl; } }

        public void SetSend(Call call)
        {
            XhrEvents.Add(new XhrEvent() { Type = XhrEventType.Send, Call = call });
        }

        public void AddAccess(Call call)
        {
            // Prevent duplicate adds (simple, but good enough)
            if (XhrEvents.Where(xe => xe.Type == XhrEventType.Access).Select(xe => xe.Call).LastOrDefault() != call)
                XhrEvents.Add(new XhrEvent() { Type = XhrEventType.Access, Call = call });
        }

        public void AddEval(Call call)
        {
            XhrEvents.Add(new XhrEvent() { Type = XhrEventType.Eval, Call = call });
        }

        public void AddReadyStateChangeEvent(Event e)
        {
            XhrEvents.Add(new XhrEvent() { Type = XhrEventType.ReadyStateChange, Event = e });
        }
    }

    public class XhrEvent
    {
        public XhrEventType Type { get; set; }
        public Call Call { get; set; }
        public Event Event { get; set; }
    }

    public enum XhrEventType
    {
        Send,
        Access,
        Eval,
        ReadyStateChange
    }
}
