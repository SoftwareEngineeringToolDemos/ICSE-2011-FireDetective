using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace FireDetectiveAnalyzer
{
    public partial class HoverableTreeView : ItemsTreeView
    {
        public HoverableTreeView()
        {
            InitializeComponent();
            //SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            //SetStyle(ControlStyles.AllPaintingInWmPaint, true);
        }

        public event EventHandler<TreeViewEventArgs> AfterHover;

        protected override void OnMouseMove(MouseEventArgs e)
        {
            TreeNode node = this.GetNodeAt(e.X, e.Y);
            if (node != null)
                node = node.Bounds.Contains(e.X, e.Y) ? node : null;
            if (node != null)
                node = !OnlyHoverSelectionChildNodes || node.Parent == SelectedNode ? node : null;
            //Hover(node); // Hovering temporarily disabled! DEBUG
            base.OnMouseMove(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            Hover(null);
            base.OnMouseLeave(e);
        }

        [Browsable(true)]
        [DefaultValue(typeof(Color), "0")]
        public Color HoverColor { get; set; }

        [Browsable(true)]
        [DefaultValue(false)]
        public bool OnlyHoverSelectionChildNodes { get; set; }

        private TreeNode m_HoveredNode;
        private Color m_SaveColor;

        private void Hover(TreeNode node)
        {
            if (m_HoveredNode != node)
            {
                Update();
                Rectangle first = Rectangle.Empty;
                BeginUpdate();
                if (m_HoveredNode != null)
                {
                    m_HoveredNode.BackColor = m_SaveColor;
                    first = m_HoveredNode.Bounds;
                    Control p = Parent; while (p.Parent != null) p = p.Parent;
                    Parent.Text += m_HoveredNode.Text + ";";
                }
                m_HoveredNode = node;
                if (node != null)
                {
                    m_SaveColor = m_HoveredNode.BackColor;
                    m_HoveredNode.BackColor = HoverColor;
                }
                EndUpdate();
                ValidateRect(Handle, IntPtr.Zero);
                if (!first.IsEmpty) Invalidate(first);
                if (node != null) Invalidate(node.Bounds);
                if (AfterHover != null)
                    AfterHover(this, new TreeViewEventArgs(node));
            }
        }

        [DllImport("user32.dll")]
        static extern bool ValidateRect(IntPtr hWnd, IntPtr lpRect);
    }
}
