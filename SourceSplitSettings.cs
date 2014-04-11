using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;

namespace LiveSplit.SourceSplit
{
    public enum AutoSplitType
    {
        Whitelist,
        Interval
    };

    public partial class SourceSplitSettings : UserControl
    {
        public bool AutoSplitEnabled { get; set; }
        public int SplitInterval { get; set; }
        public AutoSplitType AutoSplitType { get; private set; }

        private readonly object _lock = new object();

        public string[] MapWhitelist
        {
            get { return GetListboxValues(this.lbMapWhitelist); }
        }

        public string[] MapBlacklist
        {
            get { return GetListboxValues(this.lbMapBlacklist); }
        }

        public string[] GameProcesses
        {
            get {
                lock (_lock)
                    return GetListboxValues(this.lbGameProcesses);
            }
        }

        public SourceSplitSettings()
        {
            this.InitializeComponent();
            
            this.chkAutoSplitEnabled.DataBindings.Add("Checked", this, "AutoSplitEnabled", false, DataSourceUpdateMode.OnPropertyChanged);
            this.dmnSplitInterval.DataBindings.Add("Value", this, "SplitInterval", false, DataSourceUpdateMode.OnPropertyChanged);

            this.rdoWhitelist.CheckedChanged += rdoAutoSplitType_CheckedChanged;
            this.rdoInterval.CheckedChanged += rdoAutoSplitType_CheckedChanged;
            this.chkAutoSplitEnabled.CheckedChanged += UpdateDisabledControls;

            // defaults
            this.lbGameProcesses.Rows.Add("hl2.exe");
            this.lbGameProcesses.Rows.Add("portal2.exe");
            this.lbGameProcesses.Rows.Add("dearesther.exe");
            this.SplitInterval = 1;
            this.AutoSplitType = AutoSplitType.Interval;

            this.UpdateDisabledControls(this, EventArgs.Empty);
        }

        // TODO: In LiveSplit 1.3 this is called on a background thread. It'll be fixed in 1.4.
        public XmlNode GetSettings(XmlDocument doc)
        {
            XmlElement settingsNode = doc.CreateElement("Settings");

            settingsNode.AppendChild(ToElement(doc, "Version", Assembly.GetExecutingAssembly().GetName().Version.ToString(3)));

            settingsNode.AppendChild(ToElement(doc, "AutoSplitEnabled", this.AutoSplitEnabled));
            settingsNode.AppendChild(ToElement(doc, "SplitInterval", this.SplitInterval));

            string whitelist = String.Join("|", this.MapWhitelist);
            settingsNode.AppendChild(ToElement(doc, "MapWhitelist", whitelist));

            string blacklist = String.Join("|", this.MapBlacklist);
            settingsNode.AppendChild(ToElement(doc, "MapBlacklist", blacklist));

            string gameProcesses = String.Join("|", this.GameProcesses);
            settingsNode.AppendChild(ToElement(doc, "GameProcesses", gameProcesses));

            settingsNode.AppendChild(ToElement(doc, "AutoSplitType", this.AutoSplitType));

            return settingsNode;
        }

        public void SetSettings(XmlNode settings)
        {
            bool bval;
            int ival;

            this.AutoSplitEnabled = settings["AutoSplitEnabled"] != null ?
                (Boolean.TryParse(settings["AutoSplitEnabled"].InnerText, out bval) ? bval : false)
                : false;

            this.SplitInterval = settings["SplitInterval"] != null ?
                (Int32.TryParse(settings["SplitInterval"].InnerText, out ival) ? ival : 1)
                : 1;

            AutoSplitType splitType;
            this.AutoSplitType = settings["AutoSplitType"] != null ?
                (Enum.TryParse(settings["AutoSplitType"].InnerText, out splitType) ? splitType : AutoSplitType.Interval)
                : AutoSplitType.Interval;
            this.rdoInterval.Checked = this.AutoSplitType == AutoSplitType.Interval;
            this.rdoWhitelist.Checked = this.AutoSplitType == AutoSplitType.Whitelist;

            this.lbMapWhitelist.Rows.Clear();
            string whitelist = settings["MapWhitelist"] != null ? settings["MapWhitelist"].InnerText : String.Empty;
            foreach (string map in whitelist.Split('|'))
                this.lbMapWhitelist.Rows.Add(map);

            this.lbMapBlacklist.Rows.Clear();
            string blacklist = settings["MapBlacklist"] != null ? settings["MapBlacklist"].InnerText : String.Empty;
            foreach (string map in blacklist.Split('|'))
                this.lbMapBlacklist.Rows.Add(map);

            lock (_lock)
            {
                this.lbGameProcesses.Rows.Clear();
                string gameProcesses = settings["GameProcesses"] != null ? settings["GameProcesses"].InnerText : String.Empty;
                foreach (string process in gameProcesses.Split('|'))
                    this.lbGameProcesses.Rows.Add(process);
            }
        }

        void rdoAutoSplitType_CheckedChanged(object sender, EventArgs e)
        {
            this.AutoSplitType = this.rdoInterval.Checked ?
                AutoSplitType.Interval :
                AutoSplitType.Whitelist;

            this.UpdateDisabledControls(sender, e);
        }

        void UpdateDisabledControls(object sender, EventArgs e)
        {
            this.rdoInterval.Enabled = this.rdoWhitelist.Enabled = this.dmnSplitInterval.Enabled =
                this.lbMapBlacklist.Enabled = this.lbMapWhitelist.Enabled =
                this.lblMaps.Enabled = this.chkAutoSplitEnabled.Checked;

            this.lbMapWhitelist.Enabled =
                (this.AutoSplitType == AutoSplitType.Whitelist && chkAutoSplitEnabled.Checked);
            this.lbMapBlacklist.Enabled = 
                (this.AutoSplitType == AutoSplitType.Interval && chkAutoSplitEnabled.Checked);
        }

        static XmlElement ToElement<T>(XmlDocument document, string name, T value)
        {
            XmlElement str = document.CreateElement(name);
            str.InnerText = value.ToString();
            return str;
        }

        static string[] GetListboxValues(EditableListBox lb)
        {
            var ret = new List<string>();
            foreach (DataGridViewRow row in lb.Rows)
            {
                if (row.IsNewRow || (lb.CurrentRow == row && lb.IsCurrentRowDirty))
                    continue;

                string value = (string)row.Cells[0].Value;
                value = value.Trim().Replace("|", String.Empty);
                if (value.Length > 0)
                    ret.Add(value);
            }
            return ret.ToArray();
        }
    }
}
