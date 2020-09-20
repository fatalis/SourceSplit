using System;
using System.Diagnostics;

namespace LiveSplit.SourceSplit.GameSpecific
{
    abstract class GameSupport
    {
        public string FirstMap { get; protected set; }
        public string LastMap { get; protected set; }
        public string FirstMap2 { get; internal set; }
        public bool StartOnFirstMapLoad { get; internal set; } = false;

        // ticks to subtract
        public int StartOffsetTicks { get; protected set; }
        public int EndOffsetTicks { get; protected set; }

        public GameTimingMethod GameTimingMethod { get; protected set; } = GameTimingMethod.EngineTicks;

        // which player properties should be updated
        private PlayerProperties _requiredProperties;
        public PlayerProperties RequiredProperties
        {
            get
            {
#if DEBUG
                // so DebugPlayerState() works
                return PlayerProperties.ALL;
#else
                return _requiredProperties;
#endif
            }
            set { _requiredProperties = value; }
        }

        // what kind of generic auto-start detection to use
        // must call base.OnUpdate
        private AutoStart _autoStartType;
        protected AutoStart AutoStartType
        {
            get { return _autoStartType; }
            set
            {
                if (value == AutoStart.Unfrozen)
                    this.RequiredProperties |= PlayerProperties.Flags;
                else if (value == AutoStart.ViewEntityChanged)
                    this.RequiredProperties |= PlayerProperties.ViewEntity;
                else if (value == AutoStart.ParentEntityChanged)
                    this.RequiredProperties |= PlayerProperties.ParentEntity;
                _autoStartType = value;
            }
        }

        protected bool IsFirstMap { get; private set; }
        protected bool IsFirstMap2 { get; private set; }
        protected bool IsLastMap { get; private set; }


        private bool _onceFlag;

        // called when attached to a new game process
        public virtual void OnGameAttached(GameState state) { }

        public virtual void OnTimerReset(bool resetFlagTo) { }

        // called on the first tick when player is fully in the game (according to demos)
        public virtual void OnSessionStart(GameState state)
        {
            _onceFlag = false;

            this.IsFirstMap = state.CurrentMap == this.FirstMap;
            this.IsFirstMap2 = state.CurrentMap == this.FirstMap2;
            this.IsLastMap = (!this.IsFirstMap || !this.IsFirstMap2) && state.CurrentMap == this.LastMap;
        }

        // called when player no longer fully in the game (map changed, load started)
        public virtual void OnSessionEnd(GameState state) { }

        // called once per tick when player is fully in the game
        public virtual GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this.AutoStartType == AutoStart.Unfrozen
                && !state.PlayerFlags.HasFlag(FL.FROZEN)
                && state.PrevPlayerFlags.HasFlag(FL.FROZEN))
            {
                Debug.WriteLine("FL_FROZEN removed from player");
                _onceFlag = true;
                return GameSupportResult.PlayerGainedControl;
            }
            else if (this.AutoStartType == AutoStart.ViewEntityChanged
                && state.PrevPlayerViewEntityIndex != GameState.ENT_INDEX_PLAYER
                && state.PlayerViewEntityIndex == GameState.ENT_INDEX_PLAYER)
            {
                Debug.WriteLine("view entity changed to player");
                _onceFlag = true;
                return GameSupportResult.PlayerGainedControl;
            }
            else if (this.AutoStartType == AutoStart.ParentEntityChanged
                && state.PrevPlayerParentEntityHandle != -1
                && state.PlayerParentEntityHandle == -1)
            {
                Debug.WriteLine("player no longer parented");
                _onceFlag = true;
                return GameSupportResult.PlayerGainedControl;
            }

            return GameSupportResult.DoNothing;
        }

        public static GameSupport FromGameDir(string gameDir)
        {
            switch (gameDir.ToLower())
            {
                case "hl2oe":
                case "hl2":
                case "ghosting":
                case "ghostingmod":
                case "ghostingmod2": //hl2 category extensions, NOTE: these are only guesses for the folder name
                case "ghostingmod3":
                case "ghostingmod4":
                case "cutsceneless":
                    return new HL2();
                case "episodic":
                    return new HL2Ep1();
                case "ep2":
                    return new HL2Ep2();
                case "portal":
                case "portalelevators":
                    return new Portal();
                case "portal_tfv":
                    return new PortalTFV();
                case "portal2":
                    return new Portal2();
                case "aperturetag":
                    return new ApertureTag();
                case "portal_stories":
                    return new PortalStoriesMel();
                case "bms":
                    return new BMSRetail();
                case "lostcoast":
                    return new LostCoast();
                case "estrangedact1":
                    return new EstrangedAct1();
                case "ptsd":
                    return new HL2Mods_Ptsd1();
                case "missionimprobable":
                    return new HL2Mods_MImp();
                case "downfall":
                    return new HL2Mods_Downfall();
                case "uncertaintyprinciple":
                    return new HL2Mods_UncertaintyPrinciple();
                case "watchingpaintdry":
                case "watchingpaintdry2":
                    return new HL2Mods_WatchingPaintDry();
                case "mod_episodic":
                    return new HL2Mods_SnipersEp();
                case "deepdown":
                    return new HL2Mods_DeepDown();
                case "dank_memes":
                    return new HL2Mods_DankMemes();
                case "freakman":
                    return new HL2Mods_Freakman1();
                case "freakman-kleinerlife":
                    return new HL2Mods_Freakman2();
                case "crates":
                    return new HL2Mods_TooManyCrates();
                case "te120":
                    return new TE120();
                case "dear esther":
                    return new HL2Mods_DearEsther();
                case "exit 2":
                    return new HL2Mods_Exit2();
                case "dayhard":
                    return new HL2Mods_DayHard();
                case "thestanleyparable":
                    return new TheStanleyParable();
                case "hdtf":
                    return new HDTF();
                case "beginnersguide":
                    return new TheBeginnersGuide();
                case "icemod":
                    return new HL2Mods_ICE();
                case "dababy":
                    return new HL2Mods_DaBaby();
                case "infra":
                    return new Infra();
                case "yearlongalarm":
                    return new HL2Mods_YearLongAlarm();
                case "killthemonk":
                    return new HL2Mods_KillTheMonk();
                case "logistique":
                    return new HL2Mods_Logistique();
            }

            return null;
        }

        protected enum AutoStart
        {
            None,
            Unfrozen,
            ViewEntityChanged,
            ParentEntityChanged
        }
    }

    enum GameSupportResult
    {
        DoNothing,
        PlayerGainedControl,
        PlayerLostControl,
        ManualSplit
    }

    [Flags]
    enum PlayerProperties
    {
        Flags = (1 << 0),
        Position = (1 << 1),
        ViewEntity = (1 << 2),
        ParentEntity = (1 << 3),
        ALL = Flags | Position | ViewEntity | ParentEntity
    }
}
