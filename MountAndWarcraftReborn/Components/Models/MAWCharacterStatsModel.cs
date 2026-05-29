using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace MWRMode.Components.Models
{
    public class MWRCharacterStatsModel : DefaultCharacterStatsModel
    {
        private const int SmallLordHealthBonus = 100;
        private const int MediumLordHealthBonus = 250;
        private const int BigLordHealthBonus = 500;

        public override int MaxCharacterTier => 10;

        public override ExplainedNumber MaxHitpoints(CharacterObject character, bool includeDescriptions = false)
        {
            ExplainedNumber result = base.MaxHitpoints(character, includeDescriptions);

            if (character == null)
                return result;

            // 1. Lord / hero culture presets
            if (character.IsHero)
            {
                if (CultureRules.HasBigLordHealthBonus(character))
                {
                    result.Add(BigLordHealthBonus,
                        includeDescriptions ? new TextObject("{=mwr_big_lord_hp}Small Racial Lord Health Bonus") : null);
                }
                else if (CultureRules.HasMediumLordHealthBonus(character))
                {
                    result.Add(MediumLordHealthBonus,
                        includeDescriptions ? new TextObject("{=mwr_medium_lord_hp}Medium Racial Lord Health Bonus") : null);
                }
                else if (CultureRules.HasSmallLordHealthBonus(character))
                {
                    result.Add(SmallLordHealthBonus,
                        includeDescriptions ? new TextObject("{=mwr_small_lord_hp}Small Racial Lord Health Bonus") : null);
                }
            }

            // 2. Race bonus
            int raceBonus = CultureRules.GetFlatHealthBonusByRace(character);
            if (raceBonus != 0)
            {
                result.Add(raceBonus,
                    includeDescriptions ? new TextObject("{=mwr_race_hp}Racial vitality") : null);
            }

            // 3. Exact troop ID bonus
            int specialTroopBonus = CultureRules.GetSpecialTroopFlatHealthBonus(character);
            if (specialTroopBonus != 0)
            {
                result.Add(specialTroopBonus,
                    includeDescriptions ? new TextObject("{=mwr_special_troop_hp}Special unit vitality") : null);
            }

            return result;
        }
    }
}