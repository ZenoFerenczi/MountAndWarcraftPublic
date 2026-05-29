using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace MWRMode.Components.Models
{
    public class MWRPartyWageModel : DefaultPartyWageModel
    {
        public override int GetCharacterWage(CharacterObject character)
        {
            if (character == null)
                return 1;

            int wage;

            switch (character.Tier)
            {
                case 0: wage = 1; break;
                case 1: wage = 2; break;
                case 2: wage = 3; break;
                case 3: wage = 5; break;
                case 4: wage = 8; break;
                case 5: wage = 14; break;
                case 6: wage = 20; break;
                case 7: wage = 30; break;
                case 8: wage = 40; break;
                case 9: wage = 80; break;
                case 10: wage = 150; break;
                default: wage = 40; break;
            }

            if (CultureRules.HasNearFreeUpkeep(character))
                wage = 1;
            else if (CultureRules.HasDoubleWage(character))
                wage *= 2;
            else if (CultureRules.HasMediumWageIncrease(character))
                wage = (int)MathF.Round(wage * 1.5f);

            return Math.Max(1, wage);
        }

        public override ExplainedNumber GetTroopRecruitmentCost(CharacterObject troop, Hero buyerHero, bool withoutItemCost = false)
        {
            if (troop == null)
                return new ExplainedNumber(1);

            int cost;

            switch (troop.Tier)
            {
                case 0: cost = 10; break;
                case 1: cost = 25; break;
                case 2: cost = 50; break;
                case 3: cost = 100; break;
                case 4: cost = 200; break;
                case 5: cost = 500; break;
                case 6: cost = 800; break;
                case 7: cost = 1000; break;
                case 8: cost = 1500; break;
                case 9: cost = 5000; break;
                case 10: cost = 10000; break;
                default: cost = 1000; break;
            }

            if (CultureRules.HasNearFreeUpkeep(troop))
                cost = 1;
            else if (CultureRules.HasHighRecruitmentIncrease(troop))
                cost = (int)MathF.Round(cost * 2f);
            else if (CultureRules.HasMediumRecruitmentIncrease(troop))
                cost = (int)MathF.Round(cost * 1.5f);

            return new ExplainedNumber(Math.Max(1, cost));
        }

        public override ExplainedNumber GetTotalWage(MobileParty mobileParty, TroopRoster troopRoster, bool includeDescriptions = false)
        {
            ExplainedNumber total = base.GetTotalWage(mobileParty, troopRoster, includeDescriptions);

            if (mobileParty == null || !mobileParty.IsMainParty || troopRoster == null)
                return total;

            int refund = 0;

            for (int i = 0; i < troopRoster.Count; i++)
            {
                TroopRosterElement element = troopRoster.GetElementCopyAtIndex(i);
                CharacterObject troop = element.Character;

                if (CultureRules.HasNearFreeUpkeep(troop))
                {
                    refund += element.Number; // wage is forced to 1 each
                }
            }

            if (refund > 0)
            {
                total.Add(-refund, includeDescriptions ? new TextObject("{=mwr_undead_free}Undead") : null);
                total.LimitMin(0f);
            }

            return total;
        }
    }
}