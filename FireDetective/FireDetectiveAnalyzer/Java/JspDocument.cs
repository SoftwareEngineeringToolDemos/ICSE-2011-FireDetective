using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace FireDetectiveAnalyzer.Java
{
    public class JspDocument
    {
        public TextDocument Original { get; private set; }
        private List<JspToken> m_Tokens = new List<JspToken>();
        private List<JavaDocument> m_JavaSnippets = new List<JavaDocument>();

        public IEnumerable<JspToken> Tokens { get { return m_Tokens; } }
        public IEnumerable<JavaDocument> JavaSnippets { get { return m_JavaSnippets; } }

        public Method ResolvedMethod { get; set; }

        public JspDocument(TextDocument original)
        {
            Original = original;
            Parse();
        }

        private void Parse()
        {
            JspTokenStream ts = new JspTokenStream(new TextStream(Original.FullText));
            try
            {
                foreach (JspToken token in ts.ReadTokens())
                {
                    if (token.Type == JspTokenType.TemplateCode)
                    {
                        JavaDocument jd = new JavaDocument(new TextSpan(Original, token.Location.Start, token.Location.End));
                        m_JavaSnippets.Add(jd);
                    }
                    m_Tokens.Add(token);
                }
            }
            catch (EndOfStreamException)
            {
            }
        }
    }
}
