using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace FireDetectiveAnalyzer.Java
{
    public class JavaMethodBlock
    {
        public SimpleTextSpan Location { get; private set; }
        public Method ResolvedMethod { get; set; }

        public JavaMethodBlock(SimpleTextSpan location)            
        {
            Location = location;
        }
    }

    public class JavaBlock
    {
        private List<JavaToken> m_Tokens;
        private List<JavaMethodBlock> m_MethodBlocks;

        public IEnumerable<JavaMethodBlock> MethodBlocks { get { return m_MethodBlocks; } }

        public JavaBlock(IEnumerable<JavaToken> tokens)
        {
            m_MethodBlocks = new List<JavaMethodBlock>();
            m_Tokens = tokens.Where(t => t.Type != JavaTokenType.Comment && t.Type != JavaTokenType.MultiLineComment).ToList();
            Parse(new MemoryStream<JavaToken>(m_Tokens));
        }

        private void Parse(MemoryStream<JavaToken> ts)
        {
            try
            {
                ParseStream(ts);
            }
            catch (EndOfStreamException)
            {
            }
        }

        private void ParseStream(MemoryStream<JavaToken> ts)
        {
            while (true)
            {
                if (ts.TryMatch(t => t.Type == JavaTokenType.Keyword && t.Value == "class"))
                    ParseClass(ts);
                else
                    ts.Read();
            }
        }

        private void ParseClass(MemoryStream<JavaToken> ts)
        {
            while (!ts.TryMatch(t => t.Type == JavaTokenType.Operator && t.Value == "{"))
                ts.Read();
            ParseClassBody(ts);
        }

        private void ParseClassBody(MemoryStream<JavaToken> ts)
        {
            while (!ts.TryMatch(t => t.Type == JavaTokenType.Operator && t.Value == "}"))
            {
                int pos = ts.Position;
                if (ts.TryMatch(t => t.Type == JavaTokenType.Operator && t.Value == "{"))
                    MatchCurlyBraces(ts);
                else if (ts.TryMatch(t => t.Type == JavaTokenType.Keyword && (t.Value == "public" || t.Value == "protected" || t.Value == "private")))
                    ParsePossibleMethod(ts, pos);
                else
                    ts.Read();
            }
        }

        private void MatchCurlyBraces(MemoryStream<JavaToken> ts)
        {
            while (!ts.TryMatch(t => t.Type == JavaTokenType.Operator && t.Value == "}"))
                if (ts.TryMatch(t => t.Type == JavaTokenType.Operator && t.Value == "{"))
                    MatchCurlyBraces(ts);
                else
                    ts.Read();
        }

        private void ParsePossibleMethod(MemoryStream<JavaToken> ts, int pos)
        {
            while (!ts.TryMatch(t => t.Type == JavaTokenType.Operator && t.Value == ";"))
            {
                if (ts.TryMatch(t => t.Type == JavaTokenType.Operator && t.Value == "{"))
                {
                    MatchCurlyBraces(ts);
                    m_MethodBlocks.Add(new JavaMethodBlock(new SimpleTextSpan(m_Tokens[pos].Location.Start, m_Tokens[ts.Position - 1].Location.End)));
                    return;
                }
                else
                    ts.Read();
            }
        }
    }
}
