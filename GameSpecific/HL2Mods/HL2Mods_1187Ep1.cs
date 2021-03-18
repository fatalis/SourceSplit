using LiveSplit.ComponentUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_1187Ep1 : GameSupport
    {
        // start: when the view index switches to the player
        // ending: when the vort(s)' hp(s) go down to 0 AND all the vort spawners are exhausted

        private bool _onceFlag;
        private const int _baseMaxNumNPCsOffset = 0x338;
        private const int _baseLiveChildrenOffset = 0x3a0;
        private int _baseEntityHealthOffset = -1;

        private int _startCamIndex;
        
        // the run ends on final blow to the final vortigaunt, the latter we can't track precisely since there are multiple
        // and they can be killed in any order. so we'll have to track the hp of every one of them.
        // vorts' hp
        private int[] _vortHP = { -1, -1, -1, -1 };
        private int[] _vortHPOld = { 100, 100, 100, 100 };
        // vorts' entity pointer
        private IntPtr[] _vortPtr = { IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero };
        // vorts' spawners' pointers
        private IntPtr[] _spawnersPtr = { IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero };
        // vorts' spawners' live children count. this variable only gets decremented when the vorts' entity are killed
        // which takes place a bit after theyre killed by the player. used to determine when we should scan for their pointers
        private int[] _curLiveChildren = { -1, -1, -1, -1 };
        // vorts' spawners' number of spawnable npcs left. this variable gets decremented when the vorts spawn in. used to check if we have
        // killed or have begun engaging with all the vorts or not
        private int[] _curMaxNPCs = { -1, -1, -1, -1 };
        // dictionary of vorts' entity names and their spawners' names
        private Dictionary<string, string> _vortsList = new Dictionary<string, string>()
        {
            { "vort_enhanced01"         , "vort_enh_template" }, 
            { "vort_enhanced02"         , "vort_enh_template2" }, 
            { "vort_enhanced03"         , "vort_enh_template5" }, 
            { "vort_enhanced04"         , "vort_enh_template4" }
        };

        public HL2Mods_1187Ep1()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "1187d1";
            this.LastMap = "1187d10";
            this.RequiredProperties = PlayerProperties.ViewEntity;
        }

        public override void OnGameAttached(GameState state)
        {
            base.OnGameAttached(state);

            ProcessModuleWow64Safe server = state.GameProcess.ModulesWow64Safe().FirstOrDefault(x => x.ModuleName.ToLower() == "server.dll");
            Trace.Assert(server != null);

            var scanner = new SignatureScanner(state.GameProcess, server.BaseAddress, server.ModuleMemorySize);

            if (GameMemory.GetBaseEntityMemberOffset("m_iHealth", state.GameProcess, scanner, out _baseEntityHealthOffset))
                Debug.WriteLine("CBaseEntity::m_iHealth offset = 0x" + _baseEntityHealthOffset.ToString("X"));
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);

            if (this.IsFirstMap)
            {
                _startCamIndex = state.GetEntIndexByName("introcam03");
                Debug.WriteLine("found start cam index at " + _startCamIndex);
            }
            else if (this.IsLastMap)
            {
                for (int i = 0; i < 4; i++)
                {
                    _vortPtr[i] = IntPtr.Zero;
                    _spawnersPtr[i] = state.GetEntityByName(_vortsList.ElementAt(i).Value);
                    Debug.WriteLine(_vortsList.ElementAt(i).Value + " ptr is 0x" + _spawnersPtr[i].ToString("X"));
                }
            }

            _onceFlag = false;
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (IsFirstMap)
            {
                if (state.PrevPlayerViewEntityIndex == _startCamIndex && state.PlayerViewEntityIndex == 1)
                {
                    Debug.WriteLine("1187ep1 start");
                    _onceFlag = true;
                    return GameSupportResult.PlayerGainedControl;
                }
            }
            else if (IsLastMap)
            {
                // performance seems to be fine
                for (int i = 0; i < 4; i++)
                {
                    // store the hp's to the old hp array
                    _vortHPOld[i] = _vortHP[i];
                    _curMaxNPCs[i] = state.GameProcess.ReadValue<int>(_spawnersPtr[i] + _baseMaxNumNPCsOffset);
                    _curLiveChildren[i] = state.GameProcess.ReadValue<int>(_spawnersPtr[i] + _baseLiveChildrenOffset);

                    // is this vort's entity deleted?
                    if (_curLiveChildren[i] == 0)
                        _vortHP[i] = 0;
                    else
                    {
                        // if not and we don't have its pointer, scan for it
                        if (_vortPtr[i] == IntPtr.Zero)
                        {
                            _vortPtr[i] = state.GetEntityByName(_vortsList.ElementAt(i).Key);
                            Debug.WriteLine(_vortsList.ElementAt(i).Key + " ptr is 0x" + _vortPtr[i].ToString("X"));
                        }
                        // now get its health
                        _vortHP[i] = state.GameProcess.ReadValue<int>(_vortPtr[i] + _baseEntityHealthOffset);
                    }
                }

                if (_curMaxNPCs.All(x => x == 0) && _vortHPOld.Any(x => x > 0) && _vortHP.All(x => x <= 0))
                {
                    Debug.WriteLine("1187ep1 end");
                    _onceFlag = true;
                    return GameSupportResult.PlayerLostControl;
                }
            }
            return GameSupportResult.DoNothing;
        }
    }
}
