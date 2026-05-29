using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;

namespace MWRMode.Components.Models
{
    public class MWRCombatSimulationModel : DefaultCombatSimulationModel
    {
        // Global tier bonuses for all troops
        private const float Tier7BonusFactor = 0.25f;
        private const float Tier8BonusFactor = 0.40f;
        private const float Tier9BonusFactor = 1.0f;
        private const float Tier10BonusFactor = 2.0f;

        // Culture bonuses for all troop tiers
        private const float MediumCultureBonusFactor = 0.20f;
        private const float HighCultureBonusFactor = 0.50f;

        // Lord / hero bonuses by culture
        private const float MediumLordBonusFactor = 2.00f; // +200%
        private const float HighLordBonusFactor = 3.00f;   // +300%

        public override ExplainedNumber SimulateHit(
            CharacterObject strikerTroop,
            CharacterObject struckTroop,
            PartyBase strikerParty,
            PartyBase struckParty,
            float strikerAdvantage,
            MapEvent battle,
            float strikerSideMorale,
            float struckSideMorale)
        {
            ExplainedNumber result = base.SimulateHit(
                strikerTroop,
                struckTroop,
                strikerParty,
                struckParty,
                strikerAdvantage,
                battle,
                strikerSideMorale,
                struckSideMorale);

            float bonusFactor = GetAutoresolveBonusFactor(strikerTroop);

            if (bonusFactor != 0f)
            {
                result.AddFactor(bonusFactor);
            }

            float defenseReduction = CultureRules.GetSpecialCharacterAutoresolveDefenseReductionFactor(struckTroop);

            if (defenseReduction > 0f)
            {
                // 0.50f means reduce incoming damage by 50%
                result.AddFactor(-defenseReduction);
            }
            return result;
        }

        private static float GetAutoresolveBonusFactor(CharacterObject troop)
        {
            if (troop == null)
                return 0f;

            float totalBonus = 0f;

            // 1. Global tier bonus
            if (!troop.IsHero)
            {
                switch (troop.Tier)
                {
                    case 7:
                        totalBonus += Tier7BonusFactor;
                        break;
                    case 8:
                        totalBonus += Tier8BonusFactor;
                        break;
                    case 9:
                        totalBonus += Tier9BonusFactor;
                        break;
                    case 10:
                        totalBonus += Tier9BonusFactor;
                        break;
                }
            }

            // 2. Culture bonus for all troops
            if (CultureRules.HasHighAutoresolveBonus(troop))
                totalBonus += HighCultureBonusFactor;
            else if (CultureRules.HasMediumAutoresolveBonus(troop))
                totalBonus += MediumCultureBonusFactor;

            // 3. Separate lord / hero bonus by culture
            if (troop.IsHero)
            {
                if (CultureRules.HasHighLordAutoresolveBonus(troop))
                    totalBonus += HighLordBonusFactor;
                else if (CultureRules.HasMediumLordAutoresolveBonus(troop))
                    totalBonus += MediumLordBonusFactor;

                // Special lord ID attack bonus on top
                totalBonus += CultureRules.GetSpecialCharacterAutoresolveAttackBonusFactor(troop);
            }

            return totalBonus;
        }

    }
}