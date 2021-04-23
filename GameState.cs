using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using LiveSplit.ComponentUtil;
using LiveSplit.SourceSplit.GameSpecific;

namespace LiveSplit.SourceSplit
{
    // change back to struct if we ever need to give a copy of the state
    // to the ui thread
    class GameState
    {
        public const int ENT_INDEX_PLAYER = 1;

        public const float IO_EPSILON = 0.03f; // precision of about 2 ticks, could be lowered?

        public Process GameProcess;
        public GameOffsets GameOffsets;

        public HostState HostState;
        public HostState PrevHostState;

        public SignOnState SignOnState;
        public SignOnState PrevSignOnState;

        public ServerState ServerState;
        public ServerState PrevServerState;

        public string CurrentMap;
        public string GameDir;

        public float IntervalPerTick;
        public int RawTickCount;
        public float FrameTime;
        public int PrevRawTickCount;
        public int TickBase;
        public int TickCount;
        public float TickTime;

        public FL PlayerFlags;
        public FL PrevPlayerFlags;
        public Vector3f PlayerPosition;
        public Vector3f PrevPlayerPosition;
        public int PlayerViewEntityIndex;
        public int PrevPlayerViewEntityIndex;
        public int PlayerParentEntityHandle;
        public int PrevPlayerParentEntityHandle;

        public CEntInfoV2 PlayerEntInfo;
        public GameSupport GameSupport;
        public int UpdateCount;

        public GameState(Process game, GameOffsets offsets)
        {
            this.GameProcess = game;
            this.GameOffsets = offsets;
        }

        /// <summary>
        /// Gets the entity info of an entity using its index. 
        /// </summary>
        /// <param name="index">The index of the entity</param>
        public CEntInfoV2 GetEntInfoByIndex(int index)
        {
            Debug.Assert(this.GameOffsets.EntInfoSize > 0);

            if (index < 0)
                return new CEntInfoV2();

            CEntInfoV2 ret;

            IntPtr addr = this.GameOffsets.GlobalEntityListPtr + ((int)this.GameOffsets.EntInfoSize * index);

            if (this.GameOffsets.EntInfoSize == CEntInfoSize.HL2)
            {
                CEntInfoV1 v1;
                this.GameProcess.ReadValue(addr, out v1);
                ret = CEntInfoV2.FromV1(v1);
            }
            else
            {
                this.GameProcess.ReadValue(addr, out ret);
            }

            return ret;
        }

        // warning: expensive -  7ms on i5
        // do not call frequently!
        /// <summary>
        /// Gets the entity pointer of the entity with matching name
        /// </summary>
        /// <param name="name">The name of the entity</param>
        public IntPtr GetEntityByName(string name)
        {
            // se 2003 has a really convoluted ehandle system that basically equivalent to this
            // so let's just use the old system for that
            if (GameMemory.IsSource2003)
            {
                int maxEnts = GameOffsets.CurrentEntCountPtr != IntPtr.Zero ?
                    GameProcess.ReadValue<int>(GameOffsets.CurrentEntCountPtr) : 2048;

                for (int i = 0; i < maxEnts; i++)
                {
                    if (i % 100 == 0)
                        maxEnts = GameProcess.ReadValue<int>(GameOffsets.CurrentEntCountPtr);

                    CEntInfoV2 info = this.GetEntInfoByIndex(i);
                    if (info.EntityPtr == IntPtr.Zero)
                        continue;

                    IntPtr namePtr;
                    this.GameProcess.ReadPointer(info.EntityPtr + this.GameOffsets.BaseEntityTargetNameOffset, false, out namePtr);
                    if (namePtr == IntPtr.Zero)
                        continue;

                    string n;
                    this.GameProcess.ReadString(namePtr, ReadStringType.ASCII, 32, out n);  // TODO: find real max len
                    if (n == name)
                        return info.EntityPtr;
                }
            } 
            else
            {
                CEntInfoV2 nextPtr = this.GetEntInfoByIndex(0);
                do
                {
                    if (nextPtr.EntityPtr == IntPtr.Zero)
                        return IntPtr.Zero;

                    IntPtr namePtr;
                    this.GameProcess.ReadPointer(nextPtr.EntityPtr + this.GameOffsets.BaseEntityTargetNameOffset, false, out namePtr);
                    if (namePtr != IntPtr.Zero)
                    {
                        this.GameProcess.ReadString(namePtr, ReadStringType.ASCII, 32, out string n);  // TODO: find real max len
                        if (n == name)
                            return nextPtr.EntityPtr;
                    }
                    nextPtr = GameProcess.ReadValue<CEntInfoV2>((IntPtr)nextPtr.m_pNext);
                }
                while (nextPtr.EntityPtr != IntPtr.Zero);
            }
            return IntPtr.Zero;
        }

        /// <summary>
        /// Gets the entity index of the entity with matching name
        /// </summary>
        /// <param name="name">The name of the entity</param>
        public int GetEntIndexByName(string name)
        {
            int maxEnts = GameOffsets.CurrentEntCountPtr != IntPtr.Zero ?
                GameProcess.ReadValue<int>(GameOffsets.CurrentEntCountPtr) : 2048;

            for (int i = 0; i < maxEnts; i++)
            {
                // update our max entites value every 100 tries to account for mid-scan additions or removals
                if (i % 100 == 0)
                    maxEnts = GameProcess.ReadValue<int>(GameOffsets.CurrentEntCountPtr);

                CEntInfoV2 info = this.GetEntInfoByIndex(i);
                if (info.EntityPtr == IntPtr.Zero)
                    continue;

                IntPtr namePtr;
                this.GameProcess.ReadPointer(info.EntityPtr + this.GameOffsets.BaseEntityTargetNameOffset, false, out namePtr);
                if (namePtr == IntPtr.Zero)
                    continue;

                string n;
                this.GameProcess.ReadString(namePtr, ReadStringType.ASCII, 32, out n);  // TODO: find real max len
                if (n == name)
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// Gets the entity index of the entity with matching position
        /// </summary>
        /// <param name="x">The x coordinate of the entity</param>
        /// <param name="y">The y coordinate of the entity</param>
        /// <param name="z">The z coordinate of the entity</param>
        /// <param name="d">The maximum allowed distance away from the specified position</param>
        /// <param name="xy">Whether to ignore the z component when evaluating positions</param>
        public int GetEntIndexByPos(float x, float y, float z, float d = 0f, bool xy = false)
        {
            Vector3f pos = new Vector3f(x, y, z);
            int maxEnts = GameOffsets.CurrentEntCountPtr != IntPtr.Zero ?
                GameProcess.ReadValue<int>(GameOffsets.CurrentEntCountPtr) : 2048;

            for (int i = 0; i < maxEnts; i++)
            {
                // update our max entites value every 100 tries to account for mid-scan additions or removals
                if (i % 100 == 0)
                    maxEnts = GameProcess.ReadValue<int>(GameOffsets.CurrentEntCountPtr);

                CEntInfoV2 info = this.GetEntInfoByIndex(i);
                if (info.EntityPtr == IntPtr.Zero)
                    continue;

                Vector3f newpos;
                if (!this.GameProcess.ReadValue(info.EntityPtr + this.GameOffsets.BaseEntityAbsOriginOffset, out newpos))
                    continue;

                if (d == 0f)
                {
                    if (newpos.BitEquals(pos) && i != 1) //not equal 1 becase the player might be in the same exact position
                        return i;
                }
                else // check for distance if it's a non-static entity like an npc or a prop
                {
                    if (xy) 
                    {
                        if (newpos.DistanceXY(pos) <= d && i != 1) 
                            return i;
                    }
                    else
                    {
                        if (newpos.Distance(pos) <= d && i != 1) 
                            return i;
                    }
                }
            }

            return -1;
        }

        /// <summary>
        /// Gets the entity position of the entity with matching index
        /// </summary>
        /// <param name="i">The index of the entity</param>
        public Vector3f GetEntityPos(int i)
        {
            Vector3f pos;
            var ent = GetEntInfoByIndex(i);
            GameProcess.ReadValue(ent.EntityPtr + this.GameOffsets.BaseEntityAbsOriginOffset, out pos);
            return pos;
        }

        // env_fades don't hold any live fade information and instead they network over fade infos to the client which add it to a list
        /// <summary>
        /// Finds the time when a fade in or out finishes. Returns 0 on failure to find a fade with matching description.
        /// </summary>
        /// <param name="speed">The speed of the fade</param>
        public float FindFadeEndTime(float speed)
        {
            int fadeListSize = GameProcess.ReadValue<int>(GameOffsets.FadeListPtr + 0x10);
            if (fadeListSize == 0) return 0;

            ScreenFadeInfo fadeInfo;
            uint fadeListHeader = GameProcess.ReadValue<uint>(GameOffsets.FadeListPtr + 0x4);
            for (int i = 0; i < fadeListSize; i++)
            {
                fadeInfo = GameProcess.ReadValue<ScreenFadeInfo>(GameProcess.ReadPointer((IntPtr)fadeListHeader) + 0x4 * i);
                if (fadeInfo.Speed != speed)
                    continue;
                else
                    return fadeInfo.End;
            }
            return 0;
        }

        /// <summary>
        /// Finds the time when a fade in or out finishes. Returns 0 on failure to find a fade with matching description.
        /// </summary>
        /// <param name="speed">The speed of the fade</param>
        /// <param name="r">Red value of the color of the game</param>
        /// <param name="g">Green value of the color of the game</param>
        /// <param name="b">Blue value of the color of the game</param>
        public float FindFadeEndTime(float speed, byte r, byte g, byte b)
        {
            int fadeListSize = GameProcess.ReadValue<int>(GameOffsets.FadeListPtr + 0x10);
            if (fadeListSize == 0) return 0;

            ScreenFadeInfo fadeInfo;
            byte[] targColor = { r, g, b };
            uint fadeListHeader = GameProcess.ReadValue<uint>(GameOffsets.FadeListPtr + 0x4);
            for (int i = 0; i < fadeListSize; i++)
            {
                fadeInfo = GameProcess.ReadValue<ScreenFadeInfo>(GameProcess.ReadPointer((IntPtr)fadeListHeader) + 0x4 * i);
                byte[] color = { fadeInfo.r, fadeInfo.g, fadeInfo.b };
                if (fadeInfo.Speed != speed && !targColor.Equals(color))
                    continue;
                else
                    return fadeInfo.End;
            }
            return 0;
        }

        // ioEvents are stored in a non-contiguous list where every ioEvent contain pointers to the next or previous event 
        // todo: add more input types and combinations to ensure the correct result
        /// <summary>
        /// Finds the fire time of an output. Returns 0 on failure to find an output with matching description.
        /// </summary>
        /// <param name="targetName">The name of the targeted entity</param>
        /// <param name="clamp">The maximum number of inputs to check</param>
        /// <param name="inclusive">If the name specified is a substring of the target name</param>
        public float FindOutputFireTime(string targetName, int clamp = 100, bool inclusive = false)
        {
            if (GameProcess.ReadPointer(GameOffsets.EventQueuePtr) == IntPtr.Zero)
                return 0;

            EventQueuePrioritizedEvent ioEvent;
            GameProcess.ReadValue(GameProcess.ReadPointer(GameOffsets.EventQueuePtr), out ioEvent);

            // clamp the number of items to go through the list to save performance
            // the list is automatically updated once an output is fired
            for (int i = 0; i < clamp; i++)
            {
                string tempName = GameProcess.ReadString((IntPtr)ioEvent.m_iTarget, 256) ?? "";
                if (!inclusive ? tempName == targetName : tempName.Contains(targetName))
                    return ioEvent.m_flFireTime;
                else
                {
                    IntPtr nextPtr = (IntPtr)ioEvent.m_pNext;
                    if (nextPtr != IntPtr.Zero)
                    {
                        GameProcess.ReadValue(nextPtr, out ioEvent);
                        continue;
                    }
                    else return 0; // end early if we've hit the end of the list
                }
            }

            return 0;
        }

        /// <summary>
        /// Finds the fire time of an output. Returns 0 on failure to find an output with matching description.
        /// </summary>
        /// <param name="targetName">The name of the targeted entity</param>
        /// <param name="command">The command of the output</param>
        /// <param name="param">The parameters of the command</param>
        /// <param name="clamp">The maximum number of inputs to check</param>
        /// <param name="nameInclusive">If the name specified is a substring of the target name</param>
        /// <param name="commandInclusive">If the command specified is a substring of the command</param>
        /// <param name="paramInclusive">If the parameters specified are a substring of the parameters</param>
        public float FindOutputFireTime(string targetName, string command, string param, int clamp = 100, 
            bool nameInclusive = false, bool commandInclusive = false, bool paramInclusive = false)
        {
            if (GameProcess.ReadPointer(GameOffsets.EventQueuePtr) == IntPtr.Zero)
                return 0;

            EventQueuePrioritizedEvent ioEvent;
            GameProcess.ReadValue(GameProcess.ReadPointer(GameOffsets.EventQueuePtr), out ioEvent);

            for (int i = 0; i < clamp; i++) 
            {
                string tempName = GameProcess.ReadString((IntPtr)ioEvent.m_iTarget, 256) ?? "";
                string tempCommand = GameProcess.ReadString((IntPtr)ioEvent.m_iTargetInput, 256) ?? "";
                string tempParam = GameProcess.ReadString((IntPtr)ioEvent.v_union, 256) ?? "";

                if ((!nameInclusive) ? tempName == targetName : tempName.Contains(targetName) &&
                   ((!commandInclusive) ? tempCommand.ToLower() == command.ToLower() : tempCommand.ToLower().Contains(command.ToLower())) && 
                   ((!paramInclusive) ? tempParam.ToLower() == param.ToLower() : tempParam.ToLower().Contains(param.ToLower())))
                    return ioEvent.m_flFireTime;
                else
                {
                    IntPtr nextPtr = (IntPtr)ioEvent.m_pNext;
                    if (nextPtr != IntPtr.Zero)
                    {
                        GameProcess.ReadValue(nextPtr, out ioEvent);
                        continue;
                    }
                    else return 0; // end early if we've hit the end of the list
                }
            }

            return 0;
        }

        // fixme: this *could* probably return true twice if the player save/loads on an exact tick
        // precision notice: will always be too early by at most 2 ticks using the standard 0.03 epsilon
        /// <summary>
        /// Compares the inputted time to the internal timer.
        /// </summary>
        /// <param name="splitTime">The time to compare with internal time</param>
        /// <param name="epsilon">The maximum allowed distance between inputted time and internal time</param>
        /// <param name="checkBefore">Whether to check if the internal timer has just gone past inputted time</param>
        /// <param name="adjustFrameTime">Whether to account for frametime (lagginess / alt-tabbing)</param>
        public bool CompareToInternalTimer(float splitTime, float epsilon = IO_EPSILON, bool checkBefore = false, bool adjustFrameTime = false)
        {
            if (splitTime == 0f) return false;

            // adjust for lagginess for example at very low fps or if the game's alt-tabbed
            // could be exploitable but not enough to be concerning
            // max frametime without cheats should be 0.05, so leniency is ~<3 ticks
            splitTime -= adjustFrameTime ? (FrameTime > IntervalPerTick ? FrameTime / 1.15f : 0f) : 0f;

            if (epsilon == 0f)
            {
                if (checkBefore)
                    return RawTickCount * IntervalPerTick >= splitTime
                        && PrevRawTickCount * IntervalPerTick < splitTime;

                return RawTickCount * IntervalPerTick >= splitTime;
            }
            else
            {
                if (checkBefore)
                    return Math.Abs(splitTime - RawTickCount * IntervalPerTick) <= epsilon &&
                        Math.Abs(splitTime - PrevRawTickCount * IntervalPerTick) >= epsilon;
                return Math.Abs(splitTime - RawTickCount * IntervalPerTick) <= epsilon;
            }
        }

        // currently unused, useful in getting the entity index of a caller in an input/output 
        public int GetIndexOfEHANDLE(uint EHANDLE)
        {
            // FIXME: this mask is actually version dependent, newer ones use 0x1fff!!!
            // possible sig to identify: 8b ?? ?? ?? ?? ?? 8b ?? 81 e1 ff ff 00 00
            int mask = 0xFFF;
            return (EHANDLE & mask) == mask ? -1 : (int)(EHANDLE & mask);
        }
    }

    struct GameOffsets
    {
        public IntPtr CurTimePtr;
        public IntPtr TickCountPtr => this.CurTimePtr + 12;
        public IntPtr IntervalPerTickPtr => this.TickCountPtr + 4;
        public IntPtr SignOnStatePtr;
        public IntPtr CurMapPtr;
        public IntPtr GlobalEntityListPtr;
        public IntPtr GameDirPtr;
        public IntPtr HostStatePtr;
        public IntPtr FadeListPtr;
        // note: only valid during host states: NewGame, ChangeLevelSP, ChangeLevelMP
        // note: this may not work pre-ep1 (ancient engine), HLS -7 is a good example
        public IntPtr HostStateLevelNamePtr => this.HostStatePtr + (4 * (GameMemory.IsSource2003 ? 2 : 8));
        public IntPtr ServerStatePtr;
        public IntPtr EventQueuePtr;
        public IntPtr CurrentEntCountPtr;

        public CEntInfoSize EntInfoSize;

        public int BaseEntityFlagsOffset;
        public int BaseEntityEFlagsOffset => this.BaseEntityFlagsOffset > 0 ? this.BaseEntityFlagsOffset - 4 : -1;
        public int BaseEntityAbsOriginOffset;
        public int BaseEntityTargetNameOffset;
        public int BaseEntityParentHandleOffset;
        public int BasePlayerViewEntity;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct CEntInfoV1
    {
        public uint m_pEntity;
        public int m_SerialNumber;
        public uint m_pPrev;
        public uint m_pNext;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct CEntInfoV2
    {
        public uint m_pEntity;
        public int m_SerialNumber;
        public uint m_pPrev;
        public uint m_pNext;
        public int m_targetname;
        public int m_classname;

        public IntPtr EntityPtr => (IntPtr)this.m_pEntity;

        public static CEntInfoV2 FromV1(CEntInfoV1 v1)
        {
            var ret = new CEntInfoV2();
            ret.m_pEntity = v1.m_pEntity;
            ret.m_SerialNumber = v1.m_SerialNumber;
            ret.m_pPrev = v1.m_pPrev;
            ret.m_pNext = v1.m_pNext;
            return ret;
        }
    }

    // taken from source sdk
    [StructLayout(LayoutKind.Sequential)]
    public struct ScreenFadeInfo
    {
        public float Speed;            // How fast to fade (tics / second) (+ fade in, - fade out)
        public float End;              // When the fading hits maximum
        public float Reset;            // When to reset to not fading (for fadeout and hold)
        public byte r, g, b, alpha;    // Fade color
        public int Flags;              // Fading flags
    };

    // todo: figure out a way to utilize ehandles
    [StructLayout(LayoutKind.Sequential)]
    public struct EventQueuePrioritizedEvent
    {
        public float m_flFireTime;
        public uint m_iTarget;
        public uint m_iTargetInput;
        public uint m_pActivator;       // EHANDLE
        public uint m_pCaller;          // EHANDLE
        public int m_iOutputID;
        public uint m_pEntTarget;       // EHANDLE
        // variant_t m_VariantValue, class, only relevant members
        // most notable is v_union which stores the parameters of the i/o event
        public uint v_union, v_eval, v_fieldtype, v_tostringfunc, v_CVariantSaveDataOpsclass;
        public uint m_pNext;
        public uint m_pPrev;
    };
}
