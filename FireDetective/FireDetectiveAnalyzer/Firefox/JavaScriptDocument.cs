using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace FireDetectiveAnalyzer.Firefox
{
    public class JavaScriptDocument
    {
        public TextDocument Original { get; private set; }
        public TextSpan OriginalSpan { get; private set; }        
        private List<JavaScriptToken> m_Tokens = new List<JavaScriptToken>();
        public JavaScriptTopLevelBlock Block { get; private set; }

        public IEnumerable<JavaScriptToken> Tokens { get { return m_Tokens; } }

        public JavaScriptDocument(TextDocument original)
        {
            Original = original;
            OriginalSpan = new TextSpan(original, 0, original.FullText.Length);
            Parse();
        }

        public JavaScriptDocument(TextSpan originalSpan)
        {
            Original = originalSpan.Document;
            OriginalSpan = originalSpan;
            Parse();
        }

        private void Parse()
        {
            // Scanner/Tokenizer
            JavaScriptTokenStream ts = new JavaScriptTokenStream(new TextStream(OriginalSpan.Text));
            try
            {
                while (true)
                {
                    JavaScriptToken token = ts.ReadToken();
                    token.Location.Shift(OriginalSpan.Start);
                    m_Tokens.Add(token);
                }
            }
            catch (EndOfStreamException)
            {
            }

            // Parser
            Block = new JavaScriptTopLevelBlock(m_Tokens);
        }

        public JavaScriptBlock GetNextFunctionForLine(int line)
        {
            return Block.GetNextFunctionForLine(line, pos => Original.GetLine(pos));
        }

        /*
        public JavaScriptToken GetToken(int n)
        {
            return m_Tokens[n];
        }

        public IntRange GetTokenIndexRange(int startPos, int endPos)
        {
            JavaScriptToken dummy = new JavaScriptToken("", JavaScriptTokenType.Garbage, new SimpleTextSpan(startPos, endPos));
            return m_Tokens.GetRangeUsingBinarySearch(dummy, JavaScriptToken.StartComparer, dummy, JavaScriptToken.EndComparer);
        }*/
    }
}
