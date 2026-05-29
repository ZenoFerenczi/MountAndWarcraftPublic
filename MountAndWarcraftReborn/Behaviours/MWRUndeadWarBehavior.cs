using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;

namespace MWRMode.Behaviors
{
    public class MWRUndeadWarBehavior : CampaignBehaviorBase
    {
        private const string UndeadCultureId = "nord";
        private int _daysUntilWarEnforcement = 2;

        private static readonly HashSet<string> UndeadFactionIds = new HashSet<string>
        {
            "nord"
        };

        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, OnDailyTick);
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("_daysUntilWarEnforcement", ref _daysUntilWarEnforcement);
        }

        private void OnDailyTick()
        {
            if (_daysUntilWarEnforcement > 0)
            {
                _daysUntilWarEnforcement--;
                return;
            }

            EnforceWarWithUndead();
        }

        private static void EnforceWarWithUndead()
        {
            List<IFaction> allFactions = GetRelevantPoliticalFactions();

            List<IFaction> undeadFactions = allFactions
                .Where(IsUndeadFaction)
                .Distinct()
                .ToList();

            List<IFaction> nonUndeadFactions = allFactions
                .Where(x => !IsUndeadFaction(x))
                .Distinct()
                .ToList();

            foreach (IFaction undeadFaction in undeadFactions)
            {
                foreach (IFaction nonUndeadFaction in nonUndeadFactions)
                {
                    if (undeadFaction == nonUndeadFaction)
                        continue;

                    if (!undeadFaction.IsAtWarWith(nonUndeadFaction))
                    {
                        FactionManager.DeclareWar(undeadFaction, nonUndeadFaction);
                    }
                }
            }

            for (int i = 0; i < undeadFactions.Count; i++)
            {
                for (int j = i + 1; j < undeadFactions.Count; j++)
                {
                    IFaction factionA = undeadFactions[i];
                    IFaction factionB = undeadFactions[j];

                    if (factionA == factionB)
                        continue;

                    if (factionA.IsAtWarWith(factionB))
                    {
                        MakePeaceAction.Apply(factionA, factionB);
                    }
                }
            }
        }

        private static List<IFaction> GetRelevantPoliticalFactions()
        {
            List<IFaction> factions = new List<IFaction>();

            foreach (Kingdom kingdom in Kingdom.All)
            {
                if (kingdom != null && !factions.Contains(kingdom))
                    factions.Add(kingdom);
            }

            foreach (Clan clan in Clan.All)
            {
                if (clan == null || clan.IsBanditFaction)
                    continue;

                if (clan.Kingdom != null)
                    continue;

                if (!factions.Contains(clan))
                    factions.Add(clan);
            }

            IFaction playerFaction = Hero.MainHero?.MapFaction;
            if (playerFaction != null && !factions.Contains(playerFaction))
            {
                factions.Add(playerFaction);
            }

            return factions;
        }

        private static bool IsUndeadFaction(IFaction faction)
        {
            if (faction == null)
                return false;

            if (!string.IsNullOrWhiteSpace(faction.StringId) && UndeadFactionIds.Contains(faction.StringId))
                return true;

            if (faction.Culture != null && faction.Culture.StringId == UndeadCultureId)
                return true;

            if (faction is Clan clan && clan.Leader?.Culture?.StringId == UndeadCultureId)
                return true;

            if (faction is Kingdom kingdom && kingdom.Leader?.Culture?.StringId == UndeadCultureId)
                return true;

            return false;
        }
    }
}