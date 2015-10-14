using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace FireDetectiveAnalyzer.Java
{
    public class JavaDocument
    {
        public TextDocument Original { get; private set; }
        public TextSpan OriginalSpan { get; private set; }
        public IEnumerable<JavaToken> Tokens { get { return m_Tokens; } }
        public JavaBlock Block { get; private set; }

        private List<JavaToken> m_Tokens = new List<JavaToken>();

        public JavaDocument(TextDocument original)
        {
            Original = original;
            OriginalSpan = new TextSpan(original, 0, original.FullText.Length);
            Parse();
        }

        public JavaDocument(TextSpan originalSpan)
        {
            Original = originalSpan.Document;
            OriginalSpan = originalSpan;
            Parse();
        }

        private void Parse()
        {
            // Scanner/Tokenizer
            JavaTokenStream ts = new JavaTokenStream(new TextStream(OriginalSpan.Text));
            try
            {
                while (true)
                {
                    JavaToken token = ts.ReadToken();
                    token.Location.Shift(OriginalSpan.Start);
                    m_Tokens.Add(token);
                }
            }
            catch (EndOfStreamException)
            {
            }

            // Parser (very simple, only looks for method blocks)
            Block = new JavaBlock(m_Tokens);    
        }
    }
}
