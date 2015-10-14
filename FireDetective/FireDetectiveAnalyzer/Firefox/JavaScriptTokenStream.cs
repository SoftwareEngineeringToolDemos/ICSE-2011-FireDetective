using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FireDetectiveAnalyzer.Firefox
{
    public class JavaScriptTokenStream
    {
        private TextStream m_Text;
        private JavaScriptToken m_LastToken = new JavaScriptToken("", JavaScriptTokenType.Garbage, new SimpleTextSpan());

        public JavaScriptTokenStream(TextStream text)
        {
            m_Text = text;
        }

        public JavaScriptToken ReadToken()
        {
            JavaScriptToken t = ReadTokenForReal();
            if (t.Type != JavaScriptTokenType.Comment && t.Type != JavaScriptTokenType.MultiLineComment)
                m_LastToken = t;
            return t;
        }

        private JavaScriptToken ReadTokenForReal()
        {
            m_Text.MatchWhiteSpace();
            int pos = m_Text.Position;

            // Comments
            if (m_Text.TryMatchString("/*"))
            {                
                string cm = "/*";
                cm += m_Text.MatchUntilStringIgnoreCase("*/");
                cm += m_Text.MatchString("*/");
                return new JavaScriptToken(cm, JavaScriptTokenType.MultiLineComment, GetSpan(pos));
            }
            
            if (m_Text.TryMatchString("//"))
            {
                string comment = "//" + m_Text.MatchUntilStringIgnoreCase("\n");
                return new JavaScriptToken(comment, JavaScriptTokenType.Comment, GetSpan(pos));
            }

            // From ecma-script spec, doesn't match \u.... yet.
            string id = m_Text.TryMatchJavaScriptIdentifier();
            if (id != "")
            {
                string[] keywords = new string[] {
                    "break", "else", "new", "var",
                    "case", "finally", "return", "void",
                    "catch", "for", "switch", "while",
                    "continue", "function", "this", "with",
                    "default", "if", "throw", 
                    "delete", "in", "try",
                    "do", "instanceof", "typeof",
                    "debugger", "import", "export",
                    "undefined", "null", "true", "false" };
                if (keywords.Any(k => k == id))
                    return new JavaScriptToken(id, JavaScriptTokenType.Keyword, GetSpan(pos));
                else
                    return new JavaScriptToken(id, JavaScriptTokenType.Identifier, GetSpan(pos));
            }

            // Regular expressions
            if (m_Text.PeekString("/"))
            {
                if (m_LastToken.Type == JavaScriptTokenType.Number || m_LastToken.Type == JavaScriptTokenType.Identifier ||
                    (m_LastToken.Type == JavaScriptTokenType.Operator && (m_LastToken.Value == ")" || m_LastToken.Value == "]")))
                {
                    // Division operator
                }
                else
                {
                    // Regex start
                    string regex = m_Text.MatchString("/");
                    bool escaped = false;
                    bool isBreak = false;
                    do
                    {
                        regex += m_Text.MatchUntilStringIgnoreCase("/", "\n");
                        if (m_Text.PeekString("\n"))
                        {
                            isBreak = true; // We made the wrong guess here, minor TODO: go back to before reading the / symbol and parse it as a division operator.
                            throw new ApplicationException("Regex parser");
                            //break;
                        }
                        else
                        {
                            escaped = IsNextCharEscaped(regex);
                            regex += m_Text.MatchString("/");
                        }
                    }
                    while (escaped);

                    if (!isBreak)
                    {
                        string match;
                        while ((match = m_Text.TryMatchAnyString("g", "i", "m")) != "")
                            regex += match;
                    }

                    return new JavaScriptToken(regex, JavaScriptTokenType.Regex, GetSpan(pos));
                }
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
                return new JavaScriptToken(str, JavaScriptTokenType.String, GetSpan(pos));
            }
                
            // Numbers
            string number = m_Text.TryMatchJavaScriptNumber();
            if (number != "")
            {
                return new JavaScriptToken(number, JavaScriptTokenType.Number, GetSpan(pos));
            }

            // Operators
            string[] punc = new string[] { 
                "{", "}", "(", ")", "[", "]", 
                ".", ";", ",", "<", ">", "<=" , 
                ">=", "==", "!=", "===", "!==",
                "+", "-", "*", "%", "++", "--",
                "<<", ">>", ">>>", "&", "|", "^",
                "!", "~", "&&", "||", "?", ":",
                "=", "+=", "-=", "*=", "%=", "<<=",
                ">>=", ">>>=", "&=", "|=", "^=",
                "/", "/=" };
            string op = m_Text.TryMatchAnyString(punc);
            if (op != "")
            {
                return new JavaScriptToken(op, JavaScriptTokenType.Operator, GetSpan(pos));
            }

            return new JavaScriptToken(m_Text.ReadChar().ToString(), JavaScriptTokenType.Garbage, GetSpan(pos));
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

    public struct JavaScriptToken
    {
        public string Value;
        public JavaScriptTokenType Type;
        public SimpleTextSpan Location;
        public JavaScriptToken(string value, JavaScriptTokenType type, SimpleTextSpan location) { Value = value; Type = type; Location = location; }

        public static JavaScriptTokenStartComparer StartComparer = new JavaScriptTokenStartComparer();
        public static JavaScriptTokenEndComparer EndComparer = new JavaScriptTokenEndComparer();
    }

    public class JavaScriptTokenStartComparer : IComparer<JavaScriptToken>
    {
        public int Compare(JavaScriptToken x, JavaScriptToken y) { return x.Location.Start.CompareTo(y.Location.Start); }
    }

    public class JavaScriptTokenEndComparer : IComparer<JavaScriptToken>
    {
        public int Compare(JavaScriptToken x, JavaScriptToken y) { return x.Location.End.CompareTo(y.Location.End); }
    }

    public enum JavaScriptTokenType
    {
        Comment,
        MultiLineComment,
        Identifier,
        Keyword,
        Operator,
        String,
        Number,
        Regex,
        Garbage
    }

}
