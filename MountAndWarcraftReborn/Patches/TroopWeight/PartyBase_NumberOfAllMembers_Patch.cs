using HarmonyLib;
using MountAndWarcraftReborn.Patches.TroopWeight;
using TaleWorlds.CampaignSystem.Party;

namespace MWRMode.Patches.TroopWeight
{
    [HarmonyPatch(typeof(PartyBase), "get_NumberOfAllMembers")]
    public static class PartyBase_NumberOfAllMembers_Patch
    {
        [HarmonyPostfix]
        private static void Postfix(PartyBase __instance, ref int __result)
        {
            if (!TroopWeightPatchGuards.ShouldApplyGlobalWeight())
            {
                return;
            }

            int weightedCount = MWRPartyWeightHelper.GetWeightedPartyNumberOfAllMembers(__instance);
            if (weightedCount > __result)
            {
                __result = weightedCount;
            }
        }
    }
}
