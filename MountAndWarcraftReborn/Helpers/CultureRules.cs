using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace MWRMode
{
    public static class CultureRules
    {
        private static readonly HashSet<string> BlockedPrisonerRecruitmentCultures = new()
        {
            "nord",
            // "demon_cul",
            // "scourge_cul",
        };

        private static readonly HashSet<string> NearFreeUpkeepCultures = new()
        {
            "nord",
        };

        private static readonly HashSet<string> MediumWageIncreaseCultures = new()
        {
            //"battania",
            // "empire_elite_cul",
            // "some_other_cul",
        };

        private static readonly HashSet<string> DoubleWageCultures = new()
        {
        };

        private static readonly HashSet<string> MediumRecruitmentIncreaseCultures = new()
        {
            "battania",
        };

        private static readonly HashSet<string> HighRecruitmentIncreaseCultures = new()
        {
        };

        // New food groups
        private static readonly HashSet<string> NoFoodConsumptionCultures = new()
        {
            "nord",
        };

        private static readonly HashSet<string> FiftyPercentMoreFoodCultures = new()
        {
            "khuzait",
            // "another_big_beast_cul",
        };

        private static readonly HashSet<string> MediumAutoresolveBonusCultures = new()
        {
             "khuzait",
        };

        private static readonly HashSet<string> HighAutoresolveBonusCultures = new()
        {
            "battania",
        };

        private static readonly HashSet<string> MediumLordAutoresolveBonusCultures = new()
        {
             "khuzait",
        };

        private static readonly HashSet<string> HighLordAutoresolveBonusCultures = new()
        {
            "battania",
        };
        private static readonly Dictionary<string, float> SpecialCharacterAutoresolveAttackBonusFactors = new()
        {
            { "main_hero", 1.00f },

            // Troop examples
            { "imperial_elite_cataphract", 10.50f },

            // Boss examples
            // { "demon_lord_1", 2.00f },
        };
        private static readonly Dictionary<string, float> SpecialCharacterAutoresolveDefenseReductionFactors = new()
        {
            { "main_hero", 0.50f },

            // Troop examples
            { "imperial_elite_cataphract", 0.99f }, //99 reduction

            // Boss examples
            // { "demon_lord_1", 0.80f },
        };

        // Flat militia growth bonus by OWNER culture
        private static readonly Dictionary<string, float> FlatMilitiaDailyBonusCultures = new()
        {
            { "nord", 2f },
            { "sturgia", 2f },
            { "aserai", 2f },

            // add more here
            // { "high_elf_cul", 3f },
        };

        // Special stronghold bonus by settlement ID
        private static readonly Dictionary<string, float> NativeSettlementMilitiaDailyBonus = new()
        {
            // examples
            { "town_A1", 1f },
            { "castle_B1", 1f },

            // replace with your actual towns/castles
        };

        // Optional post-siege recovery bonus by settlement ID
        private static readonly Dictionary<string, int> NativeSettlementPostSiegeMilitiaBonus = new()
        {
            { "town_A1", 15 },
            { "castle_B1", 25 },

            // replace with your actual towns/castles
        };

        // ORIGINAL / NATIVE culture of the settlement
        // This stays fixed even if you later change settlement.Culture after conquest
        private static readonly Dictionary<string, string> NativeSettlementCultures = new()
        {
            { "town_A1", "nord" },
            { "castle_B1", "nord" },
            { "town_B2", "battania" },

            // replace with your actual settlement -> original culture assignments
        };

        // Party leader culture -> allowed troop cultures
        private static readonly Dictionary<string, HashSet<string>> AllowedTroopCulturesByPartyCulture = new()
        {
            { "nord", new HashSet<string> { "nord" } },
            { "khuzait", new HashSet<string> { "khuzait", "aserai" } },
            { "battania", new HashSet<string> { "battania" } },
            { "aserai", new HashSet<string> { "aserai", "khuzait" } },
            { "vlandia", new HashSet<string> { "vlandia", "empire" } },
            { "empire", new HashSet<string> { "empire", "vlandia" } },
            { "sturgia", new HashSet<string> { "sturgia" } },

            // examples:
            // { "orc_cul", new HashSet<string> { "orc_cul", "goblin_cul" } },
            // { "high_elf_cul", new HashSet<string> { "high_elf_cul" } },
        };

        // Ordered kingdom preferences for clan migration:
        // same-culture kingdoms should come first, followed by fallback pairings.
        private static readonly Dictionary<string, string[]> PreferredKingdomCulturesByClanCulture = new()
        {
            { "nord", new[] { "nord" } },
            { "khuzait", new[] { "khuzait", "aserai" } },
            { "battania", new[] { "battania" } },
            { "aserai", new[] { "aserai", "khuzait" } },
            { "vlandia", new[] { "vlandia", "empire" } },
            { "empire", new[] { "empire", "vlandia" } },
            { "sturgia", new[] { "sturgia" } },

            // examples:
            // { "orc_cul", new[] { "orc_cul", "goblin_cul" } },
            // { "high_elf_cul", new[] { "high_elf_cul" } },
        };

        private static readonly HashSet<string> SmallLordHealthBonusCultures = new()
        {
             "battania",
        };

        private static readonly HashSet<string> MediumLordHealthBonusCultures = new()
        {
             "sturgia", "aserai",
        };

        private static readonly HashSet<string> BigLordHealthBonusCultures = new()
        {
            "khuzait", "nord",
        };

        private static readonly Dictionary<string, int> SpecialTroopFlatHealthBonuses = new()
        {   
            // troop ID -> extra HP on top of vanilla HP
            { "Gurubashi_Dire_Troll", 500 },


            // examples
            // { "demon_lord_1", 5000 },
            // { "lich_king_1", 3000 },
        };

        private static readonly Dictionary<int, int> FlatHealthBonusByRace = new()
        {
            // Replace these race ints with your real race IDs
            { 5, 50 },   // vrykul 
            { 2, 100 },  // orc
            // { 3, 100 }, // 
        };
        private static readonly Dictionary<string, int> WeightedPartySizeCostByCulture = new()
        {
            // Culture -> how many slots each troop of that culture uses
          //  { "nord", 2 },
            { "battania", 2 },

            // examples
            // { "orc_cul", 2 },
            // { "giant_cul", 3 },
        };

        private static readonly Dictionary<string, int> WeightedPartySizeCostByTroopId = new()
        {
            // Troop ID -> exact slot cost override
            { "Gurubashi_Dire_Troll", 2 },

            // examples
            // { "demon_lord_1", 10 },
            // { "dragon_guardian", 5 },
        };


        public static bool IsPrisonerRecruitmentBlocked(CharacterObject character)
        {
            string cultureId = character?.Culture?.StringId ?? string.Empty;
            return BlockedPrisonerRecruitmentCultures.Contains(cultureId);
        }
        public static int GetWeightedPartySizeCost(CharacterObject character)
        {
            if (character == null)
                return 1;

            // Heroes should never consume extra "troop slots" in the global weight layer.
            // Letting hero roster entries weigh more than 1 can desync campaign counts from
            // mission spawn/participation logic and drop the player straight into spectator.
            if (character.IsHero)
                return 1;

            string troopId = character.StringId ?? string.Empty;
            if (WeightedPartySizeCostByTroopId.TryGetValue(troopId, out int troopCost))
                return troopCost;

            string cultureId = character.Culture?.StringId ?? string.Empty;
            if (WeightedPartySizeCostByCulture.TryGetValue(cultureId, out int cultureCost))
                return cultureCost;

            return 1;
        }
        public static bool HasNearFreeUpkeep(CharacterObject character)
        {
            string cultureId = character?.Culture?.StringId ?? string.Empty;
            return NearFreeUpkeepCultures.Contains(cultureId);
        }

        public static bool HasMediumWageIncrease(CharacterObject character)
        {
            string cultureId = character?.Culture?.StringId ?? string.Empty;
            return MediumWageIncreaseCultures.Contains(cultureId);
        }

        public static int GetFlatHealthBonusByRace(CharacterObject character)
        {
            if (character == null)
                return 0;

            return FlatHealthBonusByRace.TryGetValue(character.Race, out int bonus)
                ? bonus
                : 0;
        }
        public static bool HasDoubleWage(CharacterObject character)
        {
            string cultureId = character?.Culture?.StringId ?? string.Empty;
            return DoubleWageCultures.Contains(cultureId);
        }
        public static float GetSpecialCharacterAutoresolveAttackBonusFactor(CharacterObject character)
        {
            if (character == null)
                return 0f;

            string id = character.StringId ?? string.Empty;

            return SpecialCharacterAutoresolveAttackBonusFactors.TryGetValue(id, out float bonus)
                ? bonus
                : 0f;
        }
        public static bool HasMediumRecruitmentIncrease(CharacterObject character)
        {
            string cultureId = character?.Culture?.StringId ?? string.Empty;
            return MediumRecruitmentIncreaseCultures.Contains(cultureId);
        }

        public static bool HasHighRecruitmentIncrease(CharacterObject character)
        {
            string cultureId = character?.Culture?.StringId ?? string.Empty;
            return HighRecruitmentIncreaseCultures.Contains(cultureId);
        }

        public static bool HasNoFoodConsumption(CharacterObject character)
        {
            string cultureId = character?.Culture?.StringId ?? string.Empty;
            return NoFoodConsumptionCultures.Contains(cultureId);
        }

        public static bool HasFiftyPercentMoreFoodConsumption(CharacterObject character)
        {
            string cultureId = character?.Culture?.StringId ?? string.Empty;
            return FiftyPercentMoreFoodCultures.Contains(cultureId);
        }
        public static float GetSpecialCharacterAutoresolveDefenseReductionFactor(CharacterObject character)
        {
            if (character == null)
                return 0f;

            string id = character.StringId ?? string.Empty;

            return SpecialCharacterAutoresolveDefenseReductionFactors.TryGetValue(id, out float bonus)
                ? bonus
                : 0f;
        }
        public static bool HasMediumAutoresolveBonus(CharacterObject character)
        {
            string cultureId = character?.Culture?.StringId ?? string.Empty;
            return MediumAutoresolveBonusCultures.Contains(cultureId);
        }

        public static bool HasHighAutoresolveBonus(CharacterObject character)
        {
            string cultureId = character?.Culture?.StringId ?? string.Empty;
            return HighAutoresolveBonusCultures.Contains(cultureId);
        }
        public static bool HasMediumLordAutoresolveBonus(CharacterObject character)
        {
            string cultureId = character?.Culture?.StringId ?? string.Empty;
            return MediumLordAutoresolveBonusCultures.Contains(cultureId);
        }

        public static bool HasHighLordAutoresolveBonus(CharacterObject character)
        {
            string cultureId = character?.Culture?.StringId ?? string.Empty;
            return HighLordAutoresolveBonusCultures.Contains(cultureId);
        }

        public static float GetFlatMilitiaDailyBonusForCulture(string cultureId)
        {
            if (string.IsNullOrEmpty(cultureId))
                return 0f;

            return FlatMilitiaDailyBonusCultures.TryGetValue(cultureId, out float bonus)
                ? bonus
                : 0f;
        }

        public static float GetNativeSettlementMilitiaDailyBonus(string settlementId)
        {
            if (string.IsNullOrEmpty(settlementId))
                return 0f;

            return NativeSettlementMilitiaDailyBonus.TryGetValue(settlementId, out float bonus)
                ? bonus
                : 0f;
        }

        public static int GetNativeSettlementPostSiegeMilitiaBonus(string settlementId)
        {
            if (string.IsNullOrEmpty(settlementId))
                return 0;

            return NativeSettlementPostSiegeMilitiaBonus.TryGetValue(settlementId, out int bonus)
                ? bonus
                : 0;
        }

        public static bool IsNativeOwnerForMilitiaBonus(Settlement settlement)
        {
            if (settlement == null || settlement.OwnerClan == null)
                return false;

            if (!NativeSettlementCultures.TryGetValue(settlement.StringId, out string nativeCultureId))
                return false;

            string ownerCultureId = settlement.OwnerClan.Culture?.StringId ?? string.Empty;
            return ownerCultureId == nativeCultureId;
        }
        public static string GetPartyRecruitCultureId(MobileParty party)
        {
            if (party?.LeaderHero?.Culture != null)
                return party.LeaderHero.Culture.StringId;

            if (party?.ActualClan?.Culture != null)
                return party.ActualClan.Culture.StringId;

            if (party?.MapFaction?.Culture != null)
                return party.MapFaction.Culture.StringId;

            return string.Empty;
        }

        public static string GetHeroRecruitCultureId(Hero hero)
        {
            if (hero?.PartyBelongedTo?.LeaderHero?.Culture != null)
                return hero.PartyBelongedTo.LeaderHero.Culture.StringId;

            if (hero?.Clan?.Culture != null)
                return hero.Clan.Culture.StringId;

            if (hero?.MapFaction?.Culture != null)
                return hero.MapFaction.Culture.StringId;

            if (hero?.Culture != null)
                return hero.Culture.StringId;

            return string.Empty;
        }

        public static bool IsTroopCultureAllowedForParty(CharacterObject troop, MobileParty party)
        {
            string partyCultureId = GetPartyRecruitCultureId(party);
            return IsTroopCultureAllowedForCulture(troop, partyCultureId);
        }

        public static bool IsTroopCultureAllowedForHero(CharacterObject troop, Hero hero)
        {
            string partyCultureId = GetHeroRecruitCultureId(hero);
            return IsTroopCultureAllowedForCulture(troop, partyCultureId);
        }

        public static bool IsTroopCultureAllowedForCulture(CharacterObject troop, string partyCultureId)
        {
            if (troop == null)
                return true;

            string troopCultureId = troop.Culture?.StringId ?? string.Empty;
            if (string.IsNullOrEmpty(troopCultureId) || string.IsNullOrEmpty(partyCultureId))
                return true;

            if (!AllowedTroopCulturesByPartyCulture.TryGetValue(partyCultureId, out var allowed))
                return true; // default: no restriction for cultures not listed

            return allowed.Contains(troopCultureId);
        }

        public static IReadOnlyList<string> GetPreferredKingdomCultures(string clanCultureId)
        {
            if (string.IsNullOrWhiteSpace(clanCultureId))
                return new string[0];

            if (PreferredKingdomCulturesByClanCulture.TryGetValue(clanCultureId, out string[] preferredCultures))
                return preferredCultures;

            return new[] { clanCultureId };
        }

        public static bool IsKingdomCultureCompatible(string clanCultureId, string kingdomCultureId)
        {
            if (string.IsNullOrWhiteSpace(clanCultureId) || string.IsNullOrWhiteSpace(kingdomCultureId))
                return true;

            foreach (string preferredCultureId in GetPreferredKingdomCultures(clanCultureId))
            {
                if (preferredCultureId == kingdomCultureId)
                    return true;
            }

            return false;
        }
        public static bool HasSmallLordHealthBonus(CharacterObject character)
        {
            string cultureId = character?.Culture?.StringId ?? string.Empty;
            return SmallLordHealthBonusCultures.Contains(cultureId);
        }

        public static bool HasMediumLordHealthBonus(CharacterObject character)
        {
            string cultureId = character?.Culture?.StringId ?? string.Empty;
            return MediumLordHealthBonusCultures.Contains(cultureId);
        }

        public static bool HasBigLordHealthBonus(CharacterObject character)
        {
            string cultureId = character?.Culture?.StringId ?? string.Empty;
            return BigLordHealthBonusCultures.Contains(cultureId);
        }

        public static int GetSpecialTroopFlatHealthBonus(CharacterObject character)
        {
            if (character == null)
                return 0;

            string troopId = character.StringId ?? string.Empty;

            return SpecialTroopFlatHealthBonuses.TryGetValue(troopId, out int bonus)
                ? bonus
                : 0;
        }
    }
}
