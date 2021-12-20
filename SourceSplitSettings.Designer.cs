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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.cmbTimingMethod = new System.Windows.Forms.ComboBox();
            this.lbMapBlacklist = new LiveSplit.SourceSplit.EditableListBox();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lbMapWhitelist = new LiveSplit.SourceSplit.EditableListBox();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabCtrlMaster = new System.Windows.Forms.TabControl();
            this.tabPgAutoStartReset = new System.Windows.Forms.TabPage();
            this.tlpAutoStartEndReset = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.chkAutoSplitEnabled = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.rdoWhitelist = new System.Windows.Forms.RadioButton();
            this.lblMaps = new System.Windows.Forms.Label();
            this.rdoInterval = new System.Windows.Forms.RadioButton();
            this.dmnSplitInterval = new System.Windows.Forms.NumericUpDown();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.gbAutoStartEndReset = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.chkAutoStartEndReset = new System.Windows.Forms.CheckBox();
            this.labStartMap = new System.Windows.Forms.Label();
            this.boxStartMap = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnGameProcessesDefault = new System.Windows.Forms.Button();
            this.lbGameProcesses = new LiveSplit.SourceSplit.EditableListBox();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.gbTiming = new System.Windows.Forms.GroupBox();
            this.tlpTiming = new System.Windows.Forms.TableLayoutPanel();
            this.lblTimingMethod = new System.Windows.Forms.Label();
            this.tabPgMisc = new System.Windows.Forms.TabPage();
            this.gbMapTimes = new System.Windows.Forms.GroupBox();
            this.btnShowMapTimes = new System.Windows.Forms.Button();
            this.gbSecondTimer = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.chkShowGameTime = new System.Windows.Forms.CheckBox();
            this.chkShowAlt = new System.Windows.Forms.CheckBox();
            this.chkShowTickCount = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.lbMapBlacklist)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.lbMapWhitelist)).BeginInit();
            this.tabCtrlMaster.SuspendLayout();
            this.tabPgAutoStartReset.SuspendLayout();
            this.tlpAutoStartEndReset.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dmnSplitInterval)).BeginInit();
            this.groupBox3.SuspendLayout();
            this.gbAutoStartEndReset.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.lbGameProcesses)).BeginInit();
            this.gbTiming.SuspendLayout();
            this.tlpTiming.SuspendLayout();
            this.tabPgMisc.SuspendLayout();
            this.gbMapTimes.SuspendLayout();
            this.gbSecondTimer.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolTip
            // 
            this.toolTip.AutoPopDelay = 5000;
            this.toolTip.InitialDelay = 100;
            this.toolTip.ReshowDelay = 100;
            // 
            // cmbTimingMethod
            // 
            this.cmbTimingMethod.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cmbTimingMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTimingMethod.FormattingEnabled = true;
            this.cmbTimingMethod.Items.AddRange(new object[] {
            "Automatic",
            "Engine Ticks",
            "Engine Ticks with Pauses"});
            this.cmbTimingMethod.Location = new System.Drawing.Point(59, 5);
            this.cmbTimingMethod.Name = "cmbTimingMethod";
            this.cmbTimingMethod.Size = new System.Drawing.Size(149, 21);
            this.cmbTimingMethod.TabIndex = 0;
            this.toolTip.SetToolTip(this.cmbTimingMethod, "Automatic: Choose depending on rules of the game");
            // 
            // lbMapBlacklist
            // 
            this.lbMapBlacklist.AllowUserToResizeRows = false;
            this.lbMapBlacklist.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.lbMapBlacklist.BackgroundColor = System.Drawing.SystemColors.Window;
            this.lbMapBlacklist.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lbMapBlacklist.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.lbMapBlacklist.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.lbMapBlacklist.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.lbMapBlacklist.ColumnHeadersVisible = false;
            this.lbMapBlacklist.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.lbMapBlacklist.DefaultCellStyle = dataGridViewCellStyle2;
            this.lbMapBlacklist.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbMapBlacklist.Location = new System.Drawing.Point(3, 16);
            this.lbMapBlacklist.Name = "lbMapBlacklist";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.lbMapBlacklist.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.lbMapBlacklist.RowHeadersVisible = false;
            this.lbMapBlacklist.RowTemplate.Height = 14;
            this.lbMapBlacklist.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.lbMapBlacklist.Size = new System.Drawing.Size(206, 83);
            this.lbMapBlacklist.TabIndex = 15;
            this.toolTip.SetToolTip(this.lbMapBlacklist, "Don\'t split if the player was on one of these maps before a level change.");
            // 
            // Column1
            // 
            this.Column1.HeaderText = "Column1";
            this.Column1.Name = "Column1";
            // 
            // lbMapWhitelist
            // 
            this.lbMapWhitelist.AllowUserToResizeRows = false;
            this.lbMapWhitelist.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.lbMapWhitelist.BackgroundColor = System.Drawing.SystemColors.Window;
            this.lbMapWhitelist.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lbMapWhitelist.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.lbMapWhitelist.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.lbMapWhitelist.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.lbMapWhitelist.ColumnHeadersVisible = false;
            this.lbMapWhitelist.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn1});
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle5.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.lbMapWhitelist.DefaultCellStyle = dataGridViewCellStyle5;
            this.lbMapWhitelist.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbMapWhitelist.Location = new System.Drawing.Point(3, 16);
            this.lbMapWhitelist.Name = "lbMapWhitelist";
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle6.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle6.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle6.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.lbMapWhitelist.RowHeadersDefaultCellStyle = dataGridViewCellStyle6;
            this.lbMapWhitelist.RowHeadersVisible = false;
            this.lbMapWhitelist.RowTemplate.Height = 14;
            this.lbMapWhitelist.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.lbMapWhitelist.Size = new System.Drawing.Size(205, 83);
            this.lbMapWhitelist.TabIndex = 16;
            this.toolTip.SetToolTip(this.lbMapWhitelist, "Only split if the player was on one of these maps before a level change.");
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.HeaderText = "Column1";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            // 
            // tabCtrlMaster
            // 
            this.tabCtrlMaster.Controls.Add(this.tabPgAutoStartReset);
            this.tabCtrlMaster.Controls.Add(this.tabPgMisc);
            this.tabCtrlMaster.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabCtrlMaster.Location = new System.Drawing.Point(7, 7);
            this.tabCtrlMaster.Name = "tabCtrlMaster";
            this.tabCtrlMaster.SelectedIndex = 0;
            this.tabCtrlMaster.Size = new System.Drawing.Size(461, 526);
            this.tabCtrlMaster.TabIndex = 14;
            // 
            // tabPgAutoStartReset
            // 
            this.tabPgAutoStartReset.Controls.Add(this.tlpAutoStartEndReset);
            this.tabPgAutoStartReset.Location = new System.Drawing.Point(4, 22);
            this.tabPgAutoStartReset.Name = "tabPgAutoStartReset";
            this.tabPgAutoStartReset.Padding = new System.Windows.Forms.Padding(3);
            this.tabPgAutoStartReset.Size = new System.Drawing.Size(453, 500);
            this.tabPgAutoStartReset.TabIndex = 0;
            this.tabPgAutoStartReset.Text = "Main Functions";
            this.tabPgAutoStartReset.UseVisualStyleBackColor = true;
            // 
            // tlpAutoStartEndReset
            // 
            this.tlpAutoStartEndReset.ColumnCount = 2;
            this.tlpAutoStartEndReset.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpAutoStartEndReset.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpAutoStartEndReset.Controls.Add(this.groupBox1, 0, 0);
            this.tlpAutoStartEndReset.Controls.Add(this.gbAutoStartEndReset, 0, 1);
            this.tlpAutoStartEndReset.Controls.Add(this.groupBox2, 1, 1);
            this.tlpAutoStartEndReset.Controls.Add(this.gbTiming, 0, 2);
            this.tlpAutoStartEndReset.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpAutoStartEndReset.Location = new System.Drawing.Point(3, 3);
            this.tlpAutoStartEndReset.Name = "tlpAutoStartEndReset";
            this.tlpAutoStartEndReset.RowCount = 4;
            this.tlpAutoStartEndReset.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 203F));
            this.tlpAutoStartEndReset.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 139F));
            this.tlpAutoStartEndReset.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 56F));
            this.tlpAutoStartEndReset.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 112F));
            this.tlpAutoStartEndReset.Size = new System.Drawing.Size(447, 494);
            this.tlpAutoStartEndReset.TabIndex = 14;
            // 
            // groupBox1
            // 
            this.tlpAutoStartEndReset.SetColumnSpan(this.groupBox1, 2);
            this.groupBox1.Controls.Add(this.tableLayoutPanel3);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(441, 197);
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
            this.tableLayoutPanel3.Size = new System.Drawing.Size(435, 178);
            this.tableLayoutPanel3.TabIndex = 20;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.lbMapBlacklist);
            this.groupBox4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox4.Location = new System.Drawing.Point(220, 33);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(212, 102);
            this.groupBox4.TabIndex = 16;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Map Blacklist";
            // 
            // chkAutoSplitEnabled
            // 
            this.chkAutoSplitEnabled.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.chkAutoSplitEnabled.AutoSize = true;
            this.chkAutoSplitEnabled.Location = new System.Drawing.Point(3, 6);
            this.chkAutoSplitEnabled.Name = "chkAutoSplitEnabled";
            this.chkAutoSplitEnabled.Size = new System.Drawing.Size(211, 17);
            this.chkAutoSplitEnabled.TabIndex = 5;
            this.chkAutoSplitEnabled.Text = "Enabled";
            this.chkAutoSplitEnabled.UseVisualStyleBackColor = true;
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
            this.tableLayoutPanel4.Location = new System.Drawing.Point(220, 3);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 1;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(212, 24);
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
            this.lblMaps.Size = new System.Drawing.Size(36, 13);
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
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox3.Location = new System.Drawing.Point(3, 33);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(211, 102);
            this.groupBox3.TabIndex = 15;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Map Whitelist";
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
            // gbAutoStartEndReset
            // 
            this.gbAutoStartEndReset.Controls.Add(this.tableLayoutPanel2);
            this.gbAutoStartEndReset.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbAutoStartEndReset.Location = new System.Drawing.Point(3, 206);
            this.gbAutoStartEndReset.Name = "gbAutoStartEndReset";
            this.gbAutoStartEndReset.Size = new System.Drawing.Size(217, 133);
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
            this.tableLayoutPanel2.Size = new System.Drawing.Size(211, 114);
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
            this.boxStartMap.Size = new System.Drawing.Size(203, 20);
            this.boxStartMap.TabIndex = 2;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnGameProcessesDefault);
            this.groupBox2.Controls.Add(this.lbGameProcesses);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(226, 206);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(218, 133);
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
            // 
            // lbGameProcesses
            // 
            this.lbGameProcesses.AllowUserToResizeRows = false;
            this.lbGameProcesses.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbGameProcesses.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.lbGameProcesses.BackgroundColor = System.Drawing.SystemColors.Window;
            this.lbGameProcesses.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lbGameProcesses.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle7.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle7.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle7.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle7.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle7.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.lbGameProcesses.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle7;
            this.lbGameProcesses.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.lbGameProcesses.ColumnHeadersVisible = false;
            this.lbGameProcesses.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn2});
            dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle8.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle8.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle8.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle8.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle8.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.lbGameProcesses.DefaultCellStyle = dataGridViewCellStyle8;
            this.lbGameProcesses.Location = new System.Drawing.Point(3, 16);
            this.lbGameProcesses.Name = "lbGameProcesses";
            dataGridViewCellStyle9.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle9.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle9.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle9.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle9.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle9.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.lbGameProcesses.RowHeadersDefaultCellStyle = dataGridViewCellStyle9;
            this.lbGameProcesses.RowHeadersVisible = false;
            this.lbGameProcesses.RowTemplate.Height = 14;
            this.lbGameProcesses.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.lbGameProcesses.Size = new System.Drawing.Size(213, 83);
            this.lbGameProcesses.TabIndex = 17;
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.HeaderText = "Column1";
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            // 
            // gbTiming
            // 
            this.gbTiming.Controls.Add(this.tlpTiming);
            this.gbTiming.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbTiming.Location = new System.Drawing.Point(3, 345);
            this.gbTiming.Name = "gbTiming";
            this.gbTiming.Size = new System.Drawing.Size(217, 50);
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
            this.tlpTiming.RowCount = 1;
            this.tlpTiming.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpTiming.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 31F));
            this.tlpTiming.Size = new System.Drawing.Size(211, 31);
            this.tlpTiming.TabIndex = 1;
            // 
            // lblTimingMethod
            // 
            this.lblTimingMethod.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblTimingMethod.AutoSize = true;
            this.lblTimingMethod.Location = new System.Drawing.Point(10, 9);
            this.lblTimingMethod.Name = "lblTimingMethod";
            this.lblTimingMethod.Size = new System.Drawing.Size(43, 13);
            this.lblTimingMethod.TabIndex = 1;
            this.lblTimingMethod.Text = "Method";
            // 
            // tabPgMisc
            // 
            this.tabPgMisc.Controls.Add(this.gbMapTimes);
            this.tabPgMisc.Controls.Add(this.gbSecondTimer);
            this.tabPgMisc.Location = new System.Drawing.Point(4, 22);
            this.tabPgMisc.Name = "tabPgMisc";
            this.tabPgMisc.Padding = new System.Windows.Forms.Padding(3);
            this.tabPgMisc.Size = new System.Drawing.Size(453, 500);
            this.tabPgMisc.TabIndex = 1;
            this.tabPgMisc.Text = "Miscellaneous";
            this.tabPgMisc.UseVisualStyleBackColor = true;
            // 
            // gbMapTimes
            // 
            this.gbMapTimes.Controls.Add(this.btnShowMapTimes);
            this.gbMapTimes.Location = new System.Drawing.Point(227, 6);
            this.gbMapTimes.Name = "gbMapTimes";
            this.gbMapTimes.Size = new System.Drawing.Size(215, 51);
            this.gbMapTimes.TabIndex = 26;
            this.gbMapTimes.TabStop = false;
            this.gbMapTimes.Text = "Map Times";
            // 
            // btnShowMapTimes
            // 
            this.btnShowMapTimes.Location = new System.Drawing.Point(6, 19);
            this.btnShowMapTimes.Name = "btnShowMapTimes";
            this.btnShowMapTimes.Size = new System.Drawing.Size(203, 24);
            this.btnShowMapTimes.TabIndex = 25;
            this.btnShowMapTimes.Text = "Show Map Times";
            this.btnShowMapTimes.UseVisualStyleBackColor = true;
            this.btnShowMapTimes.Click += new System.EventHandler(this.btnShowMapTimes_Click);
            // 
            // gbSecondTimer
            // 
            this.gbSecondTimer.Controls.Add(this.label2);
            this.gbSecondTimer.Controls.Add(this.chkShowGameTime);
            this.gbSecondTimer.Controls.Add(this.chkShowAlt);
            this.gbSecondTimer.Controls.Add(this.chkShowTickCount);
            this.gbSecondTimer.Location = new System.Drawing.Point(6, 6);
            this.gbSecondTimer.Name = "gbSecondTimer";
            this.gbSecondTimer.Size = new System.Drawing.Size(215, 121);
            this.gbSecondTimer.TabIndex = 24;
            this.gbSecondTimer.TabStop = false;
            this.gbSecondTimer.Text = "Second Timer";
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.label2.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label2.Location = new System.Drawing.Point(6, 86);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(183, 26);
            this.label2.TabIndex = 27;
            this.label2.Text = "SourceSplit must be loaded\r\nin the Layout Edtior for these to work.\r\n";
            // 
            // chkShowGameTime
            // 
            this.chkShowGameTime.AutoSize = true;
            this.chkShowGameTime.Location = new System.Drawing.Point(6, 19);
            this.chkShowGameTime.Name = "chkShowGameTime";
            this.chkShowGameTime.Size = new System.Drawing.Size(159, 17);
            this.chkShowGameTime.TabIndex = 12;
            this.chkShowGameTime.Text = "Show Higher Precision Time";
            this.chkShowGameTime.UseVisualStyleBackColor = true;
            this.chkShowGameTime.CheckedChanged += new System.EventHandler(this.chkShowGameTime_CheckedChanged);
            // 
            // chkShowAlt
            // 
            this.chkShowAlt.AutoSize = true;
            this.chkShowAlt.Location = new System.Drawing.Point(6, 42);
            this.chkShowAlt.Name = "chkShowAlt";
            this.chkShowAlt.Size = new System.Drawing.Size(171, 17);
            this.chkShowAlt.TabIndex = 22;
            this.chkShowAlt.Text = "Show Alternate Timing Method";
            this.chkShowAlt.UseVisualStyleBackColor = true;
            // 
            // chkShowTickCount
            // 
            this.chkShowTickCount.AutoSize = true;
            this.chkShowTickCount.Location = new System.Drawing.Point(6, 65);
            this.chkShowTickCount.Name = "chkShowTickCount";
            this.chkShowTickCount.Size = new System.Drawing.Size(165, 17);
            this.chkShowTickCount.TabIndex = 21;
            this.chkShowTickCount.Text = "Show Game Time Tick Count";
            this.chkShowTickCount.UseVisualStyleBackColor = true;
            // 
            // SourceSplitSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabCtrlMaster);
            this.Name = "SourceSplitSettings";
            this.Padding = new System.Windows.Forms.Padding(7);
            this.Size = new System.Drawing.Size(475, 540);
            ((System.ComponentModel.ISupportInitialize)(this.lbMapBlacklist)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.lbMapWhitelist)).EndInit();
            this.tabCtrlMaster.ResumeLayout(false);
            this.tabPgAutoStartReset.ResumeLayout(false);
            this.tlpAutoStartEndReset.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dmnSplitInterval)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.gbAutoStartEndReset.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.lbGameProcesses)).EndInit();
            this.gbTiming.ResumeLayout(false);
            this.tlpTiming.ResumeLayout(false);
            this.tlpTiming.PerformLayout();
            this.tabPgMisc.ResumeLayout(false);
            this.gbMapTimes.ResumeLayout(false);
            this.gbSecondTimer.ResumeLayout(false);
            this.gbSecondTimer.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.TabControl tabCtrlMaster;
        private System.Windows.Forms.TabPage tabPgAutoStartReset;
        private System.Windows.Forms.TableLayoutPanel tlpAutoStartEndReset;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.GroupBox groupBox4;
        private EditableListBox lbMapBlacklist;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.CheckBox chkAutoSplitEnabled;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.RadioButton rdoWhitelist;
        private System.Windows.Forms.Label lblMaps;
        private System.Windows.Forms.RadioButton rdoInterval;
        private System.Windows.Forms.NumericUpDown dmnSplitInterval;
        private System.Windows.Forms.GroupBox groupBox3;
        private EditableListBox lbMapWhitelist;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox gbAutoStartEndReset;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.CheckBox chkAutoStartEndReset;
        private System.Windows.Forms.Label labStartMap;
        private System.Windows.Forms.TextBox boxStartMap;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnGameProcessesDefault;
        private EditableListBox lbGameProcesses;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.GroupBox gbTiming;
        private System.Windows.Forms.TableLayoutPanel tlpTiming;
        private System.Windows.Forms.ComboBox cmbTimingMethod;
        private System.Windows.Forms.Label lblTimingMethod;
        private System.Windows.Forms.TabPage tabPgMisc;
        private System.Windows.Forms.GroupBox gbMapTimes;
        private System.Windows.Forms.Button btnShowMapTimes;
        private System.Windows.Forms.GroupBox gbSecondTimer;
        private System.Windows.Forms.CheckBox chkShowGameTime;
        private System.Windows.Forms.CheckBox chkShowAlt;
        private System.Windows.Forms.CheckBox chkShowTickCount;
        private System.Windows.Forms.Label label2;
    }
}
