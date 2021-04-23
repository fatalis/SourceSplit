using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace LiveSplit.SourceSplit.GameSpecific
{
    abstract class GameSupport
    {
        /// <summary>
        /// The first map of the mod / game
        /// </summary>
        public string FirstMap { get; protected set; }
        /// <summary>
        /// The last map of the mod / game
        /// </summary>
        public string LastMap { get; protected set; }
        /// <summary>
        /// The alternative / secondary first map of the mod / game
        /// </summary>
        public string FirstMap2 { get; internal set; }
        /// <summary>
        /// The list of maps on a new session of which the timer should auto-start
        /// </summary>
        public List<string> StartOnFirstLoadMaps { get; internal set; } = new List<string>();
        // action upon the next session end
        /// <summary>
        /// Timer behavior on the next session end (game disconnect / map change)
        /// </summary>
        public GameSupportResult QueueOnNextSessionEnd { get; set; } = GameSupportResult.DoNothing;
        /// <summary>
        /// The list of mods that run off the base game
        /// </summary>
        public List<GameSupport> NonStandaloneMods { get; internal set; } = new List<GameSupport>();

        // ticks to subtract
        /// <summary>
        /// Tick offset when starting the timer
        /// </summary>
        public int StartOffsetTicks { get; protected set; }
        /// <summary>
        /// Tick offset when ending the timer
        /// </summary>
        public int EndOffsetTicks { get; protected set; }

        /// <summary>
        /// The mod / game's timing method
        /// </summary>
        public GameTimingMethod GameTimingMethod { get; protected set; } = GameTimingMethod.EngineTicks;

        // which player properties should be updated
        private PlayerProperties _requiredProperties;
        /// <summary>
        /// Required player properties for this mod / game. NOTE: Compiling in debug mode will require all possible
        /// </summary>
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
        /// <summary>
        /// Generic Auto-start method.
        /// </summary>
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
        /// <summary>
        /// Actions to do when the game process is found and game-specific code is initialized
        /// </summary>
        /// <param name="name">The name of the entity</param>
        public virtual void OnGameAttached(GameState state) 
        {
            if (NonStandaloneMods.Any())
            {
                foreach (GameSupport mod in NonStandaloneMods)
                    mod.OnGameAttached(state);
            }
        }

        // called when the timer is reset
        /// <summary>
        /// Actions to do when the timer is manually reset
        /// </summary>
        /// <param name="resetFlagTo">Value of corresponding reset flag</param>
        public virtual void OnTimerReset(bool resetFlagTo) 
        {
            if (NonStandaloneMods.Any())
            {
                foreach (GameSupport mod in NonStandaloneMods)
                    mod.OnTimerReset(resetFlagTo);
            }
        }

        // called on the first tick when player is fully in the game (according to demos)
        /// <summary>
        /// Actions to do when a new session starts and the player is fully in the game
        /// </summary>
        /// <param name="state">GameState</param>
        public virtual void OnSessionStart(GameState state)
        {
            if (NonStandaloneMods.Any())
            {
                foreach (GameSupport mod in NonStandaloneMods)
                    mod.OnSessionStart(state);
            }

            _onceFlag = false;

            this.IsFirstMap = state.CurrentMap == this.FirstMap;
            this.IsFirstMap2 = state.CurrentMap == this.FirstMap2;
            this.IsLastMap = (!this.IsFirstMap || !this.IsFirstMap2) && state.CurrentMap == this.LastMap;
        }

        // called when player no longer fully in the game (map changed, load started)
        /// <summary>
        /// Actions to do when a session ends and the player is no longer fully in-game
        /// </summary>
        /// <param name="state">GameState</param>
        public virtual void OnSessionEnd(GameState state) 
        {
            if (NonStandaloneMods.Any())
            {
                foreach (GameSupport mod in NonStandaloneMods)
                    mod.OnSessionEnd(state);
            }
        }

        // called every update loop, regardless if the player is full in-game
        /// <summary>
        /// Actions to do when the timer updates, regardless of game states
        /// </summary>
        /// <param name="state">GameState</param>
        public virtual void OnGenericUpdate(GameState state) 
        {
            if (NonStandaloneMods.Any())
            {
                foreach (GameSupport mod in NonStandaloneMods)
                    mod.OnGenericUpdate(state);
            }
        }

        // called once per tick when player is fully in the game
        /// <summary>
        /// Actions to do when the timer updates and the player is fully in the game.
        /// </summary>
        /// <param name="state">GameState</param>
        public virtual GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (NonStandaloneMods.Any())
            {
                foreach (GameSupport mod in NonStandaloneMods)
                {
                    var result = mod.OnUpdate(state);
                    if (mod.QueueOnNextSessionEnd != GameSupportResult.DoNothing)
                    {
                        this.QueueOnNextSessionEnd = mod.QueueOnNextSessionEnd;
                        return GameSupportResult.DoNothing;
                    }
                    else if (result != GameSupportResult.DoNothing)
                    {
                        return result;
                    }
                }
            }

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

        /// <summary>
        /// Get the game-specific code from specified game directory
        /// </summary>
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
                case "portal_tfv":
                    return new Portal();
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
                case "thestanleyparabledemo":
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
                case "hl1":
                    return new HLS();
                case "backwardsmod":
                    return new HL2Mods_BackwardsMod();
                case "school_adventures":
                    return new HL2Mods_SchoolAdventures();
                case "the lost city":
                case "thelostcity":
                    return new HL2Mods_TheLostCity();
                case "entropyzero":
                    return new HL2Mods_EntropyZero();
                case "deeperdown":
                    return new HL2Mods_DeeperDown();
                case "thinktank":
                    return new HL2Mods_ThinkTank();
                case "gnome":
                    return new HL2Mods_Gnome();
                case "hl2-sp-reject":
                    return new HL2Mods_Reject();
                case "thc16-trapville":
                    return new HL2Mods_TrapVille();
                case "runthinkshootliveville":
                    return new HL2Mods_RTSLVille();
                case "abridged":
                    return new HL2Mods_Abridged();
                case "episodeone":
                    return new HL2Mods_EpisodeOne();
                case "combinationville":
                    return new HL2Mods_CombinationVille();
                case "sdk-2013-sp-tlc18-c4-phaseville":
                    return new HL2Mods_PhaseVille();
                case "companionpiece":
                    return new HL2Mods_CompanionPiece();
                case "the citizen":
                case "thecitizen":
                    return new HL2Mods_TheCitizen();
                case "the citizen 2":
                case "thecitizen2":
                case "the citizen returns":
                case "thecitizenreturns":
                    return new HL2Mods_TheCitizen2AndReturns();
                case "1187":
                    return new HL2Mods_1187Ep1();
                case "prospekt":
                    return new Prospekt();
                case "t7":
                    return new HL2Mods_Terminal7();
                case "get_a_life":
                    return new HL2Mods_GetALife();
                case "grey":
                    return new HL2Mods_Grey();
                case "precursor":
                    return new HL2Mods_Precursor();
                case "portalreverse":
                    return new PortalMods_PRMO();
                case "stillalive":
                case "portal-stillalive":
                    return new PortalMods_StillAlive();
                case "ggefc13":
                    return new HL2Mods_GGEFC13();
                case "rexaura":
                    return new PortalMods_Rexaura();
                case "pcborrr":
                    return new PortalMods_PCBORRR();
                case "portal pro":
                case "portalpro":
                    return new PortalMods_PortalPro();
                case "portal prelude":
                case "portalprelude":
                    return new PortalMods_PortalPrelude();
                case "ptsd_2":
                    return new HL2Mods_Ptsd2();
                case "survivor":
                    return new HL2Survivor();
                case "offshore":
                    return new HL2Mods_Offshore();
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
