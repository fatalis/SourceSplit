using System.Diagnostics;
using LiveSplit.Model;
using LiveSplit.TimeFormatters;
using LiveSplit.UI.Components;
using LiveSplit.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Xml;
using System.Windows.Forms;

namespace LiveSplit.SourceSplit
{
    class SourceSplitComponent : IComponent
    {
        public string ComponentName
        {
            get { return "SourceSplit"; }
        }

        public SourceSplitSettings Settings { get; set; }
        public IDictionary<string, Action> ContextMenuControls { get; protected set; }
        protected InfoTimeComponent InternalComponent { get; set; }

        private SourceSplitTimerModel _model;
        private LiveSplitState _state;
        private GameMemory _gameMemory;
        private List<string> _mapsVisited;

        private readonly object _lock = new object();
        private float _mapTime;
        private float _quickLoadTime;
        private float _totalTime;
        private float _removeTime;
        private int _splitCount;
        private TimeSpan? _endTime;
        private GraphicsCache _cache;
        private bool _waitingForDelay;

        struct MapTime
        {
            public string Map { get; set; }
            public TimeSpan Time { get; set; }
        }
        private List<MapTime> _mapTimes;
        private MapTimesForm _mapTimesForm;

        private TimeSpan GameTime
        {
            get {
                lock (_lock)
                    return TimeSpan.FromSeconds(_totalTime + _mapTime - _removeTime);
            }
        }

        public SourceSplitComponent(LiveSplitState state)
        {
            this.Settings = new SourceSplitSettings();
            this.InternalComponent = new InfoTimeComponent(null, null, new RegularTimeFormatter(TimeAccuracy.Hundredths));
            
            this.ContextMenuControls = new Dictionary<String, Action>();
            this.ContextMenuControls.Add("SourceSplit: Map Times", () => {
                if (_mapTimesForm.Visible)
                    _mapTimesForm.Hide();
                else
                    _mapTimesForm.Show();
            });

            _cache = new GraphicsCache();

            _model = new SourceSplitTimerModel { CurrentState = state };
            state.RegisterTimerModel(_model);
            state.OnSplit += state_OnSplit;
            state.OnReset += state_OnReset;
            state.OnStart += state_OnStart;
            _state = state;

            _mapTimes = new List<MapTime>();
            _mapsVisited = new List<string>();
            _mapTimesForm = new MapTimesForm();

            _gameMemory = new GameMemory(this.Settings);
            _gameMemory.OnSignOnStateChange += gameMemory_OnSignOnStateChange;
            _gameMemory.OnGameTimeUpdate += gameMemory_OnGameTimeUpdate;
            _gameMemory.StartReading();
        }

        ~SourceSplitComponent()
        {
            // TODO: in LiveSplit 1.4, components will be IDisposable
            //_gameMemory.Stop();
        }

        public void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {
            if (!this.Settings.ShowGameTime)
                return;

            if (state.CurrentPhase == TimerPhase.Ended)
            {
                this.InternalComponent.TimeValue = _endTime.HasValue ? _endTime.Value : TimeSpan.Zero;
            }
            else
            {
                if (state.CurrentTime >= TimeSpan.Zero)
                {
                    // Update is called every 25ms, so up to 25ms can be lost if using delay
                    if (_waitingForDelay)
                    {
                        lock (_lock)
                            _removeTime = _mapTime;
                        _waitingForDelay = false;
                    }

                    this.InternalComponent.TimeValue = state.CurrentPhase == TimerPhase.Running
                        || state.CurrentPhase == TimerPhase.Paused
                        ? this.GameTime : TimeSpan.Zero;
                }
            }

            _cache.Restart();
            _cache["TimeValue"] = this.InternalComponent.ValueLabel.Text;
            if (invalidator != null && _cache.HasChanged)
                invalidator.Invalidate(0f, 0f, width, height);
        }

        public void DrawVertical(Graphics g, LiveSplitState state, float width, Region region)
        {
            this.InternalComponent.NameLabel.Text = "Game Time";
            this.InternalComponent.NameLabel.ForeColor = state.LayoutSettings.TextColor;
            this.InternalComponent.ValueLabel.ForeColor = state.LayoutSettings.TextColor;
            this.InternalComponent.DrawVertical(g, state, width, region);
        }

        public void DrawHorizontal(Graphics g, LiveSplitState state, float height, Region region)
        {
            this.InternalComponent.NameLabel.Text = "Game Time";
            this.InternalComponent.NameLabel.ForeColor = state.LayoutSettings.TextColor;
            this.InternalComponent.ValueLabel.ForeColor = state.LayoutSettings.TextColor;
            this.InternalComponent.DrawHorizontal(g, state, height, region);
        }

        void state_OnStart(object sender, EventArgs e)
        {
            _totalTime = 0;
            _mapsVisited.Clear();
            _mapTimes.Clear();
            _mapTimesForm.Reset();
            _splitCount = 0;
            _quickLoadTime = 0;

            _waitingForDelay = _state.PauseTime < TimeSpan.Zero;

            if (_state.PauseTime > TimeSpan.Zero)
            {
                // what are they doing?
            }

            if (!_waitingForDelay)
            {
                lock (_lock)
                    _removeTime = _mapTime;
            }
        }

        void state_OnReset(object sender, EventArgs e)
        {
            _endTime = null;
            _mapTimes.Clear();
            _mapTimesForm.Reset();
            _waitingForDelay = false;
        }

        void state_OnSplit(object sender, EventArgs e)
        {
            if (_state.CurrentPhase == TimerPhase.Ended)
            {
                _endTime = this.GameTime;
                this.AddMapTime(new MapTime { Map = _gameMemory.CurrentMap, Time = TimeSpan.FromSeconds(_mapTime + _quickLoadTime) }, true);
                this.AddMapTime(new MapTime { Map = "-Total-", Time = this.GameTime }, true);
            }
        }

        // called from thread
        void gameMemory_OnGameTimeUpdate(object sender, float gameTime)
        {
            lock (_lock)
                _mapTime = gameTime;
        }

        void gameMemory_OnSignOnStateChange(object sender, SignOnStateChangeEventArgs e)
        {
            if (_state.CurrentPhase != TimerPhase.Running || _waitingForDelay)
                return;

            // changelevel command
            if (e.SignOnState == SignOnState.Connected && e.PrevSignOnState == SignOnState.Full)
            {
                string map = _gameMemory.CurrentMap;
                // note: e.GameTime is the final map time before starting the next level
                this.AddMapTime(new MapTime { Map = map, Time = TimeSpan.FromSeconds(e.GameTime + _quickLoadTime) });
                _totalTime += e.GameTime;
                _mapTime = 0;
                _quickLoadTime = 0;
                Debug.WriteLine("changelevel time add: " + e.GameTime);

                if (!_mapsVisited.Contains(map))
                {
                    _mapsVisited.Add(map);
                    this.AutoSplit(map);
                }
            }
            // new game or quick save loaded
            else if (e.SignOnState == SignOnState.None)
            {
                Debug.WriteLine("newgame or quick load time add: " + e.GameTime);
                _totalTime += e.GameTime;
                _quickLoadTime += e.GameTime;
                _mapTime = 0;
            }
        }

        void AutoSplit(string map)
        {
            if (!this.Settings.AutoSplitEnabled)
                return;

            map = map.ToLower();

            string[] blacklist = this.Settings.MapBlacklist.Select(x => x.ToLower()).ToArray();

            if (this.Settings.AutoSplitType == AutoSplitType.Whitelist)
            {
                string[] whitelist = this.Settings.MapWhitelist.Select(x => x.ToLower()).ToArray();

                if (whitelist.Length > 0)
                {
                    if (whitelist.Contains(map))
                        _model.Split();
                }
                else if (!blacklist.Contains(map))
                {
                    _model.Split();
                }
            }
            else if (this.Settings.AutoSplitType == AutoSplitType.Interval)
            {
                if (!blacklist.Contains(map) && ++_splitCount >= this.Settings.SplitInterval)
                {
                    _splitCount = 0;
                    _model.Split();
                }
            }
        }

        void AddMapTime(MapTime time, bool end=false)
        {
            string timeStr = time.Time.ToString(time.Time >= TimeSpan.FromHours(1) ? @"hh\:mm\:ss\.fff" : @"mm\:ss\.fff");
            if (_mapTimes.Count == 0 || end)
                timeStr += " *";
            _mapTimesForm.AddMapTime(time.Map, timeStr);

            _mapTimes.Add(time);
        }

        public XmlNode GetSettings(XmlDocument document)
        {
            return this.Settings.GetSettings(document);
        }

        public Control GetSettingsControl(LayoutMode mode)
        {
            return this.Settings;
        }

        public void SetSettings(XmlNode settings)
        {
            this.Settings.SetSettings(settings);
        }

        public void RenameComparison(string oldName, string newName) { }

        public float VerticalHeight     { get { return this.Settings.ShowGameTime ? this.InternalComponent.VerticalHeight : 0; } }
        public float HorizontalWidth    { get { return this.Settings.ShowGameTime ? this.InternalComponent.HorizontalWidth : 0; } }
        public float MinimumWidth       { get { return this.InternalComponent.MinimumWidth; } }
        public float MinimumHeight      { get { return this.InternalComponent.MinimumHeight; } }
        public float PaddingLeft        { get { return this.InternalComponent.PaddingLeft; } }
        public float PaddingRight       { get { return this.InternalComponent.PaddingRight; } }
        public float PaddingTop         { get { return this.InternalComponent.PaddingTop; } }
        public float PaddingBottom      { get { return this.InternalComponent.PaddingBottom; } }
    }

    class SourceSplitTimerModel : ITimerModel
    {
        public LiveSplitState CurrentState { get; set; }

        public void Split()
        {
            if (this.CurrentState.CurrentPhase != TimerPhase.Running)
                return;

            TimeSpan? currentTime = this.CurrentState.CurrentTime;
            if ((currentTime.HasValue && currentTime.GetValueOrDefault() > TimeSpan.Zero))
            {
                this.CurrentState.CurrentSplit.SplitTime = currentTime;
                this.CurrentState.CurrentSplitIndex++;
                if (this.CurrentState.Run.Count == this.CurrentState.CurrentSplitIndex)
                    this.CurrentState.CurrentPhase = TimerPhase.Ended;
                this.CurrentState.Run.HasChanged = true;
                if (this.OnSplit != null)
                    this.OnSplit(this, null);
            }
        }
        
        public void Pause()                     { throw new NotImplementedException(); }
        public void Start()                     { throw new NotImplementedException(); }
        public void SwitchComparisonNext()      { throw new NotImplementedException(); }
        public void SwitchComparisonPrevious()  { throw new NotImplementedException(); }
        public void UndoSplit()                 { throw new NotImplementedException(); }
        public void Reset()                     { throw new NotImplementedException(); }
        public void ResetWithoutUpdating()      { throw new NotImplementedException(); }
        public void ScrollDown()                { throw new NotImplementedException(); }
        public void ScrollUp()                  { throw new NotImplementedException(); }
        public void SkipSplit()                 { throw new NotImplementedException(); }

        public event EventHandler OnPause;
        public event EventHandler OnReset;
        public event EventHandler OnResume;
        public event EventHandler OnScrollDown;
        public event EventHandler OnScrollUp;
        public event EventHandler OnSkipSplit;
        public event EventHandler OnSplit;
        public event EventHandler OnStart;
        public event EventHandler OnSwitchComparisonNext;
        public event EventHandler OnSwitchComparisonPrevious;
        public event EventHandler OnUndoSplit;
    }
}
