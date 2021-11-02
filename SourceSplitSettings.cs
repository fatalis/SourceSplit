using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;

namespace LiveSplit.SourceSplit
{
    public enum AutoSplitType
    {
        Whitelist,
        Interval
    }

    public enum GameTimingMethodSetting
    {
        Automatic,
        EngineTicks,
        EngineTicksWithPauses,
        //RealTimeWithoutLoads
    }

    public partial class SourceSplitSettings : UserControl
    {
        public bool AutoSplitEnabled { get; set; }
        public int SplitInterval { get; set; }
        public AutoSplitType AutoSplitType { get; private set; }
        public bool ShowGameTime { get; set; }
        public bool AutoStartEndResetEnabled { get; set; }
        public string StartMap { get; set; }

        public string[] MapWhitelist => GetListboxValues(this.lbMapWhitelist);
        public string[] MapBlacklist => GetListboxValues(this.lbMapBlacklist);

        public string[] GameProcesses
        {
            get {
                lock (_lock)
                    return GetListboxValues(this.lbGameProcesses);
            }
        }

        public GameTimingMethodSetting GameTimingMethod
        {
            get
            {
                switch ((string)this.cmbTimingMethod.SelectedItem)
                {
                    case "Engine Ticks":
                        return GameTimingMethodSetting.EngineTicks;
                    case "Engine Ticks with Pauses":
                        return GameTimingMethodSetting.EngineTicksWithPauses;
                    default:
                        return GameTimingMethodSetting.Automatic;
                }
            }
            set
            {
                switch (value)
                {
                    case GameTimingMethodSetting.EngineTicks:
                        this.cmbTimingMethod.SelectedItem = "Engine Ticks";
                        break;
                    case GameTimingMethodSetting.EngineTicksWithPauses:
                        this.cmbTimingMethod.SelectedItem = "Engine Ticks with Pauses";
                        break;
                    default:
                        this.cmbTimingMethod.SelectedItem = "Automatic";
                        break;
                }
            }
        }

        private readonly object _lock = new object();

        private const int DEFAULT_SPLITINTERVAL = 1;
        private const bool DEFAULT_SHOWGAMETIME = true;
        private const bool DEFAULT_AUTOSPLIT_ENABLED = true;
        private const bool DEFAULT_AUTOSTARTENDRESET_ENABLED = true;
        private const AutoSplitType DEFAULT_AUTOSPLITYPE = AutoSplitType.Interval;
        private const GameTimingMethodSetting DEFAULT_GAME_TIMING_METHOD = GameTimingMethodSetting.Automatic;

        public SourceSplitSettings()
        {
            this.InitializeComponent();
            
            this.chkAutoSplitEnabled.DataBindings.Add("Checked", this, nameof(this.AutoSplitEnabled), false, DataSourceUpdateMode.OnPropertyChanged);
            this.dmnSplitInterval.DataBindings.Add("Value", this, nameof(this.SplitInterval), false, DataSourceUpdateMode.OnPropertyChanged);
            this.chkShowGameTime.DataBindings.Add("Checked", this, nameof(this.ShowGameTime), false, DataSourceUpdateMode.OnPropertyChanged);
            this.chkAutoStartEndReset.DataBindings.Add("Checked", this, nameof(this.AutoStartEndResetEnabled), false, DataSourceUpdateMode.OnPropertyChanged);
            this.boxStartMap.DataBindings.Add("Text", this, nameof(this.StartMap), false, DataSourceUpdateMode.OnPropertyChanged);

            this.rdoWhitelist.CheckedChanged += rdoAutoSplitType_CheckedChanged;
            this.rdoInterval.CheckedChanged += rdoAutoSplitType_CheckedChanged;
            this.chkAutoSplitEnabled.CheckedChanged += UpdateDisabledControls;

            // defaults
            lbGameProcessesSetDefault();
            this.SplitInterval = DEFAULT_SPLITINTERVAL;
            this.AutoSplitType = DEFAULT_AUTOSPLITYPE;
            this.ShowGameTime = DEFAULT_SHOWGAMETIME;
            this.AutoSplitEnabled = DEFAULT_AUTOSPLIT_ENABLED;
            this.AutoStartEndResetEnabled = DEFAULT_AUTOSTARTENDRESET_ENABLED;
            this.GameTimingMethod = DEFAULT_GAME_TIMING_METHOD;
            this.StartMap = "";

            this.UpdateDisabledControls(this, EventArgs.Empty);
        }

        private void lbGameProcessesSetDefault()
        {
            this.lbGameProcesses.Rows.Clear();
            this.lbGameProcesses.Rows.Add("hl2.exe");
            this.lbGameProcesses.Rows.Add("portal2.exe");
            this.lbGameProcesses.Rows.Add("dearesther.exe");
            this.lbGameProcesses.Rows.Add("mm.exe");
            this.lbGameProcesses.Rows.Add("EYE.exe");
            this.lbGameProcesses.Rows.Add("bms.exe");
            this.lbGameProcesses.Rows.Add("infra.exe");
            this.lbGameProcesses.Rows.Add("stanley.exe");
            this.lbGameProcesses.Rows.Add("hdtf.exe");
            this.lbGameProcesses.Rows.Add("beginnersguide.exe");
            this.lbGameProcesses.Rows.Add("synergy.exe");
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);

            Version version = Assembly.GetExecutingAssembly().GetName().Version;

            if (this.Parent?.Parent?.Parent != null && this.Parent.Parent.Parent.GetType().ToString() == "LiveSplit.View.ComponentSettingsDialog")
                this.Parent.Parent.Parent.Text = $"SourceSplit v{version.ToString(3)} - Settings";
        }

        public XmlNode GetSettings(XmlDocument doc)
        {
            XmlElement settingsNode = doc.CreateElement("Settings");

            settingsNode.AppendChild(ToElement(doc, "Version", Assembly.GetExecutingAssembly().GetName().Version.ToString(3)));

            settingsNode.AppendChild(ToElement(doc, nameof(this.AutoSplitEnabled), this.AutoSplitEnabled));

            settingsNode.AppendChild(ToElement(doc, nameof(this.SplitInterval), this.SplitInterval));

            string whitelist = String.Join("|", this.MapWhitelist);
            settingsNode.AppendChild(ToElement(doc, nameof(this.MapWhitelist), whitelist));

            string blacklist = String.Join("|", this.MapBlacklist);
            settingsNode.AppendChild(ToElement(doc, nameof(this.MapBlacklist), blacklist));

            string gameProcesses = String.Join("|", this.GameProcesses);
            settingsNode.AppendChild(ToElement(doc, nameof(this.GameProcesses), gameProcesses));

            settingsNode.AppendChild(ToElement(doc, nameof(this.AutoSplitType), this.AutoSplitType));

            settingsNode.AppendChild(ToElement(doc, nameof(this.ShowGameTime), this.ShowGameTime));

            settingsNode.AppendChild(ToElement(doc, nameof(this.GameTimingMethod), this.GameTimingMethod));

            settingsNode.AppendChild(ToElement(doc, nameof(this.AutoStartEndResetEnabled), this.AutoStartEndResetEnabled));

            settingsNode.AppendChild(ToElement(doc, nameof(this.StartMap), this.StartMap));

            return settingsNode;
        }

        private T GetSetting<T>(XmlNode settings, T setting, string name, T def)
        {
            var converter = TypeDescriptor.GetConverter(typeof(T));
            if (converter == null)
                converter = TypeDescriptor.GetConverter(typeof(Enum));
            string settingEntry = settings[name]?.InnerText ?? null;
            try
            {
                return settingEntry == null ? def : (T)converter.ConvertFromString(settingEntry);
            }
            catch (NotSupportedException)
            {
                return def;
            }
        }

        public void SetSettings(XmlNode settings)
        {
            AutoSplitEnabled = GetSetting(settings, AutoSplitEnabled, nameof(AutoSplitEnabled), DEFAULT_AUTOSPLIT_ENABLED);
            AutoStartEndResetEnabled = GetSetting(settings, AutoStartEndResetEnabled, nameof(AutoStartEndResetEnabled), DEFAULT_AUTOSTARTENDRESET_ENABLED);
            SplitInterval = GetSetting(settings, SplitInterval, nameof(SplitInterval), DEFAULT_SPLITINTERVAL);
            ShowGameTime = GetSetting(settings, ShowGameTime, nameof(ShowGameTime), DEFAULT_SHOWGAMETIME);
            GameTimingMethod = GetSetting(settings, GameTimingMethod, nameof(GameTimingMethod), DEFAULT_GAME_TIMING_METHOD);
            AutoSplitType = GetSetting(settings, AutoSplitType, nameof(AutoSplitType), DEFAULT_AUTOSPLITYPE);
            StartMap = GetSetting(settings, StartMap, nameof(StartMap), "");

            this.rdoInterval.Checked = this.AutoSplitType == AutoSplitType.Interval;
            this.rdoWhitelist.Checked = this.AutoSplitType == AutoSplitType.Whitelist;

            this.lbMapWhitelist.Rows.Clear();
            string whitelist = settings[nameof(this.MapWhitelist)]?.InnerText ?? String.Empty;
            foreach (string map in whitelist.Split('|'))
                this.lbMapWhitelist.Rows.Add(map);

            this.lbMapBlacklist.Rows.Clear();
            string blacklist = settings[nameof(this.MapBlacklist)]?.InnerText ?? String.Empty;
            foreach (string map in blacklist.Split('|'))
                this.lbMapBlacklist.Rows.Add(map);

            if (settings[nameof(this.GameProcesses)] != null)
                lock (_lock)
                {
                    string gameProcesses = settings[nameof(this.GameProcesses)]?.InnerText ?? String.Empty;
                    string[] processes = gameProcesses.Split('|');

                    if (new List<string>(processes).All(x => string.IsNullOrWhiteSpace(x)))
                        MessageBox.Show("Saved Game Process list is empty!\n" +
                            "Please fill your game's process name for the spliter to function!", 
                            "SourceSplit Warning", 
                            MessageBoxButtons.OK, 
                            MessageBoxIcon.Warning);

                    this.lbGameProcesses.Rows.Clear();
                    foreach (string process in processes)
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

        private void btnGameProcessesDefault_Click(object sender, EventArgs e)
        {
            lbGameProcessesSetDefault();
        }
    }
}
