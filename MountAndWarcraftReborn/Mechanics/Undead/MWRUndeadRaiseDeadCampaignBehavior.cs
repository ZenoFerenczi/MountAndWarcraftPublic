#nullable enable
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace MWRMode.Mechanics.Undead
{
    public class MWRUndeadRaiseDeadCampaignBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.OnPlayerBattleEndEvent.AddNonSerializedListener(this, OnPlayerBattleEndEvent);
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        private void OnPlayerBattleEndEvent(MapEvent mapEvent)
        {
            if (mapEvent == null)
                return;

            if (mapEvent.PlayerSide != mapEvent.WinningSide)
                return;

            if (!MWRUndeadRaiseRules.CanRaiseDead(Hero.MainHero))
                return;

            if (PlayerEncounter.Current == null || PlayerEncounter.Current.RosterToReceiveLootMembers == null)
                return;

            float raiseDeadChance = MWRUndeadRaiseRules.GetRaiseRatio(Hero.MainHero);
            if (raiseDeadChance <= 0f)
                return;

            Dictionary<CharacterObject, int> raisedTroops = CalculateRaiseDeadTroops(mapEvent, raiseDeadChance);

            foreach (KeyValuePair<CharacterObject, int> entry in raisedTroops)
            {
                if (entry.Value > 0)
                {
                    PlayerEncounter.Current.RosterToReceiveLootMembers.AddToCounts(entry.Key, entry.Value);
                }
            }

            if (MobileParty.MainParty != null && raisedTroops.Count > 0)
            {
                int totalRaised = raisedTroops.Values.Sum();
                if (totalRaised > 0)
                {
                    InformationManager.DisplayMessage(
                        new InformationMessage($"The dead rise again: {totalRaised} undead joined your ranks."));
                }
            }
        }

        private Dictionary<CharacterObject, int> CalculateRaiseDeadTroops(MapEvent mapEvent, float raiseDeadChance)
        {
            Dictionary<CharacterObject, int> results = new Dictionary<CharacterObject, int>();

            var partiesOnSide = mapEvent.PartiesOnSide(mapEvent.DefeatedSide);
            foreach (var party in partiesOnSide)
            {
                var killedTroops = party.Troops.Where(x => x.IsKilled);
                foreach (var rosterMember in killedTroops)
                {
                    CharacterObject? slainTroop = rosterMember.Troop;
                    if (slainTroop == null)
                        continue;

                    if (slainTroop.IsHero)
                        continue;

                    if (MBRandom.RandomFloat > raiseDeadChance)
                        continue;

                    CharacterObject? raisedTroop = MWRUndeadRaiseRules.ResolveRaisedTroop(slainTroop);
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