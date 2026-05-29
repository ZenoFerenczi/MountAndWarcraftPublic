using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;
using MountAndWarcraftReborn.Patches.TroopWeight;

namespace MountAndWarcraftReborn.Behaviors
{
    public class MWRPartyWeightCleanupBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickPartyEvent.AddNonSerializedListener(this, OnDailyTickParty);
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        private void OnDailyTickParty(MobileParty party)
        {
            if (party == null || party.IsMainParty || party.MemberRoster == null)
                return;

            int partyWeightLimit = MWRPartyWeightHelper.GetPartyWeightLimit(party);
            if (partyWeightLimit <= 0)
                return;

            // Never mutate live encounter rosters from the campaign daily tick.
            // If a side is already in a map event, trimming it here can leave a
            // hero-only shell party and make the opposing side read as zero troops.
            if (party.MapEvent != null)
                return;

            while (MWRPartyWeightHelper.GetEffectivePartySize(party) > partyWeightLimit)
            {
                if (!RemoveOneWeightedTroop(party))
                    break;
            }
        }

        private static bool RemoveOneWeightedTroop(MobileParty party)
        {
            TroopRoster roster = party.MemberRoster;

            CharacterObject? bestTroop = null;
            int bestWeight = 0;

            for (int i = 0; i < roster.Count; i++)
            {
                TroopRosterElement element = roster.GetElementCopyAtIndex(i);
                CharacterObject troop = element.Character;

                if (troop == null || element.Number <= 0 || troop.IsHero)
                    continue;

                int weight = MWRMode.CultureRules.GetWeightedPartySizeCost(troop);
                if (weight > bestWeight)
                {
                    bestWeight = weight;
                    bestTroop = troop;
                }
            }

            if (bestTroop == null)
                return false;

            roster.AddToCounts(bestTroop, -1, insertAtFront: false, woundedCount: 0, xpChange: 0, removeDepleted: true);
            return true;
        }
    }
}
