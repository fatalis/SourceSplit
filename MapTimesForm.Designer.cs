namespace LiveSplit.SourceSplit
{
    partial class MapTimesForm
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
            this.lvMapTimes = new System.Windows.Forms.ListView();
            this.chMap = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btnCopy = new System.Windows.Forms.Button();
            this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // lvMapTimes
            // 
            this.lvMapTimes.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chMap,
            this.chTime});
            this.lvMapTimes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvMapTimes.FullRowSelect = true;
            this.lvMapTimes.Location = new System.Drawing.Point(3, 3);
            this.lvMapTimes.Name = "lvMapTimes";
            this.lvMapTimes.Size = new System.Drawing.Size(268, 257);
            this.lvMapTimes.TabIndex = 0;
            this.lvMapTimes.UseCompatibleStateImageBehavior = false;
            this.lvMapTimes.View = System.Windows.Forms.View.Details;
            // 
            // chMap
            // 
            this.chMap.Text = "Map";
            this.chMap.Width = 155;
            // 
            // chTime
            // 
            this.chTime.Text = "Time";
            this.chTime.Width = 84;
            // 
            // btnCopy
            // 
            this.btnCopy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCopy.Location = new System.Drawing.Point(196, 266);
            this.btnCopy.Name = "btnCopy";
            this.btnCopy.Size = new System.Drawing.Size(75, 23);
            this.btnCopy.TabIndex = 1;
            this.btnCopy.Text = "Copy";
            this.btnCopy.UseVisualStyleBackColor = true;
            this.btnCopy.Click += new System.EventHandler(this.btnCopy_Click);
            // 
            // tableLayoutPanel
            // 
            this.tableLayoutPanel.ColumnCount = 1;
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel.Controls.Add(this.btnCopy, 0, 1);
            this.tableLayoutPanel.Controls.Add(this.lvMapTimes, 0, 0);
            this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel.Name = "tableLayoutPanel";
            this.tableLayoutPanel.RowCount = 2;
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel.Size = new System.Drawing.Size(274, 292);
            this.tableLayoutPanel.TabIndex = 2;
            // 
            // MapTimesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(274, 292);
            this.Controls.Add(this.tableLayoutPanel);
            this.Name = "MapTimesForm";
            this.ShowIcon = false;
            this.Text = "SourceSplit: Map Times";
            this.tableLayoutPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView lvMapTimes;
        private System.Windows.Forms.ColumnHeader chMap;
        private System.Windows.Forms.ColumnHeader chTime;
        private System.Windows.Forms.Button btnCopy;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
    }
}