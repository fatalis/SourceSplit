using LiveSplit.ComponentUtil;
using System;
using System.Diagnostics;
using System.Linq;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_YearLongAlarm : GameSupport
    {
        // start: on first map
        // ending: when one of the 2 gunships' HP drops from 0 to lower

        private bool _onceFlag;

        private int _baseEntityHealthOffset = -1;

        private MemoryWatcher<int> _gunship1HP = new MemoryWatcher<int>(IntPtr.Zero);
        private MemoryWatcher<int> _gunship2HP = new MemoryWatcher<int>(IntPtr.Zero);
        private int _gunship2Index;
        private int _triggerIndex;
        private CEntInfoV2 _oldTrig;

        public HL2Mods_YearLongAlarm()
        {
            this.StartOnFirstMapLoad = true;
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "yla_mine";
            this.LastMap = "yla_bridge";
        }
        public override void OnGameAttached(GameState state)
        {
            ProcessModuleWow64Safe server = state.GameProcess.ModulesWow64Safe().FirstOrDefault(x => x.ModuleName.ToLower() == "server.dll");
            Trace.Assert(server != null);

            var scanner = new SignatureScanner(state.GameProcess, server.BaseAddress, server.ModuleMemorySize);
            if (GameMemory.GetBaseEntityMemberOffset("m_iHealth", state.GameProcess, scanner, out _baseEntityHealthOffset))
                Debug.WriteLine("CBaseEntity::m_iHealth offset = 0x" + _baseEntityHealthOffset.ToString("X"));
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);
            if (IsLastMap && _baseEntityHealthOffset != -1)
            {
                _gunship2Index = state.GetEntIndexByName("gunship_intro");
                _triggerIndex = state.GetEntIndexByPos(-13.39f, 227.25f, 393.67f);
                _oldTrig = state.GetEntInfoByIndex(_triggerIndex);

                if (_oldTrig.EntityPtr == IntPtr.Zero)
                    _gunship1HP = new MemoryWatcher<int>(state.GetEntityByName("gunship") + _baseEntityHealthOffset);
                else
                    _gunship1HP = new MemoryWatcher<int>(IntPtr.Zero);

                _gunship2HP = new MemoryWatcher<int>(state.GetEntInfoByIndex(_gunship2Index).EntityPtr + _baseEntityHealthOffset);
            }
            _onceFlag = false;
        }


        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsLastMap)
            {
                // this stuff is messy but it's probably the only way due to how the map works

                var newTrig = state.GetEntInfoByIndex(_triggerIndex);
                var newGunship2 = state.GetEntInfoByIndex(_gunship2Index);

                // there are 2 gunships, one at the start and one at the end, any of them killed will count as an ending

                // the 1st gunship is what you're intended to fight but its only spawned upon touching a trigger
                if (newTrig.EntityPtr == IntPtr.Zero && _oldTrig.EntityPtr != IntPtr.Zero)
                    _gunship1HP = new MemoryWatcher<int>(state.GetEntityByName("gunship") + _baseEntityHealthOffset);
                _oldTrig = newTrig;

                // the 2nd gunship is spawned by default but gets killed after hitting a path_track, but the map can be done fast enough that
                // you get to fight it
                // if the runner is slow and the 2nd gunship finishes its travel and gets deleted, reset the memory watcher
                if (newGunship2.EntityPtr == IntPtr.Zero)
                    _gunship2HP = new MemoryWatcher<int>(IntPtr.Zero);

                _gunship1HP.Update(state.GameProcess);
                _gunship2HP.Update(state.GameProcess);

                if (_gunship1HP.Old > 0 && _gunship1HP.Current <= 0 ||
                    (_gunship2HP.Old > 0 && _gunship2HP.Current <= 0))
                {
                    Debug.WriteLine("year long alarm end");
                    _onceFlag = true;
                    return GameSupportResult.PlayerLostControl;
                }
            }

            return GameSupportResult.DoNothing;
        }
    }
}
