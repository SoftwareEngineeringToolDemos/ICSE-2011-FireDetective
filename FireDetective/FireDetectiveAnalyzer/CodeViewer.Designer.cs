namespace FireDetectiveAnalyzer
{
    partial class CodeViewer
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CodeViewer));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.grpFilter = new System.Windows.Forms.GroupBox();
            this.chkFilterByCurrentTrace = new System.Windows.Forms.CheckBox();
            this.chkFilterByCurrentPage = new System.Windows.Forms.CheckBox();
            this.treeViewFiles = new FireDetectiveAnalyzer.ItemsTreeView();
            this.images = new System.Windows.Forms.ImageList(this.components);
            this.codeView1 = new FireDetectiveAnalyzer.CodeView();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnWhereCalled = new System.Windows.Forms.Button();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.grpFilter.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.grpFilter);
            this.splitContainer1.Panel1.Controls.Add(this.treeViewFiles);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.codeView1);
            this.splitContainer1.Panel2.Controls.Add(this.panel1);
            this.splitContainer1.Size = new System.Drawing.Size(1112, 766);
            this.splitContainer1.SplitterDistance = 150;
            this.splitContainer1.TabIndex = 1;
            this.splitContainer1.TabStop = false;
            this.splitContainer1.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitContainer1_SplitterMoved);
            // 
            // grpFilter
            // 
            this.grpFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.grpFilter.Controls.Add(this.chkFilterByCurrentTrace);
            this.grpFilter.Controls.Add(this.chkFilterByCurrentPage);
            this.grpFilter.Location = new System.Drawing.Point(3, 3);
            this.grpFilter.Name = "grpFilter";
            this.grpFilter.Size = new System.Drawing.Size(147, 69);
            this.grpFilter.TabIndex = 1;
            this.grpFilter.TabStop = false;
            this.grpFilter.Text = "Server filter options";
            // 
            // chkFilterByCurrentTrace
            // 
            this.chkFilterByCurrentTrace.AutoSize = true;
            this.chkFilterByCurrentTrace.Location = new System.Drawing.Point(6, 43);
            this.chkFilterByCurrentTrace.Name = "chkFilterByCurrentTrace";
            this.chkFilterByCurrentTrace.Size = new System.Drawing.Size(192, 17);
            this.chkFilterByCurrentTrace.TabIndex = 1;
            this.chkFilterByCurrentTrace.Text = "Filter server files by selected trace";
            this.chkFilterByCurrentTrace.UseVisualStyleBackColor = true;
            this.chkFilterByCurrentTrace.CheckedChanged += new System.EventHandler(this.chkFilterByCurrentTrace_CheckedChanged);
            // 
            // chkFilterByCurrentPage
            // 
            this.chkFilterByCurrentPage.AutoSize = true;
            this.chkFilterByCurrentPage.Checked = true;
            this.chkFilterByCurrentPage.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkFilterByCurrentPage.Location = new System.Drawing.Point(6, 20);
            this.chkFilterByCurrentPage.Name = "chkFilterByCurrentPage";
            this.chkFilterByCurrentPage.Size = new System.Drawing.Size(186, 17);
            this.chkFilterByCurrentPage.TabIndex = 0;
            this.chkFilterByCurrentPage.Text = "Filter server files by current page";
            this.chkFilterByCurrentPage.UseVisualStyleBackColor = true;
            this.chkFilterByCurrentPage.CheckedChanged += new System.EventHandler(this.chkFilterByCurrentPage_CheckedChanged);
            // 
            // treeViewFiles
            // 
            this.treeViewFiles.AfterSelectOverride = null;
            this.treeViewFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.treeViewFiles.HideSelection = false;
            this.treeViewFiles.ImageIndex = 0;
            this.treeViewFiles.ImageList = this.images;
            this.treeViewFiles.Location = new System.Drawing.Point(3, 78);
            this.treeViewFiles.Name = "treeViewFiles";
            this.treeViewFiles.SelectedImageIndex = 0;
            this.treeViewFiles.Size = new System.Drawing.Size(147, 676);
            this.treeViewFiles.TabIndex = 0;
            this.treeViewFiles.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeViewFiles_BeforeExpand);
            this.treeViewFiles.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewFiles_AfterSelect);
            this.treeViewFiles.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeViewFiles_NodeMouseClick);
            // 
            // images
            // 
            this.images.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("images.ImageStream")));
            this.images.TransparentColor = System.Drawing.Color.Transparent;
            this.images.Images.SetKeyName(0, "Folder.ico");
            this.images.Images.SetKeyName(1, "java-package.png");
            this.images.Images.SetKeyName(2, "java-file.png");
            this.images.Images.SetKeyName(3, "server-file.png");
            // 
            // codeView1
            // 
            this.codeView1.BackColor = System.Drawing.Color.White;
            this.codeView1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.codeView1.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.codeView1.Dock = System.Windows.Forms.DockStyle.Right;
            this.codeView1.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.codeView1.FormattedDocument = null;
            this.codeView1.Location = new System.Drawing.Point(709, 32);
            this.codeView1.Name = "codeView1";
            this.codeView1.Size = new System.Drawing.Size(249, 734);
            this.codeView1.TabIndex = 1;
            this.codeView1.TextMarginFirst = 8;
            this.codeView1.TextMarginSecond = 8;
            this.codeView1.HighlightEntityUsage += new System.EventHandler<FireDetectiveAnalyzer.CodeViewEventArgs>(this.codeView1_HighlightEntityUsage);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnWhereCalled);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(958, 32);
            this.panel1.TabIndex = 2;
            this.panel1.TabStop = true;
            // 
            // btnWhereCalled
            // 
            this.btnWhereCalled.Location = new System.Drawing.Point(3, 4);
            this.btnWhereCalled.Name = "btnWhereCalled";
            this.btnWhereCalled.Size = new System.Drawing.Size(159, 23);
            this.btnWhereCalled.TabIndex = 2;
            this.btnWhereCalled.Text = "Where is this code called?";
            this.btnWhereCalled.UseVisualStyleBackColor = true;
            this.btnWhereCalled.Click += new System.EventHandler(this.btnWhereCalled_Click);
            // 
            // CodeViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1112, 766);
            this.Controls.Add(this.splitContainer1);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.Location = new System.Drawing.Point(240, 200);
            this.Name = "CodeViewer";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Move += new System.EventHandler(this.CodeViewer_Move);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CodeViewer_FormClosing);
            this.Resize += new System.EventHandler(this.CodeViewer_Resize);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.grpFilter.ResumeLayout(false);
            this.grpFilter.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private ItemsTreeView treeViewFiles;
        private System.Windows.Forms.ImageList images;
        private System.Windows.Forms.GroupBox grpFilter;
        private System.Windows.Forms.CheckBox chkFilterByCurrentTrace;
        private System.Windows.Forms.CheckBox chkFilterByCurrentPage;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnWhereCalled;
        private CodeView codeView1;

    }
}