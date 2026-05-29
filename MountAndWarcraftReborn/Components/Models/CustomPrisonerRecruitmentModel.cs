using MountAndWarcraftReborn.Patches.TroopWeight;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;

namespace MWRMode.Components.Models
{
    public class MWRPrisonerRecruitmentCalculationModel : DefaultPrisonerRecruitmentCalculationModel
    {
        private static bool IsBlockedPrisoner(PartyBase party, CharacterObject character)
        {
            if (character == null)
                return false;

            if (CultureRules.IsPrisonerRecruitmentBlocked(character))
                return true;

            if (party?.MobileParty != null && !CultureRules.IsTroopCultureAllowedForParty(character, party.MobileParty))
                return true;

            if (party?.MobileParty != null && !MWRPartyWeightHelper.CanAddTroop(party.MobileParty, character, 1))
                return true;

            return false;
        }

        public override int GetConformityNeededToRecruitPrisoner(CharacterObject character)
        {
            // No party info here, so only use the hard lore block
            if (CultureRules.IsPrisonerRecruitmentBlocked(character))
                return int.MaxValue;

            return base.GetConformityNeededToRecruitPrisoner(character);
        }

        public override ExplainedNumber GetConformityChangePerHour(PartyBase party, CharacterObject character)
        {
            if (IsBlockedPrisoner(party, character))
                return new ExplainedNumber(0f);

            return base.GetConformityChangePerHour(party, character);
        }

        public override bool IsPrisonerRecruitable(PartyBase party, CharacterObject character, out int conformityNeeded)
        {
            if (IsBlockedPrisoner(party, character))
            {
                conformityNeeded = int.MaxValue;
                return false;
            }

            return base.IsPrisonerRecruitable(party, character, out conformityNeeded);
        }

        public override int CalculateRecruitableNumber(PartyBase party, CharacterObject character)
        {
            if (IsBlockedPrisoner(party, character))
                return 0;

            return base.CalculateRecruitableNumber(party, character);
        }

        public override int GetPrisonerRecruitmentMoraleEffect(PartyBase party, CharacterObject character, int num)
        {
            if (IsBlockedPrisoner(party, character))
                return 0;

            return base.GetPrisonerRecruitmentMoraleEffect(party, character, num);
        }
    }
}