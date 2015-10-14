using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace FireDetectiveAnalyzer
{
    public partial class CodeViewer : Form
    {
        private Firefox.PageRequest m_CurrentPage;
        private Java.TopLevelTrace m_CurrentTrace;

        private Java.JavaWorkspace m_Workspace;
        private HashSet<Java.JavaFile> m_VisibleFiles;

        private Dictionary<object, ExhibitInfo> m_ExhibitCache = new Dictionary<object, ExhibitInfo>();
        private object m_CurrentExhibit;

        public event EventHandler<CustomEventArgs<Firefox.Script>> HighlightScriptUsage;
        public event EventHandler<CustomEventArgs<Java.Method>> HighlightMethodUsage;

        public Point MoveHackWindowPosition { get; set; }

        private class ExhibitInfo
        {
            public string Title { get; set; }
            public FormattedTextDocument Document { get; set; }
            public int CurrentLine { get; set; }

            public ExhibitInfo(string title, FormattedTextDocument doc)
            {
                Title = title;
                Document = doc;
                CurrentLine = 0;
            }
        }

        private static readonly FormattedTextDocument m_NoSourceDocument = new FormattedTextDocument(new TextDocument("[No source available]"));

        public CodeViewer()
        {
            MoveHackWindowPosition = Point.Empty;
            InitializeComponent();
        }

        public CodeViewer(Java.JavaWorkspace workspace)
            : this()
        {
            m_Workspace = workspace;
            SetPageAndTrace(null, null);
            treeViewFiles.EnsureNodeSelected();
            splitContainer1.SplitterDistance = 250;
            DisplayEmpty();
        }

        private bool m_IgnoreAfterSelectEvent;

        public void ShowNoSelect()
        {
            m_IgnoreAfterSelectEvent = true;
            Show();
            m_IgnoreAfterSelectEvent = false;
        }

        private void LocateInTree(TreeItem item)
        {
            List<TreeItem> path = item.GetPath(new TreeViewFilesRootItem(m_CurrentPage), true);
            m_IgnoreAfterSelectEvent = true;
            if (path.Count > 0)
                treeViewFiles.SelectStack(path);
            m_IgnoreAfterSelectEvent = false;
        }

        private void DisplayExhibit(object exhibit)
        {
            ExhibitInfo info = m_ExhibitCache[exhibit];
            Text = info.Title;
            codeView1.FormattedDocument = info.Document;
            codeView1.ScrollTo(info.CurrentLine);
            m_CurrentExhibit = exhibit;
        }

        private void UndisplayCurrentExhibit()
        {
            if (m_CurrentExhibit != null)
            {
                m_ExhibitCache[m_CurrentExhibit].CurrentLine = codeView1.CurrentLine;
                m_CurrentExhibit = null;
            }
        }

        private void DisplayEmpty()
        {
            UndisplayCurrentExhibit();
            Text = "FireDetective Code Viewer";
            codeView1.FormattedDocument = null;
        }

        private void DisplayNoSource()
        {
            UndisplayCurrentExhibit();
            Text = "No source available";
            codeView1.FormattedDocument = m_NoSourceDocument;
        }

        private void DisplayNoSourceFor(string str)
        {
            UndisplayCurrentExhibit();
            Text = "No source available";
            codeView1.FormattedDocument = new FormattedTextDocument(new TextDocument("[No source available for: " + str + "]"));
        }

        private void DisplayContentItem(Firefox.ContentItem ci)
        {
            UndisplayCurrentExhibit();
            if (!m_ExhibitCache.ContainsKey(ci))
            {
                var jsdoc = ci.JavaScriptDocument;
                var htdoc = ci.HtmlDocument;
                var doc = jsdoc != null ? new FormattedJavaScriptDocument(jsdoc) : htdoc != null ? new FormattedJavaScriptDocument(htdoc) : new FormattedTextDocument(ci.Document);
                if (doc == null) throw new ApplicationException("Cannot display content item without text content.");
                m_ExhibitCache.Add(ci, new ExhibitInfo(
                    ci.Url != null ? ci.Url.ToString() + " (dynamically generated)" : ci.OriginalUrl.ToString() + " (dynamically generated)",
                    doc));
            }
            DisplayExhibit(ci);
            LocateInTree(new ContentItemTreeItem(ci));
        }

        private void DisplayJspFile(Java.JspFile jsp)
        {
            UndisplayCurrentExhibit();
            if (!m_ExhibitCache.ContainsKey(jsp))
            {
                m_ExhibitCache.Add(jsp, new ExhibitInfo(
                    jsp.Name,
                    new FormattedJspDocument(jsp.Document)));
            }
            DisplayExhibit(jsp);
            LocateInTree(new JspFileTreeItem(jsp));
        }

        private void DisplayJavaCodeFile(Java.JavaCodeFile file)
        {
            UndisplayCurrentExhibit();
            if (!m_ExhibitCache.ContainsKey(file))
            {
                m_ExhibitCache.Add(file, new ExhibitInfo(
                    file.ShortPath,
                    new FormattedJavaDocument(file.Document)));
            }
            DisplayExhibit(file);
            LocateInTree(new JavaCodeFileTreeItem(file));
        }

        private void SetPageAndTrace(Firefox.PageRequest page, Java.TopLevelTrace trace)
        {
            m_CurrentPage = page;
            m_CurrentTrace = trace;
            chkFilterByCurrentPage.Enabled = page != null;
            chkFilterByCurrentTrace.Enabled = trace != null;
            UpdateVisibleFiles();
            UpdateTreeViewFiles();
        }

        public bool IsSourceAvailable(Firefox.Call call)
        {
            return call.Script != null && (call.Script.DefinitionContentItem.HtmlDocument != null || call.Script.DefinitionContentItem.JavaScriptDocument != null);
        }

        public void ShowPage(Firefox.PageRequest page)
        {
            SetPageAndTrace(page, null);
        }

        public void ShowContentItem(Firefox.PageRequest page, Firefox.ContentItem ci)
        {
            SetPageAndTrace(page, null);
            if (ci.Document != null)
                DisplayContentItem(ci);
            else
                DisplayNoSource();
        }

        public void ShowJavaScriptCall(Firefox.PageRequest page, Firefox.Call call)
        {
            SetPageAndTrace(page, null);
            if (call.Script != null && call.Script.IsNative)
            {
                DisplayNoSourceFor(call.Script.Name + " [native code]");
            }
            else if (call.IsFiltered)
            {
                DisplayNoSourceFor("filtered dojo/jquery code");
            }
            else if (call.Script != null && call.Script.DefinitionContentItem != null)
            {
                DisplayContentItem(call.Script.DefinitionContentItem);
                if (call.Script.DefinitionBlock != null)
                {
                    FormattedEntity e = (codeView1.FormattedDocument as FormattedJavaScriptDocument).BlockToEntity(call.Script.DefinitionBlock);
                    codeView1.SelectAndScrollToEntity(e);
                }
                else if (call.Script.DefinitionLocation != null)
                {
                    codeView1.SelectText(call.Script.DefinitionLocation.Start, call.Script.DefinitionLocation.End);
                    codeView1.SelectAndScrollToPseudoEntity(call.Script.DefinitionLocation);
                }
                else if (call.Script.DefinitionLocationGuess != null)
                {
                    codeView1.SelectAndScrollToPseudoEntity(call.Script.DefinitionLocationGuess);
                }
            }
            else
            {
                DisplayNoSource();
            }
        }

        public void ShowJavaCall(Firefox.PageRequest page, Java.TopLevelTrace trace, Java.Call call)
        {
            SetPageAndTrace(page, trace);
            if (call.Method != null && call.Method.IsJspExecution)
            {
                DisplayJspFile(call.Method.DefinitionFileJsp);
            }
            else if (call.Method != null && call.Method.DefinitionFile != null)
            {
                DisplayJavaCodeFile(call.Method.DefinitionFile);
                codeView1.SelectAndScrollToEntityAt(call.Method.DefinitionFile.Document.Original.GetSpanForLine(call.Method.DefinitionLine));
            }
            else if (call.Method != null)
            {
                DisplayNoSourceFor(call.Method.FullName);
            }
            else
            {
                DisplayNoSource();
            }
        }

        private void codeView1_HighlightEntityUsage(object sender, CodeViewEventArgs e)
        {
            HighlightEntityUsage(e.Entity);
        }

        private void btnWhereCalled_Click(object sender, EventArgs e)
        {
            if (codeView1.FormattedDocument is FormattedJspDocument || codeView1.SelectedEntity != null)
                HighlightEntityUsage(codeView1.SelectedEntity);
            else
                MessageBox.Show(
                    string.Format("Please select a {0} first, then click this button again.", (codeView1.FormattedDocument is FormattedJavaScriptDocument) ? "function" : "method"),
                    "FireDetective",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
        }

        private void HighlightEntityUsage(FormattedEntity e)
        {
            var jsdoc = codeView1.FormattedDocument as FormattedJavaScriptDocument;
            var jspdoc = codeView1.FormattedDocument as FormattedJspDocument;
            var jdoc = codeView1.FormattedDocument as FormattedJavaDocument;
            if (jsdoc != null)
            {
                if (HighlightScriptUsage != null)
                    HighlightScriptUsage(this, new CustomEventArgs<Firefox.Script>(jsdoc.EntityToBlock(e).ResolvedScript));
            }
            else if (jspdoc != null)
            {
                if (HighlightMethodUsage != null)
                    HighlightMethodUsage(this, new CustomEventArgs<Java.Method>(jspdoc.EntityToDocument(e).ResolvedMethod));
            }
            else if (jdoc != null)
            {
                if (HighlightMethodUsage != null)
                    HighlightMethodUsage(this, new CustomEventArgs<Java.Method>(jdoc.EntityToBlock(e).ResolvedMethod));
            }
        }

        public void UpdateVisibleFiles()
        {
            bool filterByPage = chkFilterByCurrentPage.Checked && chkFilterByCurrentPage.Enabled;
            bool filterByTrace = chkFilterByCurrentTrace.Checked && chkFilterByCurrentTrace.Enabled;
            m_VisibleFiles = filterByTrace ? m_CurrentTrace.JavaFilesHit : (filterByPage ? m_CurrentPage.JavaFilesHit : null);
        }

        public void UpdateContentItems()
        {
            UpdateTreeViewFiles();
        }

        private static CodeViewer this_outer;

        private void UpdateTreeViewFiles()
        {
            this_outer = this;
            new TreeViewFilesRootItem(m_CurrentPage).UpdateUsingChildNodes(treeViewFiles);
        }

        private void treeViewFiles_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            this_outer = this;
            (e.Node.Tag as TreeItem).UpdateAndExpand(e.Node);
        }

        private void chkFilterByCurrentPage_CheckedChanged(object sender, EventArgs e)
        {
            UpdateVisibleFiles();
            UpdateTreeViewFiles();
        }

        private void chkFilterByCurrentTrace_CheckedChanged(object sender, EventArgs e)
        {
            UpdateVisibleFiles();
            UpdateTreeViewFiles();
        }

        private void treeViewFiles_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
                treeViewFiles.ForceSelect(e.Node);
        }

        private void treeViewFiles_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (m_IgnoreAfterSelectEvent) return;
            var ti = treeViewFiles.SelectedNode.Tag;
            var tiContentItem = ti as ContentItemTreeItem;
            var tiJspFile = ti as JspFileTreeItem;
            var tiJavaCodeFile = ti as JavaCodeFileTreeItem;
            if (tiContentItem != null)
                DisplayContentItem(tiContentItem.Data);
            else if (tiJspFile != null)
                DisplayJspFile(tiJspFile.Data);
            else if (tiJavaCodeFile != null)
                DisplayJavaCodeFile(tiJavaCodeFile.Data);
            else
                DisplayEmpty();
        }

        private class TreeViewFilesRootItem : TreeItem<Firefox.PageRequest>
        {
            public TreeViewFilesRootItem(Firefox.PageRequest page) : base(page) { }
            public override IEnumerable<TreeItem> GetChildren()
            {
                yield return new DynamicCategoryTreeItem();
                yield return new ServerCategoryTreeItem();
                yield return new RelatedCategoryItem();
            }
        }

        private class DynamicCategoryTreeItem : TreeItem<int>
        {
            public DynamicCategoryTreeItem() : base(0) { }
            public override void CreateSingle(TreeNode node)
            {
                SetText(node, "Dynamically generated text content");
                SetImage(node, 0);
            }
            public override IEnumerable<TreeItem> GetChildren()
            {
                return this_outer.m_CurrentPage != null ? this_outer.m_CurrentPage.ContentItemsSafe.Where(ci => ci.Document != null)
                    .Select(ci => new ContentItemTreeItem(ci)).OfType<TreeItem>() : Enumerable.Empty<TreeItem>();
            }
        }

        private class RelatedCategoryItem : TreeItem<int>
        {
            public RelatedCategoryItem() : base(1) { }
            public override void CreateSingle(TreeNode node)
            {
                SetText(node, "Server jsp source");
                SetImage(node, 0);
            }
            public override IEnumerable<TreeItem> GetChildren()
            {
                var files = this_outer.m_Workspace.JspFiles;
                if (this_outer.m_VisibleFiles != null)
                    files = files.Where(file => this_outer.m_VisibleFiles.Contains(file));
                return files.Select(file => new JspFileTreeItem(file)).OfType<TreeItem>();
            }
        }

        private class JspFileTreeItem : TreeItem<Java.JspFile>
        {
            public JspFileTreeItem(Java.JspFile file) : base(file) { }
            public override void CreateSingle(TreeNode node)
            {
                SetText(node, m_Data.Name);
                SetImage(node, 3);
            }
        }

        private class ServerCategoryTreeItem : TreeItem<int>
        {
            public ServerCategoryTreeItem() : base(2) { }
            public override void CreateSingle(TreeNode node)
            {
                SetText(node, "Server java source");
                SetImage(node, 0);
            }
            public override IEnumerable<TreeItem> GetChildren()
            {
                IEnumerable<Java.JavaPackage> packages = this_outer.m_Workspace.Packages;
                if (this_outer.m_VisibleFiles != null)
                    packages = packages.Where(package => package.ContainsAnyOf(this_outer.m_VisibleFiles));
                return packages.OrderBy(package => package.Name).Select(package => new JavaPackageTreeItem(package)).OfType<TreeItem>();
            }
        }

        private class ContentItemTreeItem : TreeItem<Firefox.ContentItem>
        {
            public ContentItemTreeItem(Firefox.ContentItem ci) : base(ci) { }
            public override void CreateSingle(TreeNode node)
            {
                SetText(node, m_Data.ShortUrl);
                SetImage(node, 3);
            }
        }

        private class JavaPackageTreeItem : TreeItem<Java.JavaPackage>
        {
            public JavaPackageTreeItem(Java.JavaPackage package) : base(package) { }
            public override void CreateSingle(TreeNode node)
            {
                SetText(node, m_Data.Name);
                SetImage(node, 1);
            }
            public override IEnumerable<TreeItem> GetChildren()
            {
                IEnumerable<Java.JavaCodeFile> files = m_Data.Files;
                if (this_outer.m_VisibleFiles != null)
                    files = files.Where(file => this_outer.m_VisibleFiles.Contains(file));
                return files.Select(file => new JavaCodeFileTreeItem(file)).OfType<TreeItem>();
            }
        }

        private class JavaCodeFileTreeItem : TreeItem<Java.JavaCodeFile>
        {
            public JavaCodeFileTreeItem(Java.JavaCodeFile file) : base(file) { }
            public override void CreateSingle(TreeNode node)
            {
                SetText(node, m_Data.Name);
                SetImage(node, 2);                
            }
        }

        private void CodeViewer_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        {
            UpdateCodeViewSize();
        }

        private void UpdateCodeViewSize()
        {
            codeView1.Width = splitContainer1.Panel2.Width;
        }

        private void CodeViewer_Resize(object sender, EventArgs e)
        {
            UpdateCodeViewSize();
        }

        private void CodeViewer_Move(object sender, EventArgs e)
        {
            if (MoveHackWindowPosition != Point.Empty && Location != MoveHackWindowPosition)
                Location = MoveHackWindowPosition;
        }
    }
}
