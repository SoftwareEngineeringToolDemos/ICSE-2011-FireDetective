using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FireDetectiveAnalyzer.Java
{
    public class Method
    {
        public string Name { get; set; }
        public string ClassName { get; set; }
        public string PackageName { get; set; }
        public JavaCodeFile DefinitionFile { get; set; }
        public JavaMethodBlock DefinitionBlock { get; private set; }
        public JspFile DefinitionFileJsp { get; private set; }

        public bool IsJspExecution { get { return DefinitionFileJsp != null; } }

        public int DefinitionLine { get; set; } // Zero-based        

        public Method(string name, string className, string packageName, JavaCodeFile file, int line)
        {
            Name = name;
            ClassName = className;
            PackageName = packageName;
            DefinitionFile = file;
            DefinitionLine = line;
            Resolve();
        }

        public Method(string name, string className, string packageName, JspFile jsp)
            : this(name, className, packageName, null, 0)
        {
            DefinitionFileJsp = jsp;
        }

        public string FullName
        {
            get { return PackageName + "." + ClassName + "." + Name; }
        }

        private void Resolve()
        {
            if (DefinitionFile != null)
            {
                var doc = DefinitionFile.Document.Original;
                SimpleTextSpan line = new SimpleTextSpan(doc.GetIndexOfLine(DefinitionLine), doc.GetIndexOfLine(DefinitionLine + 1));
                var block = DefinitionFile.Document.Block.MethodBlocks.FirstOrDefault(mb => mb.Location.Intersects(line));
                if (block != null)
                {
                    block.ResolvedMethod = this;
                    DefinitionBlock = block;
                }
            }
        }

        public bool IsSourceAvailable
        {
            get { return DefinitionFile != null || DefinitionFileJsp != null; }
        }
    }
}
