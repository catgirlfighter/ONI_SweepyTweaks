using System;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using HarmonyLib;

namespace SweepyTweaks
{
    public class SweepyDoorTransitionLayer : DoorTransitionLayer
    {
        private static readonly FieldInfo Ldoors = AccessTools.Field(typeof(DoorTransitionLayer), "doors");
        public SweepyDoorTransitionLayer(Navigator navigator) : base(navigator)
        {
        }

        public override void BeginTransition(Navigator navigator, Navigator.ActiveTransition transition)
        {
            base.BeginTransition(navigator, transition);
            var doors = Ldoors.GetValue(this) as List<INavDoor>;
            if (doors.Count > 0) transition.anim = "react_pos";
        }
    }
}
