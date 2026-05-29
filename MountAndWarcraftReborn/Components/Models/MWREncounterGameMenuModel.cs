#nullable enable
using Helpers;
using MountAndWarcraftReborn.Portals;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;

namespace MountAndWarcraftReborn.Components.Models
{
    public class MWREncounterGameMenuModel : DefaultEncounterGameMenuModel
    {
        public override string GetEncounterMenu(PartyBase attackerParty, PartyBase defenderParty, out bool startBattle, out bool joinBattle)
        {
            var settlement = MapEventHelper.GetEncounteredPartyBase(attackerParty, defenderParty).Settlement;
            if (settlement != null && settlement.SettlementComponent is MWRPortalSiteComponent)
            {
                startBattle = false;
                joinBattle = false;
                return "mwr_portal_site_menu";
            }

            return base.GetEncounterMenu(attackerParty, defenderParty, out startBattle, out joinBattle);
        }
    }
}
