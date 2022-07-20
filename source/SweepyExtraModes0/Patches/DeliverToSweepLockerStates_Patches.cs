using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace SweepyTweaks.Patches
{
    [HarmonyPatch(typeof(DeliverToSweepLockerStates), "InitializeStates")]
    public static class DeliverToSweepLockerStates_InitializeStates_SweepyTweaks
    {
        static FieldInfo LlockerFull = null;
        static FieldInfo LmovingToStorage = null;
        internal static void Prepare()
        {
            if ((LlockerFull = AccessTools.Field(typeof(DeliverToSweepLockerStates), "lockerFull")) == null) throw new Exception("Can't get field DeliverToSweepLockerStates.lockerFull");
            if ((LmovingToStorage = AccessTools.Field(typeof(DeliverToSweepLockerStates), "movingToStorage")) == null) throw new Exception("Can't get field DeliverToSweepLockerStates.movingToStorage");
        }
        internal static void Postfix(DeliverToSweepLockerStates __instance)
        {
            var lockerFull = LlockerFull.GetValue(__instance) as GameStateMachine<DeliverToSweepLockerStates, DeliverToSweepLockerStates.Instance, IStateMachineTarget, DeliverToSweepLockerStates.Def>.State;
            var movingToStorage = LmovingToStorage.GetValue(__instance) as GameStateMachine<DeliverToSweepLockerStates, DeliverToSweepLockerStates.Instance, IStateMachineTarget, DeliverToSweepLockerStates.Def>.State;
            
            lockerFull.enterActions.Clear();
            lockerFull.PlayAnim("react_stuck", KAnim.PlayMode.Once).Enter(delegate (DeliverToSweepLockerStates.Instance smi)
            {
                Storage storage = __instance.GetSweepLocker(smi);
                if(storage != null)
                    for (int i = 0; i < storage.Count; i++)
                    {
                        storage[i].GetComponent<Clearable>().MarkForClear(false, true);
                        Prioritizable component = storage[i].GetComponent<Prioritizable>();
                        if (component != null) component.SetMasterPriority(storage.masterPriority);
                    }
            }).OnAnimQueueComplete(movingToStorage);
        }
    }
}
