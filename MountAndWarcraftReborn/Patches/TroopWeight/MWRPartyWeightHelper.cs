using MWRMode;
using TaleWorlds.CampaignSystem;
using MountAndWarcraftReborn.Behaviors;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;

namespace MountAndWarcraftReborn.Patches.TroopWeight
{
    public static class MWRPartyWeightHelper
    {
        public static int GetWeightedRosterTotalManCount(TroopRoster roster)
        {
            return MWRTroopWeightService.GetWeightedRosterTotalManCount(roster);
        }

        public static int GetWeightedRosterTotalHealthyCount(TroopRoster roster)
        {
            return MWRTroopWeightService.GetWeightedRosterTotalHealthyCount(roster);
        }

        public static int GetWeightedPartyNumberOfAllMembers(PartyBase party)
        {
            return MWRTroopWeightService.GetWeightedPartyNumberOfAllMembers(party);
        }

        public static int GetWeightedPartyNumberOfRegularMembers(PartyBase party)
        {
            return MWRTroopWeightService.GetWeightedPartyNumberOfRegularMembers(party);
        }

        public static int GetRawRosterTotalManCount(TroopRoster roster)
        {
            return MWRTroopWeightService.GetRawRosterTotalManCount(roster);
        }

        public static int GetRawPartyNumberOfAllMembers(PartyBase party)
        {
            return MWRTroopWeightService.GetRawPartyNumberOfAllMembers(party);
        }

        public static int GetPartyWeightLimit(MobileParty party)
        {
            if (party?.Party == null)
                return 0;

            return (int)Campaign.Current.Models.PartySizeLimitModel
                .GetPartyMemberSizeLimit(party.Party)
                .ResultNumber;
        }

        public static int GetEffectivePartySize(MobileParty party)
        {
            if (party?.MemberRoster == null)
                return 0;

            return MWRTroopWeightService.GetWeightedRosterTotalManCount(
                party.MemberRoster,
                GetEnlistedWeightIgnoredCharacter(party));
        }

        public static bool CanAddTroop(MobileParty party, CharacterObject troop, int number)
        {
            if (party == null || troop == null || number <= 0)
                return true;

            int currentWeight = GetEffectivePartySize(party);
            int limit = GetPartyWeightLimit(party);
            int addedWeight = CultureRules.GetWeightedPartySizeCost(troop) * number;

            return currentWeight + addedWeight <= limit;
        }

        private static CharacterObject? GetEnlistedWeightIgnoredCharacter(MobileParty party)
        {
            if (party == null || Hero.MainHero?.CharacterObject == null)
                return null;

            MWRHirelingCampaignBehavior hirelingBehavior = Campaign.Current?.GetCampaignBehavior<MWRHirelingCampaignBehavior>();
            if (hirelingBehavior?.IsEnlisted() != true)
                return null;

            return hirelingBehavior.EnlistingLord?.PartyBelongedTo == party
                ? Hero.MainHero.CharacterObject
                : null;
        }
    }
}
