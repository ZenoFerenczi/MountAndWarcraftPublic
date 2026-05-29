using System;
using System.Xml.Serialization;

namespace MountAndWarcraftReborn.Magic
{
    [Serializable]
    public class MWRMagicSpellTemplate
    {
        [XmlAttribute("id")]
        public string StringId { get; set; } = string.Empty;

        [XmlAttribute("name")]
        public string Name { get; set; } = string.Empty;

        [XmlAttribute("description")]
        public string Description { get; set; } = string.Empty;

        [XmlAttribute("class")]
        public MWRMagicClassId ClassId { get; set; } = MWRMagicClassId.None;

        [XmlAttribute("short_name")]
        public string ShortName { get; set; } = string.Empty;

        [XmlAttribute("mana_cost")]
        public int ManaCost { get; set; }

        [XmlAttribute("cooldown")]
        public float CooldownSeconds { get; set; } = 5f;

        [XmlAttribute("cast_type")]
        public MWRMagicCastType CastType { get; set; } = MWRMagicCastType.Instant;

        [XmlAttribute("cast_time")]
        public float CastTime { get; set; }

        [XmlAttribute("range")]
        public float Range { get; set; } = 12f;

        [XmlAttribute("min_range")]
        public float MinRange { get; set; }

        [XmlAttribute("radius")]
        public float Radius { get; set; } = 4f;

        [XmlAttribute("target_capture_radius")]
        public float TargetCaptureRadius { get; set; } = 2.5f;

        [XmlAttribute("duration")]
        public float Duration { get; set; }

        [XmlAttribute("tick_interval")]
        public float TickInterval { get; set; } = 1f;

        [XmlAttribute("power")]
        public int Power { get; set; } = 20;

        [XmlAttribute("effect")]
        public MWRMagicSpellEffectType EffectType { get; set; } = MWRMagicSpellEffectType.Damage;

        [XmlAttribute("script")]
        public MWRMagicAbilityScriptType ScriptType { get; set; } = MWRMagicAbilityScriptType.SingleTargetDamage;

        [XmlAttribute("target")]
        public MWRMagicSpellTargetType TargetType { get; set; } = MWRMagicSpellTargetType.ClosestEnemy;

        [XmlAttribute("crosshair")]
        public MWRMagicCrosshairType CrosshairType { get; set; } = MWRMagicCrosshairType.SingleTarget;

        [XmlAttribute("projectile_item")]
        public string ProjectileItemId { get; set; } = string.Empty;

        [XmlAttribute("projectile_speed")]
        public float ProjectileSpeed { get; set; } = 28f;

        [XmlAttribute("animation")]
        public string AnimationActionName { get; set; } = string.Empty;

        [XmlAttribute("particle")]
        public string ParticleEffectPrefab { get; set; } = string.Empty;

        [XmlAttribute("sound")]
        public string SoundEffectId { get; set; } = string.Empty;

        [XmlAttribute("has_light")]
        public bool HasLight { get; set; }

        [XmlAttribute("light_radius")]
        public float LightRadius { get; set; } = 4f;

        [XmlAttribute("light_intensity")]
        public float LightIntensity { get; set; } = 1f;

        [XmlIgnore]
        public bool RequiresTargeting => CrosshairType != MWRMagicCrosshairType.Self;

        [XmlIgnore]
        public string DisplayShortName => string.IsNullOrWhiteSpace(ShortName) ? Name : ShortName;
    }
}
