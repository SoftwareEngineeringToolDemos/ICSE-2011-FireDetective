using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace FireDetectiveAnalyzer.Firefox
{
    public class ContentItem
    {
        public SimpleUri Url { get; set; }
        public bool IsXhr { get; set; }
        public TextDocument Document { get; set; }
        public string MimeType { get; set; }
        public PageRequest PageRequest { get; private set; }

        public string OriginalMimeType { get; set; }
        public SimpleUri OriginalUrl { get; set; }
        public string RequestMethod { get; set; }
        public string ResponseStatus { get; set; }
        public Encoding ResponseEncoding { get; set; }
        public long RequestStartTime { get; set; }
        public long ResponseStartTime { get; set; }
        public long ResponseEndTime { get; set; }

        public HtmlDocument HtmlDocument { get; set; }
        public JavaScriptDocument JavaScriptDocument { get; set; }
        public Java.TopLevelTrace ServerTrace { get; set; }
        public Xhr Xhr { get; set; }
        
        public bool IsDojoJQuery { get; private set; }

        public ContentItem(PageRequest page, SimpleUri originalUrl)
        {
            PageRequest = page;
            OriginalUrl = originalUrl;
            MimeType = "";
            IsDojoJQuery = originalUrl != null &&
                (originalUrl.ToString().EndsWith("/jquery-1.3.2.js") ||
                originalUrl.ToString().EndsWith("/jquery-1.3.2.min.js") ||
                originalUrl.ToString().EndsWith("/jquery.js") ||
                originalUrl.ToString().Contains("/dojo/") && originalUrl.ToString().EndsWith(".js"));
        }

        public void Parse()
        {
            if (MimeType == "text/html")
                HtmlDocument = new HtmlDocument(Document);
            else if (MimeType == "text/javascript" || MimeType == "application/x-javascript")
                JavaScriptDocument = new JavaScriptDocument(Document);
            //else
            //    throw new ApplicationException("Can't parse content type: " + MimeType + ".");
        }

        public JavaScriptDocument GetJavaScriptDocumentForLine(int line)
        {
            if (JavaScriptDocument != null)
                return JavaScriptDocument;
            else if (HtmlDocument != null)
            {
                int start = Document.GetIndexOfLine(line);
                int end = Document.GetIndexOfLine(line + 1);
                return HtmlDocument.GetJavaScriptDocument(start, end);
            }
            else
                return null;
        }

        public void OverrideMimeType(string mimeType)
        {
            if (OriginalMimeType == null)
                OriginalMimeType = MimeType;
            MimeType = mimeType;
        }

        public bool IsCompleted { get { return Url != null; } }

        public string ShortUrl { get { return Url != null ? Url.GetShortUrl(PageRequest.Location) : ""; } }

        public string ShortOriginalUrl { get { return OriginalUrl.GetShortUrl(PageRequest.Location); } }

        public bool IsImage { get { return MimeType.StartsWith("image/"); } }

        public bool IsStyleSheet { get { return MimeType == "text/css"; } }
    }
}
