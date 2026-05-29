using System;
using System.Collections.Concurrent;
using MWRMode;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;

namespace MountAndWarcraftReborn.Patches.TroopWeight
{
    internal static class MWRTroopWeightService
    {
        private sealed class CachedRosterData
        {
            public int Version;
            public int WeightedTotalManCount;
            public int WeightedHealthyCount;
        }

        private sealed class CachedPartyData
        {
            public int Version;
            public int WeightedRegularMembers;
        }

        private static readonly ConcurrentDictionary<string, int> TroopWeightCache =
            new ConcurrentDictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        private static readonly ConcurrentDictionary<TroopRoster, CachedRosterData> RosterCache =
            new ConcurrentDictionary<TroopRoster, CachedRosterData>();

        private static readonly ConcurrentDictionary<PartyBase, CachedPartyData> PartyCache =
            new ConcurrentDictionary<PartyBase, CachedPartyData>();

        private const int CacheCleanupThreshold = 1000;

        public static int GetTroopWeight(CharacterObject character)
        {
            if (character == null)
                return 1;

            string troopId = character.StringId ?? string.Empty;
            if (string.IsNullOrWhiteSpace(troopId))
                return 1;

            return TroopWeightCache.GetOrAdd(troopId, _ => CultureRules.GetWeightedPartySizeCost(character));
        }

        public static int GetWeightedRosterTotalManCount(TroopRoster roster)
        {
            if (roster == null)
                return 0;

            return GetOrCalculateRosterWeights(roster).WeightedTotalManCount;
        }

        public static int GetWeightedRosterTotalHealthyCount(TroopRoster roster)
        {
            if (roster == null)
                return 0;

            return GetOrCalculateRosterWeights(roster).WeightedHealthyCount;
        }

        public static int GetWeightedRosterTotalManCount(TroopRoster roster, CharacterObject ignoredCharacter)
        {
            if (roster == null)
                return 0;

            if (ignoredCharacter == null)
                return GetWeightedRosterTotalManCount(roster);

            int total = 0;

            for (int i = 0; i < roster.Count; i++)
            {
                TroopRosterElement element = roster.GetElementCopyAtIndex(i);
                CharacterObject troop = element.Character;

                if (troop == ignoredCharacter)
                    continue;

                total += GetWeightedElementTotalCount(element);
            }

            return total;
        }

        public static int GetWeightedPartyNumberOfAllMembers(PartyBase party)
        {
            if (party?.MemberRoster == null)
                return 0;

            return GetWeightedRosterTotalManCount(party.MemberRoster);
        }

        public static int GetWeightedPartyNumberOfRegularMembers(PartyBase party)
        {
            if (party?.MemberRoster == null)
                return 0;

            if (PartyCache.TryGetValue(party, out CachedPartyData cached) &&
                cached.Version == party.MemberRoster.VersionNo)
            {
                return cached.WeightedRegularMembers;
            }

            int weightedRegularMembers = 0;

            for (int i = 0; i < party.MemberRoster.Count; i++)
            {
                TroopRosterElement element = party.MemberRoster.GetElementCopyAtIndex(i);
                CharacterObject troop = element.Character;

                if (troop == null || troop.IsHero)
                    continue;

                weightedRegularMembers += GetWeightedElementTotalCount(element);
            }

            PartyCache[party] = new CachedPartyData
            {
                Version = party.MemberRoster.VersionNo,
                WeightedRegularMembers = weightedRegularMembers
            };

            CleanupCachesIfNeeded();
            return weightedRegularMembers;
        }

        public static int GetRawRosterTotalManCount(TroopRoster roster)
        {
            if (roster == null)
                return 0;

            int total = 0;
            for (int i = 0; i < roster.Count; i++)
            {
                TroopRosterElement element = roster.GetElementCopyAtIndex(i);
                total += Math.Max(0, element.Number);
            }

            return total;
        }

        public static int GetRawRosterTotalHealthyCount(TroopRoster roster)
        {
            if (roster == null)
                return 0;

            int total = 0;
            for (int i = 0; i < roster.Count; i++)
            {
                TroopRosterElement element = roster.GetElementCopyAtIndex(i);
                total += Math.Max(0, element.Number - element.WoundedNumber);
            }

            return total;
        }

        public static int GetRawPartyNumberOfAllMembers(PartyBase party)
        {
            return party?.MemberRoster == null
                ? 0
                : GetRawRosterTotalManCount(party.MemberRoster);
        }

        public static int GetRawPartyNumberOfRegularMembers(PartyBase party)
        {
            if (party?.MemberRoster == null)
                return 0;

            int total = 0;
            for (int i = 0; i < party.MemberRoster.Count; i++)
            {
                TroopRosterElement element = party.MemberRoster.GetElementCopyAtIndex(i);
                CharacterObject troop = element.Character;

                if (troop == null || troop.IsHero)
                    continue;

                total += Math.Max(0, element.Number);
            }

            return total;
        }

        private static CachedRosterData GetOrCalculateRosterWeights(TroopRoster roster)
        {
            if (RosterCache.TryGetValue(roster, out CachedRosterData cached) &&
                cached.Version == roster.VersionNo)
            {
                return cached;
            }

            int weightedTotalManCount = 0;
            int weightedHealthyCount = 0;

            for (int i = 0; i < roster.Count; i++)
            {
                TroopRosterElement element = roster.GetElementCopyAtIndex(i);
                weightedTotalManCount += GetWeightedElementTotalCount(element);
                weightedHealthyCount += GetWeightedElementHealthyCount(element);
            }

            CachedRosterData data = new CachedRosterData
            {
                Version = roster.VersionNo,
                WeightedTotalManCount = weightedTotalManCount,
                WeightedHealthyCount = weightedHealthyCount
            };

            RosterCache[roster] = data;
            CleanupCachesIfNeeded();
            return data;
        }

        private static int GetWeightedElementTotalCount(TroopRosterElement element)
        {
            int troopCount = Math.Max(0, element.Number);
            if (troopCount == 0)
                return 0;

            CharacterObject troop = element.Character;
            if (troop == null)
                return troopCount;

            if (troop.IsHero)
                return troopCount;

            return GetTroopWeight(troop) * troopCount;
        }

        private static int GetWeightedElementHealthyCount(TroopRosterElement element)
        {
            int healthyCount = Math.Max(0, element.Number - element.WoundedNumber);
            if (healthyCount == 0)
                return 0;

            CharacterObject troop = element.Character;
            if (troop == null)
                return healthyCount;

            if (troop.IsHero)
                return healthyCount;

            return GetTroopWeight(troop) * healthyCount;
        }

        private static void CleanupCachesIfNeeded()
        {
            if (RosterCache.Count > CacheCleanupThreshold)
            {
                RosterCache.Clear();
            }

            if (PartyCache.Count > CacheCleanupThreshold)
            {
                PartyCache.Clear();
            }

            if (TroopWeightCache.Count > CacheCleanupThreshold)
            {
                TroopWeightCache.Clear();
            }
        }
    }
}
