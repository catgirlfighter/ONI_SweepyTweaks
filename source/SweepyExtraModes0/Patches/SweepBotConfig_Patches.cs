using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace SweepyTweaks.Patches
{
    [HarmonyPatch(typeof(SweepBotConfig), "CreatePrefab")]
    public static class SweepBotConfig_CreatePrefab_SweepyTweaks
    {
        internal static void Postfix(GameObject __result)
        {
            if (__result == null)
                return;
            //
            __result.AddOrGet<SweepyExtraModes>();
            var navigator = __result.AddOrGet<Navigator>();
            navigator.NavGridName = "SweepyTweaksNavGrid";
        }
    }

    [HarmonyPatch(typeof(SweepBotConfig), "OnSpawn")]
    public static class SweepBotConfig_OnSpawn_SweepyTweaks
    {
        internal static void Postfix(GameObject inst)
        {
            if (inst == null)
                return;
            //
            var navigator = inst.AddOrGet<Navigator>();
            navigator.transitionDriver.overrideLayers.Add(new SweepyDoorTransitionLayer(navigator));
        }
    }
}