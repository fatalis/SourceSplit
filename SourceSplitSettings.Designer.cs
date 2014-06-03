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
            this.chkAutoSplitEnabled = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.rdoWhitelist = new System.Windows.Forms.RadioButton();
            this.lblMaps = new System.Windows.Forms.Label();
            this.rdoInterval = new System.Windows.Forms.RadioButton();
            this.dmnSplitInterval = new System.Windows.Forms.NumericUpDown();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.lbMapBlacklist = new LiveSplit.SourceSplit.EditableListBox();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lbMapWhitelist = new LiveSplit.SourceSplit.EditableListBox();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lbGameProcesses = new LiveSplit.SourceSplit.EditableListBox();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.chkShowGameTime = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dmnSplitInterval)).BeginInit();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.lbMapBlacklist)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.lbMapWhitelist)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.lbGameProcesses)).BeginInit();
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
            this.tableLayoutPanel1.SetColumnSpan(this.groupBox1, 2);
            this.groupBox1.Controls.Add(this.tableLayoutPanel3);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(450, 157);
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
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 16);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 2;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(444, 138);
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
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox3.Location = new System.Drawing.Point(3, 33);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(216, 102);
            this.groupBox3.TabIndex = 15;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Map Whitelist";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.lbGameProcesses);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(3, 166);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(222, 105);
            this.groupBox2.TabIndex = 11;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Game Processes";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.groupBox2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.chkShowGameTime, 1, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(7, 7);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 163F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 111F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 37F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(456, 435);
            this.tableLayoutPanel1.TabIndex = 13;
            // 
            // chkShowGameTime
            // 
            this.chkShowGameTime.AutoSize = true;
            this.chkShowGameTime.Location = new System.Drawing.Point(231, 166);
            this.chkShowGameTime.Name = "chkShowGameTime";
            this.chkShowGameTime.Size = new System.Drawing.Size(110, 17);
            this.chkShowGameTime.TabIndex = 12;
            this.chkShowGameTime.Text = "Show Game Time";
            this.chkShowGameTime.UseVisualStyleBackColor = true;
            // 
            // lbMapBlacklist
            // 
            this.lbMapBlacklist.AllowUserToResizeRows = false;
            this.lbMapBlacklist.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.lbMapBlacklist.BackgroundColor = System.Drawing.SystemColors.Window;
            this.lbMapBlacklist.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lbMapBlacklist.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.lbMapBlacklist.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.lbMapBlacklist.ColumnHeadersVisible = false;
            this.lbMapBlacklist.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1});
            this.lbMapBlacklist.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbMapBlacklist.Location = new System.Drawing.Point(3, 16);
            this.lbMapBlacklist.Name = "lbMapBlacklist";
            this.lbMapBlacklist.RowHeadersVisible = false;
            this.lbMapBlacklist.RowTemplate.Height = 14;
            this.lbMapBlacklist.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.lbMapBlacklist.Size = new System.Drawing.Size(210, 83);
            this.lbMapBlacklist.TabIndex = 15;
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
            this.lbMapWhitelist.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.lbMapWhitelist.ColumnHeadersVisible = false;
            this.lbMapWhitelist.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn1});
            this.lbMapWhitelist.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbMapWhitelist.Location = new System.Drawing.Point(3, 16);
            this.lbMapWhitelist.Name = "lbMapWhitelist";
            this.lbMapWhitelist.RowHeadersVisible = false;
            this.lbMapWhitelist.RowTemplate.Height = 14;
            this.lbMapWhitelist.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.lbMapWhitelist.Size = new System.Drawing.Size(210, 83);
            this.lbMapWhitelist.TabIndex = 16;
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.HeaderText = "Column1";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            // 
            // lbGameProcesses
            // 
            this.lbGameProcesses.AllowUserToResizeRows = false;
            this.lbGameProcesses.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.lbGameProcesses.BackgroundColor = System.Drawing.SystemColors.Window;
            this.lbGameProcesses.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lbGameProcesses.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.lbGameProcesses.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.lbGameProcesses.ColumnHeadersVisible = false;
            this.lbGameProcesses.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn2});
            this.lbGameProcesses.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbGameProcesses.Location = new System.Drawing.Point(3, 16);
            this.lbGameProcesses.Name = "lbGameProcesses";
            this.lbGameProcesses.RowHeadersVisible = false;
            this.lbGameProcesses.RowTemplate.Height = 14;
            this.lbGameProcesses.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.lbGameProcesses.Size = new System.Drawing.Size(216, 86);
            this.lbGameProcesses.TabIndex = 17;
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.HeaderText = "Column1";
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            // 
            // SourceSplitSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "SourceSplitSettings";
            this.Padding = new System.Windows.Forms.Padding(7);
            this.Size = new System.Drawing.Size(470, 449);
            this.groupBox1.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dmnSplitInterval)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.lbMapBlacklist)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.lbMapWhitelist)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.lbGameProcesses)).EndInit();
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
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.GroupBox groupBox3;
        private EditableListBox lbGameProcesses;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.CheckBox chkShowGameTime;
    }
}
