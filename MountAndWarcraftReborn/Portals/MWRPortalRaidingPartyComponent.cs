#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace MountAndWarcraftReborn.Portals
{
    public class MWRPortalRaidingPartyComponent : WarPartyComponent
    {
        private const float SettlementSearchRadius = 300f;
        private const float PartySearchRadius = 150f;

        [SaveableProperty(1)]
        public Settlement? TargetSettlement { get; set; }

        [SaveableField(2)]
        private Hero _owner;

        [SaveableField(3)]
        private Settlement _homeSettlement;

        [SaveableField(4)]
        private string _name;

        [SaveableField(5)]
        private PartyTemplateObject _template;

        [SaveableField(6)]
        private int _partySize;

        [SaveableField(7)]
        private bool _isBattleDefender;

        private MWRPortalRaidingPartyComponent(
            Settlement homeSettlement,
            string name,
            Clan ownerClan,
            PartyTemplateObject template,
            int partySize,
            bool isBattleDefender)
        {
            _homeSettlement = homeSettlement;
            _name = name;
            _owner = ownerClan.Leader;
            _template = template;
            _partySize = partySize;
            _isBattleDefender = isBattleDefender;
        }

        public bool IsBattleDefender => _isBattleDefender;

        public int DesiredPartySize => Math.Max(1, _partySize);

        public override Hero PartyOwner => _owner;

        public override Settlement HomeSettlement => _homeSettlement;

        public override TextObject Name => new TextObject(_name);

        protected override void OnMobilePartySetOnCreation()
        {
            InitializePortalParty();
        }

        public static MobileParty CreatePortalParty(
            string stringId,
            Settlement homeSettlement,
            string name,
            PartyTemplateObject template,
            Clan ownerClan,
            int partySize,
            bool isBattleDefender)
        {
            return MobileParty.CreateParty(
                stringId,
                new MWRPortalRaidingPartyComponent(homeSettlement, name, ownerClan, template, partySize, isBattleDefender));
        }

        public void HourlyTickAI(PartyThinkParams thinkParams)
        {
            if (IsBattleDefender || MobileParty == null || Clan == null)
            {
                return;
            }

            if (!TargetSettlementIsValid(TargetSettlement))
            {
                if (TryEngageNearbyParty())
                {
                    return;
                }

                TargetSettlement = FindNewTargetSettlement();
            }

            if (TargetSettlement != null)
            {
                if (TargetSettlement.IsVillage)
                {
                    SetPartyAiAction.GetActionForRaidingSettlement(MobileParty, TargetSettlement, MobileParty.NavigationType.Default, false);
                }
                else if (TargetSettlement.IsTown || TargetSettlement.IsCastle)
                {
                    SetPartyAiAction.GetActionForBesiegingSettlement(MobileParty, TargetSettlement, MobileParty.NavigationType.Default, false);
                }
            }
            else
            {
                MobileParty.SetMoveGoToSettlement(HomeSettlement, MobileParty.NavigationType.Default, false);
            }
        }

        private void InitializePortalParty()
        {
            if (_owner.Clan == null)
            {
                throw new MBNullParameterException("Portal party owner clan is null.");
            }

            MobileParty.InitializeMobilePartyAroundPosition(_template, HomeSettlement.Position, DesiredPartySize);
            EnsureDesiredPartySize();
            MobileParty.ActualClan = _owner.Clan;
            MobileParty.Aggressiveness = 2f;
            MobileParty.Party.SetVisualAsDirty();
            MobileParty.ItemRoster.AddToCounts(DefaultItems.Meat, MBRandom.RandomInt(_partySize, Math.Max(_partySize + 1, _partySize * 2)));
        }

        private void EnsureDesiredPartySize()
        {
            if (MobileParty?.MemberRoster == null)
            {
                return;
            }

            int missingTroops = DesiredPartySize - MobileParty.MemberRoster.TotalManCount;
            if (missingTroops <= 0)
            {
                return;
            }

            Dictionary<CharacterObject, int> templateWeights = GetTemplateWeights();
            if (templateWeights.Count == 0)
            {
                return;
            }

            AddTroopsUsingWeights(templateWeights, missingTroops);
        }

        private Dictionary<CharacterObject, int> GetTemplateWeights()
        {
            Dictionary<CharacterObject, int> weights = new Dictionary<CharacterObject, int>();
            if (_template?.Stacks == null)
            {
                return weights;
            }

            foreach (PartyTemplateStack stack in _template.Stacks)
            {
                CharacterObject? troop = stack.Character;
                if (troop == null)
                {
                    continue;
                }

                int weight = GetStackWeight(stack);
                if (weight <= 0)
                {
                    weight = 1;
                }

                if (weights.ContainsKey(troop))
                {
                    weights[troop] += weight;
                }
                else
                {
                    weights[troop] = weight;
                }
            }

            return weights;
        }

        private void AddTroopsUsingWeights(Dictionary<CharacterObject, int> weights, int missingTroops)
        {
            if (MobileParty?.MemberRoster == null || missingTroops <= 0 || weights.Count == 0)
            {
                return;
            }

            int totalWeight = weights.Values.Sum();
            if (totalWeight <= 0)
            {
                return;
            }

            List<(CharacterObject troop, int add, double remainder)> allocations = new List<(CharacterObject troop, int add, double remainder)>();
            int distributed = 0;

            foreach (KeyValuePair<CharacterObject, int> entry in weights)
            {
                CharacterObject troop = entry.Key;
                int weight = entry.Value;
                double exactShare = (double)missingTroops * weight / totalWeight;
                int addCount = (int)Math.Floor(exactShare);
                distributed += addCount;
                allocations.Add((troop, addCount, exactShare - addCount));
            }

            int remainderTroops = missingTroops - distributed;
            foreach ((CharacterObject troop, int add, _) in allocations
                .OrderByDescending(entry => entry.remainder)
                .ThenBy(entry => entry.troop.StringId)
                .Take(remainderTroops)
                .ToList())
            {
                int index = allocations.FindIndex(entry => entry.troop == troop);
                if (index >= 0)
                {
                    allocations[index] = (troop, allocations[index].add + 1, allocations[index].remainder);
                }
            }

            foreach ((CharacterObject troop, int add, _) in allocations)
            {
                if (add > 0)
                {
                    MobileParty.MemberRoster.AddToCounts(troop, add);
                }
            }
        }

        private static int GetStackWeight(PartyTemplateStack stack)
        {
            int? minValue = GetIntValue(stack, "MinValue") ?? GetIntValue(stack, "Min");
            int? maxValue = GetIntValue(stack, "MaxValue") ?? GetIntValue(stack, "Max");

            if (minValue.HasValue && maxValue.HasValue)
            {
                return Math.Max(1, (minValue.Value + maxValue.Value) / 2);
            }

            if (maxValue.HasValue)
            {
                return Math.Max(1, maxValue.Value);
            }

            if (minValue.HasValue)
            {
                return Math.Max(1, minValue.Value);
            }

            return 1;
        }

        private static int? GetIntValue(object instance, string memberName)
        {
            Type type = instance.GetType();
            PropertyInfo? property = type.GetProperty(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (property != null && property.PropertyType == typeof(int))
            {
                return (int?)property.GetValue(instance);
            }

            FieldInfo? field = type.GetField(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null && field.FieldType == typeof(int))
            {
                return (int?)field.GetValue(instance);
            }

            return null;
        }

        private bool TryEngageNearbyParty()
        {
            if (MobileParty == null || Clan == null)
            {
                return false;
            }

            MobileParty? enemyParty = MobileParty.All
                .Where(party => party != null && party.IsActive && party.MapFaction != null)
                .Where(party => party != MobileParty && !party.IsMilitia && !party.IsBandit)
                .Where(party => party.MapFaction != Clan && party.MapFaction.Culture?.StringId != Clan.Culture?.StringId)
                .Where(party => party.MapEvent == null && party.CurrentSettlement == null)
                .Where(party => party.MemberRoster.TotalHealthyCount <= MobileParty.MemberRoster.TotalHealthyCount)
                .Where(party => party.Position.ToVec2().DistanceSquared(MobileParty.Position.ToVec2()) <= PartySearchRadius * PartySearchRadius)
                .OrderBy(party => party.Position.ToVec2().DistanceSquared(MobileParty.Position.ToVec2()))
                .FirstOrDefault();

            if (enemyParty == null)
            {
                return false;
            }

            if (!Clan.IsAtWarWith(enemyParty.MapFaction))
            {
                DeclareWarAction.ApplyByDefault(Clan, enemyParty.MapFaction);
            }

            SetPartyAiAction.GetActionForEngagingParty(MobileParty, enemyParty, MobileParty.NavigationType.Default, false);
            return true;
        }

        private Settlement? FindNewTargetSettlement()
        {
            if (MobileParty == null || Clan == null)
            {
                return null;
            }

            Vec2 origin = MobileParty.Position.ToVec2();
            List<Settlement> hostileSettlements = Settlement.All
                .Where(settlement => settlement != null && settlement.MapFaction != null)
                .Where(settlement => settlement != HomeSettlement)
                .Where(settlement => settlement.IsVillage || settlement.IsTown || settlement.IsCastle)
                .Where(settlement => settlement.MapFaction != Clan && settlement.MapFaction.Culture?.StringId != Clan.Culture?.StringId)
                .Where(settlement => origin.DistanceSquared(settlement.Position.ToVec2()) <= SettlementSearchRadius * SettlementSearchRadius)
                .ToList();

            Settlement? bestVillage = hostileSettlements
                .Where(settlement => settlement.IsVillage && !settlement.IsRaided && !settlement.IsUnderRaid)
                .OrderBy(settlement => origin.DistanceSquared(settlement.Position.ToVec2()))
                .FirstOrDefault();

            if (bestVillage != null)
            {
                return bestVillage;
            }

            Settlement? bestTownOrCastle = hostileSettlements
                .Where(settlement => !settlement.IsVillage && !settlement.IsUnderSiege)
                .OrderBy(settlement => origin.DistanceSquared(settlement.Position.ToVec2()))
                .FirstOrDefault();

            return bestTownOrCastle;
        }

        private bool TargetSettlementIsValid(Settlement? settlement)
        {
            if (settlement == null || MobileParty == null || Clan == null)
            {
                return false;
            }

            if (settlement == HomeSettlement || settlement.MapFaction == null || settlement.MapFaction == Clan)
            {
                return false;
            }

            if (!Clan.IsAtWarWith(settlement.MapFaction))
            {
                DeclareWarAction.ApplyByDefault(Clan, settlement.MapFaction);
            }

            if (settlement.IsVillage)
            {
                return !settlement.IsRaided && (!settlement.IsUnderRaid || settlement.LastAttackerParty == MobileParty);
            }

            return !settlement.IsUnderSiege;
        }
    }
}
