using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace FireDetectiveAnalyzer.Firefox
{
    public class HtmlDocument
    {
        public TextDocument Original { get; private set; }
        private List<HtmlToken> m_Tokens = new List<HtmlToken>();
        private Dictionary<HtmlToken, JavaScriptDocument> m_JavaScriptSnippets = new Dictionary<HtmlToken, JavaScriptDocument>();

        public IEnumerable<HtmlToken> Tokens { get { return m_Tokens; } }
        public IEnumerable<JavaScriptDocument> JavaScriptSnippets { get { return m_JavaScriptSnippets.Values; } }

        public HtmlDocument(TextDocument original)
        {
            Original = original;
            Parse();
        }

        private void Parse()
        {
            HtmlTokenStream ts = new HtmlTokenStream(new TextStream(Original.FullText));
            try
            {
                while (true)
                {
                    HtmlToken token = ts.ReadToken();
                    if (token.Type == HtmlTokenType.Script)
                    {
                        JavaScriptDocument jsd = new JavaScriptDocument(new TextSpan(Original, token.Location.Start, token.Location.End));
                        m_JavaScriptSnippets.Add(token, jsd);
                    }
                    m_Tokens.Add(token);
                }
            }
            catch (EndOfStreamException)
            {
            }
        }

        private IntRange GetTokenIndexRangeWide(int start, int end) // Wide => everything (also partially) between start-end
        {            
            HtmlToken dummy = new HtmlToken("", HtmlTokenType.Text, new SimpleTextSpan(end, start));
            return m_Tokens.GetRangeUsingBinarySearch(dummy, HtmlToken.EndComparer, dummy, HtmlToken.StartComparer);
        }

        public JavaScriptDocument GetJavaScriptDocument(int start, int end)
        {
            IntRange range = GetTokenIndexRangeWide(start, end);
            HtmlToken t = range.Numbers.Select(n => m_Tokens[n]).Where(token => token.Type == HtmlTokenType.Script).FirstOrDefault();
            if (t.Type == HtmlTokenType.Script)
                return m_JavaScriptSnippets[t];
            else
                return null;
        }

        /*
        public IntRange GetJavaScriptTokenIndexRange(int start, int end, out JavaScriptDocument jsd)
        {
            jsd = GetJavaScriptDocument(start, end);
            if (jsd != null)
                return jsd.GetTokenIndexRange(start, end);
            else
                return new IntRange(0, 0);
        }*/
    }
}
