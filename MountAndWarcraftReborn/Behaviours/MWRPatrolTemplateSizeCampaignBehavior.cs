using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Roster;

namespace MountAndWarcraftReborn.Behaviors
{
    public class MWRPatrolTemplateSizeCampaignBehavior : CampaignBehaviorBase
    {
        private Dictionary<string, int> _patrolTemplateMaxCounts = new Dictionary<string, int>();

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, OnDailyTick);
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("_patrolTemplateMaxCounts", ref _patrolTemplateMaxCounts);
        }

        public void RegisterTemplateMaxCount(MobileParty party, int templateMaxCount)
        {
            if (!IsTrackedPatrolParty(party) || templateMaxCount <= 0)
            {
                return;
            }

            _patrolTemplateMaxCounts[party.StringId] = templateMaxCount;
        }

        public bool TryGetTemplateMaxCount(MobileParty party, out int templateMaxCount)
        {
            templateMaxCount = 0;

            return IsTrackedPatrolParty(party) &&
                   _patrolTemplateMaxCounts.TryGetValue(party.StringId, out templateMaxCount) &&
                   templateMaxCount > 0;
        }

        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            RegisterSafeFallbacksForExistingPatrols();
            PruneStalePatrolEntries();
        }

        private void OnDailyTick()
        {
            RegisterSafeFallbacksForExistingPatrols();
            PruneStalePatrolEntries();
        }

        private void RegisterSafeFallbacksForExistingPatrols()
        {
            foreach (MobileParty patrolParty in MobileParty.All.Where(IsTrackedPatrolParty))
            {
                if (_patrolTemplateMaxCounts.ContainsKey(patrolParty.StringId))
                {
                    continue;
                }

                int currentRosterCount = GetActualRosterCount(patrolParty.MemberRoster);
                if (currentRosterCount > 0)
                {
                    _patrolTemplateMaxCounts[patrolParty.StringId] = currentRosterCount;
                }
            }
        }

        private void PruneStalePatrolEntries()
        {
            HashSet<string> activePatrolIds = MobileParty.All
                .Where(IsTrackedPatrolParty)
                .Select(party => party.StringId)
                .ToHashSet();

            List<string> stalePatrolIds = _patrolTemplateMaxCounts.Keys
                .Where(id => !activePatrolIds.Contains(id))
                .ToList();

            foreach (string stalePatrolId in stalePatrolIds)
            {
                _patrolTemplateMaxCounts.Remove(stalePatrolId);
            }
        }

        private static bool IsTrackedPatrolParty(MobileParty party)
        {
            return party?.PartyComponent is PatrolPartyComponent;
        }

        private static int GetActualRosterCount(TroopRoster roster)
        {
            if (roster == null)
            {
                return 0;
            }

            int total = 0;
            for (int i = 0; i < roster.Count; i++)
            {
                total += roster.GetElementCopyAtIndex(i).Number;
            }

            return total;
        }
    }
}
