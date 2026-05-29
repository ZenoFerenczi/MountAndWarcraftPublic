using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace MountAndWarcraftReborn.BattleMechanics.NaturalWeapons
{
    public static class MWRNaturalWeaponRules
    {
        private readonly struct NaturalWeaponDamageProfile
        {
            public NaturalWeaponDamageProfile(float multiplier, float minimumDamage)
            {
                Multiplier = multiplier;
                MinimumDamage = minimumDamage;
            }

            public float Multiplier { get; }
            public float MinimumDamage { get; }
        }

        private static readonly Dictionary<string, MWRNaturalWeaponProfile> ExplicitProfileOverrides =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["kaldorei_warrior"] = MWRNaturalWeaponProfile.Claw,
                ["kaldorei_mountain_giant"] = MWRNaturalWeaponProfile.Claw,
                ["felhorde_beserker"] = MWRNaturalWeaponProfile.Claw,
                ["felhorde_blademaster"] = MWRNaturalWeaponProfile.Claw,
                ["felhorde_infernal"] = MWRNaturalWeaponProfile.Claw,
                ["felhorde_dreadlord"] = MWRNaturalWeaponProfile.Claw,
                ["felhorde_doomguard"] = MWRNaturalWeaponProfile.Claw,
                ["felhorde_eredar_diabolist"] = MWRNaturalWeaponProfile.Claw,
                ["felhorde_eredar_warlock"] = MWRNaturalWeaponProfile.Claw,
                ["scourge_abomination"] = MWRNaturalWeaponProfile.Claw,

                ["Gurubashi_Dire_Troll"] = MWRNaturalWeaponProfile.Succubus,
                ["felhorde_succubus"] = MWRNaturalWeaponProfile.Claw,
                ["felhorde_vile_temptress"] = MWRNaturalWeaponProfile.Succubus,
                ["felhorde_vile_tormentor"] = MWRNaturalWeaponProfile.Succubus,
                ["felhorde_maiden_of_pain"] = MWRNaturalWeaponProfile.Warden,
                ["felhorde_queen_of_suffering"] = MWRNaturalWeaponProfile.Warden,
                ["scourge_ghoul"] = MWRNaturalWeaponProfile.Succubus,
                ["scourge_shade"] = MWRNaturalWeaponProfile.Succubus,
                ["scourge_banshee"] = MWRNaturalWeaponProfile.Succubus,
                ["lord_7_23_1"] = MWRNaturalWeaponProfile.Succubus,

                ["kaldorei_warden_watcher"] = MWRNaturalWeaponProfile.Warden,
                ["kaldorei_warden_tracker"] = MWRNaturalWeaponProfile.Warden,
                ["kaldorei_warden_hunter"] = MWRNaturalWeaponProfile.Warden,
                ["kaldorei_huntress"] = MWRNaturalWeaponProfile.Warden,
                ["kaldorei_priestess_of_elune"] = MWRNaturalWeaponProfile.Warden,
                ["kaldorei_druid_of_the_talon"] = MWRNaturalWeaponProfile.Warden,
                ["kaldorei_druid_of_the_claw"] = MWRNaturalWeaponProfile.Warden,
                ["scourge_lich"] = MWRNaturalWeaponProfile.Warden,
                ["naga_myrmidon"] = MWRNaturalWeaponProfile.Warden,
                ["naga_royal_guard"] = MWRNaturalWeaponProfile.Warden,
                ["Gurubashi_Dire_Troll"] = MWRNaturalWeaponProfile.Warden,
                ["lord_7_3"] = MWRNaturalWeaponProfile.Warden,
                ["lord_7_5_2"] = MWRNaturalWeaponProfile.Warden,
                ["lord_7_14"] = MWRNaturalWeaponProfile.Warden,
                ["lord_7_14_3"] = MWRNaturalWeaponProfile.Warden,
                ["lord_7_15_2"] = MWRNaturalWeaponProfile.Warden,
                ["lord_7_16"] = MWRNaturalWeaponProfile.Warden,
                ["lord_7_23_2"] = MWRNaturalWeaponProfile.Warden,
                ["Blackrock_shaman"] = MWRNaturalWeaponProfile.Warden,
                ["lord_7_1"] = MWRNaturalWeaponProfile.Claw,
            };

        private static readonly HashSet<string> ExplicitExclusions =
            new(StringComparer.OrdinalIgnoreCase)
            {
            };

        private static readonly Dictionary<string, MWRNaturalWeaponProfile> DefaultCultureProfiles =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["nord"] = MWRNaturalWeaponProfile.Succubus,
                ["khuzait"] = MWRNaturalWeaponProfile.Claw,
            };

        private static readonly Dictionary<MWRNaturalWeaponProfile, NaturalWeaponDamageProfile> ProfileDamage =
            new()
            {
                [MWRNaturalWeaponProfile.Claw] = new NaturalWeaponDamageProfile(multiplier: 3.5f, minimumDamage: 100f),
                [MWRNaturalWeaponProfile.Succubus] = new NaturalWeaponDamageProfile(multiplier: 2.0f, minimumDamage: 150f),
                [MWRNaturalWeaponProfile.Warden] = new NaturalWeaponDamageProfile(multiplier: 2.25f, minimumDamage: 200f),
            };

        public static bool TryGetUnarmedProfile(
            BasicCharacterObject attackerCharacter,
            in AttackInformation attackInformation,
            in AttackCollisionData collisionData,
            out MWRNaturalWeaponProfile profile)
        {
            profile = default;

            if (!IsEligible(attackerCharacter) || !IsTrueUnarmedHit(attackerCharacter, attackInformation, collisionData))
            {
                return false;
            }

            return TryGetProfile(attackerCharacter, out profile);
        }

        public static bool IsEligible(BasicCharacterObject character)
        {
            return TryGetProfile(character, out _);
        }

        public static bool TryGetProfile(BasicCharacterObject character, out MWRNaturalWeaponProfile profile)
        {
            profile = default;

            if (character == null)
            {
                return false;
            }

            string characterId = character.StringId ?? string.Empty;
            if (ExplicitExclusions.Contains(characterId))
            {
                return false;
            }

            if (ExplicitProfileOverrides.TryGetValue(characterId, out profile))
            {
                return true;
            }

            string cultureId = character.Culture?.StringId ?? string.Empty;
            return DefaultCultureProfiles.TryGetValue(cultureId, out profile);
        }

        public static bool IsExplicitlyAdded(BasicCharacterObject character)
        {
            return character != null &&
                   ExplicitProfileOverrides.ContainsKey(character.StringId ?? string.Empty);
        }

        public static bool IsExplicitlyExcluded(BasicCharacterObject character)
        {
            return character != null &&
                   ExplicitExclusions.Contains(character.StringId ?? string.Empty);
        }

        public static bool ShouldPreventBlocking(Agent agent)
        {
            if (agent == null || agent.IsMount || agent.Controller != AgentControllerType.AI)
            {
                return false;
            }

            BasicCharacterObject character = agent.Character;
            if (!IsExplicitlyAdded(character) || IsExplicitlyExcluded(character))
            {
                return false;
            }

            return agent.WieldedWeapon.IsEmpty && agent.WieldedOffhandWeapon.IsEmpty;
        }

        public static float ApplyProfileDamage(MWRNaturalWeaponProfile profile, float baseDamage)
        {
            if (!ProfileDamage.TryGetValue(profile, out NaturalWeaponDamageProfile damageProfile))
            {
                return baseDamage;
            }

            float scaledDamage = baseDamage * damageProfile.Multiplier;
            return Math.Max(scaledDamage, damageProfile.MinimumDamage);
        }

        private static bool IsTrueUnarmedHit(
            BasicCharacterObject attackerCharacter,
            in AttackInformation attackInformation,
            in AttackCollisionData collisionData)
        {
            if (collisionData.IsMissile || collisionData.IsAlternativeAttack || collisionData.IsHorseCharge || collisionData.IsFallDamage)
            {
                return false;
            }

            if (!attackInformation.AttackerWeapon.IsEmpty)
            {
                return false;
            }

            int affectorSlot = collisionData.AffectorWeaponSlotOrMissileIndex;
            if (affectorSlot == (int)EquipmentIndex.None)
            {
                return true;
            }

            if (affectorSlot < (int)EquipmentIndex.WeaponItemBeginSlot || affectorSlot >= (int)EquipmentIndex.NumAllWeaponSlots)
            {
                return true;
            }

            Equipment equipment = attackerCharacter.Equipment;
            EquipmentElement wieldedElement = equipment[(EquipmentIndex)affectorSlot];
            return wieldedElement.IsEmpty;
        }
    }
}
