#nullable enable
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace MWRMode.Mechanics.Undead
{
    public static class MWRUndeadRaiseRules
    {
        // Any culture that can raise dead after battle.
        public static readonly HashSet<string> RaisingCultureIds = new HashSet<string>
        {
            "nord"
        };

        // Optional exact hero IDs that can raise dead even if culture doesn't match.
        public static readonly HashSet<string> RaisingHeroIds = new HashSet<string>
        {
            // "mwr_necromancer_lord"
        };

        public const float DefaultRaiseRatio = 1.00f;

        // Exact override per hero.
        public static readonly Dictionary<string, float> HeroRatioOverrides = new Dictionary<string, float>
        {
            // ["mwr_necromancer_lord"] = 0.35f,
        };

        // Additive bonus per hero.
        public static readonly Dictionary<string, float> HeroRatioBonuses = new Dictionary<string, float>
        {
            // ["mwr_lich_lord"] = 0.10f,
        };

        // Exact troop -> undead troop mapping.
        public static readonly Dictionary<string, string> RaisedTroopBySourceTroopId = new Dictionary<string, string>
        {
             ["Farstrider_Sentinel"] = "dark_ranger",
             ["Farstrider_Ranger"] = "dark_ranger_general",
             ["Farstrider_Mounted_Sentinel"] = "dark_ranger",
             ["Farstrider_Mounted_Ranger"] = "dark_ranger_general",
             ["Lordaeron_Paladin"] = "scourge_death_knight",
             ["Lordaeron_Mounted_Paladin"] = "scourge_death_knight",
        };

        // Fallback by source culture.
        public static readonly Dictionary<string, string> RaisedTroopBySourceCultureId = new Dictionary<string, string>
        {
            // ["empire"] = "Skeleton_Warrior",
            // ["sturgia"] = "Skeleton_Warrior",
        };

        // Final fallback.
        public const string DefaultRaisedTroopId = "Undead_SkeletonWarrior";

        public static bool CanRaiseDead(Hero? hero)
        {
            if (hero == null)
                return false;

            if (RaisingHeroIds.Contains(hero.StringId))
                return true;

            return hero.Culture != null && RaisingCultureIds.Contains(hero.Culture.StringId);
        }

        public static float GetRaiseRatio(Hero? hero)
        {
            float ratio = DefaultRaiseRatio;

            if (hero != null && HeroRatioOverrides.TryGetValue(hero.StringId, out float overrideRatio))
            {
                ratio = overrideRatio;
            }

            if (hero != null && HeroRatioBonuses.TryGetValue(hero.StringId, out float bonus))
            {
                ratio += bonus;
            }

            if (ratio < 0f)
                ratio = 0f;

            if (ratio > 1f)
                ratio = 1f;

            return ratio;
        }

        public static CharacterObject? ResolveRaisedTroop(CharacterObject slainTroop)
        {
            if (slainTroop == null)
                return null;

            if (RaisedTroopBySourceTroopId.TryGetValue(slainTroop.StringId, out string? exactId))
            {
                CharacterObject? exactTroop = SafeGetCharacter(exactId);
                if (exactTroop != null)
                    return exactTroop;
            }

            if (slainTroop.Culture != null &&
                RaisedTroopBySourceCultureId.TryGetValue(slainTroop.Culture.StringId, out string? cultureId))
            {
                CharacterObject? cultureTroop = SafeGetCharacter(cultureId);
                if (cultureTroop != null)
                    return cultureTroop;
            }

            return SafeGetCharacter(DefaultRaisedTroopId);
        }

        private static CharacterObject? SafeGetCharacter(string? stringId)
        {
            if (string.IsNullOrWhiteSpace(stringId) || Game.Current == null)
                return null;

            try
            {
                return Game.Current.ObjectManager.GetObject<CharacterObject>(stringId);
            }
            catch
            {
                return null;
            }
        }
    }
}
