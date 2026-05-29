using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.ObjectSystem;

namespace MountAndWarcraftReborn.Behaviors
{
    public class MWRSettlementCultureConversionBehavior : CampaignBehaviorBase
    {
        // settlementId -> cultureId
        private Dictionary<string, string> _settlementCultureOverrides = new Dictionary<string, string>();

        public override void RegisterEvents()
        {
            CampaignEvents.OnSettlementOwnerChangedEvent.AddNonSerializedListener(this, OnSettlementOwnerChanged);
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("_settlementCultureOverrides", ref _settlementCultureOverrides);
        }

        private void OnSettlementOwnerChanged(
            Settlement settlement,
            bool openToClaim,
            Hero newOwner,
            Hero oldOwner,
            Hero capturerHero,
            ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail detail)
        {
            if (settlement == null || newOwner?.MapFaction?.Culture == null)
                return;

            CultureObject newCulture = newOwner.MapFaction.Culture;
            if (!ShouldApplyCultureConversion(detail) || !NeedsCultureConversion(settlement, newCulture))
                return;

            ApplyCultureOnConquest(settlement, newCulture);

            _settlementCultureOverrides[settlement.StringId] = newCulture.StringId;

            if (settlement.BoundVillages != null)
            {
                foreach (var village in settlement.BoundVillages)
                {
                    if (village?.Settlement != null)
                    {
                        _settlementCultureOverrides[village.Settlement.StringId] = newCulture.StringId;
                    }
                }
            }
        }

        private static bool ShouldApplyCultureConversion(ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail detail)
        {
            return detail == ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail.BySiege ||
                   detail == ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail.ByRebellion;
        }

        private static bool NeedsCultureConversion(Settlement settlement, CultureObject newCulture)
        {
            if (settlement.Culture != newCulture)
                return true;

            if (settlement.BoundVillages == null)
                return false;

            foreach (Village village in settlement.BoundVillages)
            {
                if (village?.Settlement?.Culture != newCulture)
                    return true;
            }

            return false;
        }
        private static void PurgeVillagerParties(Settlement villageSettlement)
        {
            if (villageSettlement == null)
                return;

            for (int i = MobileParty.All.Count - 1; i >= 0; i--)
            {
                MobileParty party = MobileParty.All[i];

                if (party == null)
                    continue;

                if (party.HomeSettlement == villageSettlement &&
                    party.PartyComponent is VillagerPartyComponent)
                {
                    DestroyPartyAction.Apply(null, party);
                }
            }
        }
        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            if (_settlementCultureOverrides == null || _settlementCultureOverrides.Count == 0)
                return;

            foreach (var pair in _settlementCultureOverrides)
            {
                Settlement settlement = Settlement.All.FirstOrDefault(s => s.StringId == pair.Key);
                if (settlement == null)
                    continue;

                CultureObject culture = MBObjectManager.Instance.GetObject<CultureObject>(pair.Value);
                if (culture == null)
                    continue;

                ReapplyCultureOnLoad(settlement, culture);
            }
        }

        private static void PurgeVolunteers(Hero notable)
        {
            if (notable?.VolunteerTypes == null)
                return;

            for (int i = 0; i < Hero.MaximumNumberOfVolunteers; i++)
            {
                notable.VolunteerTypes[i] = null;
            }
        }
        private static void PurgeMilitia(Settlement settlement)
        {
            if (settlement == null)
                return;

            settlement.Militia = 0f;

            if (settlement.MilitiaPartyComponent?.MobileParty != null)
            {
                DestroyPartyAction.Apply(null, settlement.MilitiaPartyComponent.MobileParty);
            }
        }
        private static void ApplyCultureOnConquest(Settlement settlement, CultureObject newCulture)
        {
            settlement.Culture = newCulture;

            foreach (var notable in settlement.Notables)
            {
                if (notable.Culture != newCulture)
                    notable.Culture = newCulture;

                PurgeVolunteers(notable);
            }

            PurgeMilitia(settlement);
            PurgeVillagerParties(settlement);

            if (settlement.BoundVillages == null)
                return;

            foreach (var village in settlement.BoundVillages)
            {
                if (village?.Settlement == null)
                    continue;

                village.Settlement.Culture = newCulture;

                foreach (var villageNotable in village.Settlement.Notables)
                {
                    if (villageNotable.Culture != newCulture)
                        villageNotable.Culture = newCulture;

                    PurgeVolunteers(villageNotable);
                }

                PurgeMilitia(village.Settlement);
                PurgeVillagerParties(village.Settlement);
            }
        }

        private static void ReapplyCultureOnLoad(Settlement settlement, CultureObject newCulture)
        {
            settlement.Culture = newCulture;

            foreach (var notable in settlement.Notables)
            {
                if (notable.Culture != newCulture)
                    notable.Culture = newCulture;
            }

            if (settlement.BoundVillages == null)
                return;

            foreach (var village in settlement.BoundVillages)
            {
                if (village?.Settlement == null)
                    continue;

                village.Settlement.Culture = newCulture;

                foreach (var villageNotable in village.Settlement.Notables)
                {
                    if (villageNotable.Culture != newCulture)
                        villageNotable.Culture = newCulture;
                }
            }
        }
    }
}
