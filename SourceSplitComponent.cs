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

        public bool Disposed { get; private set; }
        public bool IsLayoutComponent { get; private set; }

        private TimerModel _timer;
        private LiveSplitState _state;
        private GraphicsCache _cache;

        private GameMemory _gameMemory;

        private float _sessionTime;
        private float _totalMapTime;
        private float _totalTime;
        private float _sessionTimeOffset;

        private bool _waitingForDelay;

        private string _currentMap = String.Empty;
        private int _splitCount;
        private List<string> _mapsVisited;

        private TimeSpan GameTime
        {
            get { return TimeSpan.FromSeconds(_totalTime + _sessionTime - _sessionTimeOffset); }
        }

        public SourceSplitComponent(LiveSplitState state, bool isLayoutComponent)
        {
            // make Debug.WriteLine prepend update count and tick count
            while (Debug.Listeners.Count > 0)
                Debug.Listeners.RemoveAt(0);
            Debug.Listeners.Add(TimedTraceListener.Instance);
            while (Trace.Listeners.Count > 0)
                Trace.Listeners.RemoveAt(0);
            Trace.Listeners.Add(TimedTraceListener.Instance); // is it okay to use the same instance?
            //Debug.AutoFlush = Trace.AutoFlush = true;

            _state = state;
            this.IsLayoutComponent = isLayoutComponent;

            this.Settings = new SourceSplitSettings();
            this.InternalComponent = new InfoTimeComponent("Game Time", null, new RegularTimeFormatter(TimeAccuracy.Hundredths));
            
            this.ContextMenuControls = new Dictionary<String, Action>();
            this.ContextMenuControls.Add("SourceSplit: Map Times", () => MapTimesForm.Instance.Show());

            _cache = new GraphicsCache();

            _timer = new TimerModel { CurrentState = state };
            state.OnSplit += state_OnSplit;
            state.OnReset += state_OnReset;
            state.OnStart += state_OnStart;

            _mapsVisited = new List<string>();

            _gameMemory = new GameMemory(this.Settings);
            _gameMemory.OnSessionTimeUpdate += gameMemory_OnSessionTimeUpdate;
            _gameMemory.OnPlayerGainedControl += gameMemory_OnPlayerGainedControl;
            _gameMemory.OnPlayerLostControl += gameMemory_OnPlayerLostControl;
            _gameMemory.OnMapChanged += gameMemory_OnMapChanged;
            _gameMemory.OnSessionStarted += gameMemory_OnSessionStarted;
            _gameMemory.OnSessionEnded += gameMemory_OnSessionEnded;
            _gameMemory.OnNewGameStarted += gameMemory_OnNewGameStarted;
            _gameMemory.StartReading();
        }

#if DEBUG
        ~SourceSplitComponent()
        {
            Debug.WriteLine("SourceSplitComponent finalizer");
        }
#endif

        public void Dispose()
        {
            this.Disposed = true;

            _state.OnSplit -= state_OnSplit;
            _state.OnReset -= state_OnReset;
            _state.OnStart -= state_OnStart;

            _state.IsGameTimePaused = false; // hack
            _state.LoadingTimes = TimeSpan.Zero;

            if (_gameMemory != null)
                _gameMemory.Stop();
        }

        public void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {
            // hack to prevent flicker, doesn't actually pause anything
            state.IsGameTimePaused = true; 

            // Update is called every 25ms, so up to 25ms IGT can be lost if using delay and no auto-start
            if (_waitingForDelay)
            {
                if (state.CurrentTime.RealTime >= TimeSpan.Zero)
                {
                    _sessionTimeOffset = _sessionTime;
                    _waitingForDelay = false;
                }
                else
                {
                    state.SetGameTime(state.CurrentTime.RealTime);
                }
            }

            if (!_waitingForDelay)
                // update game time, don't show negative time due to tick adjusting
                state.SetGameTime(this.GameTime >= TimeSpan.Zero ? this.GameTime : TimeSpan.Zero);

            if (!this.Settings.ShowGameTime)
                return;

            this.InternalComponent.TimeValue =
                state.CurrentTime[state.CurrentTimingMethod == TimingMethod.GameTime
                    ? TimingMethod.RealTime : TimingMethod.GameTime];
            this.InternalComponent.InformationName = state.CurrentTimingMethod == TimingMethod.GameTime
                ? "Real Time" : "Game Time";

            _cache.Restart();
            _cache["TimeValue"] = this.InternalComponent.ValueLabel.Text;
            _cache["TimingMethod"] = state.CurrentTimingMethod;
            if (invalidator != null && _cache.HasChanged)
                invalidator.Invalidate(0f, 0f, width, height);
        }

        public void DrawVertical(Graphics g, LiveSplitState state, float width, Region region)
        {
            this.PrepareDraw(state);
            this.InternalComponent.DrawVertical(g, state, width, region);
        }

        public void DrawHorizontal(Graphics g, LiveSplitState state, float height, Region region)
        {
            this.PrepareDraw(state);
            this.InternalComponent.DrawHorizontal(g, state, height, region);
        }

        void PrepareDraw(LiveSplitState state)
        {
            this.InternalComponent.NameLabel.ForeColor = state.LayoutSettings.TextColor;
            this.InternalComponent.ValueLabel.ForeColor = state.LayoutSettings.TextColor;
            this.InternalComponent.NameLabel.HasShadow = this.InternalComponent.ValueLabel.HasShadow = state.LayoutSettings.DropShadows;
        }

        void state_OnStart(object sender, EventArgs e)
        {
            _totalTime = 0;
            _mapsVisited.Clear();
            MapTimesForm.Instance.Reset();
            _splitCount = 0;
            _totalMapTime = 0;

            if (_state.PauseTime >= TimeSpan.Zero)
                _sessionTimeOffset = _sessionTime - (float)_state.PauseTime.TotalSeconds;
            else
                _waitingForDelay = true;
        }

        void state_OnReset(object sender, TimerPhase t)
        {
            MapTimesForm.Instance.Reset();
            _waitingForDelay = false;
        }

        void state_OnSplit(object sender, EventArgs e)
        {
            Debug.WriteLine("split at time " + this.GameTime);

            if (_state.CurrentPhase == TimerPhase.Ended)
            {
                this.AddMapTime(_currentMap, TimeSpan.FromSeconds(_totalMapTime + _sessionTime - _sessionTimeOffset));
                this.AddMapTime("-Total-", this.GameTime);
            }
        }

        // first tick when player is fully in game
        void gameMemory_OnSessionStarted(object sender, SessionStartedEventArgs e)
        {
            _currentMap = e.Map;
        }

        // called when player is fully in game
        void gameMemory_OnSessionTimeUpdate(object sender, SessionTimeUpdateEventArgs e)
        {
            _sessionTime = e.SessionTime;
        }

        // player is no longer fully in the game
        void gameMemory_OnSessionEnded(object sender, EventArgs e)
        {
            Debug.WriteLine("session ended, total time was " + (_sessionTime - _sessionTimeOffset));

            // add up total time and reset session time
            _totalTime += _sessionTime - _sessionTimeOffset;
            _totalMapTime += _sessionTime - _sessionTimeOffset;
            _sessionTime = 0;
            _sessionTimeOffset = 0;
        }

        // called immediately after OnSessionEnded if it was a changelevel
        void gameMemory_OnMapChanged(object sender, MapChangedEventArgs e)
        {
            Debug.WriteLine("gameMemory_OnMapChanged " + e.Map + " " + e.PrevMap);

            // this is in case they load a save that was made before current map
            // fuck time travel
            if (!_mapsVisited.Contains(e.PrevMap))
            {
                _mapsVisited.Add(e.PrevMap);
                this.AutoSplit(e.PrevMap);
            }

            // prevent adding map time twice
            if (_state.CurrentPhase != TimerPhase.Ended && _state.CurrentPhase != TimerPhase.NotRunning)
                this.AddMapTime(e.PrevMap, TimeSpan.FromSeconds(_totalMapTime));
            _totalMapTime = 0;
        }

        void gameMemory_OnPlayerGainedControl(object sender, PlayerControlChangedEventArgs e)
        {
            if (!this.Settings.AutoStartEndResetEnabled)
                return;

            _timer.Reset(); // make sure to reset for games that start from a quicksave (Aperture Tag)
            _timer.Start();
            _sessionTimeOffset += e.TimeOffset;
        }

        void gameMemory_OnPlayerLostControl(object sender, PlayerControlChangedEventArgs e)
        {
            if (!this.Settings.AutoStartEndResetEnabled)
                return;
            
            _sessionTimeOffset += e.TimeOffset;
            this.DoSplit();
        }

        void gameMemory_OnNewGameStarted(object sender, EventArgs e)
        {
            if (!this.Settings.AutoStartEndResetEnabled)
                return;

            _timer.Reset();
        }

        void AutoSplit(string map)
        {
            if (!this.Settings.AutoSplitEnabled)
                return;

            Debug.WriteLine("AutoSplit " + map);

            map = map.ToLower();

            string[] blacklist = this.Settings.MapBlacklist.Select(x => x.ToLower()).ToArray();

            if (this.Settings.AutoSplitType == AutoSplitType.Whitelist)
            {
                string[] whitelist = this.Settings.MapWhitelist.Select(x => x.ToLower()).ToArray();

                if (whitelist.Length > 0)
                {
                    if (whitelist.Contains(map))
                        this.DoSplit();
                }
                else if (!blacklist.Contains(map))
                {
                    this.DoSplit();
                }
            }
            else if (this.Settings.AutoSplitType == AutoSplitType.Interval)
            {
                if (!blacklist.Contains(map) && ++_splitCount >= this.Settings.SplitInterval)
                {
                    _splitCount = 0;
                    this.DoSplit();
                }
            }
        }

        void DoSplit()
        {
            // make split times accurate
            _state.SetGameTime(this.GameTime);

            bool before = _state.Settings.DoubleTapPrevention;
            _state.Settings.DoubleTapPrevention = false;
            _timer.Split();
            _state.Settings.DoubleTapPrevention = before;
        }

        // TODO: asterisk for manual start and splits
        void AddMapTime(string map, TimeSpan time)
        {
            string timeStr = time.ToString(time >= TimeSpan.FromHours(1) ? @"hh\:mm\:ss\.fff" : @"mm\:ss\.fff");
            MapTimesForm.Instance.AddMapTime(map, timeStr);
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
        public float MinimumWidth    { get { return this.InternalComponent.MinimumWidth; } }
        public float MinimumHeight   { get { return this.InternalComponent.MinimumHeight; } }
        public float VerticalHeight  { get { return this.Settings.ShowGameTime ? this.InternalComponent.VerticalHeight : 0; } }
        public float HorizontalWidth { get { return this.Settings.ShowGameTime ? this.InternalComponent.HorizontalWidth : 0; } }
        public float PaddingLeft     { get { return this.Settings.ShowGameTime ? this.InternalComponent.PaddingLeft : 0; } }
        public float PaddingRight    { get { return this.Settings.ShowGameTime ? this.InternalComponent.PaddingRight : 0; } }
        public float PaddingTop      { get { return this.Settings.ShowGameTime ? this.InternalComponent.PaddingTop : 0; } }
        public float PaddingBottom   { get { return this.Settings.ShowGameTime ? this.InternalComponent.PaddingBottom : 0; } }
    }
}
