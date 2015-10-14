using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace FireDetectiveAnalyzer.Java
{
    public class JavaWorkspace
    {
        private Dictionary<string, JavaPackage> m_Packages = new Dictionary<string, JavaPackage>();
        private Dictionary<string, JspFile> m_JspFiles = new Dictionary<string, JspFile>();

        public IEnumerable<JavaPackage> Packages { get { return m_Packages.Values; } }
        public IEnumerable<JspFile> JspFiles { get { return m_JspFiles.Values; } }

        public JavaWorkspace(string[] folders, string[] jspFolders)
        {
            foreach (string folder in folders)
                foreach (JavaPackage package in GetPackagesInFolder(folder))
                    if (package.Name != "com.thechiselgroup.firedetective") // Filter internals
                        m_Packages.Add(package.Name, package);
            
            foreach (string folder in jspFolders)
                foreach (JspFile jsp in GetJspFilesInFolder(folder))
                    m_JspFiles.Add(jsp.Name, jsp);
        }

        private IEnumerable<JavaPackage> GetPackagesInFolder(string folder)
        {
            string[] dirs = Directory.GetDirectories(folder, "*.*", SearchOption.AllDirectories);
            foreach (string dir in dirs)
            {
                string[] files = Directory.GetFiles(dir, "*.java");
                if (files.Length > 0)
                {
                    string packageName = dir.Substring(folder.Length).Replace(@"\", ".");
                    JavaPackage package = new JavaPackage(packageName, dir);
                    package.AddFiles(files.Select(file => new JavaCodeFile(package, Path.GetFileName(file))));
                    yield return package;
                }
            }
        }

        private IEnumerable<JspFile> GetJspFilesInFolder(string folder)
        {
            string[] files = Directory.GetFiles(folder, "*.jsp");
            return files.Select(file => new JspFile(Path.GetFileName(file), folder));
        }

        public JavaPackage GetPackage(string packageName)
        {
            JavaPackage package;
            return m_Packages.TryGetValue(packageName, out package) ? package : null;
        }

        public JspFile GetJspFile(string jspName)
        {
            JspFile jsp;
            return m_JspFiles.TryGetValue(jspName, out jsp) ? jsp : null;
        }
    }

    public class JavaPackage
    {
        public string Name { get; set; }
        public string Folder { get; set; }

        private Dictionary<string, JavaCodeFile> m_Files = new Dictionary<string, JavaCodeFile>();

        public IEnumerable<JavaCodeFile> Files { get { return m_Files.Values; } }

        public JavaPackage(string name, string folder)
        {
            Name = name;
            Folder = folder;
        }

        public void AddFile(JavaCodeFile file)
        {
            m_Files.Add(file.Name, file);
        }

        public void AddFiles(IEnumerable<JavaCodeFile> files)
        {
            foreach (JavaCodeFile file in files)
                AddFile(file);
        }

        public JavaCodeFile GetFile(string filename)
        {
            JavaCodeFile file;
            return m_Files.TryGetValue(filename, out file) ? file : null;
        }

        public bool ContainsAnyOf(HashSet<JavaCodeFile> visible)
        {
            return Files.Any(file => visible.Contains(file));
        }

        // Waiting for co/contra variance in C# 4.0!
        public bool ContainsAnyOf(HashSet<JavaFile> visible)
        {
            return Files.Any(file => visible.Contains(file));
        }
    }
}
