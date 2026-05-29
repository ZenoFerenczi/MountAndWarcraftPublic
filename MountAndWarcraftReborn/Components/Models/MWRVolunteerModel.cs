using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using MountAndWarcraftReborn.Patches.TroopWeight;

namespace MWRMode.Components.Models
{
    public class MWRVolunteerModel : DefaultVolunteerModel
    {
        public override int MaximumIndexHeroCanRecruitFromHero(Hero buyerHero, Hero sellerHero, int useValueAsRelation = -101)
        {
            int maxIndex = base.MaximumIndexHeroCanRecruitFromHero(buyerHero, sellerHero, useValueAsRelation);

            if (maxIndex < 0 || buyerHero == null || sellerHero == null)
                return maxIndex;

            CharacterObject? sampleTroop = GetRecruitCultureSampleTroop(sellerHero);
            if (sampleTroop == null)
                return maxIndex;

            if (!CultureRules.IsTroopCultureAllowedForHero(sampleTroop, buyerHero))
                return -1;

            if (buyerHero.PartyBelongedTo != null && !MWRPartyWeightHelper.CanAddTroop(buyerHero.PartyBelongedTo, sampleTroop, 1))
                return -1;

            return maxIndex;
        }

        public override int MaximumIndexGarrisonCanRecruitFromHero(Settlement settlement, Hero sellerHero)
        {
            int maxIndex = base.MaximumIndexGarrisonCanRecruitFromHero(settlement, sellerHero);

            if (maxIndex < 0 || settlement == null || sellerHero == null)
                return maxIndex;

            CharacterObject? sampleTroop = GetRecruitCultureSampleTroop(sellerHero);
            if (sampleTroop == null)
                return maxIndex;

            string ownerCultureId =
                settlement.OwnerClan?.Culture?.StringId ??
                settlement.MapFaction?.Culture?.StringId ??
                string.Empty;

            if (!CultureRules.IsTroopCultureAllowedForCulture(sampleTroop, ownerCultureId))
                return -1;

            return maxIndex;
        }

        private static CharacterObject? GetRecruitCultureSampleTroop(Hero sellerHero)
        {
            return sellerHero.CharacterObject;
        }
    }
}