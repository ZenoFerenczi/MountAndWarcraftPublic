using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;

namespace MWRMode.Behaviors
{
    public class MWRPartyCultureCleanupBehavior : CampaignBehaviorBase
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
            if (party == null)
                return;

            // AI only
            if (party.IsMainParty)
                return;

            if (party.MemberRoster == null || party.MemberRoster.Count <= 0)
                return;

            TroopRoster roster = party.MemberRoster;

            for (int i = roster.Count - 1; i >= 0; i--)
            {
                TroopRosterElement element = roster.GetElementCopyAtIndex(i);
                CharacterObject troop = element.Character;

                if (troop == null || element.Number <= 0)
                    continue;

                // Leave heroes alone for now
                if (troop.IsHero)
                    continue;

                if (!CultureRules.IsTroopCultureAllowedForParty(troop, party))
                {
                    roster.AddToCounts(
                        troop,
                        -element.Number,
                        insertAtFront: false,
                        woundedCount: 0,
                        xpChange: 0,
                        removeDepleted: true);
                }
            }
        }
    }
}