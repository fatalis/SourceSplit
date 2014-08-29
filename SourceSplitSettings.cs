using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public bool ShowGameTime { get; set; }
        public bool AutoStartEndResetEnabled { get; set; }

        private readonly object _lock = new object();

        private const int DEFAULT_SPLITINTERVAL = 1;
        private const bool DEFAULT_SHOWGAMETIME = true;
        private const bool DEFAULT_AUTOSPLIT_ENABLED = true;
        private const bool DEFAULT_AUTOSTARTENDRESET_ENABLED = true;
        private const AutoSplitType DEFAULT_AUTOSPLITYPE = AutoSplitType.Interval;

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
            this.chkShowGameTime.DataBindings.Add("Checked", this, "ShowGameTime", false, DataSourceUpdateMode.OnPropertyChanged);
            this.chkAutoStartEndReset.DataBindings.Add("Checked", this, "AutoStartEndResetEnabled", false, DataSourceUpdateMode.OnPropertyChanged);

            this.rdoWhitelist.CheckedChanged += rdoAutoSplitType_CheckedChanged;
            this.rdoInterval.CheckedChanged += rdoAutoSplitType_CheckedChanged;
            this.chkAutoSplitEnabled.CheckedChanged += UpdateDisabledControls;

            // defaults
            this.lbGameProcesses.Rows.Add("hl2.exe");
            this.lbGameProcesses.Rows.Add("portal2.exe");
            this.lbGameProcesses.Rows.Add("dearesther.exe");
            this.lbGameProcesses.Rows.Add("mm.exe");
            this.lbGameProcesses.Rows.Add("EYE.exe");
            this.SplitInterval = DEFAULT_SPLITINTERVAL;
            this.AutoSplitType = DEFAULT_AUTOSPLITYPE;
            this.ShowGameTime = DEFAULT_SHOWGAMETIME;
            this.AutoSplitEnabled = DEFAULT_AUTOSPLIT_ENABLED;
            this.AutoStartEndResetEnabled = DEFAULT_AUTOSTARTENDRESET_ENABLED;

            this.UpdateDisabledControls(this, EventArgs.Empty);
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);

            Version version = Assembly.GetExecutingAssembly().GetName().Version;

            if (this.Parent != null && this.Parent.Parent != null && this.Parent.Parent.Parent != null
                && this.Parent.Parent.Parent.GetType().ToString() == "LiveSplit.View.ComponentSettingsDialog")
                this.Parent.Parent.Parent.Text = String.Format("SourceSplit v{0} - Settings", version.ToString(3));
        }

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

            settingsNode.AppendChild(ToElement(doc, "ShowGameTime", this.ShowGameTime));

            settingsNode.AppendChild(ToElement(doc, "AutoStartEndResetEnabled", this.AutoStartEndResetEnabled));

            return settingsNode;
        }

        public void SetSettings(XmlNode settings)
        {
            bool bval;
            int ival;

            this.AutoSplitEnabled = settings["AutoSplitEnabled"] != null ?
                (Boolean.TryParse(settings["AutoSplitEnabled"].InnerText, out bval) ? bval : DEFAULT_AUTOSPLIT_ENABLED)
                : false;

            this.AutoStartEndResetEnabled = settings["AutoStartEndResetEnabled"] != null ?
                (Boolean.TryParse(settings["AutoStartEndResetEnabled"].InnerText, out bval) ? bval : DEFAULT_AUTOSTARTENDRESET_ENABLED)
                : false;

            this.SplitInterval = settings["SplitInterval"] != null ?
                (Int32.TryParse(settings["SplitInterval"].InnerText, out ival) ? ival : DEFAULT_SPLITINTERVAL)
                : 1;

            this.ShowGameTime = settings["ShowGameTime"] != null ?
                (Boolean.TryParse(settings["ShowGameTime"].InnerText, out bval) ? bval : DEFAULT_SHOWGAMETIME)
                : true;

            AutoSplitType splitType;
            this.AutoSplitType = settings["AutoSplitType"] != null ?
                (Enum.TryParse(settings["AutoSplitType"].InnerText, out splitType) ? splitType : DEFAULT_AUTOSPLITYPE)
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
                if (value == null)
                    continue;

                value = value.Trim().Replace("|", String.Empty);
                if (value.Length > 0)
                    ret.Add(value);
            }
            return ret.ToArray();
        }

        void btnShowMapTimes_Click(object sender, EventArgs e)
        {
            MapTimesForm.Instance.Show();
        }

        void btnOpenDocs_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("https://github.com/fatalis/sourcesplit/blob/master/README.md");
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
            }
        }
    }
}
