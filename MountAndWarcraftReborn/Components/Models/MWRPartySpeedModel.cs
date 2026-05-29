using MWRMode;
using MountAndWarcraftReborn.Patches.TroopWeight;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Localization;

namespace MountAndWarcraftReborn.Components.Models
{
    public class MWRPartySpeedModel : PartySpeedModel
    {
        private const float UndeadMapSpeedBonus = 1.2f;

        private const float ElfMapSpeedBonus = 0.5f; 

        private readonly PartySpeedModel _previousModel;

        public MWRPartySpeedModel(PartySpeedModel previousModel)
        {
            _previousModel = previousModel;
        }

        public override float BaseSpeed => _previousModel.BaseSpeed;

        public override float MinimumSpeed => _previousModel.MinimumSpeed;

        public override ExplainedNumber CalculateBaseSpeed(
            MobileParty mobileParty,
            bool includeDescriptions = false,
            int totalManCount = 0,
            int totalHorseCount = 0)
        {
            int correctedManCount = totalManCount;
            if (mobileParty?.MemberRoster != null)
            {
                correctedManCount = MWRPartyWeightHelper.GetRawRosterTotalManCount(mobileParty.MemberRoster);
            }

            using (TroopWeightPatchGuards.SuppressGlobalWeight())
            {
                return _previousModel.CalculateBaseSpeed(
                    mobileParty,
                    includeDescriptions,
                    correctedManCount,
                    totalHorseCount);
            }
        }

        public override ExplainedNumber CalculateFinalSpeed(MobileParty mobileParty, ExplainedNumber finalSpeed)
        {
            ExplainedNumber result;
            using (TroopWeightPatchGuards.SuppressGlobalWeight())
            {
                result = _previousModel.CalculateFinalSpeed(mobileParty, finalSpeed);
            }

            if (mobileParty == null)
                return result;

            if (CultureRules.GetPartyRecruitCultureId(mobileParty) == "nord")
            {
                result.Add(
                    UndeadMapSpeedBonus,
                    true
                        ? new TextObject("{=mwr_undead_party_speed}Undead relentless march")
                        : null);
            }

            if (CultureRules.GetPartyRecruitCultureId(mobileParty) == "battania")
            {
                result.Add(
                    ElfMapSpeedBonus,
                    true
                        ? new TextObject("{=mwr_elven_party_speed}Elven Agility")
                        : null);
            }

            return result;
        }
    }
}
