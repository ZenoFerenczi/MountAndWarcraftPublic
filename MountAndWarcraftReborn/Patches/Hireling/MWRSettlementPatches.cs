using HarmonyLib;
using MountAndWarcraftReborn.Extensions;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;

namespace MountAndWarcraftReborn.Patches.Hireling
{
    [HarmonyPatch]
    public static class MWRSettlementPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerTownVisitCampaignBehavior), "game_menu_town_town_leave_on_condition")]
        public static void DisableEnlistedLeaveTown(ref bool __result)
        {
            if (Hero.MainHero.IsEnlisted())
            {
                __result = false;
            }
        }
    }
}
