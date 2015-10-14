using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace FireDetectiveAnalyzer
{
    public partial class MainForm : Form
    {
        private TraceProviderConnection m_Client, m_Server;
        private Firefox.FirefoxEventProcessor m_Firefox;
        private Java.JavaEventProcessor m_Java;

        private Firefox.PageRequestCollection m_Pages;
        private Java.JavaWorkspace m_Workspace;
        private CodeViewer m_CodeViewer;

        private ViewOptions m_ViewOptions = new ViewOptions();
        private HighlightedSet m_HighlightedSet = HighlightedSet.Empty;

        public MainForm()
        {
            m_Workspace = GetWorkspaceSettings();

            m_Pages = new Firefox.PageRequestCollection();

            m_CodeViewer = new CodeViewer(m_Workspace);
            m_CodeViewer.HighlightScriptUsage += new EventHandler<CustomEventArgs<FireDetectiveAnalyzer.Firefox.Script>>(m_CodeViewer_HighlightScriptUsage);
            m_CodeViewer.HighlightMethodUsage += new EventHandler<CustomEventArgs<FireDetectiveAnalyzer.Java.Method>>(m_CodeViewer_HighlightMethodUsage);

            InitializeComponent();

            UpdateHighlightBox();
        }

        private Java.JavaWorkspace GetWorkspaceSettings()
        {
            using (System.IO.StreamReader sr = new System.IO.StreamReader("settings.ini"))
            {
                return new Java.JavaWorkspace(
                    sr.ReadLine().Split('*').Select(s => s.Trim()).Where(s => s != "").ToArray(),
                    sr.ReadLine().Split('*').Select(s => s.Trim()).Where(s => s != "").ToArray());
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            m_Client = new TraceProviderConnection();
            m_Client.Closed += new EventHandler(m_Client_Closed);
            m_Client.IsBusy += new EventHandler(m_Client_IsBusy);

            ConnectFirefox();

            m_Server = new TraceProviderConnection();
            m_Server.Closed += new EventHandler(m_Server_Closed);

            ConnectServer();

            cmdView1_Click(this, EventArgs.Empty);
        }

        private void ConnectFirefox()
        {
            m_Firefox = new Firefox.FirefoxEventProcessor();
            m_Firefox.PageRequested += new EventHandler<CustomEventArgs<Firefox.PageRequest>>(firefox_PageRequested);

            var ok = m_Client.ConnectWithLocalFirefox(m_Firefox);
            UpdateFirefoxConnectionStatus(ok, false);
        }

        private void ConnectServer()
        {
            m_Java = new Java.JavaEventProcessor(m_Workspace);
            m_Java.ContentItemStarted += new EventHandler<CustomEventArgs<FireDetectiveAnalyzer.Java.TopLevelTrace>>(m_Java_ContentItemStarted);
            m_Java.ContentItemCompleted += new EventHandler<CustomEventArgs<Java.TopLevelTrace>>(java_ContentItemCompleted);

            var ok = m_Server.ConnectWithLocalJava(m_Java);
            UpdateServerConnectionStatus(ok);
        }

        private void m_Client_Closed(object sender, EventArgs e)
        {
            BeginInvoke(new Action(() =>
            {
                UpdateFirefoxConnectionStatus(false, false);
            }));
        }

        void m_Client_IsBusy(object sender, EventArgs e)
        {
            BeginInvoke(new Action(() =>
            {
                UpdateFirefoxConnectionStatus(false, true);
            }));
        }

        private void m_Server_Closed(object sender, EventArgs e)
        {
            BeginInvoke(new Action(() =>
            {
                UpdateServerConnectionStatus(false);
            }));
        }

        private void cmdFirefoxReconnect_Click(object sender, EventArgs e)
        {
            ConnectFirefox();
        }

        private void cmdServerReconnect_Click(object sender, EventArgs e)
        {
            ConnectServer();
        }

        private void firefox_PageRequested(object sender, CustomEventArgs<Firefox.PageRequest> e)
        {
            BeginInvoke(new Action(() =>
            {
                OnNewPage(e);
            }));
        }

        private void m_Java_ContentItemStarted(object sender, CustomEventArgs<Java.TopLevelTrace> e)
        {
            BeginInvoke(new Action(() =>
            {
                picSpinner.Visible = m_Java.HasOutstandingRequests;
                //m_Pages.MatchContentItemServerTrace(e.Data);
            }));
        }

        private void java_ContentItemCompleted(object sender, CustomEventArgs<Java.TopLevelTrace> e)
        {
            BeginInvoke(new Action(() =>
            {
                picSpinner.Visible = m_Java.HasOutstandingRequests;
                m_Pages.MatchContentItemServerTrace(e.Data); //// This is not strictly necessary, but the first match might have failed. TODO: build a decent solution.
                m_CodeViewer.UpdateVisibleFiles();
            }));
        }

        private void UpdateFirefoxConnectionStatus(bool ok, bool busy)
        {
            UpdateConnectionStatus(cmdFirefoxReconnect, lblFirefoxConnection, "Firefox", ok, busy);
        }

        private void UpdateServerConnectionStatus(bool ok)
        {
            UpdateConnectionStatus(cmdServerReconnect, lblServerConnection, "Server", ok, false);
            picSpinner.Visible = false;            
        }

        private void UpdateConnectionStatus(Button reconnect, Label label, string which, bool ok, bool busy)
        {
            reconnect.Enabled = !ok;
            label.Text = which + ": " + (ok ? "connected" : (busy ? "not connected (busy)" : "not connected"));
            label.ForeColor = ok ? Color.Black : Color.Red;
        }

        private void OnNewPage(CustomEventArgs<Firefox.PageRequest> e)
        {
            m_Pages.Add(e.Data);
            UpdateTreeViewPagesAuto();

            // This might have left us in a state where no node is selected in the left pane.
            if (treeViewPages.SelectedNode == null)
            {
                treeViewPages.SelectNodeOverrideAfterSelect(treeViewPages.Nodes[0], new Action<TreeViewEventArgs>(
                    (ea) => { ShowPage((ea.Node.Tag as PageRequestTreeItem).Data); }
                ));
            }
        }

        private void btnShowCodePane_Click(object sender, EventArgs e)
        {
            if (m_CodeViewer.WindowState == FormWindowState.Minimized)
                m_CodeViewer.WindowState = FormWindowState.Normal;
            m_CodeViewer.ShowNoSelect();
            m_CodeViewer.BringToFront();
            m_CodeViewer.Focus();
        }

        public bool m_View1_Active = true;

        private bool m_Adjusting = false;

        private void cmdView1_Click(object sender, EventArgs e)
        {
            m_Adjusting = true;

            Rectangle desktop = Screen.GetWorkingArea(this);
            
            m_View1_Active = true;
            m_CodeViewer.MoveHackWindowPosition = new Point(0, desktop.Height / 2);
            WindowState = FormWindowState.Normal;
            m_CodeViewer.ShowInTaskbar = false;
            m_CodeViewer.ControlBox = false;
            MaximizeBox = false;
            AddOwnedForm(m_CodeViewer);

            SetBounds(0, 0, desktop.Width, desktop.Height / 2);
            WindowState = FormWindowState.Normal;
            m_CodeViewer.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            m_CodeViewer.SetBounds(0, desktop.Height / 2, desktop.Width, desktop.Height - desktop.Height / 2);
            m_CodeViewer.WindowState = FormWindowState.Normal;

            m_CodeViewer.ShowNoSelect();
            m_CodeViewer.BringToFront();
            Focus();

            m_Adjusting = false;
        }

        private void cmdView2_Click(object sender, EventArgs e)
        {
            m_Adjusting = true;

            m_View1_Active = false;
            RemoveOwnedForm(m_CodeViewer);
            m_CodeViewer.MoveHackWindowPosition = Point.Empty;
            m_CodeViewer.ShowInTaskbar = true;
            m_CodeViewer.ControlBox = true;
            MaximizeBox = true;

            SetBounds(1600, 0, 1600, 1200);
            WindowState = FormWindowState.Maximized;
            m_CodeViewer.FormBorderStyle = FormBorderStyle.Sizable;
            m_CodeViewer.SetBounds(125, 150, 1350, 900);
            if (m_CodeViewer.WindowState == FormWindowState.Minimized)
                m_CodeViewer.WindowState = FormWindowState.Normal;

            m_CodeViewer.ShowNoSelect();
            m_CodeViewer.BringToFront();
            Focus();

            m_Adjusting = false;
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (m_View1_Active && !m_Adjusting)
            {
                Rectangle desktop = Screen.GetWorkingArea(this);

                m_CodeViewer.WindowState = WindowState;
                if (WindowState == FormWindowState.Minimized)
                    return;

                int h = Height;
                m_CodeViewer.MoveHackWindowPosition = new Point(0, h);

                Location = new Point(0, 0);
                SetBounds(0, 0, desktop.Width, h);
                WindowState = FormWindowState.Normal;
                m_CodeViewer.FormBorderStyle = FormBorderStyle.FixedToolWindow;
                m_CodeViewer.SetBounds(0, h, desktop.Width, desktop.Height - h);
                m_CodeViewer.WindowState = FormWindowState.Normal;

                m_CodeViewer.ShowNoSelect();
                m_CodeViewer.BringToFront();
                Focus();
            }
        }

        private void MainForm_Activated(object sender, EventArgs e)
        {
            if (m_View1_Active && !m_Adjusting)
            {
                //m_CodeViewer.BringToFront();                
            }
        }

        private void MainForm_Move(object sender, EventArgs e)
        {
            if (m_View1_Active && WindowState != FormWindowState.Minimized && !m_Adjusting)
                if (Location.X != 0 || Location.Y != 0)
                    Location = new Point(0, 0);
        }

        private static MainForm this_outer;

        private void UpdateTreeViewPages()
        {
            this_outer = this;
            new TreeViewPagesRootItem(m_Pages).UpdateUsingChildNodes(treeViewPages);
            treeViewPages.EnsureNodeSelected();
            m_CodeViewer.UpdateContentItems();
        }

        private void UpdateTreeViewPagesAuto()
        {
            this_outer = this;
            new TreeViewPagesRootItem(m_Pages).UpdateUsingChildNodes(treeViewPages);
            m_CodeViewer.UpdateContentItems();
        }

        private void radLastOnly_CheckedChanged(object sender, EventArgs e)
        {
            UpdateTreeViewPages();
        }

        private void radAll_CheckedChanged(object sender, EventArgs e)
        {
            UpdateTreeViewPages();
        }

        private void chkShowRequests_CheckedChanged(object sender, EventArgs e)
        {
            m_ViewOptions.ShowRequests = chkShowRequests.Checked;
            UpdateTreeViewPages();
        }

        private void chkShowEvents_CheckedChanged(object sender, EventArgs e)
        {
            m_ViewOptions.ShowEvents = chkShowEvents.Checked;
            UpdateTreeViewPages();
        }

        private void chkSections_CheckedChanged(object sender, EventArgs e)
        {
            m_ViewOptions.ShowSections = chkSections.Checked;
            UpdateTreeViewPages();
        }

        private void tmrUpdate_Tick(object sender, EventArgs e)
        {
            UpdateTreeViewPages();
        }

        private void treeViewPages_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            this_outer = this;
            (e.Node.Tag as TreeItem).UpdateAndExpand(e.Node);
        }

        private void treeViewPages_AfterSelectOrSelectedNodeClick(object sender, TreeViewEventArgs e)
        {
            var ti = e.Node.Tag;
            var tiPage = ti as PageRequestTreeItem;
            var tiContentItem = ti as ContentItemTreeItem;
            var tiJavaTrace = ti as JavaTraceTreeItem;
            var tiTopLevel = ti as TopLevelCallTreeItem;
            var tiCombined = ti as CombinedEventTopLevelCallTreeItem;
            var tiCallReference = ti as CallReferenceTreeItem;
            if (tiPage != null)
                ShowPage(tiPage.Data);
            else if (tiContentItem != null)
                ShowContentItem(tiContentItem.Data);
            else if (tiJavaTrace != null)
                ShowJavaCall(tiJavaTrace.Data, tiJavaTrace.Call);
            else if (tiTopLevel != null)
                ShowJavaScriptCall(tiTopLevel.Call);
            else if (tiCombined != null && tiCombined.Call != null)
                ShowJavaScriptCall(tiCombined.Call);
            else if (tiCallReference != null)
                ShowJavaScriptCall(tiCallReference.Data.Call);
        }

        private void ShowPage(Firefox.PageRequest page)
        {
            treeViewCalls.Nodes.Clear();
            m_CodeViewer.ShowPage(page);
        }

        private void ShowContentItem(Firefox.ContentItem ci)
        {
            treeViewCalls.Nodes.Clear();
            m_CodeViewer.ShowContentItem(ci.PageRequest, ci);
        }

        private void ShowJavaScriptCall(Firefox.Call call)
        {
            ShowJavaScriptCallRightPane(call.Script.DefinitionContentItem.PageRequest, call);
        }

        private void ShowJavaCall(Java.TopLevelTrace trace, Java.Call call)
        {
            if (trace != null && trace.IsCompleted)
            {
                ShowJavaCallRightPane(trace, call);
            }
            else
            {
                treeViewCalls.Nodes.Clear();
                treeViewCalls.Nodes.Add(new TreeNode("Processing..."));
            }
        }

        private void CheckUpdateCallsView()
        {
            var ti = treeViewPages.SelectedNode.Tag;
            if (ti != null)
            {
                if (ti is TopLevelCallTreeItem || ti is CombinedEventTopLevelCallTreeItem || ti is JavaTraceTreeItem)
                    treeViewPages.ForceReselectSelectedNode();
            }
        }

        private class TreeViewPagesRootItem : TreeItem<Firefox.PageRequestCollection>
        {
            public TreeViewPagesRootItem(Firefox.PageRequestCollection pages) : base(pages) { }
            public override IEnumerable<TreeItem> GetChildren()
            {
                return (this_outer.radAll.Checked ? m_Data : m_Data.Where((page, i) => i == m_Data.Count - 1))
                    .Select(p => new PageRequestTreeItem(p)).OfType<TreeItem>();
            }
        }

        private class PageRequestTreeItem : TreeItem<Firefox.PageRequest>
        {
            public PageRequestTreeItem(Firefox.PageRequest page) : base(page) { }
            public override void CreateSingle(TreeNode node)
            {
                SetImage(node, 1);
            }
            public override void UpdateSingle(TreeNode node)
            {
                SetText(node, string.Format("{0} ({1})", m_Data.Location, m_Data.Date));
            }
            public override IEnumerable<TreeItem> GetChildren()
            {
                if (this_outer.m_ViewOptions.ShowRequests)
                    foreach (TreeItem t in m_Data.ContentItemsSafe
                        .Where(ci => !ci.IsXhr && (!this_outer.m_ViewOptions.FilterImages || !ci.IsImage))
                        .Select(ci => new ContentItemTreeItem(ci)).OfType<TreeItem>()) yield return t;
                if (this_outer.m_ViewOptions.ShowSections)
                {
                    foreach (TreeItem t in m_Data.SectionsSafe
                        .Where(section => !section.IsEmptySafe).Select(section => new SectionTreeItem(section)).OfType<TreeItem>()) yield return t;
                }
                else if (this_outer.m_ViewOptions.ShowEvents)
                    foreach (TreeItem t in GetEventTreeNodes(m_Data.EventsSafe)) yield return t;
            }
            public override bool HasChildren()
            {
                return this_outer.m_ViewOptions.ShowRequests || this_outer.m_ViewOptions.ShowEvents;
            }
        }

        private static IEnumerable<TreeItem> GetEventTreeNodes(IEnumerable<Firefox.Event> events)
        {
            if (this_outer.m_ViewOptions.FilterUnhandledEvents)
                events = events.Where(e => e.HasTopLevelCallsSafe(this_outer.m_ViewOptions.FilterDojoJQuery));
            if (this_outer.m_ViewOptions.ShowCombinedEventsToplevelCalls)
                return events.Select(e => new CombinedEventTopLevelCallTreeItem(e)).OfType<TreeItem>();
            else
                return events.Select(e => new EventTreeItem(e)).OfType<TreeItem>();
        }

        private class ContentItemTreeItem : TreeItem<Firefox.ContentItem>
        {
            public ContentItemTreeItem(Firefox.ContentItem ci) : base(ci) { }
            public override void CreateSingle(TreeNode node)
            {
            }
            public override void UpdateSingle(TreeNode node)
            {
                SetImage(node, m_Data.Url != null ? 5 : 0);
                SetText(node, m_Data.Url != null ? m_Data.ShortUrl : m_Data.ShortOriginalUrl);
            }
            public override IEnumerable<TreeItem> GetChildren()
            {
                if (m_Data.ServerTrace != null)
                    yield return new JavaTraceTreeItem(m_Data.ServerTrace);
            }
        }

        private class SectionTreeItem : TreeItem<Firefox.Section>
        {
            public SectionTreeItem(Firefox.Section section) : base(section) { }
            public override void CreateSingle(TreeNode node)
            {
                SetImage(node, 11);
                SetText(node, m_Data.Marked ? "Marked section" : "Section");
            }
            public override IEnumerable<TreeItem> GetChildren()
            {
                if (this_outer.m_ViewOptions.ShowEvents)
                    return GetEventTreeNodes(m_Data.EventsSafe);
                else
                    return Enumerable.Empty<TreeItem>();
            }
        }

        private class EventTreeItem : TreeItem<Firefox.Event>
        {
            public EventTreeItem(Firefox.Event e) : base(e) { }
            public override void CreateSingle(TreeNode node)
            {
                SetText(node, m_Data.DisplayName);
                SetImage(node, 6);
            }
            public override IEnumerable<TreeItem> GetChildren()
            {
                return m_Data.GetTopLevelCallsSafe(this_outer.m_ViewOptions.FilterDojoJQuery).Select(top => new TopLevelCallTreeItem(top)).OfType<TreeItem>();
            }
        }

        private class TopLevelCallTreeItem : TreeItem<Firefox.TopLevelCall>
        {
            public TopLevelCallTreeItem(Firefox.TopLevelCall top) : base(top) { }
            public Firefox.Call Call { get { return m_Data.GetRoot(this_outer.m_ViewOptions.FilterDojoJQuery); } }
            public override void UpdateSingle(TreeNode node)
            {
                SetText(node, Call.TopLevelDisplayName);
                this_outer.SetImageAndHighlight(node, Call.Xhrs.Count > 0 ? 3 : 2, Data.IsHighlighted);
            }
            public override IEnumerable<TreeItem> GetChildren()
            {
                if (this_outer.m_ViewOptions.ShowXhrs)
                    foreach (TreeItem t in Call.Xhrs.Select(xhr => new XhrTreeItem(xhr)).OfType<TreeItem>()) yield return t;
                if (this_outer.m_ViewOptions.ShowIntervalTimeouts)
                    foreach (TreeItem t in Call.IntervalTimeouts.Select(it => new IntervalTimeoutTreeItem(it)).OfType<TreeItem>()) yield return t;
            }
        }

        private class CombinedEventTopLevelCallTreeItem : TreeItem<Firefox.Event>
        {
            public CombinedEventTopLevelCallTreeItem(Firefox.Event e) : base(e) { }

            private Firefox.TopLevelCall GetTopLevelCall()
            {
                return m_Data.GetTopLevelCallsSafe(this_outer.m_ViewOptions.FilterDojoJQuery).FirstOrDefault();
            }

            private Firefox.Call GetCall(Firefox.TopLevelCall top)
            {
                return top != null ? top.GetRoot(this_outer.m_ViewOptions.FilterDojoJQuery) : null;
            }

            public Firefox.Call Call
            {
                get { return GetCall(GetTopLevelCall()); }
            }

            // Warning: this is some poor code, it relies on the assumption that GetChildren() will always be called after UpdateSingle().

            private bool m_UpdateSingleCalled = false;
            private int m_Count; // Only to be used between UpdateSingle and GetChildren call (will not be persisted any longer).
            private Firefox.TopLevelCall m_TopLevelCall; // Only to be used between UpdateSingle and GetChildren call (will not be persisted any longer).

            private void CalcVars()
            {
                m_Count = m_Data.GetTopLevelCallsCountSafe(this_outer.m_ViewOptions.FilterDojoJQuery);
                if (m_Count == 1)
                {
                    // Theoretical race condition, but count only grows (and chkFilter*.Checked only gets updated in this thread)
                    m_TopLevelCall = GetTopLevelCall();
                }
                else
                    m_TopLevelCall = null;
            }

            public override void UpdateSingle(TreeNode node)
            {
                CalcVars();
                m_UpdateSingleCalled = true;

                if (m_Count == 1)
                {
                    SetText(node, m_Data.DisplayName + " : " + GetCall(m_TopLevelCall).TopLevelDisplayName);
                    this_outer.SetImageAndHighlight(node, 7, m_TopLevelCall.IsHighlighted);
                }
                else
                {
                    var e = new EventTreeItem(m_Data);
                    e.CreateSingle(node);
                    e.UpdateSingle(node);
                }
            }

            public override IEnumerable<TreeItem> GetChildren()
            {
                if (m_UpdateSingleCalled)
                    m_UpdateSingleCalled = false;
                else
                    CalcVars();

                if (m_Count == 0)
                    return Enumerable.Empty<TreeItem>();
                else if (m_Count == 1)
                    return new TopLevelCallTreeItem(m_TopLevelCall).GetChildren();
                else
                    return new EventTreeItem(m_Data).GetChildren();
            }
        }

        private class XhrTreeItem : TreeItem<Firefox.Xhr>
        {
            public XhrTreeItem(Firefox.Xhr xhr) : base(xhr) { }
            public override void UpdateSingle(TreeNode node)
            {
                SetImage(node, m_Data.IsCompleted ? 5 : 0);
                SetText(node, m_Data.IsCompleted ? m_Data.ShortUrl : m_Data.ShortOriginalUrl);
            }
            public override IEnumerable<TreeItem> GetChildren()
            {
                foreach (Firefox.XhrEvent xe in m_Data.XhrEvents)
                    if (xe.Type == Firefox.XhrEventType.Send)
                    {
                        yield return new CallReferenceTreeItem("[send]", xe.Call);
                        if (m_Data.ContentItem.ServerTrace != null)
                            yield return new JavaTraceTreeItem(m_Data.ContentItem.ServerTrace);
                    }
                    else if (xe.Type == FireDetectiveAnalyzer.Firefox.XhrEventType.Access)
                        yield return new CallReferenceTreeItem("[access]", xe.Call);
                    else if (xe.Type == FireDetectiveAnalyzer.Firefox.XhrEventType.ReadyStateChange)
                    {
                        if (!this_outer.m_ViewOptions.FilterUnhandledEvents || xe.Event.HasTopLevelCallsSafe(this_outer.m_ViewOptions.FilterDojoJQuery))
                            yield return new EventReferenceTreeItem(xe.Event);
                    }
                    else if (xe.Type == FireDetectiveAnalyzer.Firefox.XhrEventType.Eval)
                        yield return new CallReferenceTreeItem("[eval]", xe.Call);
            }
        }

        private class IntervalTimeoutTreeItem : TreeItem<Firefox.IntervalTimeout>
        {
            public IntervalTimeoutTreeItem(Firefox.IntervalTimeout it) : base(it) { }
            public override void UpdateSingle(TreeNode node)
            {
                SetText(node, m_Data.SetCall.Script.Name);
                SetImage(node, 8);
            }
            public override IEnumerable<TreeItem> GetChildren()
            {
                foreach (Firefox.Event e in m_Data.Events)
                    yield return new EventReferenceTreeItem(e);
                if (m_Data.ClearCall != null)
                    yield return new CallReferenceTreeItem(m_Data.ClearCall);
            }
        }

        private struct CallReference
        {
            public Firefox.Call Call;
            public string Desc;
            public CallReference(string desc, Firefox.Call call) { Call = call; Desc = desc; }
            public string DisplayName { get { return Desc + " " + Call.DisplayName; } }
        }

        private class CallReferenceTreeItem : TreeItem<CallReference>
        {
            public CallReferenceTreeItem(Firefox.Call call) : base(new CallReference(null, call)) { }
            public CallReferenceTreeItem(string desc, Firefox.Call call) : base(new CallReference(desc, call)) { }
            public override void UpdateSingle(TreeNode node)
            {
                SetText(node, m_Data.DisplayName);
                SetImage(node, 10);
            }
        }

        private class EventReferenceTreeItem : TreeItem<Firefox.Event>
        {
            public EventReferenceTreeItem(Firefox.Event e) : base(e) { }
            public override void UpdateSingle(TreeNode node)
            {
                SetText(node, m_Data.DisplayName);
                SetImage(node, 9);
            }
        }

        private class TextContentTreeItem : TreeItem<TextDocument>
        {
            private string m_Description;
            public TextContentTreeItem(TextDocument doc, string description) : base(doc) { m_Description = description; }
            public override void CreateSingle(TreeNode node)
            {
                SetImage(node, 1);
                node.Text = m_Description;
            }
        }

        private class JavaTraceTreeItem : TreeItem<Java.TopLevelTrace>
        {
            public JavaTraceTreeItem(Java.TopLevelTrace st) : base(st) { }
            public Java.Call Call { get { return m_Data.GetRoot(this_outer.m_ViewOptions.FilterJava); } }
            public override void UpdateSingle(TreeNode node)
            {
                SetText(node, m_Data.IsCompleted ? "<java code> [" + (Call.Count - 1).ToString() + "]" : "<java code> [?]");
                this_outer.SetImageAndHighlight(node, 4, m_Data.IsHighlighted);
            }
        }

        // ---- Calls view --------------------------------------------------------------------

        private Firefox.PageRequest m_RightPaneCurrentPage;
        private Java.TopLevelTrace m_RightPaneSelectedJavaTrace;

        private void treeViewCalls_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            (e.Node.Tag as TreeItem).UpdateAndExpand(e.Node);
        }

        private void treeViewCalls_AfterSelectOrSelectedNodeClick(object sender, TreeViewEventArgs e)
        {
            var ti = e.Node.Tag;
            var tiJavaCall = ti as JavaCVTreeItem;
            var tiJavaScriptCall = ti as JavaScriptCVTreeItem;
            if (tiJavaCall != null)
                m_CodeViewer.ShowJavaCall(m_RightPaneCurrentPage, m_RightPaneSelectedJavaTrace, tiJavaCall.Data);
            else if (tiJavaScriptCall != null)
                m_CodeViewer.ShowJavaScriptCall(m_RightPaneCurrentPage, tiJavaScriptCall.Data);
        }

        private void treeViewCalls_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            btnShowCodePane.PerformClick();
        }

        private void ShowJavaScriptCallRightPane(Firefox.PageRequest page, Firefox.Call call)
        {
            var top = call.GetTopLevel();
            m_RightPaneCurrentPage = page;
            new JavaScriptCVTreeItem(top).Update(treeViewCalls);
            treeViewCalls.SelectStack(call.GetStack().Select(c => new JavaScriptCVTreeItem(c)).OfType<TreeItem>());
            treeViewCalls.EnsureNodeSelected();
        }

        private class JavaScriptCVTreeItem : TreeItem<Firefox.Call>
        {
            public JavaScriptCVTreeItem(Firefox.Call call) : base(call) { }
            public override void CreateSingle(TreeNode node)
            {
                SetText(node, m_Data.DisplayName);
            }
            public override void UpdateSingle(TreeNode node)
            {
                SetBackColor(node, m_Data.IsHighlighted ? Color.FromArgb(255, 255, 160) : Color.Empty);
            }
            public override IEnumerable<TreeItem> GetChildren()
            {
                return m_Data.SubCalls.Select(call => new JavaScriptCVTreeItem(call)).OfType<TreeItem>();
            }
        }

        private void ShowJavaCallRightPane(Java.TopLevelTrace trace, Java.Call call)
        {
            var top = call.GetTopLevel();
            m_RightPaneCurrentPage = trace.Request.PageRequest;
            m_RightPaneSelectedJavaTrace = trace;
            new JavaCVTreeItem(top).Update(treeViewCalls);
            treeViewCalls.SelectStack(call.GetStack().Select(c => new JavaCVTreeItem(c)).OfType<TreeItem>());
            if (top == call) treeViewCalls.Nodes[0].Expand();
            treeViewCalls.EnsureNodeSelected();
        }

        private class JavaCVTreeItem : TreeItem<Java.Call>
        {
            public JavaCVTreeItem(Java.Call call) : base(call) { }
            public override void CreateSingle(TreeNode node)
            {
                SetText(node, m_Data.DisplayName);
            }
            public override void UpdateSingle(TreeNode node)
            {
                SetBackColor(node, m_Data.IsHighlighted ? Color.FromArgb(255, 255, 160) : m_Data.IsMasked ? Color.LightGray : Color.Empty);
            }
            public override IEnumerable<TreeItem> GetChildren()
            {
                return m_Data.SubCalls.Select(call => new JavaCVTreeItem(call)).OfType<TreeItem>();
            }
        }

        private void UpdateTreeViewCalls()
        {
            if (treeViewCalls.Nodes.Count > 0)
                (treeViewCalls.Nodes[0].Tag as TreeItem).Update(treeViewCalls);
        }

        private Firefox.PageRequest m_CurrentHighlightPage;
        private string m_CurrentHighlightDescription;
        private int m_CurrentHighlightIndex;

        private void m_CodeViewer_HighlightScriptUsage(object sender, CustomEventArgs<FireDetectiveAnalyzer.Firefox.Script> e)
        {
            var page = m_Pages.LastOrDefault();
            if (page != null)
            {
                List<Firefox.TopLevelCall> interestingTops = new List<Firefox.TopLevelCall>();
                List<Firefox.Call> calls = new List<Firefox.Call>();
                List<Firefox.Call> filteredCalls = new List<Firefox.Call>();

                var tops = page.EventsSafe.SelectMany(ev => ev.GetTopLevelCallsSafe(false)).ToList();
                foreach (Firefox.TopLevelCall top in tops)
                {
                    int prevSize = calls.Count;
                    calls.AddRange(top.Root.GetScriptCalls(e.Data));
                    if (calls.Count > prevSize)
                    {
                        filteredCalls.AddRange(top.FilteredRoot.GetScriptCalls(e.Data));
                        interestingTops.Add(top);
                    }
                }

                if (calls.Count != filteredCalls.Count)
                    throw new ApplicationException("Highlight not supported.");

                if (interestingTops.Count > 0)
                {
                    ShowNewHighlightedSet(page, new HighlightedFirefoxCallSet(interestingTops, calls, filteredCalls));
                    return;
                }
            }
            MessageBox.Show("This script has not (yet) been called on this page.", "FireDetective", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void m_CodeViewer_HighlightMethodUsage(object sender, CustomEventArgs<FireDetectiveAnalyzer.Java.Method> e)
        {
            var page = m_Pages.LastOrDefault();
            if (page != null && e.Data != null)
            {
                List<Java.TopLevelTrace> interestingServerTraces = new List<Java.TopLevelTrace>();
                List<Java.Call> calls = new List<Java.Call>();
                List<Java.Call> filteredCalls = new List<Java.Call>();
                
                var traces = page.ContentItemsSafe.Select(ci => ci.ServerTrace).Where(st => st != null);
                foreach (Java.TopLevelTrace trace in traces)
                {
                    int prevSize = calls.Count;
                    calls.AddRange(trace.Root.Find(call => call.Method == e.Data));
                    filteredCalls.AddRange(trace.FilteredRoot.Find(call => call.Method == e.Data));
                    if (calls.Count > prevSize)
                        interestingServerTraces.Add(trace);
                }

                if (calls.Count != filteredCalls.Count)
                    throw new ApplicationException("Highlight not supported.");

                if (interestingServerTraces.Count > 0)
                {
                    ShowNewHighlightedSet(page, new HighlightedJavaCallSet(interestingServerTraces, calls, filteredCalls));
                    return;
                }
            }

            MessageBox.Show("This method has not (yet) been called on this page.", "FireDetective", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnPrevHighlight_Click(object sender, EventArgs e)
        {
            ShowHighlightAtIndex((m_CurrentHighlightIndex + m_HighlightedSet.Count - 1) % m_HighlightedSet.Count);
        }

        private void btnNextHighlight_Click(object sender, EventArgs e)
        {
            ShowHighlightAtIndex((m_CurrentHighlightIndex + 1) % m_HighlightedSet.Count);
        }

        private void lblHighlight_MouseEnter(object sender, EventArgs e)
        {
            lblHighlight.ForeColor = SystemColors.Highlight;
        }

        private void lblHighlight_MouseLeave(object sender, EventArgs e)
        {
            lblHighlight.ForeColor = SystemColors.WindowText;
        }

        private void lblHighlight_Click(object sender, EventArgs e)
        {
            ShowHighlightAtIndex(m_CurrentHighlightIndex);
        }

        private void btnClearHighlight_Click(object sender, EventArgs e)
        {
            ShowNewHighlightedSet(null, HighlightedSet.Empty);
        }

        private void ShowHighlightAtIndex(int index)
        {
            m_CurrentHighlightIndex = index;
            if (index < m_HighlightedSet.Count)
            {
                if (m_HighlightedSet is HighlightedFirefoxCallSet)
                {
                    var call = (m_HighlightedSet as HighlightedFirefoxCallSet).GetIndex(index);
                    m_CurrentHighlightDescription = string.Format("{0} (root: {1})", call.DisplayName, call.GetTopLevel().TopLevelDisplayName);
                    SelectCall(m_CurrentHighlightPage, call);
                }
                else if (m_HighlightedSet is HighlightedJavaCallSet)
                {
                    var call = (m_HighlightedSet as HighlightedJavaCallSet).GetIndex(index);
                    m_CurrentHighlightDescription = string.Format("{0}", call.DisplayName);
                    SelectCall(m_CurrentHighlightPage, call);
                }
            }
            UpdateHighlightBox();
        }

        private void SelectCall(Firefox.PageRequest page, Firefox.Call call)
        {
            var root = call.GetTopLevel();
            var events = page.EventsSafe;
            foreach (Firefox.Event e in events)
            {
                var t = e.GetTopLevelCallsSafe(false).FirstOrDefault(top => top.Root == root || top.FilteredRoot == root);
                if (t != null)
                {
                    chkShowEvents.Checked = true;
                    var stack = new CombinedEventTopLevelCallTreeItem(e).GetPath(new PageRequestTreeItem(page), false);
                    if (stack.Count == 0) stack = new TopLevelCallTreeItem(t).GetPath(new PageRequestTreeItem(page), false);
                    if (stack.Count == 0) stack = new TreeItem[] { new PageRequestTreeItem(page) }.ToList();
                    treeViewPages.SelectStack(stack);
                }
            }
            ShowJavaScriptCallRightPane(m_CurrentHighlightPage, call);
            treeViewCalls.Focus();
        }

        private void SelectCall(Firefox.PageRequest page, Java.Call call)
        {
            var root = call.GetTopLevel();
            var trace = page.ContentItemsSafe.Select(ci => ci.ServerTrace).Where(tr => tr != null).FirstOrDefault(tr => tr.Root == root || tr.FilteredRoot == root);
            if (trace != null)
            {
                if (trace.Request.IsXhr)
                    chkShowEvents.Checked = true; // = chkShowXhrs.Checked = true;
                else
                    chkShowRequests.Checked = true;
                var stack = new JavaTraceTreeItem(trace).GetPath(new PageRequestTreeItem(page), false);
                if (stack.Count == 0) stack = new TreeItem[] { new PageRequestTreeItem(page) }.ToList();
                treeViewPages.SelectStack(stack);
            }
            ShowJavaCallRightPane(trace, call);
            treeViewCalls.Focus();
        }

        private void ShowNewHighlightedSet(Firefox.PageRequest page, HighlightedSet hs)
        {
            m_HighlightedSet.Toggle(false);
            if (m_HighlightedSet.Count > 0)
                UpdateTreeViewCalls();
            m_CurrentHighlightPage = page;
            m_HighlightedSet = hs;
            m_HighlightedSet.Toggle(true);
            UpdateHighlightedSetActiveFilters();
            ShowHighlightAtIndex(0);
        }

        private void UpdateHighlightedSetActiveFilters()
        {
            m_HighlightedSet.IsJavaScriptFiltered = m_ViewOptions.FilterDojoJQuery;
            m_HighlightedSet.IsJavaFiltered = m_ViewOptions.FilterJava;
        }

        private void UpdateHighlightBox()
        {
            grpHighlight.Visible = m_HighlightedSet.Count > 0;
            tableLeft.RowStyles[1].Height = (m_HighlightedSet.Count > 0) ? 50 : 0;
            lblHighlight.Text = (m_HighlightedSet.Count > 0) ? string.Format("({0} / {1}) {2}", m_CurrentHighlightIndex + 1, m_HighlightedSet.Count, m_CurrentHighlightDescription) : "(nothing highlighted)";
            //btnPrevHighlight.Enabled = m_HighlightedSet.Count > 0;
            //btnNextHighlight.Enabled = m_HighlightedSet.Count > 0;
            //lblHighlight.Enabled = m_HighlightedSet.Count > 0;
        }

        private void SetImageAndHighlight(TreeNode node, int normalImageIndex, bool on)
        {
            TreeItem.SetBackColor(node, on ? Color.FromArgb(255, 255, 160) : Color.Empty);
            TreeItem.SetImage(node, on ? 12 : normalImageIndex);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (m_View1_Active)
                RemoveOwnedForm(m_CodeViewer);
            e.Cancel = false;
        }

        private Firefox.IntervalTimeout m_RightClickedTimeoutInterval;

        private void treeViewPages_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (e.Node != null && e.Node.Tag is CombinedEventTopLevelCallTreeItem)
                {
                    var tiCombined = e.Node.Tag as CombinedEventTopLevelCallTreeItem;
                    if (tiCombined.Data.IntervalTimeout != null)
                    {
                        m_RightClickedTimeoutInterval = tiCombined.Data.IntervalTimeout;
                        mnuShowTimeoutIntervalSet.Text = string.Format("Show {0} set...", m_RightClickedTimeoutInterval.IsInterval ? "interval" : "timeout");
                        mnuShowTimeoutIntervalClear.Text = string.Format("Show {0} clear...", m_RightClickedTimeoutInterval.IsInterval ? "interval" : "timeout");
                        mnuShowTimeoutIntervalSet.Enabled = tiCombined.Data.IntervalTimeout.SetCall != null;
                        mnuShowTimeoutIntervalClear.Enabled = tiCombined.Data.IntervalTimeout.ClearCall != null;
                        contextMenuStripPages.Show(treeViewPages, e.Location);
                    }
                }
            }
        }

        private void mnuShowTimeoutIntervalSet_Click(object sender, EventArgs e)
        {
            SelectCall(m_Pages.Last(), m_RightClickedTimeoutInterval.SetCall);
        }

        private void mnuShowTimeoutIntervalClear_Click(object sender, EventArgs e)
        {
            SelectCall(m_Pages.Last(), m_RightClickedTimeoutInterval.ClearCall);
        }

        /*
        private void treeViewCalls_AfterHover(object sender, TreeViewEventArgs e)
        {
            object call = null;
            if (e.Node != null)
            {
                var jsCall = e.Node.Tag as JavaScriptCallTreeItem;
                var javaCall = e.Node.Tag as JavaCallTreeItem;
                call = jsCall != null ? (object)jsCall.Data : (javaCall != null ? (object)javaCall.Data : null);
                //if (jsCall != null)
                //    m_Client.SendHighlight(jsCall.Data.ModifiedNodes);
            }
            //m_CodeViewer.Highlight(call);
        }*/
    }

    public class HighlightedSet
    {
        protected HighlightedSet()
        {
            IsJavaScriptFiltered = true;
            IsJavaFiltered = true;
        }

        public bool IsJavaScriptFiltered { get; set; }
        public bool IsJavaFiltered { get; set; }

        public virtual int Count { get { return 0; } }

        public virtual void Toggle(bool on) { }

        public static HighlightedSet Empty { get { return new HighlightedSet(); } }
    }

    public class HighlightedFirefoxCallSet : HighlightedSet
    {
        private List<Firefox.TopLevelCall> m_Tops = new List<Firefox.TopLevelCall>();
        private List<Firefox.Call> m_Calls = new List<Firefox.Call>();
        private List<Firefox.Call> m_FilteredCalls = new List<Firefox.Call>();

        public HighlightedFirefoxCallSet(IEnumerable<Firefox.TopLevelCall> tops, IEnumerable<Firefox.Call> calls, IEnumerable<Firefox.Call> filteredCalls)
        {
            m_Tops = tops.ToList();
            m_Calls = calls.ToList();
            m_FilteredCalls = calls.ToList();
        }

        public override void Toggle(bool on)
        {
            m_Tops.ForEach(top => { top.IsHighlighted = on; });
            m_Calls.ForEach(call => { call.IsHighlighted = on; });
            m_FilteredCalls.ForEach(call => { call.IsHighlighted = on; });
        }

        public override int Count { get { return m_Calls.Count; } }

        public Firefox.Call GetIndex(int index)
        {
            return IsJavaScriptFiltered ? m_FilteredCalls[index] : m_Calls[index];
        }
    }

    public class HighlightedJavaCallSet : HighlightedSet
    {
        private List<Java.TopLevelTrace> m_Traces = new List<Java.TopLevelTrace>();
        private List<Java.Call> m_Calls = new List<Java.Call>();
        private List<Java.Call> m_FilteredCalls = new List<Java.Call>();

        public HighlightedJavaCallSet(IEnumerable<Java.TopLevelTrace> traces, IEnumerable<Java.Call> calls, IEnumerable<Java.Call> filteredCalls)
        {
            m_Traces = traces.ToList();
            m_Calls = calls.ToList();
            m_FilteredCalls = filteredCalls.ToList();
        }

        public override void Toggle(bool on)
        {
            m_Traces.ForEach(trace => { trace.IsHighlighted = on; });
            m_Calls.ForEach(call => { call.IsHighlighted = on; });
            m_FilteredCalls.ForEach(call => { call.IsHighlighted = on; });
        }

        public override int Count { get { return m_Calls.Count; } }

        public Java.Call GetIndex(int index)
        {
            return IsJavaFiltered ? m_FilteredCalls[index] : m_Calls[index];
        }
    }

    public class ViewOptions
    {
        public bool ShowRequests { get; set; }
        public bool ShowEvents { get; set; }
        public bool ShowSections { get; set; }
        public bool ShowXhrs { get; set; }
        public bool ShowIntervalTimeouts { get; set; }
        public bool ShowCombinedEventsToplevelCalls { get; set; }

        public bool FilterImages { get; set; }
        public bool FilterUnhandledEvents { get; set; }
        public bool FilterDojoJQuery { get; set; }
        public bool FilterJava { get; set; }

        public ViewOptions()
        {
            ShowXhrs = ShowCombinedEventsToplevelCalls = true;
            FilterImages = FilterUnhandledEvents = true; // FilterDojoJQuery = true;
        }
    }
}
