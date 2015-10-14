using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FireDetectiveAnalyzer
{
    public class ItemsTreeView : TreeView
    {
        public bool Updating;
        public Action<TreeViewEventArgs> AfterSelectOverride { get; set; }

        public event TreeViewEventHandler AfterSelectOrSelectedNodeClick;

        private void FireAfterSelectOrSelectedNodeClick(TreeViewEventArgs e)
        {
            if (AfterSelectOrSelectedNodeClick != null)
                AfterSelectOrSelectedNodeClick(this, e);
        }

        protected override void OnBeforeSelect(TreeViewCancelEventArgs e)
        {
            if (Updating)
                e.Cancel = true;
            else
                base.OnBeforeSelect(e);
        }

        protected override void OnAfterSelect(TreeViewEventArgs e)
        {
            if (AfterSelectOverride != null)
                AfterSelectOverride(e);
            else
            {
                FireAfterSelectOrSelectedNodeClick(e);
                base.OnAfterSelect(e);
            }
        }

        protected override void OnNodeMouseClick(TreeNodeMouseClickEventArgs e)
        {
            if (e.Node == SelectedNode)
                FireAfterSelectOrSelectedNodeClick(new TreeViewEventArgs(e.Node));
            base.OnNodeMouseClick(e);
        }

        public void SelectNodeDuringUpdate(TreeNode node)
        {
            bool prev = Updating;
            Updating = false;
            SelectedNode = node;
            Updating = prev;
        }

        public void ForceSelect(TreeNode node)
        {
            SelectedNode = null;
            SelectedNode = node;
        }

        public void ForceReselectSelectedNode()
        {
            TreeNode node = SelectedNode;
            SelectedNode = null;
            SelectedNode = node;
        }

        public void SelectNodeOverrideAfterSelect(TreeNode node, Action<TreeViewEventArgs> func)
        {
            var prev = AfterSelectOverride;
            AfterSelectOverride = func;
            SelectedNode = node;
            AfterSelectOverride = prev;
        }
    }

    public abstract class TreeItem
    {
        public virtual void CreateSingle(TreeNode node) { }
        public virtual void UpdateSingle(TreeNode node) { }
        public virtual bool HasChildren() { return GetChildren().FirstOrDefault() != null ? true : false; }
        public virtual IEnumerable<TreeItem> GetChildren() { return Enumerable.Empty<TreeItem>(); }

        private TreeNode CreateNode()
        {
            TreeNode node = new TreeNode();
            node.Tag = this;
            CreateSingle(node);
            UpdateInternal(node);
            return node;
        }

        private void StartUpdate(TreeView view)
        {
            if ((view as ItemsTreeView).Updating) throw new ApplicationException("Infinite treeview update loop occured.");
            (view as ItemsTreeView).Updating = true;
        }

        private void EndUpdate(TreeView view)
        {
            (view as ItemsTreeView).Updating = false;
        }

        private void EnableFutureExpand(TreeNode node, bool enable)
        {
            if (enable)
            {
                if (node.Nodes.Count == 0) node.Nodes.Add(new TreeNode());
            }
            else
            {
                if (node.Nodes.Count > 0) node.Nodes.Clear();
            }
        }

        private void UpdateInternal(TreeNode node)
        {
            UpdateSingle(node);
            if (node.IsExpanded) UpdateNodesInternal(node.Nodes); else EnableFutureExpand(node, HasChildren());
        }

        private void UpdateNodesInternal(TreeNodeCollection nodes)
        {
            List<TreeItem> children = GetChildren().ToList();
            Dictionary<TreeItem, bool> shouldShow = children.ToDictionary(x => x, x => true);
            int i, j;
            for (i = 0, j = 0; j < children.Count; )
            {
                TreeNode node = i < nodes.Count ? nodes[i] : null;
                TreeItem item = node != null ? node.Tag as TreeItem : null;

                if (children[j].Equals(item))
                {
                    children[j].UpdateInternal(node);
                    i++; j++;
                }
                else if (node == null || (item != null && shouldShow.ContainsKey(item)))
                {
                    nodes.Insert(i, children[j].CreateNode());
                    i++; j++;
                }
                else
                {
                    nodes.RemoveAtWithSelectionBackup(i, nodes[i].TreeView, nodes[i].Parent);
                }
            }
            for (int k = nodes.Count - 1; k >= i; k--)
                nodes.RemoveAtWithSelectionBackup(k, nodes[k].TreeView, nodes[k].Parent);
        }

        private class HolderTreeItem : TreeItem
        {
            private TreeItem m_Hold;
            public HolderTreeItem(TreeItem hold) { m_Hold = hold; }
            public override IEnumerable<TreeItem> GetChildren() { yield return m_Hold; }
        }

        public void Update(TreeView view)
        {
            StartUpdate(view);
            new HolderTreeItem(this).UpdateNodesInternal(view.Nodes);
            EndUpdate(view);
        }

        public void UpdateUsingChildNodes(TreeView view)
        {
            StartUpdate(view);
            UpdateNodesInternal(view.Nodes);
            EndUpdate(view);
        }

        public void UpdateAndExpand(TreeNode node)
        {
            StartUpdate(node.TreeView);
            UpdateSingle(node);
            UpdateNodesInternal(node.Nodes);
            EndUpdate(node.TreeView);
        }

        public static void SetText(TreeNode node, string text)
        {
            text = text ?? "";
            if (node.Text != text) node.Text = text;
        }

        public static void SetImage(TreeNode node, int image)
        {
            if (node.ImageIndex != image) node.ImageIndex = image;
            if (node.SelectedImageIndex != image) node.SelectedImageIndex = image;
        }

        public static void SetBackColor(TreeNode node, System.Drawing.Color color)
        {
            if (node.BackColor != color) node.BackColor = color;
        }

        public List<TreeItem> GetPath(TreeItem root, bool leaveOutRootInReturnValue)
        {
            Stack<TreeItem> stack = new Stack<TreeItem>();
            this.GetItemPath(root, ref stack);
            return leaveOutRootInReturnValue ? stack.Take(stack.Count - 1).Reverse().ToList() : stack.Reverse().ToList();
        }

        private bool GetItemPath(TreeItem root, ref Stack<TreeItem> stack)
        {
            stack.Push(root);
            if (this.Equals(root))
                return true;
            else
                foreach (TreeItem c in root.GetChildren())
                    if (GetItemPath(c, ref stack))
                        return true;
            stack.Pop();
            return false;
        }
    }

    public abstract class TreeItem<T> : TreeItem
    {
        protected T m_Data;
        protected TreeItem(T data)
        {
            m_Data = data;
        }

        public T Data { get { return m_Data; } }

        public override bool Equals(object obj)
        {
            return obj != null && this.GetType() == obj.GetType() && this.m_Data.Equals((obj as TreeItem<T>).m_Data);
        }

        public override int GetHashCode()
        {
            return m_Data.GetHashCode();
        }
    }

    public static class TreeViewExtensions
    {
        public static void SelectStack(this TreeView view, IEnumerable<TreeItem> stack)
        {
            view.SelectStack(stack, false);
        }

        public static void SelectStackSkipNonMatches(this TreeView view, IEnumerable<TreeItem> stack)
        {
            view.SelectStack(stack, true);
        }

        private static void SelectStack(this TreeView view, IEnumerable<TreeItem> stack, bool skipNonMatches)
        {
            bool continueForeach;
            TreeNodeCollection nodes = view.Nodes;
            TreeNode node = null;
            foreach (TreeItem item in stack)
            {
                continueForeach = false;
                if (node != null && !node.IsExpanded)
                    node.Expand();
                for (int i = 0; i < nodes.Count; i++)
                    if (item.Equals(nodes[i].Tag))
                    {
                        node = nodes[i];
                        nodes = nodes[i].Nodes;
                        continueForeach = true;
                        break;
                    }
                if (!continueForeach && !skipNonMatches)
                    return;
            }
            view.SelectedNode = node;
        }

        public static void RemoveAtWithSelectionBackup(this TreeNodeCollection nodes, int index, TreeView view, TreeNode selectionBackup)
        {
            TreeNode prev = view.SelectedNode;
            nodes.RemoveAt(index);
            if (view.SelectedNode == null && prev != null)
                (view as ItemsTreeView).SelectNodeDuringUpdate(selectionBackup);
        }

        public static void EnsureNodeSelected(this TreeView treeview)
        {
            if (treeview.SelectedNode == null)
                treeview.SelectedNode = treeview.Nodes.OfType<TreeNode>().FirstOrDefault();
        }
    }
}
