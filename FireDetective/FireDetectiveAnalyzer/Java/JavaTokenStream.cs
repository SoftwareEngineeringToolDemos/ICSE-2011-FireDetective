using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FireDetectiveAnalyzer.Java
{
    public class JavaTokenStream
    {
        private TextStream m_Text;
        private JavaToken m_LastToken = new JavaToken("", JavaTokenType.Garbage, new SimpleTextSpan());

        public JavaTokenStream(TextStream text)
        {
            m_Text = text;
        }

        public JavaToken ReadToken()
        {
            m_Text.MatchWhiteSpace();
            int pos = m_Text.Position;

            // Comments
            if (m_Text.TryMatchString("/*"))
            {
                string cm = "/*";
                cm += m_Text.MatchUntilStringIgnoreCase("*/");
                cm += m_Text.MatchString("*/");
                return new JavaToken(cm, JavaTokenType.MultiLineComment, GetSpan(pos));
            }

            if (m_Text.TryMatchString("//"))
            {
                string comment = "//" + m_Text.MatchUntilStringIgnoreCase("\n");
                return new JavaToken(comment, JavaTokenType.Comment, GetSpan(pos));
            }

            // Java identifiers... pretty much similar to JavaScript.
            string id = m_Text.TryMatchJavaScriptIdentifier();
            if (id != "")
            {
                string[] keywords = new string[] {
                    "abstract", "continue", "for", "new", "switch",
                    "assert", "default", "goto", "package", "synchronized",
                    "boolean", "do", "if", "private", "this",
                    "break", "double", "implements", "protected", "throw",
                    "byte", "else", "import", "public", "throws",
                    "case", "enum", "instanceof", "return", "transient",
                    "catch", "extends", "int", "short", "try",
                    "char", "final", "interface", "static", "void",
                    "class", "finally", "long", "strictfp", "volatile",
                    "const", "float", "native", "super", "while",
                    "true", "false", "null" };
                if (keywords.Any(k => k == id))
                    return new JavaToken(id, JavaTokenType.Keyword, GetSpan(pos));
                else
                    return new JavaToken(id, JavaTokenType.Identifier, GetSpan(pos));
            }

            // String literals
            string ch = m_Text.TryMatchAnyString("\"", "'");
            if (ch != "")
            {
                string str = ch;
                bool escaped = false;
                do
                {
                    str += m_Text.MatchUntilStringIgnoreCase(ch);
                    escaped = IsNextCharEscaped(str);
                    str += m_Text.MatchString(ch);
                }
                while (escaped);
                return new JavaToken(str, JavaTokenType.String, GetSpan(pos));
            }

            // Numbers
            string number = m_Text.TryMatchJavaScriptNumber();
            if (number != "")
            {
                return new JavaToken(number, JavaTokenType.Number, GetSpan(pos));
            }

            // Operators
            string[] punc = new string[] { 
                "{", "}", "(", ")", "[", "]", 
                ".", ";", ",", "<", ">", "<=" , 
                ">=", "==", "!=",
                "+", "-", "*", "%", "++", "--",
                "<<", ">>", ">>>", "&", "|", "^",
                "!", "~", "&&", "||", "?", ":",
                "=", "+=", "-=", "*=", "%=", "<<=",
                ">>=", ">>>=", "&=", "|=", "^=",
                "/", "/=" };
            string op = m_Text.TryMatchAnyString(punc);
            if (op != "")
            {
                return new JavaToken(op, JavaTokenType.Operator, GetSpan(pos));
            }

            return new JavaToken(m_Text.ReadChar().ToString(), JavaTokenType.Garbage, GetSpan(pos));
        }

        private bool IsNextCharEscaped(string s)
        {
            int count = 0;
            for (int i = s.Length - 1; i >= 0 && s[i] == '\\'; i--)
                count++;
            return (count % 2) == 1;
        }

        private SimpleTextSpan GetSpan(int pos)
        {
            return new SimpleTextSpan(pos, m_Text.Position);
        }
    }

    public struct JavaToken
    {
        public string Value;
        public JavaTokenType Type;
        public SimpleTextSpan Location;
        public JavaToken(string value, JavaTokenType type, SimpleTextSpan location) { Value = value; Type = type; Location = location; }

        public static JavaTokenStartComparer StartComparer = new JavaTokenStartComparer();
        public static JavaTokenEndComparer EndComparer = new JavaTokenEndComparer();
    }

    public class JavaTokenStartComparer : IComparer<JavaToken>
    {
        public int Compare(JavaToken x, JavaToken y) { return x.Location.Start.CompareTo(y.Location.Start); }
    }

    public class JavaTokenEndComparer : IComparer<JavaToken>
    {
        public int Compare(JavaToken x, JavaToken y) { return x.Location.End.CompareTo(y.Location.End); }
    }

    public enum JavaTokenType
    {
        Comment,
        MultiLineComment,
        Identifier,
        Keyword,
        Operator,
        String,
        Number,
        Garbage
    }
}
