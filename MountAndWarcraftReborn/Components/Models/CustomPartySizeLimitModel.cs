using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using MountAndWarcraftReborn.Behaviors;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Localization;

namespace MountAndWarcraftReborn.Components.Models
{
    public class CustomPartySizeLimitModel : DefaultPartySizeLimitModel
    {
        private static readonly Dictionary<string, int> CulturePartySizeBonus = new Dictionary<string, int>
        {
            { "empire", 50 },
            { "nord", 300 },
            { "vlandia", 50 },
            { "battania", 30 },
            { "khuzait", 100 },
            { "aserai", 150 },
            { "sturgia", 20 }
        };

        public override ExplainedNumber GetPartyMemberSizeLimit(PartyBase party, bool includeDescriptions = false)
        {
            ExplainedNumber result = base.GetPartyMemberSizeLimit(party, includeDescriptions);

            if (party?.MobileParty == null)
                return result;

            if (party.MobileParty.PartyComponent is MountAndWarcraftReborn.Portals.MWRPortalRaidingPartyComponent portalParty)
            {
                int desiredSize = portalParty.DesiredPartySize;
                int currentLimit = (int)result.ResultNumber;
                if (desiredSize > currentLimit)
                {
                    result.Add(desiredSize - currentLimit, new TextObject("{=mwr_portal_party_size}Portal raiding party size"));
                }

                return result;
            }

            if (party.MobileParty.PartyComponent is PatrolPartyComponent &&
                TryGetPatrolTemplateMaxCount(party.MobileParty, out int patrolTemplateMaxCount))
            {
                int currentLimit = (int)result.ResultNumber;
                if (patrolTemplateMaxCount > currentLimit)
                {
                    result.Add(
                        patrolTemplateMaxCount - currentLimit,
                        new TextObject("{=mwr_patrol_party_size}Patrol template size"));
                }
            }

            if (party.LeaderHero != null || party.MobileParty.IsGarrison)
            {
                int bonus = GetCultureBonus(party);
                if (bonus != 0)
                {
                    result.Add(bonus, new TextObject("{=mwr_culture_party_bonus}Racial culture party size bonus"));
                }
            }

            return result;
        }

        public override TroopRoster FindAppropriateInitialRosterForMobileParty(MobileParty party, PartyTemplateObject partyTemplate)
        {
            if (party?.PartyComponent is not PatrolPartyComponent || partyTemplate == null)
            {
                return base.FindAppropriateInitialRosterForMobileParty(party, partyTemplate);
            }

            int templateMaxCount = GetTemplateMaxCount(partyTemplate);
            if (templateMaxCount > 0)
            {
                RegisterPatrolTemplateMaxCount(party, templateMaxCount);
            }

            TroopRoster roster = TroopRoster.CreateDummyTroopRoster();
            foreach (PartyTemplateStack stack in partyTemplate.Stacks)
            {
                if (stack.Character != null && stack.MaxValue > 0)
                {
                    roster.AddToCounts(stack.Character, stack.MaxValue);
                }
            }

            return roster.Count > 0
                ? roster
                : base.FindAppropriateInitialRosterForMobileParty(party, partyTemplate);
        }

        public override int GetAssumedPartySizeForLordParty(Hero leaderHero, IFaction partyMapFaction, Clan actualClan)
        {
            int result = base.GetAssumedPartySizeForLordParty(leaderHero, partyMapFaction, actualClan);
            int bonus = GetCultureBonus(leaderHero, partyMapFaction, actualClan);

            if (bonus != 0)
            {
                result += bonus;
            }

            return result;
        }

        private static int GetCultureBonus(PartyBase party)
        {
            string cultureId = GetPartyCultureId(party);
            if (string.IsNullOrWhiteSpace(cultureId))
                return 0;

            if (CulturePartySizeBonus.TryGetValue(cultureId, out int bonus))
                return bonus;

            return 0;
        }

        private static int GetCultureBonus(Hero leaderHero, IFaction partyMapFaction, Clan actualClan)
        {
            string cultureId = GetPartyCultureId(leaderHero, partyMapFaction, actualClan);
            if (string.IsNullOrWhiteSpace(cultureId))
                return 0;

            if (CulturePartySizeBonus.TryGetValue(cultureId, out int bonus))
                return bonus;

            return 0;
        }

        private static string GetPartyCultureId(PartyBase party)
        {
            if (party?.LeaderHero?.Culture != null)
                return party.LeaderHero.Culture.StringId;

            if (party?.MapFaction?.Culture != null)
                return party.MapFaction.Culture.StringId;

            if (party?.MobileParty?.Party?.Culture != null)
                return party.MobileParty.Party.Culture.StringId;

            return string.Empty;
        }

        private static string GetPartyCultureId(Hero leaderHero, IFaction partyMapFaction, Clan actualClan)
        {
            if (leaderHero?.Culture != null)
                return leaderHero.Culture.StringId;

            if (actualClan?.Culture != null)
                return actualClan.Culture.StringId;

            if (partyMapFaction?.Culture != null)
                return partyMapFaction.Culture.StringId;

            return string.Empty;
        }

        private static bool TryGetPatrolTemplateMaxCount(MobileParty party, out int templateMaxCount)
        {
            templateMaxCount = 0;
            return Campaign.Current?.GetCampaignBehavior<MWRPatrolTemplateSizeCampaignBehavior>()
                ?.TryGetTemplateMaxCount(party, out templateMaxCount) == true;
        }

        private static void RegisterPatrolTemplateMaxCount(MobileParty party, int templateMaxCount)
        {
            Campaign.Current?.GetCampaignBehavior<MWRPatrolTemplateSizeCampaignBehavior>()
                ?.RegisterTemplateMaxCount(party, templateMaxCount);
        }

        private static int GetTemplateMaxCount(PartyTemplateObject partyTemplate)
        {
            int total = 0;
            foreach (PartyTemplateStack stack in partyTemplate.Stacks)
            {
                total += stack.MaxValue;
            }

            return total;
        }
    }
}
