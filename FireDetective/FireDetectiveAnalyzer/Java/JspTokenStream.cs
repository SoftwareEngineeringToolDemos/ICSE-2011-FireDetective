using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FireDetectiveAnalyzer.Java
{
    public class JspTokenStream
    {
        private TextStream m_Text;

        public JspTokenStream(TextStream text)
        {            
            m_Text = text;
        }

        public IEnumerable<JspToken> ReadTokens()
        {
            while (true)
            {
                int pos = m_Text.Position;
                string val = m_Text.MatchUntilStringIgnoreCase("<%", "<jsp:", "${", "#{");
                if (val != "")
                    yield return new JspToken(val, JspTokenType.Text, GetSpan(pos));

                pos = m_Text.Position;
                string which = m_Text.TryMatchAnyString("<%", "<jsp:", "${", "#{");
                if (which == "<%")
                {
                    yield return new JspToken(which, JspTokenType.Text, GetSpan(pos));

                    pos = m_Text.Position;
                    if (m_Text.PeekString("--"))
                        yield return new JspToken(m_Text.MatchUntilStringIgnoreCase("%>"), JspTokenType.TemplateComment, GetSpan(pos));
                    else
                        yield return new JspToken(MatchUntilStringSkipStringLiterals("%>"), JspTokenType.TemplateCode, GetSpan(pos));
                }
                else if (which == "<jsp:")
                {
                    yield return new JspToken(MatchUntilStringSkipStringLiterals(">") + m_Text.MatchString(">"), JspTokenType.TemplateOtherColored, GetSpan(pos));
                }
                else if (which == "${" || which == "#{")
                {
                    yield return new JspToken(which, JspTokenType.TemplateOtherColored, GetSpan(pos));

                    pos = m_Text.Position;
                    yield return new JspToken(MatchUntilStringSkipStringLiterals("}"), JspTokenType.TemplateCode, GetSpan(pos));

                    pos = m_Text.Position;
                    yield return new JspToken(m_Text.MatchString("}"), JspTokenType.TemplateOtherColored, GetSpan(pos));
                }
            }
        }

        private string MatchUntilStringSkipStringLiterals(string end)
        {
            string result = "";
            do
            {
                result += m_Text.MatchUntilStringIgnoreCase(end, "\"", "'");
                string ch = m_Text.TryMatchAnyString("\"", "'");
                if (ch != "")
                {
                    ch += "\n";
                    result += ch + m_Text.TryMatchRegex("^([^" + ch + "]*[" + ch + "])");
                }
            }
            while (!m_Text.PeekString(end) && !m_Text.EndOfStream);
            return result;
        }

        private SimpleTextSpan GetSpan(int pos)
        {
            return new SimpleTextSpan(pos, m_Text.Position);
        }
    }

    public struct JspToken
    {
        public string Value;
        public JspTokenType Type;
        public SimpleTextSpan Location;
        public JspToken(string value, JspTokenType type, SimpleTextSpan location) { Value = value; Type = type; Location = location; }

        //public static JspTokenStartComparer StartComparer = new JspTokenStartComparer();
        //public static JspTokenEndComparer EndComparer = new JspTokenEndComparer();
    }

    public enum JspTokenType
    {
        Text,
        TemplateComment,
        TemplateCode,
        TemplateOther,
        TemplateOtherColored
    }
}
