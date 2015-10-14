using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FireDetectiveAnalyzer
{
    public class TextDocument
    {
        private string m_FullText;
        private List<int> m_LineStarts = new List<int>();

        public TextDocument(string fullText)
        {
            m_FullText = fullText.Replace("\r\n", "\n").Replace("\r", "\n");
            ParseLineStarts();
        }

        public string FullText { get { return m_FullText; } }

        public int LineCount { get { return m_LineStarts.Count; } }

        public int GetIndexOfLine(int lineNumber)
        {
            if (lineNumber < 0) return 0;
            else if (lineNumber >= LineCount) return m_FullText.Length;
            else return m_LineStarts[lineNumber];
        }

        public int GetLine(int index)
        {
            int find = m_LineStarts.BinarySearch(index);
            if (find >= 0) return find;
            else return ~find - 1;
        }

        public int GetLineForEndMarker(int index)
        {
            int find = m_LineStarts.BinarySearch(index);
            if (find >= 0) return find - 1;
            else return ~find - 1;
        }

        public TextSpan GetSpanForLine(int line)
        {
            return new TextSpan(this, GetIndexOfLine(line), GetIndexOfLine(line + 1));
        }

        private void ParseLineStarts()
        {
            m_LineStarts.Clear();
            m_LineStarts.Add(0);
            int index = 0;
            while ((index = m_FullText.IndexOf('\n', index)) >= 0)
            {
                m_LineStarts.Add(++index);
            }
        }
    }

    public class TextSpan : IComparable<TextSpan>
    {
        private TextDocument m_Doc;
        private int m_Start;
        private int m_End;

        public TextSpan(TextDocument doc, int start, int end)
        {
            m_Doc = doc;
            m_Start = start;
            m_End = end;
        }

        public TextSpan(TextDocument doc, SimpleTextSpan span)
            : this(doc, span.Start, span.End)
        {
        }

        public int Start { get { return m_Start; } }
        public int End { get { return m_End; } }

        public TextSpan Clone()
        {
            return new TextSpan(m_Doc, m_Start, m_End);
        }

        public string Text
        {
            get { return m_Doc.FullText.Substring(m_Start, m_End - m_Start); }
        }

        public TextDocument Document
        {
            get { return m_Doc; }
        }

        public TextSpan ExtendByCharacters(int numCharacters)
        {
            return new TextSpan(m_Doc,
                Math.Max(0, m_Start - numCharacters),
                Math.Min(m_Doc.FullText.Length, m_End + numCharacters));
        }

        public TextSpan ExtendByLines(int numLines)
        {
            return new TextSpan(m_Doc,
                m_Doc.GetIndexOfLine(m_Doc.GetLine(m_Start) - numLines),
                m_Doc.GetIndexOfLine(m_Doc.GetLineForEndMarker(m_End) + numLines + 1));
        }

        public bool Contains(TextSpan that)
        {
            return this.Start <= that.Start && this.End >= that.End;
        }

        public int CompareTo(TextSpan that)
        {
            return this.Start.CompareTo(that.Start);
        }
    }

    public struct SimpleTextSpan
    {
        public SimpleTextSpan(int start, int end)
        {
            Start = start;
            End = end;
        }

        public void Shift(int offset)
        {
            Start += offset;
            End += offset;
        }

        public bool Intersects(SimpleTextSpan other)
        {
            return !((Start <= other.Start && End <= other.Start) || (Start >= other.End && End >= other.End));                
        }

        public int Start;
        public int End;
    }

    public static class TextExtensions
    {
        public static TextSpan GetIdentifierAt(this TextDocument doc, int index)
        {
            Func<char, int> f = c => char.IsWhiteSpace(c) ? 0 : (char.IsLetterOrDigit(c) || c == '_' || c == '$' ? 1 : 2);
            string text = doc.FullText;
            if (index >= text.Length) return null;
            int kind;
            for (kind = f(text[index]); kind == 0 && index > 0; kind = f(text[--index])) ;
            if (kind == 0) return new TextSpan(doc, 0, 0);
            int i;
            for (i = index; i > 0 && f(text[i - 1]) == kind; i--) ;
            int j;
            for (j = index + 1; j < text.Length && f(text[j]) == kind; j++) ;
            return new TextSpan(doc, i, j);
        }

        public static List<SimpleTextSpan> InvertBits(this SimpleTextSpan span, IEnumerable<SimpleTextSpan> bits)
        {
            List<SimpleTextSpan> result = new List<SimpleTextSpan>();
            SimpleTextSpan current;
            current.Start = span.Start;
            foreach (SimpleTextSpan bit in bits)
            {
                current.End = bit.Start;
                if (current.Start < current.End)
                    result.Add(current);
                current.Start = bit.End;
            }
            current.End = span.End;
            if (current.Start < current.End)
                result.Add(current);
            return result;
        }

        public static int GetLongestLineLength(this TextDocument doc)
        {
            return new IntRange(0, doc.LineCount).Numbers.Select(i => doc.GetSpanForLine(i).Text.Length).Max();
        }
    }
}
