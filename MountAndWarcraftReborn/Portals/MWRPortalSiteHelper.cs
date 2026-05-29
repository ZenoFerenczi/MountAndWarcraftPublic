#nullable enable
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;

namespace MountAndWarcraftReborn.Portals
{
    public static class MWRPortalSiteHelper
    {
        public static bool IsPortalSite(Settlement? settlement) => settlement?.SettlementComponent is MWRPortalSiteComponent;

        public static IEnumerable<Settlement> GetAllPortalSites()
        {
            return Settlement.All.Where(IsPortalSite);
        }

        public static int CountNonPortalSettlements(IEnumerable<Settlement> settlements)
        {
            return settlements.Count(settlement => !IsPortalSite(settlement));
        }
    }
}
