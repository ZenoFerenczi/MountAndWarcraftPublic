using HarmonyLib;
using MountAndWarcraftReborn.Patches.TroopWeight;
using TaleWorlds.CampaignSystem.Roster;

namespace MWRMode.Patches.TroopWeight
{
    [HarmonyPatch(typeof(TroopRoster), "get_TotalManCount")]
    public static class TroopRoster_TotalManCount_Patch
    {
        [HarmonyPostfix]
        private static void Postfix(TroopRoster __instance, ref int __result)
        {
            if (!TroopWeightPatchGuards.ShouldApplyGlobalWeight())
            {
                return;
            }

            int weightedCount = MWRPartyWeightHelper.GetWeightedRosterTotalManCount(__instance);
            if (weightedCount > __result)
            {
                __result = weightedCount;
            }
        }
    }
}
