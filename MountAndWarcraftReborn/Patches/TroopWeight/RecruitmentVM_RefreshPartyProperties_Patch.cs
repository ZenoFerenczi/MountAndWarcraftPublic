using System;
using HarmonyLib;
using MountAndWarcraftReborn.Patches.TroopWeight;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu.Recruitment;
using TaleWorlds.Core;

namespace MWRMode.Patches.TroopWeight
{
    [HarmonyPatch(typeof(RecruitmentVM), "RefreshPartyProperties")]
    public static class RecruitmentVM_RefreshPartyProperties_Patch
    {
        private static void Postfix(RecruitmentVM __instance)
        {
            try
            {
                if (__instance?.TroopsInCart == null)
                    return;

                float cartWeightedCount = 0f;

                foreach (var troop in __instance.TroopsInCart)
                {
                    if (troop?.Character == null)
                        continue;

                    cartWeightedCount += MWRTroopWeightService.GetTroopWeight(troop.Character);
                }

                int basePartyWeightedCount = PartyBase.MainParty?.NumberOfAllMembers ?? 0;
                int totalWeightedSize = basePartyWeightedCount + (int)Math.Ceiling(cartWeightedCount);

                __instance.CurrentPartySize = totalWeightedSize;
                __instance.IsPartyCapacityWarningEnabled = totalWeightedSize > __instance.PartyCapacity;

                GameTexts.SetVariable("LEFT", totalWeightedSize.ToString());
                GameTexts.SetVariable("RIGHT", __instance.PartyCapacity.ToString());
                __instance.PartyCapacityText = GameTexts.FindText("str_LEFT_over_RIGHT").ToString();
            }
            catch
            {
            }
        }
    }
}
