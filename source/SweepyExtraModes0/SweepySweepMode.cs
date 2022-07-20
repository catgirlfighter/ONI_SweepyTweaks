using System;
using System.Collections.Generic;
using System.Linq;
using KSerialization;

namespace SweepyTweaks
{   
    [Flags]
    public enum SweepySweepFlags
    {
        None = 0,
        Sweep = 1,
        Mop = 2,
        Extra = 4,
        OpenDoors = 8,
        NavigatePlatforms = 16,
        Standard = Sweep + Mop,
    }

    public class SweepyExtraModes : KMonoBehaviour
    {
        protected override void OnPrefabInit()
        {
            base.Subscribe((int)GameHashes.RefreshUserMenu, delegate (object data)
            {
                this.OnRefreshUserMenu();
            });
        }

        private void OnRefreshUserMenu()
        {
            Action shortcutKey;
            Enum.TryParse("NumActions", out shortcutKey);
            if (SweepySweepFlags.HasFlag(SweepySweepFlags.Standard))
            {
                Game.Instance.userMenu.AddButton(gameObject, new KIconButtonMenu.ButtonInfo("action_navigable_regions", "Mode: Standard", new System.Action(SetSweepMode), shortcutKey, null, null, null, "Working normally", true), 1f);
            }
            else if (SweepySweepFlags.HasFlag(SweepySweepFlags.Sweep))
            {
                Game.Instance.userMenu.AddButton(gameObject, new KIconButtonMenu.ButtonInfo("action_navigable_regions", "Mode: Sweeper", new System.Action(SetMopMode), shortcutKey, null, null, null, "Sweeping Only", true), 1f);
            }
            else
            {
                Game.Instance.userMenu.AddButton(gameObject, new KIconButtonMenu.ButtonInfo("action_navigable_regions", "Mode: Mop", new System.Action(SetStandardMode), shortcutKey, null, null, null, "Mopping Only", true), 1f);
            }
            //
            if (SweepySweepFlags.HasFlag(SweepySweepFlags.Sweep))
            {
                if (SweepySweepFlags.HasFlag(SweepySweepFlags.Extra))
                {
                    Game.Instance.userMenu.AddButton(gameObject, new KIconButtonMenu.ButtonInfo("action_navigable_regions", "Patrol: Cleaning", new System.Action(SetCancelExtraSweepy), shortcutKey, null, null, null, "Sweepy focuses on cleaning instead of covering the area", true), 1f);
                }
                else
                {
                    Game.Instance.userMenu.AddButton(gameObject, new KIconButtonMenu.ButtonInfo("action_navigable_regions", "Patrol: Roaming", new System.Action(SetExtraSweepy), shortcutKey, null, null, null, "Sweepy roams from wall to wall picking up little by little on the way", true), 1f);
                }
            }
            //
            if (SweepySweepFlags.HasFlag(SweepySweepFlags.OpenDoors))
            {
                Game.Instance.userMenu.AddButton(gameObject, new KIconButtonMenu.ButtonInfo("icon_action_cancel", "Forbid Open Doors", new System.Action(ForbidOpenDoors), shortcutKey, null, null, null, "", true), 1f);
            }
            else
            {
                Game.Instance.userMenu.AddButton(gameObject, new KIconButtonMenu.ButtonInfo("action_navigable_regions", "Allow Open Doors", new System.Action(AllowOpenDoors), shortcutKey, null, null, null, "", true), 1f);
            }
            //
            if (SweepySweepFlags.HasFlag(SweepySweepFlags.NavigatePlatforms))
            {
                Game.Instance.userMenu.AddButton(gameObject, new KIconButtonMenu.ButtonInfo("icon_action_cancel", "Forbid Navigate Platforms", new System.Action(ForbidNavigatePlatforms), shortcutKey, null, null, null, "", true), 1f);
            }
            else
            {
                Game.Instance.userMenu.AddButton(gameObject, new KIconButtonMenu.ButtonInfo("action_navigable_regions", "Allow Navigate Platforms", new System.Action(AllowNavigatePlatforms), shortcutKey, null, null, null, "", true), 1f);
            }
        }

        internal void SetStandardMode()
        {
            SweepySweepFlags |= SweepySweepFlags.Mop | SweepySweepFlags.Sweep;
        }

        internal void SetSweepMode()
        {
            SweepySweepFlags |= SweepySweepFlags.Sweep;
            SweepySweepFlags &= ~SweepySweepFlags.Mop;
        }

        internal void SetMopMode()
        {
            SweepySweepFlags |= SweepySweepFlags.Mop;
            SweepySweepFlags &= ~SweepySweepFlags.Sweep;
        }

        internal void SetExtraSweepy()
        {
            SweepySweepFlags |= SweepySweepFlags.Extra;
        }

        internal void SetCancelExtraSweepy()
        {
            SweepySweepFlags &= ~SweepySweepFlags.Extra;
        }

        internal void AllowOpenDoors()
        {
            SweepySweepFlags |= SweepySweepFlags.OpenDoors;
        }

        internal void ForbidOpenDoors()
        {
            SweepySweepFlags &= ~SweepySweepFlags.OpenDoors;
        }
        internal void AllowNavigatePlatforms()
        {
            SweepySweepFlags |= SweepySweepFlags.NavigatePlatforms;
        }

        internal void ForbidNavigatePlatforms()
        {
            SweepySweepFlags &= ~SweepySweepFlags.NavigatePlatforms;
        }

        [Serialize]
        public SweepySweepFlags SweepySweepFlags = SweepySweepFlags.Standard;
    }
}
