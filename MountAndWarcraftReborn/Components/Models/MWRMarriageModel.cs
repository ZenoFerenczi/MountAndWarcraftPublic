using MountAndWarcraftReborn.Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;

namespace MountAndWarcraftReborn.Components.Models
{
    public class MWRMarriageModel : DefaultMarriageModel
    {
        public override bool IsCoupleSuitableForMarriage(Hero firstHero, Hero secondHero)
        {
            return base.IsCoupleSuitableForMarriage(firstHero, secondHero) &&
                   MWRMarriageRules.IsCoupleSuitableForMarriage(firstHero, secondHero);
        }

        public override bool IsSuitableForMarriage(Hero maidenOrSuitor)
        {
            return base.IsSuitableForMarriage(maidenOrSuitor) &&
                   MWRMarriageRules.IsHeroSuitableForMarriage(maidenOrSuitor);
        }
    }
}
