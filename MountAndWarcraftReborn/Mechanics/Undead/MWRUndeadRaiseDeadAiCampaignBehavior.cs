using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.Core;

namespace MWRMode.Mechanics.Undead
{
    public class MWRUndeadRaiseDeadAiCampaignBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.MapEventEnded.AddNonSerializedListener(this, OnMapEventEnded);
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        private void OnMapEventEnded(MapEvent mapEvent)
        {
            if (mapEvent == null)
                return;

            // Keep player battles on the TOR-style player behavior only.
            if (mapEvent.PlayerSide == BattleSideEnum.Attacker || mapEvent.PlayerSide == BattleSideEnum.Defender)
                return;

            MapEventSide winningSide = GetWinningSide(mapEvent);
            if (winningSide?.LeaderParty?.MobileParty == null)
                return;

            Hero winnerLeader = winningSide.LeaderParty.MobileParty.LeaderHero;
            if (!MWRUndeadRaiseRules.CanRaiseDead(winnerLeader))
                return;

            float raiseDeadChance = MWRUndeadRaiseRules.GetRaiseRatio(winnerLeader);
            if (raiseDeadChance <= 0f)
                return;

            Dictionary<CharacterObject, int> raisedTroops = CalculateRaiseDeadTroops(mapEvent, raiseDeadChance);
            if (raisedTroops.Count == 0)
                return;

            foreach (KeyValuePair<CharacterObject, int> entry in raisedTroops)
            {
                if (entry.Value > 0)
                {
                    winningSide.LeaderParty.MobileParty.MemberRoster.AddToCounts(entry.Key, entry.Value);
                }
            }
        }

        private static MapEventSide GetWinningSide(MapEvent mapEvent)
        {
            if (mapEvent.WinningSide == BattleSideEnum.Attacker)
                return mapEvent.AttackerSide;

            if (mapEvent.WinningSide == BattleSideEnum.Defender)
                return mapEvent.DefenderSide;

            return null;
        }

        private static Dictionary<CharacterObject, int> CalculateRaiseDeadTroops(MapEvent mapEvent, float raiseDeadChance)
        {
            Dictionary<CharacterObject, int> results = new Dictionary<CharacterObject, int>();

            var partiesOnSide = mapEvent.PartiesOnSide(mapEvent.DefeatedSide);
            foreach (var party in partiesOnSide)
            {
                var killedTroops = party.Troops.Where(x => x.IsKilled);
                foreach (var rosterMember in killedTroops)
                {
                    CharacterObject slainTroop = rosterMember.Troop;
                    if (slainTroop == null)
                        continue;

                    if (slainTroop.IsHero)
                        continue;

                    if (MBRandom.RandomFloat > raiseDeadChance)
                        continue;

                    CharacterObject raisedTroop = MWRUndeadRaiseRules.ResolveRaisedTroop(slainTroop);
                    if (raisedTroop == null)
                        continue;

                    if (results.TryGetValue(raisedTroop, out int existing))
                    {
                        results[raisedTroop] = existing + 1;
                    }
                    else
                    {
                        results.Add(raisedTroop, 1);
                    }
                }
            }

            return results;
        }
    }
}