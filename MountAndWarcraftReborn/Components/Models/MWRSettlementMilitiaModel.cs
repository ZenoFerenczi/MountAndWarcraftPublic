using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;

namespace MWRMode.Components.Models
{
    public class MWRSettlementMilitiaModel : DefaultSettlementMilitiaModel
    {
        public override ExplainedNumber CalculateMilitiaChange(Settlement settlement, bool includeDescriptions = false)
        {
            ExplainedNumber result = base.CalculateMilitiaChange(settlement, includeDescriptions);

            if (settlement == null || settlement.OwnerClan == null)
                return result;

            string ownerCultureId = settlement.OwnerClan.Culture?.StringId ?? string.Empty;

            // 1. Flat militia bonus from CURRENT OWNER culture
            float cultureBonus = CultureRules.GetFlatMilitiaDailyBonusForCulture(ownerCultureId);
            if (cultureBonus != 0f)
            {
                result.Add(
                    cultureBonus,
                    includeDescriptions ? new TextObject("{=mwr_militia_culture_bonus}Racial militia tradition") : null
                );
            }

            // 2. Native stronghold bonus only if CURRENT owner matches ORIGINAL culture map
            if (CultureRules.IsNativeOwnerForMilitiaBonus(settlement))
            {
                float nativeBonus = CultureRules.GetNativeSettlementMilitiaDailyBonus(settlement.StringId);
                if (nativeBonus != 0f)
                {
                    result.Add(
                        nativeBonus,
                        includeDescriptions ? new TextObject("{=mwr_native_stronghold_bonus}Racial stronghold bonus") : null
                    );
                }
            }

            return result;
        }

        public override int MilitiaToSpawnAfterSiege(Town town)
        {
            int result = base.MilitiaToSpawnAfterSiege(town);

            if (town?.Settlement == null)
                return result;

            if (CultureRules.IsNativeOwnerForMilitiaBonus(town.Settlement))
            {
                result += CultureRules.GetNativeSettlementPostSiegeMilitiaBonus(town.Settlement.StringId);
            }

            return result;
        }
    }
}