using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.ViewModelCollection.Map.MapBar;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Localization;

namespace MountAndWarcraftReborn.Patches.TroopWeight
{
    [HarmonyPatch(typeof(MapInfoVM), "Refresh")]
    public static class MapInfoVM_Refresh_Patch
    {
        private static readonly FieldInfo TroopsInfoField =
            typeof(MapInfoVM).GetField("_troopsInfo", BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly ConditionalWeakTable<MapInfoVM, MapInfoItemVM> WeightedTroopsInfoCache =
            new ConditionalWeakTable<MapInfoVM, MapInfoItemVM>();

        private static MapInfoVM _activeMapInfoVm;

        private static void Postfix(MapInfoVM __instance)
        {
            try
            {
                if (__instance == null || TroopsInfoField == null)
                    return;

                _activeMapInfoVm = __instance;

                MapInfoItemVM existingTroopsInfo = TroopsInfoField.GetValue(__instance) as MapInfoItemVM;
                if (existingTroopsInfo == null)
                    return;

                MapInfoItemVM weightedTroopsInfo = WeightedTroopsInfoCache.GetValue(
                    __instance,
                    _ => new MapInfoItemVM("troops", GetWeightedTroopsHintText));

                ReplaceTroopsInfoItemIfNeeded(__instance, existingTroopsInfo, weightedTroopsInfo);
                UpdateWeightedTroopsInfo(weightedTroopsInfo);
            }
            catch
            {
            }
        }

        private static void ReplaceTroopsInfoItemIfNeeded(
            MapInfoVM mapInfoVm,
            MapInfoItemVM existingTroopsInfo,
            MapInfoItemVM weightedTroopsInfo)
        {
            if (!ReferenceEquals(existingTroopsInfo, weightedTroopsInfo))
            {
                weightedTroopsInfo.VisualId = existingTroopsInfo.VisualId;

                for (int i = 0; i < mapInfoVm.PrimaryInfoItems.Count; i++)
                {
                    MapInfoItemVM item = mapInfoVm.PrimaryInfoItems[i];
                    if (item != null && item.ItemId == "troops" && !ReferenceEquals(item, weightedTroopsInfo))
                    {
                        mapInfoVm.PrimaryInfoItems.RemoveAt(i);
                        mapInfoVm.PrimaryInfoItems.Insert(i, weightedTroopsInfo);
                        break;
                    }
                }

                TroopsInfoField.SetValue(mapInfoVm, weightedTroopsInfo);
            }
        }

        private static void UpdateWeightedTroopsInfo(MapInfoItemVM troopsInfo)
        {
            int weightedTotal = GetWeightedMainPartyTotal();

            troopsInfo.HasWarning = false;
            troopsInfo.IntValue = weightedTotal;
            troopsInfo.Value = weightedTotal.ToString();
        }

        private static List<TooltipProperty> GetWeightedTroopsHintText()
        {
            int weightedTotal = GetWeightedMainPartyTotal();
            int weightedHealthy = GetWeightedMainPartyHealthy();
            int weightedWounded = Math.Max(0, weightedTotal - weightedHealthy);
            int limit = MobileParty.MainParty != null
                ? MWRPartyWeightHelper.GetPartyWeightLimit(MobileParty.MainParty)
                : 0;

            List<TooltipProperty> tooltip =
            new List<TooltipProperty>
            {
                new TooltipProperty(
                    new TextObject("{=mwr_weighted_troops_title}Troops").ToString(),
                    weightedTotal.ToString(),
                    0,
                    false,
                    TooltipProperty.TooltipPropertyFlags.Title)
            };

            if (weightedWounded > 0)
            {
                tooltip.Add(new TooltipProperty(
                    new TextObject("{=mwr_weighted_troops_healthy}Healthy").ToString(),
                    weightedHealthy.ToString(),
                    0,
                    false,
                    TooltipProperty.TooltipPropertyFlags.None));
                tooltip.Add(new TooltipProperty(
                    new TextObject("{=mwr_weighted_troops_wounded}Wounded").ToString(),
                    weightedWounded.ToString(),
                    0,
                    false,
                    TooltipProperty.TooltipPropertyFlags.None));
            }

            if (limit > 0)
            {
                tooltip.Add(new TooltipProperty(
                    new TextObject("{=mwr_weighted_troops_capacity}Party Capacity").ToString(),
                    limit.ToString(),
                    0,
                    false,
                    TooltipProperty.TooltipPropertyFlags.None));
            }

            return tooltip;
        }

        private static int GetWeightedMainPartyTotal()
        {
            PartyBase mainParty = PartyBase.MainParty;
            return mainParty != null
                ? MWRPartyWeightHelper.GetWeightedPartyNumberOfAllMembers(mainParty)
                : 0;
        }

        private static int GetWeightedMainPartyHealthy()
        {
            TroopRoster roster = MobileParty.MainParty?.MemberRoster;
            return roster != null
                ? MWRPartyWeightHelper.GetWeightedRosterTotalHealthyCount(roster)
                : 0;
        }
    }
}
