using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace MountAndWarcraftReborn.Helpers
{
    public static class MWRMarriageRules
    {
        private static readonly HashSet<string> NoMarriageCultureIds = new(StringComparer.OrdinalIgnoreCase)
        {
            "nord",
        };

        private static readonly HashSet<string> NoMarriageRaceNames = new(StringComparer.OrdinalIgnoreCase)
        {
            "skeleton",
            "spirit_host",
            "wraith",
        };

        private static readonly HashSet<string> BlockedCulturePairKeys = new(StringComparer.OrdinalIgnoreCase)
        {
            // Add explicit symmetric culture pair blocks here as needed.
            // PairKey("culture_a", "culture_b"),
        };

        private static readonly HashSet<string> BlockedRacePairKeys = new(StringComparer.OrdinalIgnoreCase)
        {
            // Add explicit symmetric race pair blocks here as needed.
             PairKey("orc", "elf"),
             PairKey("orc", "troll"),
             PairKey("orc", "human"),
             PairKey("orc", "dwarf"),
             PairKey("troll", "elf"),
             PairKey("troll", "human"),
             PairKey("troll", "dwarf"),
             PairKey("elf", "human"),
             PairKey("elf", "dwarf"),
             PairKey("human", "dwarf"),
        };

        public static bool IsHeroSuitableForMarriage(Hero hero)
        {
            if (hero?.CharacterObject == null)
            {
                return false;
            }

            string cultureId = GetCultureId(hero);
            if (!string.IsNullOrWhiteSpace(cultureId) && NoMarriageCultureIds.Contains(cultureId))
            {
                return false;
            }

            string raceName = GetRaceName(hero);
            if (!string.IsNullOrWhiteSpace(raceName) && NoMarriageRaceNames.Contains(raceName))
            {
                return false;
            }

            return true;
        }

        public static bool IsCoupleSuitableForMarriage(Hero firstHero, Hero secondHero)
        {
            if (!IsHeroSuitableForMarriage(firstHero) || !IsHeroSuitableForMarriage(secondHero))
            {
                return false;
            }

            string firstCultureId = GetCultureId(firstHero);
            string secondCultureId = GetCultureId(secondHero);
            if (!string.IsNullOrWhiteSpace(firstCultureId) &&
                !string.IsNullOrWhiteSpace(secondCultureId) &&
                BlockedCulturePairKeys.Contains(PairKey(firstCultureId, secondCultureId)))
            {
                return false;
            }

            string firstRaceName = GetRaceName(firstHero);
            string secondRaceName = GetRaceName(secondHero);
            if (!string.IsNullOrWhiteSpace(firstRaceName) &&
                !string.IsNullOrWhiteSpace(secondRaceName) &&
                BlockedRacePairKeys.Contains(PairKey(firstRaceName, secondRaceName)))
            {
                return false;
            }

            return true;
        }

        private static string GetCultureId(Hero hero)
        {
            return hero?.CharacterObject?.Culture?.StringId ?? string.Empty;
        }

        private static string GetRaceName(Hero hero)
        {
            int race = hero?.CharacterObject?.Race ?? -1;
            string[] raceNames = FaceGen.GetRaceNames();
            if (race < 0 || race >= raceNames.Length)
            {
                return string.Empty;
            }

            return raceNames[race] ?? string.Empty;
        }

        private static string PairKey(string firstId, string secondId)
        {
            string first = (firstId ?? string.Empty).Trim().ToLowerInvariant();
            string second = (secondId ?? string.Empty).Trim().ToLowerInvariant();

            return string.Compare(first, second, StringComparison.Ordinal) <= 0
                ? $"{first}|{second}"
                : $"{second}|{first}";
        }
    }
}
