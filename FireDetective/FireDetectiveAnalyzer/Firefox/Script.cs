using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FireDetectiveAnalyzer.Firefox
{
    public class Script
    {
        public string Name { get; set; }
        public bool IsTopLevelEval { get; private set; }

        public ContentItem DefinitionContentItem { get; private set; }
        public JavaScriptBlock DefinitionBlock { get; private set; }
        public TextSpan DefinitionLocation { get; private set; }
        public TextSpan DefinitionLocationGuess { get; private set; }
        public bool IsNative { get; private set; }

        public int LineDelta { get; private set; }
        private bool m_ResolveLocation;
        private bool m_ResolveLineNumbers;
        private int m_UnresolvedStartLine;
        private int m_UnresolvedEndLine;

        public bool IsDojoJQuery { get { return DefinitionContentItem.IsDojoJQuery; } }

        public Script(string unresolvedName, ContentItem ci)
        {
            Name = unresolvedName;
            DefinitionContentItem = ci;
        }

        public Script(string unresolvedName, bool isTopLevelEval, ContentItem ci)
            : this(unresolvedName, ci)
        {
            IsTopLevelEval = isTopLevelEval;
        }

        public void SetPosInfoUnknown()
        {
            m_ResolveLocation = false;
        }

        public void SetPosInfo(int start, int end, int delta)
        {
            m_ResolveLocation = false;
            DefinitionLocation = new TextSpan(DefinitionContentItem.Document, start, end);
            LineDelta = delta;
        }

        public void SetPosInfoAll(int delta)
        {
            m_ResolveLocation = true;
            m_ResolveLineNumbers = false;
            LineDelta = delta;
        }

        public void SetLineInfo(int start, int end, int delta)
        {
            m_ResolveLocation = true;
            m_ResolveLineNumbers = true;
            m_UnresolvedStartLine = start + delta;
            m_UnresolvedEndLine = end + delta;
            LineDelta = delta;            
        }

        public void Resolve()
        {
            TextDocument doc = DefinitionContentItem.Document;

            // Resolve name
            if (Name.StartsWith("wf_"))
            {
                if (Name == "wf_newSendMethod")
                    Name = "XmlHttpRequest.send";
                else if (Name == "wf_newEvalMethod")
                    Name = "window.eval";
                else if (Name == "wf_newSetIntervalMethod")
                    Name = "window.setInterval";
                else if (Name == "wf_newSetTimeoutMethod")
                    Name = "window.setTimeout";
                else if (Name == "wf_newClearIntervalMethod")
                    Name = "window.clearInterval";
                else if (Name == "wf_newClearTimeoutMethod")
                    Name = "window.clearTimeout";
                DefinitionBlock = null;
                DefinitionLocation = null;
                DefinitionLocationGuess = null;
                m_ResolveLocation = false;
                IsNative = true;
            }

            // Locate definition if needed
            if (m_ResolveLocation)
            {
                if (m_ResolveLineNumbers)
                {
                    JavaScriptDocument jsd = DefinitionContentItem.GetJavaScriptDocumentForLine(m_UnresolvedStartLine);
                    if (jsd != null)
                    {
                        JavaScriptBlock block = jsd.GetNextFunctionForLine(m_UnresolvedStartLine);
                        if (block != null)
                        {
                            DefinitionBlock = block;
                            DefinitionBlock.ResolvedScript = this;
                            if (block.IsNamedFunction(false))
                            {
                                Name = block.GetAnonymousFunctionName();
                            }
                            return;
                        }
                    }

                    // Definition lookup failed. There may be different reasons.
                    // TODO: Hack to get this to work now, should set flag or something to indicate artificially generated content item
                    if (DefinitionContentItem.OriginalUrl == null)
                    {
                        // Artifically generated content item.
                        // Most likely, this is the execution of a function inside a DOM event handler that we were not able to get exact position info for
                        // (because, for instance, the handler was added dynamically after the page was constructed). 
                        // Or it's a function defined inside an eval expression (basically the same thing) and lookup has failed.
                        // Note: this cannot be a top-level script, the firefox plugin does not send line numbers for those and m_ResolveLineNumbers cannot be true.
                        DefinitionLocationGuess = new TextSpan(doc, 0, doc.FullText.Length); // We really don't know
                        if (Name == "") Name = "<unknown3>";
                    }
                    else if (DefinitionContentItem.JavaScriptDocument != null)
                    {
                        // Script in JS file.
                        if (m_UnresolvedStartLine == 0)
                        {
                            // Top-level script.
                            DefinitionBlock = DefinitionContentItem.JavaScriptDocument.Block;
                            DefinitionBlock.ResolvedScript = this;
                            Name = "<toplevel>";
                        }
                        else
                        {
                            // Shouldn't happen (it means our parser is broken).
                            DefinitionLocationGuess = new TextSpan(doc, doc.GetIndexOfLine(m_UnresolvedStartLine), doc.GetIndexOfLine(m_UnresolvedEndLine));
                            Name += " (<unknown2>)";
                        }
                    }
                    else
                    {
                        // Script from HTML file
                        if (jsd != null)
                        {
                            // Top-level execution of an embedded script.
                            DefinitionBlock = jsd.Block;
                            DefinitionBlock.ResolvedScript = this;
                            Name = "<toplevel>";
                        }
                        else
                        {
                            // No script found in this part of the HTML file, best guess.
                            DefinitionLocationGuess = new TextSpan(doc, doc.GetIndexOfLine(m_UnresolvedStartLine), doc.GetIndexOfLine(m_UnresolvedEndLine));
                            if (Name == "") Name = "<unknown>";
                        }
                    }
                }
                else
                {
                    if (DefinitionContentItem.JavaScriptDocument != null)
                    {
                        DefinitionBlock = DefinitionContentItem.JavaScriptDocument.Block;
                        DefinitionBlock.ResolvedScript = this;
                    }
                    else
                        throw new ApplicationException("Resolve all src without java script document.");
                }
            }
        }
    }
}
