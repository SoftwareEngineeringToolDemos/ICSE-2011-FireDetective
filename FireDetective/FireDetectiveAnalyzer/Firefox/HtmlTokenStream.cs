using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FireDetectiveAnalyzer.Firefox
{
    public class HtmlTokenStream
    {
        private TextStream m_Text;
        private bool inScript = false;

        public HtmlTokenStream(TextStream text)
        {            
            m_Text = text;
        }

        public HtmlToken ReadToken()
        {
            int pos = m_Text.Position;
            if (inScript)
            {
                string src = m_Text.MatchUntilStringIgnoreCase("</script>");
                inScript = false;
                return new HtmlToken(src, HtmlTokenType.Script, GetSpan(pos));
            }
            else
            {
                string str = m_Text.MatchUntilStringIgnoreCase("<");
                if (str.Length > 0) return new HtmlToken(str, HtmlTokenType.Text, GetSpan(pos));

                string tag = m_Text.MatchString("<");
                if (m_Text.PeekString("!--"))
                {
                    tag += m_Text.MatchString("!--");
                    tag += m_Text.MatchUntilStringIgnoreCase("-->");
                    tag += m_Text.MatchString("-->");
                    return new HtmlToken(tag, HtmlTokenType.Tag, GetSpan(pos));
                }

                do
                {
                    tag += m_Text.MatchUntilStringIgnoreCase(">", "\"", "'");
                    string ch = m_Text.TryMatchAnyString("\"", "'");
                    if (ch != "")
                    {
                        ch += "\n";
                        tag += ch + m_Text.TryMatchRegex("^([^" + ch + "]*[" + ch + "])");
                    }
                }
                while (!m_Text.TryMatchString(">"));
                tag += ">";

                //if (Regex.IsMatch(tag, @"^<script\s", RegexOptions.IgnoreCase))
                if (tag.StartsWith("<script", StringComparison.CurrentCultureIgnoreCase) && tag.Length >= 8 && (char.IsWhiteSpace(tag[7]) || tag[7] == '>'))
                    inScript = true;
                return new HtmlToken(tag, HtmlTokenType.Tag, GetSpan(pos));
            }
        }

        private SimpleTextSpan GetSpan(int pos)
        {
            return new SimpleTextSpan(pos, m_Text.Position);
        }
    }

    public struct HtmlToken
    {
        public string Value;
        public HtmlTokenType Type;
        public SimpleTextSpan Location;
        public HtmlToken(string value, HtmlTokenType type, SimpleTextSpan location) { Value = value; Type = type; Location = location; }

        public static HtmlTokenStartComparer StartComparer = new HtmlTokenStartComparer();
        public static HtmlTokenEndComparer EndComparer = new HtmlTokenEndComparer();
    }

    public class HtmlTokenStartComparer : IComparer<HtmlToken>
    {
        public int Compare(HtmlToken x, HtmlToken y) { return x.Location.Start.CompareTo(y.Location.Start); }
    }

    public class HtmlTokenEndComparer : IComparer<HtmlToken>
    {
        public int Compare(HtmlToken x, HtmlToken y) { return x.Location.End.CompareTo(y.Location.End); }
    }

    public enum HtmlTokenType
    {
        None = 0,
        Text,
        Tag,
        Script
    }
}
