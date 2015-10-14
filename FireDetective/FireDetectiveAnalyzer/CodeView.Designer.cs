namespace FireDetectiveAnalyzer
{
    partial class CodeView
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.scrollbar = new System.Windows.Forms.VScrollBar();
            this.tmrScrollAnchor = new System.Windows.Forms.Timer(this.components);
            this.hscrollbar = new System.Windows.Forms.HScrollBar();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuWhereCalled = new System.Windows.Forms.ToolStripMenuItem();
            this.btnFocusDummy = new FireDetectiveAnalyzer.FocusDummyButton();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // scrollbar
            // 
            this.scrollbar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.scrollbar.Cursor = System.Windows.Forms.Cursors.Default;
            this.scrollbar.LargeChange = 50;
            this.scrollbar.Location = new System.Drawing.Point(398, 0);
            this.scrollbar.Name = "scrollbar";
            this.scrollbar.Size = new System.Drawing.Size(17, 398);
            this.scrollbar.TabIndex = 0;
            this.scrollbar.Scroll += new System.Windows.Forms.ScrollEventHandler(this.scrollbar_Scroll);
            // 
            // tmrScrollAnchor
            // 
            this.tmrScrollAnchor.Interval = 50;
            this.tmrScrollAnchor.Tick += new System.EventHandler(this.tmrScrollAnchor_Tick);
            // 
            // hscrollbar
            // 
            this.hscrollbar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.hscrollbar.Cursor = System.Windows.Forms.Cursors.Default;
            this.hscrollbar.Location = new System.Drawing.Point(0, 398);
            this.hscrollbar.Name = "hscrollbar";
            this.hscrollbar.Size = new System.Drawing.Size(398, 17);
            this.hscrollbar.TabIndex = 1;
            this.hscrollbar.Scroll += new System.Windows.Forms.ScrollEventHandler(this.hscrollbar_Scroll);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuWhereCalled});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(209, 26);
            // 
            // mnuWhereCalled
            // 
            this.mnuWhereCalled.Name = "mnuWhereCalled";
            this.mnuWhereCalled.Size = new System.Drawing.Size(208, 22);
            this.mnuWhereCalled.Text = "Where is this code called?";
            this.mnuWhereCalled.Click += new System.EventHandler(this.mnuWhereCalled_Click);
            // 
            // btnFocusDummy
            // 
            this.btnFocusDummy.Location = new System.Drawing.Point(-50, -50);
            this.btnFocusDummy.Name = "btnFocusDummy";
            this.btnFocusDummy.Size = new System.Drawing.Size(18, 18);
            this.btnFocusDummy.TabIndex = 0;
            this.btnFocusDummy.UseVisualStyleBackColor = true;
            this.btnFocusDummy.KeyDown += new System.Windows.Forms.KeyEventHandler(this.btnFocusDummy_KeyDown);
            // 
            // CodeView
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
            this.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.Controls.Add(this.btnFocusDummy);
            this.Controls.Add(this.hscrollbar);
            this.Controls.Add(this.scrollbar);
            this.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.DoubleBuffered = true;
            this.Name = "CodeView";
            this.Size = new System.Drawing.Size(419, 419);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.VScrollBar scrollbar;
        private System.Windows.Forms.Timer tmrScrollAnchor;
        private System.Windows.Forms.HScrollBar hscrollbar;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem mnuWhereCalled;
        private FocusDummyButton btnFocusDummy;
    }
}
