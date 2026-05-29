using HarmonyLib;
using MountAndWarcraftReborn.BattleMechanics.NaturalWeapons;
using TaleWorlds.MountAndBlade;

namespace MountAndWarcraftReborn.Patches.NaturalWeapons
{
    [HarmonyPatch(typeof(Agent), nameof(Agent.SetWeaponGuard))]
    public static class Agent_SetWeaponGuard_Patch
    {
        public static bool Prefix(Agent __instance)
        {
            if (!MWRNaturalWeaponRules.ShouldPreventBlocking(__instance))
            {
                return true;
            }

            __instance.ResetGuard();
            return false;
        }
    }
}
