using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;


namespace SweepyTweaks
{
    [HarmonyPatch(typeof(GameNavGrids), MethodType.Constructor, new Type[] { typeof(Pathfinding) })]
    public static class GameNavGrids_Constructor_SweepyTweaks
    {
        static MethodInfo LMirrorTransitions = AccessTools.Method(typeof(GameNavGrids), "MirrorTransitions");

        internal static void Postfix(GameNavGrids __instance, Pathfinding pathfinding)
        {
            __instance.CreateSweepyTweaksNavigation(pathfinding, "SweepyTweaksNavGrid", new CellOffset[]
            {
                new CellOffset(0, 0)
            });
        }

        private static NavGrid CreateSweepyTweaksNavigation(this GameNavGrids gameNavGrids, Pathfinding pathfinding, string id, CellOffset[] bounding_offsets)
        {
            NavGrid.Transition[] transitions = new NavGrid.Transition[]
            {
                new NavGrid.Transition(NavType.Floor, NavType.Floor, 1, 0, NavAxis.NA, true, true, true, 1, "", new CellOffset[0], new CellOffset[0], new NavOffset[0], new NavOffset[0], false, 1f),
                new NavGrid.Transition(NavType.Hover, NavType.Hover, 1, 0, NavAxis.NA, true, false, true, 1, "", new CellOffset[0], new CellOffset[0], new NavOffset[0], new NavOffset[]
                {
                    new NavOffset(NavType.Hover, 1, -1),
                    new NavOffset(NavType.Hover, 1, 1)
                }, false, 1f),
            };
            NavGrid.Transition[] array = LMirrorTransitions.Invoke(gameNavGrids, new object[] { transitions }) as NavGrid.Transition[];
            NavGrid.NavTypeData[] nav_type_data = new NavGrid.NavTypeData[]
            {
                new NavGrid.NavTypeData
                {
                    navType = NavType.Floor,
                    idleAnim = "idle_loop"
                },
                new NavGrid.NavTypeData
                {
                    navType = NavType.Hover,
                    idleAnim = "idle_loop"
                }
            };
            NavGrid navGrid = new NavGrid(id, array, nav_type_data, bounding_offsets, new NavTableValidator[]
            {
                new SweepyWalkValidator(),
            }, 2, 3, array.Length);
            pathfinding.AddNavGrid(navGrid);
            return navGrid;
        }
    }

    public class SweepyWalkValidator : NavTableValidator
    {
        public SweepyWalkValidator()
        {
            World instance = World.Instance;
            instance.OnSolidChanged = (Action<int>)Delegate.Combine(instance.OnSolidChanged, new Action<int>(MarkCellDirty));
        }

        public override void UpdateCell(int cell, NavTable nav_table, CellOffset[] bounding_offsets)
        {
            int num = Grid.CellBelow(cell);
            if (Grid.IsWorldValidCell(num))
            {
                bool flag = Grid.Solid[num] || Grid.FakeFloor[num]; 
                    //|| Grid.HasLadder[num] && !Grid.HasLadder[cell] 
                    //|| Grid.HasPole[num] && !Grid.HasPole[cell];
                nav_table.SetValid(cell, NavType.Floor, flag && IsClear(cell, bounding_offsets, true));
                nav_table.SetValid(cell, NavType.Hover, flag && IsClear(cell, bounding_offsets, true));
            }
        }

        private void MarkCellDirty(int cell)
        {
            onDirty?.Invoke(cell);
        }

        public override void Clear()
        {
            World instance = World.Instance;
            instance.OnSolidChanged = (Action<int>)Delegate.Remove(instance.OnSolidChanged, new Action<int>(MarkCellDirty));
        }
    }
}
