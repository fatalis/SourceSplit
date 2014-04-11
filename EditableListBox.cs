using System;
using System.Windows.Forms;
using System.Drawing;

namespace LiveSplit.SourceSplit
{
    /// <summary>
    /// A DataGridView that emulates the look of a ListBox and can be edited.
    /// </summary>
    class EditableListBox : DataGridView
    {
        private ContextMenu menuRemove;

        public EditableListBox()
        {
            this.menuRemove = new ContextMenu();
            var delete = new MenuItem("Remove Selected");
            delete.Click += delete_Click;
            this.menuRemove.MenuItems.Add(delete);

            this.AllowUserToResizeRows = false;
            this.RowHeadersVisible = false;
            this.ColumnHeadersVisible = false;
            this.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            this.CellBorderStyle = DataGridViewCellBorderStyle.None;
            this.BorderStyle = BorderStyle.Fixed3D;
            this.BackgroundColor = SystemColors.Window;

            this.RowTemplate.Height = base.Font.Height + 1;
        }

        void delete_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in this.SelectedRows)
            {
                if (!row.IsNewRow)
                    this.Rows.Remove(row);
            }
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);

            if (!this.Enabled)
                this.CurrentCell = null;
            this.DefaultCellStyle.BackColor = this.Enabled ? SystemColors.Window : SystemColors.Control;
            this.DefaultCellStyle.ForeColor = this.Enabled ? SystemColors.ControlText : SystemColors.GrayText;
            this.BackgroundColor = this.Enabled ? SystemColors.Window : SystemColors.Control;
        }

        protected override void OnCellMouseUp(DataGridViewCellMouseEventArgs e)
        {
            base.OnCellMouseUp(e);

            if (e.Button == MouseButtons.Right)
                this.menuRemove.Show(this, e.Location);
        }
    }
}
