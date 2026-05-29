using TaleWorlds.CampaignSystem;
using MountAndWarcraftReborn.Behaviors;

namespace MountAndWarcraftReborn.Extensions
{
    public static class MWRHirelingHeroExtensions
    {
        public static bool IsEnlisted(this Hero hero)
        {
            if (hero != Hero.MainHero || Campaign.Current == null)
            {
                return false;
            }

            return Campaign.Current.GetCampaignBehavior<MWRHirelingCampaignBehavior>()?.IsEnlisted() == true;
        }

        public static Hero GetEnlistingHero(this Hero hero)
        {
            return Campaign.Current?.GetCampaignBehavior<MWRHirelingCampaignBehavior>()?.EnlistingLord;
        }
    }
}
