using System.Collections.Generic;
using System.Linq;
using MountAndWarcraftReborn.Portals;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace MWRMode.Behaviors
{
    public class MWRFactionDiscontinuationBehavior : CampaignBehaviorBase
    {
        private const float SurvivalDurationForIndependentClanInWeeks = 4f;
        private Dictionary<string, double> _independentClans = new Dictionary<string, double>();

        public override void RegisterEvents()
        {
            CampaignEvents.OnSettlementOwnerChangedEvent.AddNonSerializedListener(this, OnSettlementOwnerChanged);
            CampaignEvents.OnClanChangedKingdomEvent.AddNonSerializedListener(this, OnClanChangedKingdom);
            CampaignEvents.DailyTickClanEvent.AddNonSerializedListener(this, OnDailyTickClan);
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("_independentClans", ref _independentClans);
        }

        private void OnSettlementOwnerChanged(
            Settlement settlement,
            bool openToClaim,
            Hero newOwner,
            Hero oldOwner,
            Hero capturerHero,
            ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail detail)
        {
            if (newOwner?.Clan != null)
            {
                _independentClans.Remove(newOwner.Clan.StringId);
            }

            if (oldOwner?.Clan == null)
                return;

            if (CanClanBeDiscontinued(oldOwner.Clan))
            {
                AddIndependentClan(oldOwner.Clan);
            }

            Kingdom oldKingdom = oldOwner.Clan.Kingdom;
            if (oldKingdom != null && CanKingdomBeDiscontinued(oldKingdom))
            {
                DiscontinueKingdom(oldKingdom);
            }
        }

        private void OnClanChangedKingdom(
            Clan clan,
            Kingdom oldKingdom,
            Kingdom newKingdom,
            ChangeKingdomAction.ChangeKingdomActionDetail detail,
            bool showNotification = true)
        {
            if (clan == null)
                return;

            if (newKingdom == null)
            {
                if (CanClanBeDiscontinued(clan))
                {
                    AddIndependentClan(clan);
                }
            }
            else
            {
                _independentClans.Remove(clan.StringId);
            }

            if (clan == Clan.PlayerClan && oldKingdom != null && CanKingdomBeDiscontinued(oldKingdom))
            {
                DiscontinueKingdom(oldKingdom);
            }
        }

        private void OnDailyTickClan(Clan clan)
        {
            if (clan == null || !_independentClans.ContainsKey(clan.StringId))
                return;

            if (!clan.Heroes.Any(hero => hero.IsLord))
            {
                DiscontinueClan(clan);
                return;
            }

            if (MBRandom.RandomFloat > 0.7f)
            {
                Kingdom targetKingdom = GetCandidateKingdomsForJoiningClan(clan)
                    .OrderBy(kingdom => kingdom.CurrentTotalStrength)
                    .FirstOrDefault();

                if (targetKingdom != null)
                {
                    ChangeKingdomAction.ApplyByJoinToKingdom(clan, targetKingdom);
                    return;
                }
            }

            if (_independentClans[clan.StringId] < CampaignTime.Now.ToWeeks)
            {
                DiscontinueClan(clan);
            }
        }

        private static IEnumerable<Kingdom> GetCandidateKingdomsForJoiningClan(Clan clan)
        {
            string clanCultureId = clan?.Culture?.StringId ?? string.Empty;
            if (string.IsNullOrWhiteSpace(clanCultureId))
                return Enumerable.Empty<Kingdom>();

            IReadOnlyList<string> preferredCultureIds = CultureRules.GetPreferredKingdomCultures(clanCultureId);
            foreach (string cultureId in preferredCultureIds)
            {
                List<Kingdom> candidateKingdoms = Kingdom.All
                    .Where(kingdom => kingdom != null && !kingdom.IsEliminated && kingdom.Culture?.StringId == cultureId)
                    .ToList();

                if (candidateKingdoms.Count > 0)
                {
                    return candidateKingdoms;
                }
            }

            return Enumerable.Empty<Kingdom>();
        }

        private static bool CanKingdomBeDiscontinued(Kingdom kingdom)
        {
            bool canBeDiscontinued =
                kingdom != null &&
                !kingdom.IsEliminated &&
                kingdom != Clan.PlayerClan?.Kingdom &&
                MWRPortalSiteHelper.CountNonPortalSettlements(kingdom.Settlements) == 0;

            if (canBeDiscontinued)
            {
                CampaignEventDispatcher.Instance.CanKingdomBeDiscontinued(kingdom, ref canBeDiscontinued);
            }

            return canBeDiscontinued;
        }

        private static void DiscontinueKingdom(Kingdom kingdom)
        {
            foreach (Clan clan in new List<Clan>(kingdom.Clans))
            {
                FinalizeMapEvents(clan);
                ChangeKingdomAction.ApplyByLeaveByKingdomDestruction(clan, true);
            }

            kingdom.RulingClan = null;
            DestroyKingdomAction.Apply(kingdom);
        }

        private static void FinalizeMapEvents(Clan clan)
        {
            foreach (WarPartyComponent warPartyComponent in clan.WarPartyComponents.ToList())
            {
                if (warPartyComponent?.MobileParty?.MapEvent != null)
                {
                    warPartyComponent.MobileParty.MapEvent.FinalizeEvent();
                }

                if (warPartyComponent?.MobileParty?.SiegeEvent != null)
                {
                    warPartyComponent.MobileParty.SiegeEvent.FinalizeSiegeEvent();
                }
            }

            foreach (Settlement settlement in clan.Settlements.ToList())
            {
                if (settlement?.Party?.MapEvent != null)
                {
                    settlement.Party.MapEvent.FinalizeEvent();
                }

                if (settlement?.Party?.SiegeEvent != null)
                {
                    settlement.Party.SiegeEvent.FinalizeSiegeEvent();
                }
            }
        }

        private static bool CanClanBeDiscontinued(Clan clan)
        {
            return clan != null &&
                   clan.Kingdom == null &&
                   !clan.IsRebelClan &&
                   !clan.IsBanditFaction &&
                   !clan.IsMinorFaction &&
                   clan != Clan.PlayerClan &&
                   MWRPortalSiteHelper.CountNonPortalSettlements(clan.Settlements) == 0;
        }

        private void DiscontinueClan(Clan clan)
        {
            DestroyClanAction.Apply(clan);
            _independentClans.Remove(clan.StringId);
        }

        private void AddIndependentClan(Clan clan)
        {
            if (_independentClans.ContainsKey(clan.StringId))
                return;

            _independentClans.Add(
                clan.StringId,
                CampaignTime.WeeksFromNow(SurvivalDurationForIndependentClanInWeeks).ToWeeks);
        }
    }
}
