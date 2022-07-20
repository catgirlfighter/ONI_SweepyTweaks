using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace SweepyTweaks.Patches
{
    [HarmonyPatch(typeof(SweepStates), "InitializeStates")]
    public static class SweepStates_InitializeStates_SweepyTweaks
    {
        static FieldInfo Lsweep = null;
        static FieldInfo Lpause = null;
        static FieldInfo Lmopping = null;
        static FieldInfo Lmoving = null;
        static FieldInfo Lredirected = null;
        static FieldInfo LbeginPatrol = null;
        static FieldInfo LtimeUntilBored = null;

        internal static void Prepare()
        {
            if ((Lsweep = AccessTools.Field(typeof(SweepStates), "sweep")) == null) throw new Exception("Can't get field SweepStates.sweep");
            if ((Lpause = AccessTools.Field(typeof(SweepStates), "pause")) == null) throw new Exception("Can't get field SweepStates.pause");
            if ((Lmopping = AccessTools.Field(typeof(SweepStates), "mopping")) == null) throw new Exception("Can't get field SweepStates.mopping");
            if ((Lmoving = AccessTools.Field(typeof(SweepStates), "moving")) == null) throw new Exception("Can't get field SweepStates.moving");
            if ((Lredirected = AccessTools.Field(typeof(SweepStates), "redirected")) == null) throw new Exception("Can't get field SweepStates.redirected");
            if ((LbeginPatrol = AccessTools.Field(typeof(SweepStates), "beginPatrol")) == null) throw new Exception("Can't get field SweepStates.beginPatrol");
            if ((LtimeUntilBored = AccessTools.Field(typeof(SweepStates), "timeUntilBored")) == null) throw new Exception("Can't get field SweepStates.timeUntilBored");
        }
        internal static void Postfix(SweepStates __instance)
        {
            var pause = Lpause.GetValue(__instance) as GameStateMachine<SweepStates, SweepStates.Instance, IStateMachineTarget, SweepStates.Def>.State;
            var sweep = Lsweep.GetValue(__instance) as GameStateMachine<SweepStates, SweepStates.Instance, IStateMachineTarget, SweepStates.Def>.State;
            var mopping = Lmopping.GetValue(__instance) as GameStateMachine<SweepStates, SweepStates.Instance, IStateMachineTarget, SweepStates.Def>.State;
            var moving = Lmoving.GetValue(__instance) as GameStateMachine<SweepStates, SweepStates.Instance, IStateMachineTarget, SweepStates.Def>.State;
            var redirected = Lredirected.GetValue(__instance) as GameStateMachine<SweepStates, SweepStates.Instance, IStateMachineTarget, SweepStates.Def>.State;
            var beginPatrol = LbeginPatrol.GetValue(__instance) as GameStateMachine<SweepStates, SweepStates.Instance, IStateMachineTarget, SweepStates.Def>.State;
            //

            StateMachine.BaseState baseState = (StateMachine.BaseState)Activator.CreateInstance(typeof(GameStateMachine<SweepStates, SweepStates.Instance, IStateMachineTarget, SweepStates.Def>.State));
            __instance.CreateStates(baseState);
            var extrasweep = baseState as GameStateMachine<SweepStates, SweepStates.Instance, IStateMachineTarget, SweepStates.Def>.State;
            extrasweep.name = "extrasweep";
            extrasweep.PlayAnim("pickup").QueueAnim("pickup", true).ToggleEffect("BotSweeping").Enter(delegate (SweepStates.Instance smi)
            {
                __instance.StopMoveSound(smi);
                smi.sm.bored.Set(false, smi);
                var timeUntilBored = LtimeUntilBored.GetValue(smi.sm) as StateMachine<SweepStates, SweepStates.Instance, IStateMachineTarget, SweepStates.Def>.FloatParameter;
                timeUntilBored.Set(30f, smi);
            }).Update(delegate (SweepStates.Instance smi, float dt)
            {
                if (smi.timeinstate < 1f)
                    return;
                //
                if (!__instance.TrySweep(smi))
                {
                    smi.GoTo(moving);
                    return;
                }
            }, UpdateRate.SIM_1000ms, false);
            //
            beginPatrol.enterActions.Clear();
            beginPatrol.Enter(delegate (SweepStates.Instance smi)
                {
                    var timeUntilBored = LtimeUntilBored.GetValue(smi.sm) as StateMachine<SweepStates, SweepStates.Instance, IStateMachineTarget, SweepStates.Def>.FloatParameter;
                    timeUntilBored.Set(30f, smi);
                    smi.GoTo(pause);
                    SweepStates.Instance smi2 = smi;
                    smi2.OnStop = (Action<string, StateMachine.Status>)Delegate.Combine(smi2.OnStop, new Action<string, StateMachine.Status>(delegate (string data, StateMachine.Status status)
                        {
                            __instance.StopMoveSound(smi);
                        }));
                });
            //
            pause.enterActions.Clear();
            pause.Enter(delegate (SweepStates.Instance smi)
                {
                    var comp = smi.gameObject.GetComponent<SweepyExtraModes>();
                    if (comp?.SweepySweepFlags.HasFlag(SweepySweepFlags.Mop) != false && Grid.IsLiquid(Grid.PosToCell(smi)))
                    {
                        smi.GoTo(mopping);
                        return;
                    }
                    if (comp?.SweepySweepFlags.HasFlag(SweepySweepFlags.Sweep) != false && __instance.TrySweep(smi))
                    {
                        if (comp?.SweepySweepFlags.HasFlag(SweepySweepFlags.Extra) == true)
                            smi.GoTo(extrasweep);
                        else
                            smi.GoTo(sweep);
                        return;
                    }
                    smi.GoTo(moving);
                });
        }
    }

    [HarmonyPatch(typeof(SweepStates), "GetNextCell")]
    public static class SweepStates_GetNextCell_SweepyTweaks
    {
        internal static bool Prefix(SweepStates.Instance smi, ref int __result)
        {
            var comp = smi.gameObject.GetComponent<SweepyExtraModes>();
            var canOpenDoors = comp?.SweepySweepFlags.HasFlag(SweepySweepFlags.OpenDoors) == true;
            var canNavigatePlatforms = comp?.SweepySweepFlags.HasFlag(SweepySweepFlags.NavigatePlatforms) == true;
            if (!canOpenDoors && !canNavigatePlatforms)
                return true;

            __result = intGetNextCell(smi, canOpenDoors, canNavigatePlatforms);
            return false;
        }

        private static int intGetNextCell(SweepStates.Instance smi, bool canOpenDoors, bool canNavigatePlatforms)
        {
            int i = 0;
            int num = Grid.PosToCell(smi);
            int num2;
            if (!PassableCell(num, canOpenDoors, canNavigatePlatforms))
                return Grid.InvalidCell;

            while (i < 1)
            {
                num2 = (smi.sm.headingRight.Get(smi) ? Grid.CellRight(num) : Grid.CellLeft(num));
                if (!PassableCell(num2, canOpenDoors, canNavigatePlatforms))
                {
                    break;
                }
                num = num2;
                i++;
            }
            if (num == Grid.PosToCell(smi))
            {
                return Grid.InvalidCell;
            }
            return num;

        }

        private static bool PassableCell(int cell, bool canOpenDoors, bool canNavigatePlatforms)
        {
            int below = Grid.CellBelow(cell);
            //Debug.LogWarning($"cell,below {Grid.IsValidCell(cell)} && {Grid.IsValidCell(below) } && (!{Grid.Solid[cell]} || {canOpenDoors} && {Grid.HasDoor[cell]}) && ({Grid.Solid[below]} || {canNavigatePlatforms} && ({Grid.FakeFloor[below]} || {Grid.HasLadder[below]} && !{Grid.HasLadder[cell]} || {Grid.HasPole[below]} && !{Grid.HasPole[cell]}))");
            return Grid.IsValidCell(cell)
                && Grid.IsValidCell(below)
                && (!Grid.Solid[cell] || canOpenDoors && Grid.HasDoor[cell])
                && (Grid.Solid[below] || canNavigatePlatforms && Grid.FakeFloor[below]); 
                        //|| Grid.HasLadder[below] && !Grid.HasLadder[cell]
                        //|| Grid.HasPole[below] && !Grid.HasPole[cell]));
        }
    }
}
