using HarmonyLib;
using MountAndWarcraftReborn.Patches.TroopWeight;
using TaleWorlds.CampaignSystem.Roster;

namespace MWRMode.Patches.TroopWeight
{
    [HarmonyPatch(typeof(TroopRoster), "get_TotalHealthyCount")]
    public static class TroopRoster_TotalHealthyCount_Patch
    {
        [HarmonyPostfix]
        private static void Postfix(TroopRoster __instance, ref int __result)
        {
            if (!TroopWeightPatchGuards.ShouldApplyGlobalWeight())
            {
                return;
            }

            int weightedCount = MWRPartyWeightHelper.GetWeightedRosterTotalHealthyCount(__instance);
            if (weightedCount > __result)
            {
                __result = weightedCount;
            }
        }
    }
}
