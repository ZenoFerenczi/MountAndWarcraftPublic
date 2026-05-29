using HarmonyLib;
using MountAndWarcraftReborn.Extensions;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;

namespace MountAndWarcraftReborn.Patches.Hireling
{
    [HarmonyPatch]
    public static class MWRMobilePartyPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PartyBase), "UpdateVisibilityAndInspected", MethodType.Normal)]
        public static bool PreUpdateVisibility(ref PartyBase __instance)
        {
            if (!__instance.IsMobile || !__instance.MobileParty.IsMainParty)
            {
                return true;
            }

            if (Hero.MainHero.IsEnlisted())
            {
                __instance.MobileParty.IsVisible = false;
                return false;
            }

            return true;
        }
    }
}
