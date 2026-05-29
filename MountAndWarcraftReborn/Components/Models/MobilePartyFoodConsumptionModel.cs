using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Localization;

namespace MWRMode.Components.Models
{
    public class MWRMobilePartyFoodConsumptionModel : DefaultMobilePartyFoodConsumptionModel
    {
        public override ExplainedNumber CalculateDailyFoodConsumptionf(MobileParty party, ExplainedNumber baseConsumption)
        {
            ExplainedNumber result = base.CalculateDailyFoodConsumptionf(party, baseConsumption);

            if (party == null || party.MemberRoster == null || party.MemberRoster.TotalManCount <= 0)
                return result;

            int totalTroops = 0;
            int noFoodTroops = 0;
            int extraFoodTroops = 0;

            TroopRoster roster = party.MemberRoster;

            for (int i = 0; i < roster.Count; i++)
            {
                TroopRosterElement element = roster.GetElementCopyAtIndex(i);
                CharacterObject troop = element.Character;
                int count = element.Number;

                if (troop == null || count <= 0)
                    continue;

                totalTroops += count;

                if (CultureRules.HasNoFoodConsumption(troop))
                    noFoodTroops += count;

                // Extra appetite is player-party only
                if (party.IsMainParty && CultureRules.HasFiftyPercentMoreFoodConsumption(troop))
                    extraFoodTroops += count;
            }

            if (totalTroops <= 0)
                return result;

            float currentFood = result.ResultNumber;

            // In Bannerlord food consumption is typically NEGATIVE.
            // So "less food consumed" means adding a POSITIVE amount toward zero.
            if (noFoodTroops > 0 && currentFood < 0f)
            {
                float noFoodRatio = (float)noFoodTroops / totalTroops;
                float reduction = (-currentFood) * noFoodRatio;

                result.Add(reduction, new TextObject("{=mwr_no_food}Undead do not consume food"));
            }

            // "More food consumed" means adding a NEGATIVE amount.
            if (party.IsMainParty && extraFoodTroops > 0 && result.ResultNumber < 0f)
            {
                float currentAbsFood = -result.ResultNumber;
                float perTroopFood = currentAbsFood / totalTroops;
                float extraFood = perTroopFood * extraFoodTroops * 0.5f;

                result.Add(-extraFood, new TextObject("{=mwr_big_appetite}Big appetite"));
            }

            // Fully no-food party can reach 0, but never go above 0
            if (result.ResultNumber > 0f)
            {
                result.Add(-result.ResultNumber);
            }

            return result;
        }
    }
}