using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace FireDetectiveAnalyzer.Java
{
    public interface JavaFile
    {
    }

    public class JavaCodeFile : JavaFile
    {
        private JavaDocument m_Document;
        public JavaPackage Package { get; set; }
        public string Name { get; set; }

        public JavaCodeFile(JavaPackage package, string name)
        {
            Package = package;
            Name = name;
        }

        public JavaDocument Document
        {
            get
            {
                if (m_Document == null)
                {
                    using (StreamReader sr = new StreamReader(Path))
                        m_Document = new JavaDocument(new TextDocument(sr.ReadToEnd()));
                }
                return m_Document;
            }
        }

        public string Path
        {
            get { return Package.Folder + @"\" + Name; }
        }

        public string ShortPath
        {
            get { return Package.Name.Replace(".", @"\") + @"\" + Name; }
        }
    }

    public class JspFile : JavaFile
    {
        private JspDocument m_Document;

        public string Name { get; set; }
        public string Folder { get; private set; }

        public JspFile(string name, string folder)
        {
            Name = name;
            Folder = folder;
        }

        public JspDocument Document
        {
            get
            {
                if (m_Document == null)
                {
                    using (StreamReader sr = new StreamReader(Folder + @"\" + Name))
                        m_Document = new JspDocument(new TextDocument(sr.ReadToEnd()));
                }
                return m_Document;
            }
        }

    }
}
