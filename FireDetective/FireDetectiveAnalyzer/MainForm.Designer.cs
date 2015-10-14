namespace FireDetectiveAnalyzer
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.tmrUpdate = new System.Windows.Forms.Timer(this.components);
            this.tmrServerReconnect = new System.Windows.Forms.Timer(this.components);
            this.images = new System.Windows.Forms.ImageList(this.components);
            this.splitterTop = new System.Windows.Forms.SplitContainer();
            this.tableLeft = new System.Windows.Forms.TableLayoutPanel();
            this.grpHighlight = new System.Windows.Forms.GroupBox();
            this.btnClearHighlight = new System.Windows.Forms.Button();
            this.lblHighlight = new System.Windows.Forms.Label();
            this.btnNextHighlight = new System.Windows.Forms.Button();
            this.btnPrevHighlight = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.chkSections = new System.Windows.Forms.CheckBox();
            this.radLastOnly = new System.Windows.Forms.RadioButton();
            this.chkShowEvents = new System.Windows.Forms.CheckBox();
            this.radAll = new System.Windows.Forms.RadioButton();
            this.chkShowRequests = new System.Windows.Forms.CheckBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnShowCodePane = new System.Windows.Forms.Button();
            this.cmdView1 = new System.Windows.Forms.Button();
            this.cmdView2 = new System.Windows.Forms.Button();
            this.picSpinner = new System.Windows.Forms.PictureBox();
            this.cmdServerReconnect = new System.Windows.Forms.Button();
            this.cmdFirefoxReconnect = new System.Windows.Forms.Button();
            this.lblServerConnection = new System.Windows.Forms.Label();
            this.lblFirefoxConnection = new System.Windows.Forms.Label();
            this.contextMenuStripPages = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuShowTimeoutIntervalSet = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuShowTimeoutIntervalClear = new System.Windows.Forms.ToolStripMenuItem();
            this.treeViewPages = new FireDetectiveAnalyzer.ItemsTreeView();
            this.treeViewCalls = new FireDetectiveAnalyzer.HoverableTreeView();
            this.splitterTop.Panel1.SuspendLayout();
            this.splitterTop.Panel2.SuspendLayout();
            this.splitterTop.SuspendLayout();
            this.tableLeft.SuspendLayout();
            this.grpHighlight.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picSpinner)).BeginInit();
            this.contextMenuStripPages.SuspendLayout();
            this.SuspendLayout();
            // 
            // tmrUpdate
            // 
            this.tmrUpdate.Enabled = true;
            this.tmrUpdate.Tick += new System.EventHandler(this.tmrUpdate_Tick);
            // 
            // tmrServerReconnect
            // 
            this.tmrServerReconnect.Interval = 1000;
            // 
            // images
            // 
            this.images.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("images.ImageStream")));
            this.images.TransparentColor = System.Drawing.Color.Transparent;
            this.images.Images.SetKeyName(0, "GoToNextHS.png");
            this.images.Images.SetKeyName(1, "DocumentHS.png");
            this.images.Images.SetKeyName(2, "red.png");
            this.images.Images.SetKeyName(3, "xhr.png");
            this.images.Images.SetKeyName(4, "purple.png");
            this.images.Images.SetKeyName(5, "done.png");
            this.images.Images.SetKeyName(6, "event.png");
            this.images.Images.SetKeyName(7, "combined.png");
            this.images.Images.SetKeyName(8, "timeout.png");
            this.images.Images.SetKeyName(9, "event_ref.png");
            this.images.Images.SetKeyName(10, "call_ref.png");
            this.images.Images.SetKeyName(11, "section6.png");
            this.images.Images.SetKeyName(12, "highlight.png");
            this.images.Images.SetKeyName(13, "highlight2.png");
            // 
            // splitterTop
            // 
            this.splitterTop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitterTop.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitterTop.Location = new System.Drawing.Point(0, 0);
            this.splitterTop.Margin = new System.Windows.Forms.Padding(0);
            this.splitterTop.Name = "splitterTop";
            // 
            // splitterTop.Panel1
            // 
            this.splitterTop.Panel1.Controls.Add(this.tableLeft);
            // 
            // splitterTop.Panel2
            // 
            this.splitterTop.Panel2.Controls.Add(this.panel1);
            this.splitterTop.Panel2.Controls.Add(this.treeViewCalls);
            this.splitterTop.Panel2.Padding = new System.Windows.Forms.Padding(0, 0, 3, 3);
            this.splitterTop.Size = new System.Drawing.Size(999, 166);
            this.splitterTop.SplitterDistance = 520;
            this.splitterTop.TabIndex = 2;
            this.splitterTop.TabStop = false;
            // 
            // tableLeft
            // 
            this.tableLeft.ColumnCount = 1;
            this.tableLeft.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLeft.Controls.Add(this.treeViewPages, 0, 2);
            this.tableLeft.Controls.Add(this.grpHighlight, 0, 1);
            this.tableLeft.Controls.Add(this.panel2, 0, 0);
            this.tableLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLeft.Location = new System.Drawing.Point(0, 0);
            this.tableLeft.Margin = new System.Windows.Forms.Padding(0);
            this.tableLeft.Name = "tableLeft";
            this.tableLeft.RowCount = 3;
            this.tableLeft.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 51F));
            this.tableLeft.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLeft.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLeft.Size = new System.Drawing.Size(520, 166);
            this.tableLeft.TabIndex = 21;
            // 
            // grpHighlight
            // 
            this.grpHighlight.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.grpHighlight.Controls.Add(this.btnClearHighlight);
            this.grpHighlight.Controls.Add(this.lblHighlight);
            this.grpHighlight.Controls.Add(this.btnNextHighlight);
            this.grpHighlight.Controls.Add(this.btnPrevHighlight);
            this.grpHighlight.Location = new System.Drawing.Point(3, 51);
            this.grpHighlight.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.grpHighlight.Name = "grpHighlight";
            this.grpHighlight.Size = new System.Drawing.Size(517, 43);
            this.grpHighlight.TabIndex = 16;
            this.grpHighlight.TabStop = false;
            this.grpHighlight.Text = "Highlight";
            // 
            // btnClearHighlight
            // 
            this.btnClearHighlight.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClearHighlight.Location = new System.Drawing.Point(436, 14);
            this.btnClearHighlight.Name = "btnClearHighlight";
            this.btnClearHighlight.Size = new System.Drawing.Size(75, 23);
            this.btnClearHighlight.TabIndex = 0;
            this.btnClearHighlight.Text = "Hide";
            this.btnClearHighlight.UseVisualStyleBackColor = true;
            this.btnClearHighlight.Click += new System.EventHandler(this.btnClearHighlight_Click);
            // 
            // lblHighlight
            // 
            this.lblHighlight.AutoSize = true;
            this.lblHighlight.Location = new System.Drawing.Point(68, 20);
            this.lblHighlight.Name = "lblHighlight";
            this.lblHighlight.Size = new System.Drawing.Size(106, 13);
            this.lblHighlight.TabIndex = 4;
            this.lblHighlight.Text = "(nothing highlighted)";
            this.lblHighlight.MouseLeave += new System.EventHandler(this.lblHighlight_MouseLeave);
            this.lblHighlight.Click += new System.EventHandler(this.lblHighlight_Click);
            this.lblHighlight.MouseEnter += new System.EventHandler(this.lblHighlight_MouseEnter);
            // 
            // btnNextHighlight
            // 
            this.btnNextHighlight.Location = new System.Drawing.Point(37, 15);
            this.btnNextHighlight.Name = "btnNextHighlight";
            this.btnNextHighlight.Size = new System.Drawing.Size(25, 23);
            this.btnNextHighlight.TabIndex = 3;
            this.btnNextHighlight.Text = ">";
            this.btnNextHighlight.UseVisualStyleBackColor = true;
            this.btnNextHighlight.Click += new System.EventHandler(this.btnNextHighlight_Click);
            // 
            // btnPrevHighlight
            // 
            this.btnPrevHighlight.Location = new System.Drawing.Point(6, 15);
            this.btnPrevHighlight.Name = "btnPrevHighlight";
            this.btnPrevHighlight.Size = new System.Drawing.Size(25, 23);
            this.btnPrevHighlight.TabIndex = 2;
            this.btnPrevHighlight.Text = "<";
            this.btnPrevHighlight.UseVisualStyleBackColor = true;
            this.btnPrevHighlight.Click += new System.EventHandler(this.btnPrevHighlight_Click);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.chkSections);
            this.panel2.Controls.Add(this.radLastOnly);
            this.panel2.Controls.Add(this.chkShowEvents);
            this.panel2.Controls.Add(this.radAll);
            this.panel2.Controls.Add(this.chkShowRequests);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(3, 3);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(514, 45);
            this.panel2.TabIndex = 17;
            // 
            // chkSections
            // 
            this.chkSections.AutoSize = true;
            this.chkSections.Location = new System.Drawing.Point(200, 26);
            this.chkSections.Name = "chkSections";
            this.chkSections.Size = new System.Drawing.Size(94, 17);
            this.chkSections.TabIndex = 24;
            this.chkSections.Text = "Show sections";
            this.chkSections.UseVisualStyleBackColor = true;
            this.chkSections.CheckedChanged += new System.EventHandler(this.chkSections_CheckedChanged);
            // 
            // radLastOnly
            // 
            this.radLastOnly.AutoSize = true;
            this.radLastOnly.Checked = true;
            this.radLastOnly.Location = new System.Drawing.Point(3, 3);
            this.radLastOnly.Name = "radLastOnly";
            this.radLastOnly.Size = new System.Drawing.Size(71, 17);
            this.radLastOnly.TabIndex = 21;
            this.radLastOnly.TabStop = true;
            this.radLastOnly.Text = "Show last";
            this.radLastOnly.UseVisualStyleBackColor = true;
            this.radLastOnly.CheckedChanged += new System.EventHandler(this.radLastOnly_CheckedChanged);
            // 
            // chkShowEvents
            // 
            this.chkShowEvents.AutoSize = true;
            this.chkShowEvents.Location = new System.Drawing.Point(106, 26);
            this.chkShowEvents.Name = "chkShowEvents";
            this.chkShowEvents.Size = new System.Drawing.Size(88, 17);
            this.chkShowEvents.TabIndex = 25;
            this.chkShowEvents.Text = "Show events";
            this.chkShowEvents.UseVisualStyleBackColor = true;
            this.chkShowEvents.CheckedChanged += new System.EventHandler(this.chkShowEvents_CheckedChanged);
            // 
            // radAll
            // 
            this.radAll.AutoSize = true;
            this.radAll.Location = new System.Drawing.Point(80, 3);
            this.radAll.Name = "radAll";
            this.radAll.Size = new System.Drawing.Size(64, 17);
            this.radAll.TabIndex = 22;
            this.radAll.Text = "Show all";
            this.radAll.UseVisualStyleBackColor = true;
            this.radAll.CheckedChanged += new System.EventHandler(this.radAll_CheckedChanged);
            // 
            // chkShowRequests
            // 
            this.chkShowRequests.AutoSize = true;
            this.chkShowRequests.Location = new System.Drawing.Point(3, 26);
            this.chkShowRequests.Name = "chkShowRequests";
            this.chkShowRequests.Size = new System.Drawing.Size(97, 17);
            this.chkShowRequests.TabIndex = 23;
            this.chkShowRequests.Text = "Show requests";
            this.chkShowRequests.UseVisualStyleBackColor = true;
            this.chkShowRequests.CheckedChanged += new System.EventHandler(this.chkShowRequests_CheckedChanged);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.panel1.Controls.Add(this.btnShowCodePane);
            this.panel1.Controls.Add(this.cmdView1);
            this.panel1.Controls.Add(this.cmdView2);
            this.panel1.Controls.Add(this.picSpinner);
            this.panel1.Controls.Add(this.cmdServerReconnect);
            this.panel1.Controls.Add(this.cmdFirefoxReconnect);
            this.panel1.Controls.Add(this.lblServerConnection);
            this.panel1.Controls.Add(this.lblFirefoxConnection);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(472, 48);
            this.panel1.TabIndex = 11;
            // 
            // btnShowCodePane
            // 
            this.btnShowCodePane.Location = new System.Drawing.Point(6, 4);
            this.btnShowCodePane.Name = "btnShowCodePane";
            this.btnShowCodePane.Size = new System.Drawing.Size(115, 23);
            this.btnShowCodePane.TabIndex = 17;
            this.btnShowCodePane.Text = "Show code pane";
            this.btnShowCodePane.UseVisualStyleBackColor = true;
            this.btnShowCodePane.Click += new System.EventHandler(this.btnShowCodePane_Click);
            // 
            // cmdView1
            // 
            this.cmdView1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdView1.Location = new System.Drawing.Point(407, 4);
            this.cmdView1.Name = "cmdView1";
            this.cmdView1.Size = new System.Drawing.Size(28, 23);
            this.cmdView1.TabIndex = 16;
            this.cmdView1.Text = "1";
            this.cmdView1.UseVisualStyleBackColor = true;
            this.cmdView1.Click += new System.EventHandler(this.cmdView1_Click);
            // 
            // cmdView2
            // 
            this.cmdView2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdView2.Location = new System.Drawing.Point(441, 4);
            this.cmdView2.Name = "cmdView2";
            this.cmdView2.Size = new System.Drawing.Size(28, 23);
            this.cmdView2.TabIndex = 5;
            this.cmdView2.Text = "2";
            this.cmdView2.UseVisualStyleBackColor = true;
            this.cmdView2.Click += new System.EventHandler(this.cmdView2_Click);
            // 
            // picSpinner
            // 
            this.picSpinner.Image = ((System.Drawing.Image)(resources.GetObject("picSpinner.Image")));
            this.picSpinner.Location = new System.Drawing.Point(-100, 0);
            this.picSpinner.Name = "picSpinner";
            this.picSpinner.Size = new System.Drawing.Size(16, 16);
            this.picSpinner.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.picSpinner.TabIndex = 15;
            this.picSpinner.TabStop = false;
            this.picSpinner.Visible = false;
            // 
            // cmdServerReconnect
            // 
            this.cmdServerReconnect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdServerReconnect.Location = new System.Drawing.Point(132, 4);
            this.cmdServerReconnect.Name = "cmdServerReconnect";
            this.cmdServerReconnect.Size = new System.Drawing.Size(75, 23);
            this.cmdServerReconnect.TabIndex = 4;
            this.cmdServerReconnect.Text = "Reconnect";
            this.cmdServerReconnect.UseVisualStyleBackColor = true;
            this.cmdServerReconnect.Click += new System.EventHandler(this.cmdServerReconnect_Click);
            // 
            // cmdFirefoxReconnect
            // 
            this.cmdFirefoxReconnect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdFirefoxReconnect.Location = new System.Drawing.Point(-81, 4);
            this.cmdFirefoxReconnect.Name = "cmdFirefoxReconnect";
            this.cmdFirefoxReconnect.Size = new System.Drawing.Size(75, 23);
            this.cmdFirefoxReconnect.TabIndex = 3;
            this.cmdFirefoxReconnect.Text = "Reconnect";
            this.cmdFirefoxReconnect.UseVisualStyleBackColor = true;
            this.cmdFirefoxReconnect.Click += new System.EventHandler(this.cmdFirefoxReconnect_Click);
            // 
            // lblServerConnection
            // 
            this.lblServerConnection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblServerConnection.AutoSize = true;
            this.lblServerConnection.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblServerConnection.Location = new System.Drawing.Point(213, 9);
            this.lblServerConnection.Name = "lblServerConnection";
            this.lblServerConnection.Size = new System.Drawing.Size(44, 13);
            this.lblServerConnection.TabIndex = 2;
            this.lblServerConnection.Text = "$server";
            // 
            // lblFirefoxConnection
            // 
            this.lblFirefoxConnection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblFirefoxConnection.AutoSize = true;
            this.lblFirefoxConnection.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFirefoxConnection.Location = new System.Drawing.Point(0, 9);
            this.lblFirefoxConnection.Name = "lblFirefoxConnection";
            this.lblFirefoxConnection.Size = new System.Drawing.Size(45, 13);
            this.lblFirefoxConnection.TabIndex = 1;
            this.lblFirefoxConnection.Text = "$firefox";
            // 
            // contextMenuStripPages
            // 
            this.contextMenuStripPages.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuShowTimeoutIntervalSet,
            this.mnuShowTimeoutIntervalClear});
            this.contextMenuStripPages.Name = "contextMenuStripPages";
            this.contextMenuStripPages.Size = new System.Drawing.Size(267, 48);
            // 
            // mnuShowTimeoutIntervalSet
            // 
            this.mnuShowTimeoutIntervalSet.Enabled = false;
            this.mnuShowTimeoutIntervalSet.Name = "mnuShowTimeoutIntervalSet";
            this.mnuShowTimeoutIntervalSet.Size = new System.Drawing.Size(266, 22);
            this.mnuShowTimeoutIntervalSet.Text = "Show timeout/interval set location";
            this.mnuShowTimeoutIntervalSet.Click += new System.EventHandler(this.mnuShowTimeoutIntervalSet_Click);
            // 
            // mnuShowTimeoutIntervalClear
            // 
            this.mnuShowTimeoutIntervalClear.Enabled = false;
            this.mnuShowTimeoutIntervalClear.Name = "mnuShowTimeoutIntervalClear";
            this.mnuShowTimeoutIntervalClear.Size = new System.Drawing.Size(266, 22);
            this.mnuShowTimeoutIntervalClear.Text = "Show timeout/interval clear location";
            this.mnuShowTimeoutIntervalClear.Click += new System.EventHandler(this.mnuShowTimeoutIntervalClear_Click);
            // 
            // treeViewPages
            // 
            this.treeViewPages.AfterSelectOverride = null;
            this.treeViewPages.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewPages.FullRowSelect = true;
            this.treeViewPages.HideSelection = false;
            this.treeViewPages.ImageIndex = 0;
            this.treeViewPages.ImageList = this.images;
            this.treeViewPages.Location = new System.Drawing.Point(3, 101);
            this.treeViewPages.Margin = new System.Windows.Forms.Padding(3, 0, 0, 3);
            this.treeViewPages.Name = "treeViewPages";
            this.treeViewPages.SelectedImageIndex = 0;
            this.treeViewPages.Size = new System.Drawing.Size(517, 62);
            this.treeViewPages.TabIndex = 9;
            this.treeViewPages.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeViewPages_BeforeExpand);
            this.treeViewPages.AfterSelectOrSelectedNodeClick += new System.Windows.Forms.TreeViewEventHandler(this.treeViewPages_AfterSelectOrSelectedNodeClick);
            this.treeViewPages.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeViewPages_NodeMouseClick);
            // 
            // treeViewCalls
            // 
            this.treeViewCalls.AfterSelectOverride = null;
            this.treeViewCalls.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.treeViewCalls.HideSelection = false;
            this.treeViewCalls.HoverColor = System.Drawing.Color.White;
            this.treeViewCalls.Location = new System.Drawing.Point(0, 51);
            this.treeViewCalls.Name = "treeViewCalls";
            this.treeViewCalls.OnlyHoverSelectionChildNodes = true;
            this.treeViewCalls.Size = new System.Drawing.Size(472, 112);
            this.treeViewCalls.TabIndex = 10;
            this.treeViewCalls.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeViewCalls_NodeMouseDoubleClick);
            this.treeViewCalls.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeViewCalls_BeforeExpand);
            this.treeViewCalls.AfterSelectOrSelectedNodeClick += new System.Windows.Forms.TreeViewEventHandler(this.treeViewCalls_AfterSelectOrSelectedNodeClick);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(999, 166);
            this.Controls.Add(this.splitterTop);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "FireDetective Visualizer";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Activated += new System.EventHandler(this.MainForm_Activated);
            this.Move += new System.EventHandler(this.MainForm_Move);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            this.splitterTop.Panel1.ResumeLayout(false);
            this.splitterTop.Panel2.ResumeLayout(false);
            this.splitterTop.ResumeLayout(false);
            this.tableLeft.ResumeLayout(false);
            this.grpHighlight.ResumeLayout(false);
            this.grpHighlight.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picSpinner)).EndInit();
            this.contextMenuStripPages.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer tmrUpdate;
        private System.Windows.Forms.Timer tmrServerReconnect;
        private System.Windows.Forms.ImageList images;
        private System.Windows.Forms.SplitContainer splitterTop;
        private System.Windows.Forms.TableLayoutPanel tableLeft;
        private ItemsTreeView treeViewPages;
        private System.Windows.Forms.GroupBox grpHighlight;
        private System.Windows.Forms.Button btnClearHighlight;
        private System.Windows.Forms.Label lblHighlight;
        private System.Windows.Forms.Button btnNextHighlight;
        private System.Windows.Forms.Button btnPrevHighlight;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.CheckBox chkSections;
        private System.Windows.Forms.RadioButton radLastOnly;
        private System.Windows.Forms.CheckBox chkShowEvents;
        private System.Windows.Forms.RadioButton radAll;
        private System.Windows.Forms.CheckBox chkShowRequests;
        private HoverableTreeView treeViewCalls;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnShowCodePane;
        private System.Windows.Forms.Button cmdView1;
        private System.Windows.Forms.Button cmdView2;
        private System.Windows.Forms.PictureBox picSpinner;
        private System.Windows.Forms.Button cmdServerReconnect;
        private System.Windows.Forms.Button cmdFirefoxReconnect;
        private System.Windows.Forms.Label lblServerConnection;
        private System.Windows.Forms.Label lblFirefoxConnection;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripPages;
        private System.Windows.Forms.ToolStripMenuItem mnuShowTimeoutIntervalSet;
        private System.Windows.Forms.ToolStripMenuItem mnuShowTimeoutIntervalClear;
    }
}