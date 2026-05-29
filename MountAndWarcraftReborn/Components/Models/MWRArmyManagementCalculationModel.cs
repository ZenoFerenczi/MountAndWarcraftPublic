using System;
using System.Collections.Generic;
using MWRMode;
using MountAndWarcraftReborn.Patches.TroopWeight;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Localization;

namespace MountAndWarcraftReborn.Components.Models
{
    public class MWRArmyManagementCalculationModel : ArmyManagementCalculationModel
    {
        private const float UndeadDailyCohesionBonus = 1f;
        private const float MaxWeightInflationDailyCohesionBonus = 1f;

        private readonly ArmyManagementCalculationModel _previousModel;

        public MWRArmyManagementCalculationModel(ArmyManagementCalculationModel previousModel)
        {
            _previousModel = previousModel;
        }

        public override float AIMobilePartySizeRatioToCallToArmy => _previousModel.AIMobilePartySizeRatioToCallToArmy;

        public override float PlayerMobilePartySizeRatioToCallToArmy => _previousModel.PlayerMobilePartySizeRatioToCallToArmy;

        public override float MinimumNeededFoodInDaysToCallToArmy => _previousModel.MinimumNeededFoodInDaysToCallToArmy;

        public override float MaximumDistanceToCallToArmy => _previousModel.MaximumDistanceToCallToArmy;

        public override int InfluenceValuePerGold => _previousModel.InfluenceValuePerGold;

        public override int AverageCallToArmyCost => _previousModel.AverageCallToArmyCost;

        public override int CohesionThresholdForDispersion => _previousModel.CohesionThresholdForDispersion;

        public override float MaximumWaitTime => _previousModel.MaximumWaitTime;

        public override bool CanPlayerCreateArmy(out TextObject explanation)
        {
            return _previousModel.CanPlayerCreateArmy(out explanation);
        }

        public override int CalculatePartyInfluenceCost(MobileParty armyLeaderParty, MobileParty party)
        {
            return _previousModel.CalculatePartyInfluenceCost(armyLeaderParty, party);
        }

        public override float DailyBeingAtArmyInfluenceAward(MobileParty armyMemberParty)
        {
            return _previousModel.DailyBeingAtArmyInfluenceAward(armyMemberParty);
        }

        public override List<MobileParty> GetMobilePartiesToCallToArmy(MobileParty leaderParty)
        {
            return _previousModel.GetMobilePartiesToCallToArmy(leaderParty);
        }

        public override int CalculateTotalInfluenceCost(Army army, float percentage)
        {
            return _previousModel.CalculateTotalInfluenceCost(army, percentage);
        }

        public override float GetPartySizeScore(MobileParty party)
        {
            float value;
            using (TroopWeightPatchGuards.SuppressGlobalWeight())
            {
                value = _previousModel.GetPartySizeScore(party);
            }

            if (party?.Party == null)
                return value;

            int rawCount = MWRPartyWeightHelper.GetRawPartyNumberOfAllMembers(party.Party);
            int weightedCount = MWRPartyWeightHelper.GetWeightedPartyNumberOfAllMembers(party.Party);
            if (rawCount <= 0 || weightedCount <= rawCount)
                return value;

            return value * ((float)rawCount / weightedCount);
        }

        public override bool CheckPartyEligibility(MobileParty party, out TextObject explanation)
        {
            return _previousModel.CheckPartyEligibility(party, out explanation);
        }

        public override int GetPartyRelation(Hero hero)
        {
            return _previousModel.GetPartyRelation(hero);
        }

        public override ExplainedNumber CalculateDailyCohesionChange(Army army, bool includeDescriptions = false)
        {
            ExplainedNumber result = _previousModel.CalculateDailyCohesionChange(army, includeDescriptions);
            if (army == null)
                return result;

            float compensation = 0f;

            if (CultureRules.GetPartyRecruitCultureId(army.LeaderParty) == "nord")
            {
                compensation += UndeadDailyCohesionBonus;
            }

            compensation += GetWeightInflationDailyCohesionBonus(army);

            if (compensation > 0f)
            {
                result.Add(
                    compensation,
                    includeDescriptions
                        ? new TextObject("{=mwr_weighted_army_cohesion}Racial army cohesion compensation")
                        : null);
            }

            return result;
        }

        public override int CalculateNewCohesion(Army army, PartyBase newParty, int calculatedCohesion, int sign)
        {
            return _previousModel.CalculateNewCohesion(army, newParty, calculatedCohesion, sign);
        }

        public override int GetCohesionBoostInfluenceCost(Army army, int percentageToBoost = 100)
        {
            return _previousModel.GetCohesionBoostInfluenceCost(army, percentageToBoost);
        }

        private static float GetWeightInflationDailyCohesionBonus(Army army)
        {
            if (army == null)
                return 0f;

            int weightedTotal = 0;
            int rawTotal = 0;

            foreach (MobileParty party in army.Parties)
            {
                if (party?.Party == null)
                    continue;

                weightedTotal += MWRPartyWeightHelper.GetWeightedPartyNumberOfAllMembers(party.Party);
                rawTotal += MWRPartyWeightHelper.GetRawPartyNumberOfAllMembers(party.Party);
            }

            int inflatedCount = Math.Max(0, weightedTotal - rawTotal);
            if (inflatedCount <= 0)
                return 0f;

            return Math.Min(MaxWeightInflationDailyCohesionBonus, inflatedCount / 150f);
        }
    }
}
