namespace LiveSplit.SourceSplit
{
    partial class SourceSplitSettings
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle10 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle11 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle12 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle13 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle14 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle15 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle16 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle17 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle18 = new System.Windows.Forms.DataGridViewCellStyle();
            this.chkAutoSplitEnabled = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.lbMapBlacklist = new LiveSplit.SourceSplit.EditableListBox();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.rdoWhitelist = new System.Windows.Forms.RadioButton();
            this.lblMaps = new System.Windows.Forms.Label();
            this.rdoInterval = new System.Windows.Forms.RadioButton();
            this.dmnSplitInterval = new System.Windows.Forms.NumericUpDown();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.lbMapWhitelist = new LiveSplit.SourceSplit.EditableListBox();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnGameProcessesDefault = new System.Windows.Forms.Button();
            this.lbGameProcesses = new LiveSplit.SourceSplit.EditableListBox();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tlpAutoStartEndReset = new System.Windows.Forms.TableLayoutPanel();
            this.gbMisc = new System.Windows.Forms.GroupBox();
            this.tlpMisc = new System.Windows.Forms.TableLayoutPanel();
            this.chkShowGameTime = new System.Windows.Forms.CheckBox();
            this.btnShowMapTimes = new System.Windows.Forms.Button();
            this.chkShowAlt = new System.Windows.Forms.CheckBox();
            this.chkShowTickCount = new System.Windows.Forms.CheckBox();
            this.gbAutoStartEndReset = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.chkAutoStartEndReset = new System.Windows.Forms.CheckBox();
            this.labStartMap = new System.Windows.Forms.Label();
            this.boxStartMap = new System.Windows.Forms.TextBox();
            this.gbTiming = new System.Windows.Forms.GroupBox();
            this.tlpTiming = new System.Windows.Forms.TableLayoutPanel();
            this.cmbTimingMethod = new System.Windows.Forms.ComboBox();
            this.lblTimingMethod = new System.Windows.Forms.Label();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.lbMapBlacklist)).BeginInit();
            this.tableLayoutPanel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dmnSplitInterval)).BeginInit();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.lbMapWhitelist)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.lbGameProcesses)).BeginInit();
            this.tlpAutoStartEndReset.SuspendLayout();
            this.gbMisc.SuspendLayout();
            this.tlpMisc.SuspendLayout();
            this.gbAutoStartEndReset.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.gbTiming.SuspendLayout();
            this.tlpTiming.SuspendLayout();
            this.SuspendLayout();
            // 
            // chkAutoSplitEnabled
            // 
            this.chkAutoSplitEnabled.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.chkAutoSplitEnabled.AutoSize = true;
            this.chkAutoSplitEnabled.Location = new System.Drawing.Point(3, 6);
            this.chkAutoSplitEnabled.Name = "chkAutoSplitEnabled";
            this.chkAutoSplitEnabled.Size = new System.Drawing.Size(216, 17);
            this.chkAutoSplitEnabled.TabIndex = 5;
            this.chkAutoSplitEnabled.Text = "Enabled";
            this.chkAutoSplitEnabled.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.tlpAutoStartEndReset.SetColumnSpan(this.groupBox1, 2);
            this.groupBox1.Controls.Add(this.tableLayoutPanel3);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(450, 197);
            this.groupBox1.TabIndex = 10;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Auto Split";
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Controls.Add(this.groupBox4, 1, 1);
            this.tableLayoutPanel3.Controls.Add(this.chkAutoSplitEnabled, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.tableLayoutPanel4, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.groupBox3, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.label1, 0, 2);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 16);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 3;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(444, 178);
            this.tableLayoutPanel3.TabIndex = 20;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.lbMapBlacklist);
            this.groupBox4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox4.Location = new System.Drawing.Point(225, 33);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(216, 102);
            this.groupBox4.TabIndex = 16;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Map Blacklist";
            // 
            // lbMapBlacklist
            // 
            this.lbMapBlacklist.AllowUserToResizeRows = false;
            this.lbMapBlacklist.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.lbMapBlacklist.BackgroundColor = System.Drawing.SystemColors.Window;
            this.lbMapBlacklist.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lbMapBlacklist.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            dataGridViewCellStyle10.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle10.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle10.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle10.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle10.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle10.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle10.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.lbMapBlacklist.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle10;
            this.lbMapBlacklist.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.lbMapBlacklist.ColumnHeadersVisible = false;
            this.lbMapBlacklist.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1});
            dataGridViewCellStyle11.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle11.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle11.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle11.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle11.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle11.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle11.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.lbMapBlacklist.DefaultCellStyle = dataGridViewCellStyle11;
            this.lbMapBlacklist.Location = new System.Drawing.Point(3, 16);
            this.lbMapBlacklist.Name = "lbMapBlacklist";
            dataGridViewCellStyle12.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle12.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle12.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle12.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle12.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle12.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle12.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.lbMapBlacklist.RowHeadersDefaultCellStyle = dataGridViewCellStyle12;
            this.lbMapBlacklist.RowHeadersVisible = false;
            this.lbMapBlacklist.RowTemplate.Height = 14;
            this.lbMapBlacklist.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.lbMapBlacklist.Size = new System.Drawing.Size(210, 83);
            this.lbMapBlacklist.TabIndex = 15;
            this.toolTip.SetToolTip(this.lbMapBlacklist, "Don\'t split if the player was on one of these maps before a level change.");
            // 
            // Column1
            // 
            this.Column1.HeaderText = "Column1";
            this.Column1.Name = "Column1";
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 4;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel4.Controls.Add(this.rdoWhitelist, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.lblMaps, 3, 0);
            this.tableLayoutPanel4.Controls.Add(this.rdoInterval, 1, 0);
            this.tableLayoutPanel4.Controls.Add(this.dmnSplitInterval, 2, 0);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(225, 3);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 1;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(216, 24);
            this.tableLayoutPanel4.TabIndex = 20;
            // 
            // rdoWhitelist
            // 
            this.rdoWhitelist.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.rdoWhitelist.AutoSize = true;
            this.rdoWhitelist.Location = new System.Drawing.Point(3, 3);
            this.rdoWhitelist.Name = "rdoWhitelist";
            this.rdoWhitelist.Size = new System.Drawing.Size(65, 17);
            this.rdoWhitelist.TabIndex = 18;
            this.rdoWhitelist.Text = "Whitelist";
            this.rdoWhitelist.UseVisualStyleBackColor = true;
            // 
            // lblMaps
            // 
            this.lblMaps.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMaps.AutoSize = true;
            this.lblMaps.Location = new System.Drawing.Point(173, 5);
            this.lblMaps.Name = "lblMaps";
            this.lblMaps.Size = new System.Drawing.Size(40, 13);
            this.lblMaps.TabIndex = 11;
            this.lblMaps.Text = "maps";
            // 
            // rdoInterval
            // 
            this.rdoInterval.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.rdoInterval.AutoSize = true;
            this.rdoInterval.Checked = true;
            this.rdoInterval.Location = new System.Drawing.Point(74, 3);
            this.rdoInterval.Name = "rdoInterval";
            this.rdoInterval.Size = new System.Drawing.Size(52, 17);
            this.rdoInterval.TabIndex = 17;
            this.rdoInterval.TabStop = true;
            this.rdoInterval.Text = "Every";
            this.rdoInterval.UseVisualStyleBackColor = true;
            // 
            // dmnSplitInterval
            // 
            this.dmnSplitInterval.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.dmnSplitInterval.Enabled = false;
            this.dmnSplitInterval.Location = new System.Drawing.Point(132, 3);
            this.dmnSplitInterval.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.dmnSplitInterval.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.dmnSplitInterval.Name = "dmnSplitInterval";
            this.dmnSplitInterval.Size = new System.Drawing.Size(35, 20);
            this.dmnSplitInterval.TabIndex = 10;
            this.dmnSplitInterval.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.lbMapWhitelist);
            this.groupBox3.Location = new System.Drawing.Point(3, 33);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(216, 102);
            this.groupBox3.TabIndex = 15;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Map Whitelist";
            // 
            // lbMapWhitelist
            // 
            this.lbMapWhitelist.AllowUserToResizeRows = false;
            this.lbMapWhitelist.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.lbMapWhitelist.BackgroundColor = System.Drawing.SystemColors.Window;
            this.lbMapWhitelist.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lbMapWhitelist.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            dataGridViewCellStyle13.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle13.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle13.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle13.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle13.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle13.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle13.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.lbMapWhitelist.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle13;
            this.lbMapWhitelist.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.lbMapWhitelist.ColumnHeadersVisible = false;
            this.lbMapWhitelist.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn1});
            dataGridViewCellStyle14.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle14.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle14.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle14.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle14.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle14.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle14.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.lbMapWhitelist.DefaultCellStyle = dataGridViewCellStyle14;
            this.lbMapWhitelist.Location = new System.Drawing.Point(3, 16);
            this.lbMapWhitelist.Name = "lbMapWhitelist";
            dataGridViewCellStyle15.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle15.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle15.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle15.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle15.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle15.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle15.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.lbMapWhitelist.RowHeadersDefaultCellStyle = dataGridViewCellStyle15;
            this.lbMapWhitelist.RowHeadersVisible = false;
            this.lbMapWhitelist.RowTemplate.Height = 14;
            this.lbMapWhitelist.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.lbMapWhitelist.Size = new System.Drawing.Size(210, 83);
            this.lbMapWhitelist.TabIndex = 16;
            this.toolTip.SetToolTip(this.lbMapWhitelist, "Only split if the player was on one of these maps before a level change.");
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.HeaderText = "Column1";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.label1.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label1.Location = new System.Drawing.Point(3, 145);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(191, 26);
            this.label1.TabIndex = 21;
            this.label1.Text = "(these lists will be checked against the previous map in a level change)";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnGameProcessesDefault);
            this.groupBox2.Controls.Add(this.lbGameProcesses);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(231, 206);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(222, 133);
            this.groupBox2.TabIndex = 11;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Game Process List";
            // 
            // btnGameProcessesDefault
            // 
            this.btnGameProcessesDefault.Location = new System.Drawing.Point(144, 105);
            this.btnGameProcessesDefault.Name = "btnGameProcessesDefault";
            this.btnGameProcessesDefault.Size = new System.Drawing.Size(75, 23);
            this.btnGameProcessesDefault.TabIndex = 18;
            this.btnGameProcessesDefault.Text = "Defaults";
            this.btnGameProcessesDefault.UseVisualStyleBackColor = true;
            this.btnGameProcessesDefault.Click += new System.EventHandler(this.btnGameProcessesDefault_Click);
            // 
            // lbGameProcesses
            // 
            this.lbGameProcesses.AllowUserToResizeRows = false;
            this.lbGameProcesses.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.lbGameProcesses.BackgroundColor = System.Drawing.SystemColors.Window;
            this.lbGameProcesses.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lbGameProcesses.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            dataGridViewCellStyle16.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle16.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle16.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle16.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle16.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle16.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle16.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.lbGameProcesses.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle16;
            this.lbGameProcesses.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.lbGameProcesses.ColumnHeadersVisible = false;
            this.lbGameProcesses.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn2});
            dataGridViewCellStyle17.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle17.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle17.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle17.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle17.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle17.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle17.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.lbGameProcesses.DefaultCellStyle = dataGridViewCellStyle17;
            this.lbGameProcesses.Location = new System.Drawing.Point(3, 16);
            this.lbGameProcesses.Name = "lbGameProcesses";
            dataGridViewCellStyle18.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle18.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle18.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle18.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle18.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle18.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle18.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.lbGameProcesses.RowHeadersDefaultCellStyle = dataGridViewCellStyle18;
            this.lbGameProcesses.RowHeadersVisible = false;
            this.lbGameProcesses.RowTemplate.Height = 14;
            this.lbGameProcesses.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.lbGameProcesses.Size = new System.Drawing.Size(216, 83);
            this.lbGameProcesses.TabIndex = 17;
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.HeaderText = "Column1";
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            // 
            // tlpAutoStartEndReset
            // 
            this.tlpAutoStartEndReset.ColumnCount = 2;
            this.tlpAutoStartEndReset.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpAutoStartEndReset.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpAutoStartEndReset.Controls.Add(this.groupBox1, 0, 0);
            this.tlpAutoStartEndReset.Controls.Add(this.gbMisc, 0, 2);
            this.tlpAutoStartEndReset.Controls.Add(this.gbAutoStartEndReset, 0, 1);
            this.tlpAutoStartEndReset.Controls.Add(this.groupBox2, 1, 1);
            this.tlpAutoStartEndReset.Controls.Add(this.gbTiming, 1, 2);
            this.tlpAutoStartEndReset.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpAutoStartEndReset.Location = new System.Drawing.Point(7, 7);
            this.tlpAutoStartEndReset.Name = "tlpAutoStartEndReset";
            this.tlpAutoStartEndReset.RowCount = 4;
            this.tlpAutoStartEndReset.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 203F));
            this.tlpAutoStartEndReset.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 139F));
            this.tlpAutoStartEndReset.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 125F));
            this.tlpAutoStartEndReset.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 12F));
            this.tlpAutoStartEndReset.Size = new System.Drawing.Size(456, 484);
            this.tlpAutoStartEndReset.TabIndex = 13;
            // 
            // gbMisc
            // 
            this.gbMisc.Controls.Add(this.tlpMisc);
            this.gbMisc.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbMisc.Location = new System.Drawing.Point(3, 345);
            this.gbMisc.Name = "gbMisc";
            this.gbMisc.Size = new System.Drawing.Size(222, 119);
            this.gbMisc.TabIndex = 14;
            this.gbMisc.TabStop = false;
            this.gbMisc.Text = "Misc.";
            // 
            // tlpMisc
            // 
            this.tlpMisc.ColumnCount = 1;
            this.tlpMisc.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMisc.Controls.Add(this.chkShowGameTime, 0, 0);
            this.tlpMisc.Controls.Add(this.btnShowMapTimes, 0, 3);
            this.tlpMisc.Controls.Add(this.chkShowAlt, 0, 1);
            this.tlpMisc.Controls.Add(this.chkShowTickCount, 0, 2);
            this.tlpMisc.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMisc.Location = new System.Drawing.Point(3, 16);
            this.tlpMisc.Name = "tlpMisc";
            this.tlpMisc.RowCount = 4;
            this.tlpMisc.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMisc.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMisc.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMisc.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMisc.Size = new System.Drawing.Size(216, 100);
            this.tlpMisc.TabIndex = 0;
            // 
            // chkShowGameTime
            // 
            this.chkShowGameTime.AutoSize = true;
            this.chkShowGameTime.Location = new System.Drawing.Point(3, 3);
            this.chkShowGameTime.Name = "chkShowGameTime";
            this.chkShowGameTime.Size = new System.Drawing.Size(159, 17);
            this.chkShowGameTime.TabIndex = 12;
            this.chkShowGameTime.Text = "Show Higher Precision Time";
            this.chkShowGameTime.UseVisualStyleBackColor = true;
            this.chkShowGameTime.CheckedChanged += new System.EventHandler(this.chkShowGameTime_CheckedChanged);
            // 
            // btnShowMapTimes
            // 
            this.btnShowMapTimes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnShowMapTimes.Location = new System.Drawing.Point(3, 69);
            this.btnShowMapTimes.Name = "btnShowMapTimes";
            this.btnShowMapTimes.Size = new System.Drawing.Size(210, 28);
            this.btnShowMapTimes.TabIndex = 20;
            this.btnShowMapTimes.Text = "Show Map Times";
            this.btnShowMapTimes.UseVisualStyleBackColor = true;
            this.btnShowMapTimes.Click += new System.EventHandler(this.btnShowMapTimes_Click);
            // 
            // chkShowAlt
            // 
            this.chkShowAlt.AutoSize = true;
            this.chkShowAlt.Location = new System.Drawing.Point(20, 23);
            this.chkShowAlt.Margin = new System.Windows.Forms.Padding(20, 0, 3, 3);
            this.chkShowAlt.Name = "chkShowAlt";
            this.chkShowAlt.Size = new System.Drawing.Size(171, 17);
            this.chkShowAlt.TabIndex = 22;
            this.chkShowAlt.Text = "Show Alternate Timing Method";
            this.chkShowAlt.UseVisualStyleBackColor = true;
            // 
            // chkShowTickCount
            // 
            this.chkShowTickCount.AutoSize = true;
            this.chkShowTickCount.Location = new System.Drawing.Point(3, 46);
            this.chkShowTickCount.Name = "chkShowTickCount";
            this.chkShowTickCount.Size = new System.Drawing.Size(165, 17);
            this.chkShowTickCount.TabIndex = 21;
            this.chkShowTickCount.Text = "Show Game Time Tick Count";
            this.chkShowTickCount.UseVisualStyleBackColor = true;
            // 
            // gbAutoStartEndReset
            // 
            this.gbAutoStartEndReset.Controls.Add(this.tableLayoutPanel2);
            this.gbAutoStartEndReset.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbAutoStartEndReset.Location = new System.Drawing.Point(3, 206);
            this.gbAutoStartEndReset.Name = "gbAutoStartEndReset";
            this.gbAutoStartEndReset.Size = new System.Drawing.Size(222, 133);
            this.gbAutoStartEndReset.TabIndex = 13;
            this.gbAutoStartEndReset.TabStop = false;
            this.gbAutoStartEndReset.Text = "Auto Start / End / Reset";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.chkAutoStartEndReset, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.labStartMap, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.boxStartMap, 0, 2);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 16);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 3;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 21F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(216, 114);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // chkAutoStartEndReset
            // 
            this.chkAutoStartEndReset.AutoSize = true;
            this.chkAutoStartEndReset.Location = new System.Drawing.Point(3, 3);
            this.chkAutoStartEndReset.Name = "chkAutoStartEndReset";
            this.chkAutoStartEndReset.Size = new System.Drawing.Size(177, 17);
            this.chkAutoStartEndReset.TabIndex = 0;
            this.chkAutoStartEndReset.Text = "Enabled (supported games only)";
            this.chkAutoStartEndReset.UseVisualStyleBackColor = true;
            // 
            // labStartMap
            // 
            this.labStartMap.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labStartMap.AutoSize = true;
            this.labStartMap.Location = new System.Drawing.Point(3, 27);
            this.labStartMap.Name = "labStartMap";
            this.labStartMap.Size = new System.Drawing.Size(193, 13);
            this.labStartMap.TabIndex = 1;
            this.labStartMap.Text = "Auto Start upon newly loading this map:";
            // 
            // boxStartMap
            // 
            this.boxStartMap.Location = new System.Drawing.Point(3, 47);
            this.boxStartMap.Name = "boxStartMap";
            this.boxStartMap.Size = new System.Drawing.Size(210, 20);
            this.boxStartMap.TabIndex = 2;
            // 
            // gbTiming
            // 
            this.gbTiming.Controls.Add(this.tlpTiming);
            this.gbTiming.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbTiming.Location = new System.Drawing.Point(231, 345);
            this.gbTiming.Name = "gbTiming";
            this.gbTiming.Size = new System.Drawing.Size(222, 119);
            this.gbTiming.TabIndex = 21;
            this.gbTiming.TabStop = false;
            this.gbTiming.Text = "Game Time";
            // 
            // tlpTiming
            // 
            this.tlpTiming.ColumnCount = 2;
            this.tlpTiming.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 26.85185F));
            this.tlpTiming.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 73.14815F));
            this.tlpTiming.Controls.Add(this.cmbTimingMethod, 1, 0);
            this.tlpTiming.Controls.Add(this.lblTimingMethod, 0, 0);
            this.tlpTiming.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpTiming.Location = new System.Drawing.Point(3, 16);
            this.tlpTiming.Name = "tlpTiming";
            this.tlpTiming.RowCount = 2;
            this.tlpTiming.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpTiming.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpTiming.Size = new System.Drawing.Size(216, 100);
            this.tlpTiming.TabIndex = 1;
            // 
            // cmbTimingMethod
            // 
            this.cmbTimingMethod.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmbTimingMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTimingMethod.FormattingEnabled = true;
            this.cmbTimingMethod.Items.AddRange(new object[] {
            "Automatic",
            "Engine Ticks",
            "Engine Ticks with Pauses"});
            this.cmbTimingMethod.Location = new System.Drawing.Point(60, 3);
            this.cmbTimingMethod.Name = "cmbTimingMethod";
            this.cmbTimingMethod.Size = new System.Drawing.Size(153, 21);
            this.cmbTimingMethod.TabIndex = 0;
            this.toolTip.SetToolTip(this.cmbTimingMethod, "Automatic: Choose depending on rules of the game");
            // 
            // lblTimingMethod
            // 
            this.lblTimingMethod.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblTimingMethod.AutoSize = true;
            this.lblTimingMethod.Location = new System.Drawing.Point(11, 7);
            this.lblTimingMethod.Name = "lblTimingMethod";
            this.lblTimingMethod.Size = new System.Drawing.Size(43, 13);
            this.lblTimingMethod.TabIndex = 1;
            this.lblTimingMethod.Text = "Method";
            // 
            // toolTip
            // 
            this.toolTip.AutoPopDelay = 5000;
            this.toolTip.InitialDelay = 100;
            this.toolTip.ReshowDelay = 100;
            // 
            // SourceSplitSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tlpAutoStartEndReset);
            this.Name = "SourceSplitSettings";
            this.Padding = new System.Windows.Forms.Padding(7);
            this.Size = new System.Drawing.Size(470, 498);
            this.groupBox1.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.lbMapBlacklist)).EndInit();
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dmnSplitInterval)).EndInit();
            this.groupBox3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.lbMapWhitelist)).EndInit();
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.lbGameProcesses)).EndInit();
            this.tlpAutoStartEndReset.ResumeLayout(false);
            this.gbMisc.ResumeLayout(false);
            this.tlpMisc.ResumeLayout(false);
            this.tlpMisc.PerformLayout();
            this.gbAutoStartEndReset.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.gbTiming.ResumeLayout(false);
            this.tlpTiming.ResumeLayout(false);
            this.tlpTiming.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckBox chkAutoSplitEnabled;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label lblMaps;
        private System.Windows.Forms.NumericUpDown dmnSplitInterval;
        private System.Windows.Forms.GroupBox groupBox2;
        private EditableListBox lbMapWhitelist;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private EditableListBox lbMapBlacklist;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.RadioButton rdoWhitelist;
        private System.Windows.Forms.RadioButton rdoInterval;
        private System.Windows.Forms.TableLayoutPanel tlpAutoStartEndReset;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.GroupBox groupBox3;
        private EditableListBox lbGameProcesses;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.CheckBox chkShowGameTime;
        private System.Windows.Forms.GroupBox gbAutoStartEndReset;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.CheckBox chkAutoStartEndReset;
        private System.Windows.Forms.GroupBox gbMisc;
        private System.Windows.Forms.TableLayoutPanel tlpMisc;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.Button btnShowMapTimes;
        private System.Windows.Forms.GroupBox gbTiming;
        private System.Windows.Forms.ComboBox cmbTimingMethod;
        private System.Windows.Forms.TableLayoutPanel tlpTiming;
        private System.Windows.Forms.Label lblTimingMethod;
        private System.Windows.Forms.Button btnGameProcessesDefault;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labStartMap;
        private System.Windows.Forms.TextBox boxStartMap;
        private System.Windows.Forms.CheckBox chkShowTickCount;
        private System.Windows.Forms.CheckBox chkShowAlt;
    }
}
