using System.Diagnostics;
using LiveSplit.Model;
using LiveSplit.Options;
using LiveSplit.TimeFormatters;
using LiveSplit.UI.Components;
using LiveSplit.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Xml;
using System.Windows.Forms;
using LiveSplit.SourceSplit.GameSpecific;

namespace LiveSplit.SourceSplit
{
    class SourceSplitComponent : IComponent
    {
        public string ComponentName => "SourceSplit";

        public SourceSplitSettings Settings { get; set; }
        public IDictionary<string, Action> ContextMenuControls { get; protected set; }
        protected InfoTimeComponent InternalComponent { get; set; }

        public bool Disposed { get; private set; }
        public bool IsLayoutComponent { get; private set; }

        private TimerModel _timer;
        private GraphicsCache _cache;

        private GameMemory _gameMemory;

        private static bool DoSpecialSplit = false;
        private static bool ss_init_time = false;
        private static bool ss_init_measure_pauses = false;
        private TimeSpan ss_time_to_split;
        private static int pauseamount;
        private static bool gamepaused = false;
        public static float ss_delta = 0.1f;
        private static int _tickoffset;

        private float _intervalPerTick;
        private int _sessionTicks;
        private int _totalMapTicks;
        private int _totalTicks;
        private int _sessionTicksOffset;
        private DateTime? _gamePauseTime;
        private int _gamePauseTick;
        private GameTimingMethod _gameRecommendedTimingMethod;


        private bool _waitingForDelay;

        private string _currentMap = String.Empty;
        private int _splitCount;
        private List<string> _mapsVisited;

        private GameTimingMethod GameTimingMethod
        {
            get
            {
                switch (this.Settings.GameTimingMethod)
                {
                    case GameTimingMethodSetting.EngineTicks:
                        return GameTimingMethod.EngineTicks;
                    case GameTimingMethodSetting.EngineTicksWithPauses:
                        return GameTimingMethod.EngineTicksWithPauses;
                    default:
                        return _gameRecommendedTimingMethod;
                }
            }
        }

        private TimeSpan GameTime
        {
            get
            {
                if (_gamePauseTime != null && this.GameTimingMethod == GameTimingMethod.EngineTicksWithPauses)
                {
                    return TimeSpan.FromSeconds((_totalTicks + _gamePauseTick + FakeTicks(_gamePauseTime.Value, DateTime.Now) - _sessionTicksOffset) * _intervalPerTick);
                }
                else
                {
                    return TimeSpan.FromSeconds((_totalTicks + _sessionTicks - _sessionTicksOffset) * _intervalPerTick);
                }
            }
        }

        public SourceSplitComponent(LiveSplitState state, bool isLayoutComponent)
        {
#if DEBUG
            // make Debug.WriteLine prepend update count and tick count
            Debug.Listeners.Clear();
            Debug.Listeners.Add(TimedTraceListener.Instance);
            Trace.Listeners.Clear();
            Trace.Listeners.Add(TimedTraceListener.Instance);
#endif

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

            _intervalPerTick = 0.015f; // will update these when attached to game
            _gameRecommendedTimingMethod = GameTimingMethod.EngineTicks;

            _gameMemory = new GameMemory(this.Settings);
            _gameMemory.OnSetTickRate += gameMemory_OnSetTickRate;
            _gameMemory.OnSetTimingMethod += gameMemory_OnSetTimingMethod;
            _gameMemory.OnSessionTimeUpdate += gameMemory_OnSessionTimeUpdate;
            _gameMemory.OnPlayerGainedControl += gameMemory_OnPlayerGainedControl;
            _gameMemory.OnPlayerLostControl += gameMemory_OnPlayerLostControl;
            _gameMemory.ManualSplit += gameMemory_ManualSplit;
            _gameMemory.OnMapChanged += gameMemory_OnMapChanged;
            _gameMemory.OnSessionStarted += gameMemory_OnSessionStarted;
            _gameMemory.OnSessionEnded += gameMemory_OnSessionEnded;
            _gameMemory.OnNewGameStarted += gameMemory_OnNewGameStarted;
            _gameMemory.OnGamePaused += gameMemory_OnGamePaused;
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

            _timer.CurrentState.OnSplit -= state_OnSplit;
            _timer.CurrentState.OnReset -= state_OnReset;
            _timer.CurrentState.OnStart -= state_OnStart;

            _timer.CurrentState.IsGameTimePaused = false; // hack
            _timer.CurrentState.LoadingTimes = TimeSpan.Zero;

            _gameMemory?.Stop();
        }



        public void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {
            // hack to prevent flicker, doesn't actually pause anything`
            state.IsGameTimePaused = true;

            // Update is called every 25ms, so up to 25ms IGT can be lost if using delay and no auto-start
            if (_waitingForDelay)
            {
                if (state.CurrentTime.RealTime >= TimeSpan.Zero)
                {
                    _sessionTicksOffset = _sessionTicks;
                    _waitingForDelay = false;
                }
                else
                {
                    state.SetGameTime(state.CurrentTime.RealTime);
                }
            }

            if (!_waitingForDelay)
            {
                if (TheStanleyParable._stanley)
                {
                    // for bug detection only since tsp support isn't final yet
                    state.SetGameTime(this.GameTime);
                }
                else
                    // update game time, don't show negative time due to tick adjusting
                    state.SetGameTime(this.GameTime >= TimeSpan.Zero ? this.GameTime : TimeSpan.Zero);
            }

            if (DoSpecialSplit)
            {
                SplitAfterSpecifiedTime(this.GameTime);
            }

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
            _timer.InitializeGameTime();
            _totalTicks = 0;
            _mapsVisited.Clear();
            MapTimesForm.Instance.Reset();
            _splitCount = 0;
            _totalMapTicks = 0;
            _gamePauseTime = null;

            // something weird happens in the case of tsp, when the player resets the timer and starts it manually, then
            // start the map normally then move their view then state_onreset gets fired non stop
            // so this is a hack to prevent the game from autostarting the run if the timer has already started
            if (TheStanleyParable._stanley)
                GameMemory.GameSupportOutBoundCalls.GameSupport.OnTimerReset(true);


            // hack to make sure Portal players aren't using manual offset. we handle offset automatically now.
            // remove this eventually
            if (_timer.CurrentState.TimePausedAt.Seconds == 53 && _timer.CurrentState.TimePausedAt.Milliseconds == 10)
            {
                _timer.CurrentState.TimePausedAt = TimeSpan.Zero;
                _timer.CurrentState.Run.Offset = TimeSpan.Zero;
            }

            if (_timer.CurrentState.TimePausedAt >= TimeSpan.Zero)
                _sessionTicksOffset = _sessionTicks - (int)(_timer.CurrentState.TimePausedAt.TotalSeconds / _intervalPerTick);
            else
                _waitingForDelay = true;
        }

        void state_OnReset(object sender, TimerPhase t)
        {
            MapTimesForm.Instance.Reset();
            _waitingForDelay = false;

            // some game has unspecific starts like if the player's position isn't something which
            // can be repeated easily by accident, so this is a _onceflag but reset on timer reset.
            if (GameMemory.GameSupportOutBoundCalls != null)
                GameMemory.GameSupportOutBoundCalls.GameSupport.OnTimerReset(false);
        }

        void state_OnSplit(object sender, EventArgs e)
        {
            Debug.WriteLine("split at time " + this.GameTime);

            if (_timer.CurrentState.CurrentPhase == TimerPhase.Ended)
            {
                this.AddMapTime(_currentMap, TimeSpan.FromSeconds(_totalMapTicks + _sessionTicks - _sessionTicksOffset));
                this.AddMapTime("-Total-", this.GameTime);
            }
        }

        // first tick when player is fully in game
        void gameMemory_OnSessionStarted(object sender, SessionStartedEventArgs e)
        {
            _currentMap = e.Map;
            gamepaused = false;
        }

        void gameMemory_OnSetTickRate(object sender, SetTickRateEventArgs e)
        {
            Debug.WriteLine("tickrate " + e.IntervalPerTick);
            _intervalPerTick = e.IntervalPerTick;
        }

        void gameMemory_OnSetTimingMethod(object sender, SetTimingMethodEventArgs e)
        {
            _gameRecommendedTimingMethod = e.GameTimingMethod;
        }

        // called when player is fully in game
        void gameMemory_OnSessionTimeUpdate(object sender, SessionTicksUpdateEventArgs e)
        {
            _sessionTicks = e.SessionTicks;
        }

        // player is no longer fully in the game
        void gameMemory_OnSessionEnded(object sender, EventArgs e)
        {
            Debug.WriteLine("session ended, total time was " + TimeSpan.FromSeconds((_sessionTicks - _sessionTicksOffset) * _intervalPerTick));

            if (_gamePauseTime != null && this.GameTimingMethod == GameTimingMethod.EngineTicksWithPauses)
            {
                _sessionTicksOffset -= FakeTicks(_gamePauseTime.Value, DateTime.Now);
                _gamePauseTime = null;
            }

            // add up total time and reset session time
            _totalTicks += _sessionTicks - _sessionTicksOffset;
            _totalMapTicks += _sessionTicks - _sessionTicksOffset;
            _sessionTicks = 0;
            _sessionTicksOffset = 0;
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
            if (_timer.CurrentState.CurrentPhase != TimerPhase.Ended && _timer.CurrentState.CurrentPhase != TimerPhase.NotRunning)
                this.AddMapTime(e.PrevMap, TimeSpan.FromSeconds(_totalMapTicks * _intervalPerTick));
            _totalMapTicks = 0;
        }

        void gameMemory_OnPlayerGainedControl(object sender, PlayerControlChangedEventArgs e)
        {
            if (!this.Settings.AutoStartEndResetEnabled)
                return;
            _timer.Reset(); // make sure to reset for games that start from a quicksave (Aperture Tag)
            _timer.Start();
            _sessionTicksOffset += e.TicksOffset;
            _tickoffset = e.TicksOffset;
        }

        void gameMemory_OnPlayerLostControl(object sender, PlayerControlChangedEventArgs e)
        {
            if (!this.Settings.AutoStartEndResetEnabled)
                return;

            _sessionTicksOffset += e.TicksOffset;
            this.DoSplit();
        }

        void gameMemory_ManualSplit(object sender, PlayerControlChangedEventArgs e)
        {
            if (!this.Settings.AutoStartEndResetEnabled)
                return;

            _tickoffset = e.TicksOffset;

            Debug.WriteLine("** time adjusted, " + _tickoffset + " ticks were added to time");
            this.DoSplitandRevertOffset();
        }

        void gameMemory_OnNewGameStarted(object sender, EventArgs e)
        {
            if (!this.Settings.AutoStartEndResetEnabled)
                return;

            _timer.Reset();
        }

        void gameMemory_OnGamePaused(object sender, GamePausedEventArgs e)
        {
            if (this.GameTimingMethod != GameTimingMethod.EngineTicksWithPauses)
                return;

            if (e.Paused)
            {
                _gamePauseTime = DateTime.Now;
                _gamePauseTick = _sessionTicks;
                gamepaused = true;
            }
            else
            {
                gamepaused = false;
                if (_gamePauseTime != null)
                {
                    Debug.WriteLine("pause done, adding  " + TimeSpan.FromSeconds((DateTime.Now - _gamePauseTime.Value).TotalSeconds));
                    _sessionTicksOffset -= FakeTicks(_gamePauseTime.Value, DateTime.Now);
                    pauseamount = FakeTicks(_gamePauseTime.Value, DateTime.Now);
                    ss_init_measure_pauses = true;
                }
                _gamePauseTime = null;
            }
        }

        int FakeTicks(DateTime start, DateTime end)
        {
            return (int)((end - start).TotalSeconds / _intervalPerTick);
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
            _timer.CurrentState.SetGameTime(this.GameTime);

            HotkeyProfile profile = _timer.CurrentState.Settings.HotkeyProfiles[_timer.CurrentState.CurrentHotkeyProfile];
            bool before = profile.DoubleTapPrevention;
            profile.DoubleTapPrevention = false;
            _timer.Split();
            profile.DoubleTapPrevention = before;
        }

        public static void ResetSpecialSplit()
        {
            DoSpecialSplit = false;
            ss_init_time = true;
            ss_init_measure_pauses = false;
            pauseamount = 0;
            gamepaused = false;
            Debug.WriteLine("special split reset");
        }

        // for negative values of endoffsetticks, we are now predicting the split end some time in the future.
        // static endoffsetticks values only work if we assume the player never pauses during that period of time
        // between sourcesplit splitting and the actual ending

        // this function will account for pause time so the split is accurate
        private void SplitAfterSpecifiedTime(TimeSpan time)
        {
            // first set initial target time
            if (ss_init_time)
            {
                ss_time_to_split = time - TimeSpan.FromSeconds(_tickoffset * _intervalPerTick);
                ss_init_time = false;
            }
            // then measure pauses and add it to target time
            else if (pauseamount > 0 && ss_init_measure_pauses)
            {
                ss_time_to_split = TimeSpan.FromSeconds(ss_time_to_split.TotalSeconds + (pauseamount * _intervalPerTick));
                ss_init_measure_pauses = false;
            }

            // due to update() 25ms imprecision we'll have to settle with detecting if the time is within 0.1s of target time
            double delta = Math.Abs(this.GameTime.TotalSeconds - ss_time_to_split.TotalSeconds);
            if (!gamepaused && (delta <= 0.1))
            {
                _timer.Split();
                Debug.WriteLine("special split done, delta was " + delta + " on split");
                _timer.CurrentState.SetGameTime(this.GameTime);
                ResetSpecialSplit();
            }
            //Debug.WriteLine(_tickoffset);
        }

        // what is this?
        // for the stanley parable the precision of splits needs to be near-perfect so some endings must have an end offset
        // however because endoffsetticks was only meant to be used at the end of a run, that means
        // when using it mid-run the timer will go back into the past to split then never get bumped forward again,
        // losing a few ticks

        void DoSplitandRevertOffset()
        {
            // make split times accurate
            _timer.CurrentState.SetGameTime(this.GameTime);

            HotkeyProfile profile = _timer.CurrentState.Settings.HotkeyProfiles[_timer.CurrentState.CurrentHotkeyProfile];
            bool before = profile.DoubleTapPrevention;
            profile.DoubleTapPrevention = false;
            if (_tickoffset < 0)
            {
                Debug.WriteLine("using special split");
                pauseamount = 0;
                DoSpecialSplit = true;
                ss_init_time = true;
            }
            else if (_tickoffset > 0)
            {
                _timer.CurrentState.SetGameTime(this.GameTime - TimeSpan.FromSeconds(_tickoffset * _intervalPerTick));
                _timer.Split();
                _timer.CurrentState.SetGameTime(this.GameTime);
            }
            profile.DoubleTapPrevention = before;
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

        public float MinimumWidth => this.InternalComponent.MinimumWidth;
        public float MinimumHeight => this.InternalComponent.MinimumHeight;
        public float VerticalHeight => this.Settings.ShowGameTime ? this.InternalComponent.VerticalHeight : 0;
        public float HorizontalWidth => this.Settings.ShowGameTime ? this.InternalComponent.HorizontalWidth : 0;
        public float PaddingLeft => this.Settings.ShowGameTime ? this.InternalComponent.PaddingLeft : 0;
        public float PaddingRight => this.Settings.ShowGameTime ? this.InternalComponent.PaddingRight : 0;
        public float PaddingTop => this.Settings.ShowGameTime ? this.InternalComponent.PaddingTop : 0;
        public float PaddingBottom => this.Settings.ShowGameTime ? this.InternalComponent.PaddingBottom : 0;
    }
}
