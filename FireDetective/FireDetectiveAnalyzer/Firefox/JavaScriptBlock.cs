using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace FireDetectiveAnalyzer.Firefox
{
    public class JavaScriptBlock
    {
        public int Start { get; private set; }
        public int End { get; private set; }
        public JavaScriptBlockType Type { get; private set; }
        protected JavaScriptBlock m_Parent;
        protected JavaScriptTopLevelBlock m_TopLevel;
        protected List<JavaScriptBlock> m_SubBlocks = new List<JavaScriptBlock>();
        protected List<JavaScriptToken> m_Tokens;

        public IEnumerable<JavaScriptBlock> SubBlocks { get { return m_SubBlocks; } }
        public JavaScriptBlock Parent { get { return m_Parent; } }

        // Specific to Type == Function
        protected JavaScriptBlock m_FunctionBody;
        public JavaScriptBlock FunctionBody { get { return m_FunctionBody; } }

        // After resolving
        public Script ResolvedScript { get; set; }

        public JavaScriptBlock(int start, JavaScriptBlockType type, JavaScriptBlock parent, JavaScriptTopLevelBlock topLevel, List<JavaScriptToken> tokens)
        {
            Start = start;
            End = start;
            Type = type;
            m_Parent = parent;
            m_TopLevel = topLevel;
            m_Tokens = tokens;
        }

        protected void Parse(MemoryStream<JavaScriptToken> ts)
        {
            TryParseAny(ts, (dummy) => false);
            End = ts.Position;
        }

        protected void TryParseAny(MemoryStream<JavaScriptToken> ts, Func<JavaScriptToken, bool> stopFunc)
        {
            int before = ts.Position;
            JavaScriptToken token;
            while (!ts.EndOfStream && !stopFunc(token = ts.Peek()))
            {
                if (TryParseFunction(ts) == null &&
                    TryParseBrackets(ts, "{", "}") == null &&
                    TryParseBrackets(ts, "(", ")") == null &&
                    TryParseBrackets(ts, "[", "]") == null &&
                    TryParseLVar(ts) == null)
                {
                    ts.Read();
                }
            }
        }

        protected JavaScriptBlock TryParseBrackets(MemoryStream<JavaScriptToken> ts, string ch, string endCh)
        {
            int before = ts.Position;
            if (ts.TryMatch((token) => token.Type == JavaScriptTokenType.Operator && token.Value == ch))
            {
                JavaScriptBlock block = new JavaScriptBlock(before, JavaScriptBlockType.Brackets, this, m_TopLevel, m_Tokens);
                block.TryParseAny(ts, (token) => token.Type == JavaScriptTokenType.Operator && token.Value == endCh);
                ts.Read();
                AddBlock(block, ts);
                return block;
            }
            return null;
        }

        protected JavaScriptBlock TryParseFunction(MemoryStream<JavaScriptToken> ts)
        {
            int before = ts.Position;
            if (ts.TryMatch((token) => token.Type == JavaScriptTokenType.Keyword && token.Value == "function"))
            {
                JavaScriptBlock block = new JavaScriptBlock(before, JavaScriptBlockType.Function, this, m_TopLevel, m_Tokens);
                block.TryMatchComments(ts);
                block.TryParseLVar(ts); // Too generic for optional function name, but never mind
                block.TryMatchComments(ts);
                block.TryParseBrackets(ts, "(", ")");
                block.TryMatchComments(ts);
                block.m_FunctionBody = block.TryParseBrackets(ts, "{", "}");
                AddBlock(block, ts);
                return block;
            }
            return null;
        }

        protected JavaScriptBlock TryParseLVar(MemoryStream<JavaScriptToken> ts)
        {
            int before = ts.Position;
            if (ts.TryMatch((token) => token.Type == JavaScriptTokenType.Identifier))
            {
                JavaScriptBlock block = new JavaScriptBlock(before, JavaScriptBlockType.LVar, this, m_TopLevel, m_Tokens);
                while (true)
                {
                    while (block.TryParseBrackets(ts, "[", "]") != null)
                        ;
                    if (!ts.TryMatch((token) => token.Type == JavaScriptTokenType.Operator && token.Value == "."))
                        break;
                    if (!ts.TryMatch((token) => token.Type == JavaScriptTokenType.Identifier))
                        break;
                }
                block.m_SubBlocks.Clear(); // Don't want sub blocks in LVar blocks.
                AddBlock(block, ts);
                return block;
            }
            return null;
        }

        protected void TryMatchComments(MemoryStream<JavaScriptToken> ts)
        {
            while (ts.TryMatch(token => token.Type == JavaScriptTokenType.Comment || token.Type == JavaScriptTokenType.MultiLineComment))
                ;
        }

        protected void AddBlock(JavaScriptBlock block, MemoryStream<JavaScriptToken> ts)
        {
            block.End = ts.Position;
            m_SubBlocks.Add(block);
        }

        public IntRange GetTokenIndexRange()
        {
            return new IntRange(Start, End);
        }

        public SimpleTextSpan GetTextLocation()
        {
            IntRange range = GetTokenIndexRange();
            return new SimpleTextSpan(m_Tokens[range.Start].Location.Start, m_Tokens[range.End - 1].Location.End);
        }

        public bool IsFunction()
        {
            return Type == JavaScriptBlockType.Function;
        }

        public bool IsNamedFunction(bool named)
        {
            if (!IsFunction()) return false;
            return
                (MatchTokens(Start + 1, 1, token => token.Type == JavaScriptTokenType.Operator && token.Value == "(") != null)
                ==
                !named;
        }

        public bool IsFirstParsedFunction()
        {
            return IsNamedFunction(true);
        }

        public string GetAnonymousFunctionName()
        {
            int[] result = MatchTokens(Start - 1, -1,
                t => t.Type == JavaScriptTokenType.Operator && (t.Value == ":" || t.Value == "="),
                u => true);
            if (result != null)
            {
                JavaScriptBlock block = m_TopLevel.GetBlockAtIndex(result[1]);
                if (block.Type == JavaScriptBlockType.LVar)
                    return new IntRange(block.Start, block.End).Numbers.Select(i => m_Tokens[i].Value).Aggregate("", (a, s) => a + s);
                else
                    return m_Tokens[result[1]].Value;
            }
            else
                return "__anonymous__";
        }

        private int[] MatchTokens(int index, int delta, params Func<JavaScriptToken, bool>[] funcs)
        {
            int[] result = new int[funcs.Length];
            for (int i = 0; i < funcs.Length; i++)
            {
                while (index >= 0 && index < m_Tokens.Count && (m_Tokens[index].Type == JavaScriptTokenType.Comment || m_Tokens[index].Type == JavaScriptTokenType.MultiLineComment))
                    index += delta;
                if (index < 0 || index >= m_Tokens.Count) return null;
                if (!funcs[i](m_Tokens[index])) return null;
                result[i] = index;
                index += delta;
            }
            return result;
        }

        protected void BuildBlockIndex(JavaScriptBlock[] blockPos)
        {
            int j = 0;
            for (int i = Start; i < End; )
                if (j < m_SubBlocks.Count && m_SubBlocks[j].Start == i)
                {
                    m_SubBlocks[j].BuildBlockIndex(blockPos);
                    i = m_SubBlocks[j++].End;
                }
                else
                {
                    blockPos[i++] = this;
                }
        }

        protected void BuildLineScanIndex(Dictionary<int, Queue<JavaScriptBlock>> scan, Func<int, int> getLine)
        {
            foreach (JavaScriptBlock block in m_SubBlocks.Where(b => b.IsFirstParsedFunction()))
                block.BuildLineScanIndex(scan, getLine);
            foreach (JavaScriptBlock block in m_SubBlocks.Where(b => !b.IsFirstParsedFunction()))
                block.BuildLineScanIndex(scan, getLine);
            if (IsFunction() && m_FunctionBody != null)
            {
                int line = getLine(m_Tokens[Start].Location.Start);
                if (!scan.ContainsKey(line)) scan.Add(line, new Queue<JavaScriptBlock>());
                scan[line].Enqueue(this);
            }
        }
    }

    public class JavaScriptTopLevelBlock : JavaScriptBlock
    {
        private JavaScriptBlock[] m_BlockPos;
        private Dictionary<int, Queue<JavaScriptBlock>> m_ScanLine;

        public JavaScriptTopLevelBlock(List<JavaScriptToken> tokens)
            : base(0, JavaScriptBlockType.TopLevel, null, null, tokens)
        {
            m_TopLevel = this;
            Parse(new MemoryStream<JavaScriptToken>(tokens));
            BuildBlockIndex();
        }

        private void BuildBlockIndex()
        {
            m_BlockPos = new JavaScriptBlock[m_Tokens.Count];
            BuildBlockIndex(m_BlockPos);
        }

        public JavaScriptBlock GetBlockAtIndex(int index)
        {
            return m_BlockPos[index];
        }

        private void BuildLineScanIndexIfNeeded(Func<int, int> getLine)
        {
            if (m_ScanLine == null)
            {
                m_ScanLine = new Dictionary<int, Queue<JavaScriptBlock>>();
                BuildLineScanIndex(m_ScanLine, getLine);
            }
        }

        public JavaScriptBlock GetNextFunctionForLine(int line, Func<int, int> getLine)
        {
            BuildLineScanIndexIfNeeded(getLine);
            Queue<JavaScriptBlock> q;
            if (m_ScanLine.TryGetValue(line, out q) && q.Count > 0)
                return q.Dequeue();
            else
                return null;
        }
    }

    public enum JavaScriptBlockType
    {
        TopLevel,
        LVar,
        Function,
        Brackets
    }
}
