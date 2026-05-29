using System;
using HarmonyLib;
using MWRMode.Patches;
using MountAndWarcraftReborn.Patches.TroopWeight;
using TaleWorlds.CampaignSystem.ViewModelCollection.Party;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace MWRMode.Patches.TroopWeight
{
    [HarmonyPatch(typeof(PartyVM), "PopulatePartyListLabel")]
    public static class PartyVM_PopulatePartyListLabel_Patch
    {
        private static bool Prefix(
            ref string __result,
            MBBindingList<PartyCharacterVM> partyList,
            int limit)
        {
            try
            {
                float weightedHealthy = 0f;
                float weightedWounded = 0f;

                foreach (var item in partyList)
                {
                    if (item?.Character == null)
                        continue;

                    int weight = MWRTroopWeightService.GetTroopWeight(item.Character);
                    int healthy = Math.Max(0, item.Number - item.WoundedCount);
                    int wounded = item.WoundedCount;

                    weightedHealthy += healthy * weight;
                    weightedWounded += wounded * weight;
                }

                int totalHealthy = (int)Math.Ceiling(weightedHealthy);
                int totalWounded = (int)Math.Ceiling(weightedWounded);

                MBTextManager.SetTextVariable("COUNT", totalHealthy);
                MBTextManager.SetTextVariable("WEAK_COUNT", totalWounded);

                if (limit != 0)
                {
                    MBTextManager.SetTextVariable("MAX_COUNT", limit);

                    if (totalWounded > 0)
                    {
                        MBTextManager.SetTextVariable("PARTY_LIST_TAG", "");
                        MBTextManager.SetTextVariable("WEAK_COUNT", totalWounded);
                        MBTextManager.SetTextVariable("TOTAL_COUNT", totalHealthy + totalWounded);
                        __result = new TextObject("{=str_party_list_label_with_weak}{PARTY_LIST_TAG}{TOTAL_COUNT}/{MAX_COUNT} ({COUNT} + {WEAK_COUNT}w)").ToString();
                    }
                    else
                    {
                        MBTextManager.SetTextVariable("PARTY_LIST_TAG", "");
                        __result = new TextObject("{=str_party_list_label}{PARTY_LIST_TAG}({COUNT} / {MAX_COUNT})").ToString();
                    }
                }
                else
                {
                    if (totalWounded > 0)
                    {
                        MBTextManager.SetTextVariable("PARTY_LIST_TAG", "");
                        __result = new TextObject("{=str_party_list_label_with_weak_without_max}{PARTY_LIST_TAG}({COUNT} + {WEAK_COUNT}w)").ToString();
                    }
                    else
                    {
                        MBTextManager.SetTextVariable("PARTY_LIST_TAG", "");
                        __result = new TextObject("{=str_party_list_label_without_max}Party").ToString();
                    }
                }

                return false;
            }
            catch
            {
                return true;
            }
        }
    }
}
