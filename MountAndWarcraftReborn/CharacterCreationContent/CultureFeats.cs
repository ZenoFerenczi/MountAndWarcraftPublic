using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Core;

namespace MWRMode.Cultures
{
    [HarmonyPatch(typeof(DefaultCulturalFeats), "InitializeAll")]
    public static class MWRCustomCulturalFeatsPatch
    {
        public static FeatObject UndeadNoFoodFeat = null!;
        public static FeatObject UndeadRaiseDeadFeat = null!;
        public static FeatObject UndeadNoMoraleFeat = null!;
        public static FeatObject StormwindPartySizeFeat = null!;
        public static FeatObject StormwindAllianceLeviesFeat = null!;
        public static FeatObject LordaeronPartySizeFeat = null!;
        public static FeatObject LordaeronAllianceLeviesFeat = null!;
        public static FeatObject HighElfPartySizeFeat = null!;
        public static FeatObject HighElfWarfareFeat = null!;
        public static FeatObject HighElfRecruitmentCostFeat = null!;
        public static FeatObject HighElfWeightedMusterFeat = null!;
        public static FeatObject HighElfExclusiveLeviesFeat = null!;
        public static FeatObject GurubashiPartySizeFeat = null!;
        public static FeatObject GurubashiMilitiaFeat = null!;
        public static FeatObject GurubashiLordVitalityFeat = null!;
        public static FeatObject GurubashiWarbandLeviesFeat = null!;
        public static FeatObject HordePartySizeFeat = null!;
        public static FeatObject HordeSavageConquestFeat = null!;
        public static FeatObject HordeWarbandLeviesFeat = null!;
        public static FeatObject HordeBigAppetiteFeat = null!;
        public static FeatObject DwarvenPartySizeFeat = null!;
        public static FeatObject DwarvenMilitiaFeat = null!;
        public static FeatObject DwarvenResilienceFeat = null!;
        public static FeatObject DwarvenExclusiveLeviesFeat = null!;
        public static FeatObject ScourgePartySizeFeat = null!;
        public static FeatObject ScourgeNoFoodFeat = null!;
        public static FeatObject ScourgeRaiseDeadFeat = null!;
        public static FeatObject ScourgeNoMoraleFeat = null!;
        public static FeatObject ScourgeLogisticsFeat = null!;
        public static FeatObject ScourgeMarchFeat = null!;
        public static FeatObject ScourgeMilitiaFeat = null!;
        public static FeatObject ScourgeLordVitalityFeat = null!;
        public static FeatObject ScourgeExclusiveLeviesFeat = null!;
        public static FeatObject ScourgeNoPrisonerRecruitmentFeat = null!;
        public static FeatObject ScourgeEternalWarFeat = null!;

        [HarmonyPostfix]
        private static void Postfix()
        {
            RegisterCustomFeat(
                ref UndeadNoFoodFeat,
                "mwr_undead_no_food",
                "{=mwr_undead_no_food_name}Unliving Sustenance",
                "{=mwr_undead_no_food_desc}Undead parties do not require food.",
                0f,
                true,
                FeatObject.AdditionType.Add);

            RegisterCustomFeat(
                ref UndeadRaiseDeadFeat,
                "mwr_undead_raise_dead",
                "{=mwr_undead_raise_dead_name}Necromantic Harvest",
                "{=mwr_undead_raise_dead_desc}After battles, a portion of enemy dead may rise again as undead.",
                0f,
                true,
                FeatObject.AdditionType.Add);

            RegisterCustomFeat(
                ref UndeadNoMoraleFeat,
                "mwr_undead_no_morale",
                "{=mwr_undead_no_morale_name}Fearless Dead",
                "{=mwr_undead_no_morale_desc}Undead are unaffected by morale loss.",
                0f,
                true,
                FeatObject.AdditionType.Add);

            RegisterInfoFeat(
                ref StormwindPartySizeFeat,
                "mwr_stormwind_party_size",
                "{=mwr_stormwind_party_size_name}Stormwind Muster",
                "{=mwr_stormwind_party_size_desc}Stormwind parties can field 50 additional troops.",
                true);

            RegisterInfoFeat(
                ref StormwindAllianceLeviesFeat,
                "mwr_stormwind_alliance_levies",
                "{=mwr_stormwind_alliance_levies_name}Alliance Levies",
                "{=mwr_stormwind_alliance_levies_desc}Stormwind parties can recruit both Stormwind and Lordaeron troops.",
                true);

            RegisterInfoFeat(
                ref LordaeronPartySizeFeat,
                "mwr_lordaeron_party_size",
                "{=mwr_lordaeron_party_size_name}Lordaeron Muster",
                "{=mwr_lordaeron_party_size_desc}Lordaeron parties can field 50 additional troops.",
                true);

            RegisterInfoFeat(
                ref LordaeronAllianceLeviesFeat,
                "mwr_lordaeron_alliance_levies",
                "{=mwr_lordaeron_alliance_levies_name}Alliance Levies",
                "{=mwr_lordaeron_alliance_levies_desc}Lordaeron parties can recruit both Lordaeron and Stormwind troops.",
                true);

            RegisterInfoFeat(
                ref HighElfPartySizeFeat,
                "mwr_high_elf_party_size",
                "{=mwr_high_elf_party_size_name}High Elven Hosts",
                "{=mwr_high_elf_party_size_desc}High Elf parties can field 30 additional troops.",
                true);

            RegisterInfoFeat(
                ref HighElfWarfareFeat,
                "mwr_high_elf_warfare",
                "{=mwr_high_elf_warfare_name}High Elven Mastery",
                "{=mwr_high_elf_warfare_desc}High Elf troops gain stronger autoresolve results, High Elf lords are sturdier, and High Elf-led parties move faster on the campaign map.",
                true);

            RegisterInfoFeat(
                ref HighElfRecruitmentCostFeat,
                "mwr_high_elf_recruitment_cost",
                "{=mwr_high_elf_recruitment_cost_name}Rare Recruits",
                "{=mwr_high_elf_recruitment_cost_desc}High Elf troop recruitment costs 50% more.",
                false);

            RegisterInfoFeat(
                ref HighElfWeightedMusterFeat,
                "mwr_high_elf_weighted_muster",
                "{=mwr_high_elf_weighted_muster_name}Elite Muster",
                "{=mwr_high_elf_weighted_muster_desc}High Elf troops count as 2 slots in the party weight system.",
                false);

            RegisterInfoFeat(
                ref HighElfExclusiveLeviesFeat,
                "mwr_high_elf_exclusive_levies",
                "{=mwr_high_elf_exclusive_levies_name}Exclusive Levies",
                "{=mwr_high_elf_exclusive_levies_desc}High Elf parties recruit only High Elf troops.",
                false);

            RegisterInfoFeat(
                ref GurubashiPartySizeFeat,
                "mwr_gurubashi_party_size",
                "{=mwr_gurubashi_party_size_name}Gurubashi Warbands",
                "{=mwr_gurubashi_party_size_desc}Gurubashi parties can field 150 additional troops.",
                true);

            RegisterInfoFeat(
                ref GurubashiMilitiaFeat,
                "mwr_gurubashi_militia",
                "{=mwr_gurubashi_militia_name}Jungle Watchers",
                "{=mwr_gurubashi_militia_desc}Gurubashi-owned settlements gain +2 militia per day.",
                true);

            RegisterInfoFeat(
                ref GurubashiLordVitalityFeat,
                "mwr_gurubashi_lord_vitality",
                "{=mwr_gurubashi_lord_vitality_name}Troll Vitality",
                "{=mwr_gurubashi_lord_vitality_desc}Gurubashi lords and heroes gain +250 hit points.",
                true);

            RegisterInfoFeat(
                ref GurubashiWarbandLeviesFeat,
                "mwr_gurubashi_warband_levies",
                "{=mwr_gurubashi_warband_levies_name}Shared Warbands",
                "{=mwr_gurubashi_warband_levies_desc}Gurubashi parties can recruit both Gurubashi and Horde troops.",
                true);

            RegisterInfoFeat(
                ref HordePartySizeFeat,
                "mwr_horde_party_size",
                "{=mwr_horde_party_size_name}Great Horde",
                "{=mwr_horde_party_size_desc}Horde parties can field 100 additional troops.",
                true);

            RegisterInfoFeat(
                ref HordeSavageConquestFeat,
                "mwr_horde_savage_conquest",
                "{=mwr_horde_savage_conquest_name}Savage Conquest",
                "{=mwr_horde_savage_conquest_desc}Horde troops gain stronger autoresolve results, and Horde lords are far tougher in combat.",
                true);

            RegisterInfoFeat(
                ref HordeWarbandLeviesFeat,
                "mwr_horde_warband_levies",
                "{=mwr_horde_warband_levies_name}Shared Warbands",
                "{=mwr_horde_warband_levies_desc}Horde parties can recruit both Horde and Gurubashi troops.",
                true);

            RegisterInfoFeat(
                ref HordeBigAppetiteFeat,
                "mwr_horde_big_appetite",
                "{=mwr_horde_big_appetite_name}Ravenous Appetites",
                "{=mwr_horde_big_appetite_desc}Horde troops consume 50% more food in the player party.",
                false);

            RegisterInfoFeat(
                ref DwarvenPartySizeFeat,
                "mwr_dwarven_party_size",
                "{=mwr_dwarven_party_size_name}Dwarven Muster",
                "{=mwr_dwarven_party_size_desc}Dwarven parties can field 20 additional troops.",
                true);

            RegisterInfoFeat(
                ref DwarvenMilitiaFeat,
                "mwr_dwarven_militia",
                "{=mwr_dwarven_militia_name}Hold Militias",
                "{=mwr_dwarven_militia_desc}Dwarven-owned settlements gain +2 militia per day.",
                true);

            RegisterInfoFeat(
                ref DwarvenResilienceFeat,
                "mwr_dwarven_resilience",
                "{=mwr_dwarven_resilience_name}Dwarven Resilience",
                "{=mwr_dwarven_resilience_desc}Dwarven lords and heroes gain +250 hit points.",
                true);

            RegisterInfoFeat(
                ref DwarvenExclusiveLeviesFeat,
                "mwr_dwarven_exclusive_levies",
                "{=mwr_dwarven_exclusive_levies_name}Exclusive Levies",
                "{=mwr_dwarven_exclusive_levies_desc}Dwarven parties recruit only Dwarven troops.",
                false);

            RegisterInfoFeat(
                ref ScourgePartySizeFeat,
                "mwr_scourge_party_size",
                "{=mwr_scourge_party_size_name}Endless Legions",
                "{=mwr_scourge_party_size_desc}Scourge parties can field 300 additional troops.",
                true);

            RegisterInfoFeat(
                ref ScourgeNoFoodFeat,
                "mwr_scourge_no_food",
                "{=mwr_scourge_no_food_name}Unliving Sustenance",
                "{=mwr_scourge_no_food_desc}Scourge troops do not consume food.",
                true);

            RegisterInfoFeat(
                ref ScourgeRaiseDeadFeat,
                "mwr_scourge_raise_dead",
                "{=mwr_scourge_raise_dead_name}Necromantic Harvest",
                "{=mwr_scourge_raise_dead_desc}After battle, enemy dead rise again as undead in Scourge ranks.",
                true);

            RegisterInfoFeat(
                ref ScourgeNoMoraleFeat,
                "mwr_scourge_no_morale",
                "{=mwr_scourge_no_morale_name}Fearless Dead",
                "{=mwr_scourge_no_morale_desc}Scourge troops do not rout from morale loss.",
                true);

            RegisterInfoFeat(
                ref ScourgeLogisticsFeat,
                "mwr_scourge_logistics",
                "{=mwr_scourge_logistics_name}Deathless Logistics",
                "{=mwr_scourge_logistics_desc}Scourge troops recruit for 1 gold and pay no upkeep.",
                true);

            RegisterInfoFeat(
                ref ScourgeMarchFeat,
                "mwr_scourge_march",
                "{=mwr_scourge_march_name}Relentless March",
                "{=mwr_scourge_march_desc}Scourge-led parties move faster on the campaign map and Scourge-led armies lose less cohesion each day.",
                true);

            RegisterInfoFeat(
                ref ScourgeMilitiaFeat,
                "mwr_scourge_militia",
                "{=mwr_scourge_militia_name}Gravebound Garrisons",
                "{=mwr_scourge_militia_desc}Scourge-owned settlements gain +2 militia per day.",
                true);

            RegisterInfoFeat(
                ref ScourgeLordVitalityFeat,
                "mwr_scourge_lord_vitality",
                "{=mwr_scourge_lord_vitality_name}Deathless Lords",
                "{=mwr_scourge_lord_vitality_desc}Scourge lords and heroes gain +500 hit points.",
                true);

            RegisterInfoFeat(
                ref ScourgeExclusiveLeviesFeat,
                "mwr_scourge_exclusive_levies",
                "{=mwr_scourge_exclusive_levies_name}Exclusive Levies",
                "{=mwr_scourge_exclusive_levies_desc}Scourge parties recruit only Scourge troops.",
                false);

            RegisterInfoFeat(
                ref ScourgeNoPrisonerRecruitmentFeat,
                "mwr_scourge_no_prisoner_recruitment",
                "{=mwr_scourge_no_prisoner_recruitment_name}No Living Captives",
                "{=mwr_scourge_no_prisoner_recruitment_desc}Scourge parties cannot recruit prisoners.",
                false);

            RegisterInfoFeat(
                ref ScourgeEternalWarFeat,
                "mwr_scourge_eternal_war",
                "{=mwr_scourge_eternal_war_name}Eternal War",
                "{=mwr_scourge_eternal_war_desc}Scourge factions are forced into war with the living and cannot make peace with them.",
                false);
        }

        private static void RegisterInfoFeat(
            ref FeatObject target,
            string id,
            string name,
            string description,
            bool isPositive)
        {
            RegisterCustomFeat(
                ref target,
                id,
                name,
                description,
                0f,
                isPositive,
                FeatObject.AdditionType.Add);
        }

        private static void RegisterCustomFeat(
            ref FeatObject target,
            string id,
            string name,
            string description,
            float bonus,
            bool isPositive,
            FeatObject.AdditionType additionType)
        {
            if (target != null)
                return;

            target = Game.Current.ObjectManager.RegisterPresumedObject(new FeatObject(id));
            target.Initialize(name, description, bonus, isPositive, additionType);
        }
    }
}
