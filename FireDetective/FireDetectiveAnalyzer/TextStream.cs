using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Globalization;

namespace FireDetectiveAnalyzer
{
    public class TextStream
    {
        private string m_Text;
        private int m_Pos;

        public int Position { get { return m_Pos; } }

        // Note: new-line character: \n
        public TextStream(string text)
            : this(text, 0)
        {
        }

        public TextStream(string text, int startPosition)
        {
            m_Text = text;
            m_Pos = startPosition;
        }

        private void Advance(int d)
        {
            m_Pos += d;
            /*
            while (true)
            {
                int newLine = m_Text.IndexOf('\n', m_Pos.Position);
                if (newLine >= m_Pos.Position && newLine < m_Pos.Position + d)
                {
                    d -= (newLine + 1 - m_Pos.Position);
                    m_Pos.Position = newLine + 1;
                    m_Pos.Line++;
                    m_Pos.Column = 1;
                }
                else
                {
                    m_Pos.Position += d;
                    m_Pos.Column += d;
                    return;
                }
            }*/
        }

        public bool EndOfStream { get { return m_Pos == m_Text.Length; } }

        public void MatchWhiteSpace()
        {
            int pos = m_Pos;
            while (pos < m_Text.Length && char.IsWhiteSpace(m_Text[pos]))
                pos++;
            Advance(pos - m_Pos);
        }

        public bool PeekString(string str)
        {
            if (m_Pos + str.Length > m_Text.Length) return false;
            return m_Text.Substring(m_Pos, str.Length) == str;
        }

        public bool TryMatchString(string str)
        {
            if (m_Pos == m_Text.Length)
                throw new EndOfStreamException();
            if (PeekString(str))
            {
                Advance(str.Length);
                return true;
            }
            else
                return false;
        }

        public string MatchString(string str)
        {
            if (TryMatchString(str))                
                return str;
            else
                throw new TextStreamException("Expected: " + str, m_Pos);
        }

        public string TryMatchAnyString(params string[] str)
        {
            return str.Where(s => TryMatchString(s)).FirstOrDefault() ?? "";
        }

        public string MatchUntilStringIgnoreCase(params string[] str)
        {
            int index = m_Text.IndexOfAny(str, m_Pos, StringComparison.CurrentCultureIgnoreCase);
            if (index < 0) index = m_Text.Length;

            string result = m_Text.Substring(m_Pos, index - m_Pos);
            Advance(index - m_Pos);
            return result;
        }

        public string TryMatchRegex(string regex)
        {
            Match m = Regex.Match(m_Text.Substring(m_Pos), regex);
            if (m.Success)
            {
                if (m.Index != 0) throw new ArgumentException("Regex must start with ^.");
                if (m.Groups.Count <= 1) throw new ArgumentException("Regex must have at least one (capture group).");
                Advance(m.Groups[1].Length);
                return m.Groups[1].Value;
            }
            else
                return "";
        }

        public char ReadChar()
        {
            if (m_Pos == m_Text.Length)
                throw new EndOfStreamException();
            char result = m_Text[m_Pos];
            Advance(1);
            return result;
        }

        private static bool[] JavaScriptIdentifierFirst = BuildUnicodeAcceptor(
            UnicodeCategory.UppercaseLetter,
            UnicodeCategory.LowercaseLetter,
            UnicodeCategory.TitlecaseLetter,
            UnicodeCategory.ModifierLetter,
            UnicodeCategory.LetterNumber);

        private static bool[] JavaScriptIdentifierRest = BuildUnicodeAcceptor(
            JavaScriptIdentifierFirst,
            UnicodeCategory.NonSpacingMark,
            UnicodeCategory.SpacingCombiningMark,
            UnicodeCategory.DecimalDigitNumber,
            UnicodeCategory.ConnectorPunctuation);

        private static bool[] BuildUnicodeAcceptor(bool[] basedOn, params UnicodeCategory[] accept)
        {
            bool[] result = (bool[])basedOn.Clone();
            foreach (UnicodeCategory cat in accept)
                result[(int)cat] = true;
            return result;
        }

        private static bool[] BuildUnicodeAcceptor(params UnicodeCategory[] accept)
        {
            bool[] result = new bool[30];
            foreach (UnicodeCategory cat in accept)
                result[(int)cat] = true;
            return result;
        }

        public string TryMatchJavaScriptIdentifier()
        {
            int pos = m_Pos;
            bool first = true;
            while (pos < m_Text.Length)
            {
                char ch = m_Text[pos];
                int cat = (int)char.GetUnicodeCategory(ch);
                bool[] acceptor = first ? JavaScriptIdentifierFirst : JavaScriptIdentifierRest;
                if ((cat >= 0 && cat < acceptor.Length && acceptor[cat])
                    || ch == '$' || ch == '_')
                    pos++;
                else 
                    break;
                first = false;
            }
            string result = m_Text.Substring(m_Pos, pos - m_Pos);
            Advance(pos - m_Pos);
            return result;
        }

        public string TryMatchJavaScriptNumber()
        {
            int pos = m_Pos;
            if (PeekString("0x") || PeekString("0X"))
            {
                while (pos < m_Text.Length)
                {
                    char ch = char.ToLower(m_Text[pos]);
                    if (char.IsDigit(ch) || (ch >= 'a' && ch <= 'f'))
                        pos++;
                    else
                        break;
                }
                if (pos - m_Pos <= 2) return "";
            }
            else
            {
                if (m_Text[pos] == '-' || m_Text[pos] == '+')
                    pos++;
                bool first = true;
                for (; pos < m_Text.Length; pos++)
                    if (char.IsDigit(m_Text[pos]))
                        first = false;
                    else
                        break;
                if (m_Text[pos] == '.')
                    pos++;
                for (; pos < m_Text.Length; pos++)
                    if (char.IsDigit(m_Text[pos]))
                        first = false;
                    else
                        break;
                if (first) return "";
                if (m_Text[pos] == 'E' || m_Text[pos] == 'e')
                {
                    pos++;
                    if (m_Text[pos] == '+' || m_Text[pos] == '-')
                        pos++;
                    for (; pos < m_Text.Length; pos++)
                        if (!char.IsDigit(m_Text[pos]))
                            break;
                }
            }
            string result = m_Text.Substring(m_Pos, pos - m_Pos);
            Advance(pos - m_Pos);
            return result;
        }
    }

    public class TextStreamException : ApplicationException
    {
        public int Position { get; set; }

        public TextStreamException(string message, int pos) 
            : base(message)
        {
            Position = pos;
        }
    }
}
