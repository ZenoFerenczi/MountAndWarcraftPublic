using System;
using HarmonyLib;
using MountAndWarcraftReborn.Extensions;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;

namespace MountAndWarcraftReborn.Patches.Hireling
{
    [HarmonyPatch]
    public static class MWRModelPatches
    {
        private const float InfluenceEpsilon = 0.001f;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ChangeClanInfluenceAction), nameof(ChangeClanInfluenceAction.Apply))]
        private static void FreezeHirelingInfluence(Clan clan, ref float amount)
        {
            if (clan == Clan.PlayerClan && Hero.MainHero.IsEnlisted())
            {
                bool isExplicitResetToZero = amount < 0f && Math.Abs(clan.Influence + amount) <= InfluenceEpsilon;
                if (!isExplicitResetToZero)
                {
                    amount = 0f;
                }
            }
        }
    }
}
