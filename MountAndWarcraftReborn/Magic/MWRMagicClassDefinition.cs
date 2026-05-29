using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace MountAndWarcraftReborn.Magic
{
    [Serializable]
    public class MWRMagicClassDefinition
    {
        [XmlAttribute("id")]
        public MWRMagicClassId ClassId { get; set; } = MWRMagicClassId.None;

        [XmlAttribute("base_max_mana")]
        public float BaseMaxMana { get; set; }

        [XmlAttribute("mana_regen_per_hour")]
        public float ManaRegenPerHour { get; set; }

        [XmlAttribute("mission_troop_mana")]
        public float MissionTroopMana { get; set; }

        [XmlAttribute("starter_spell_ids")]
        public string StarterSpellIdsRaw { get; set; } = string.Empty;

        [XmlAttribute("default_selected_spell_ids")]
        public string DefaultSelectedSpellIdsRaw { get; set; } = string.Empty;

        public IReadOnlyList<string> GetStarterSpellIds()
        {
            return SplitIds(StarterSpellIdsRaw);
        }

        public IReadOnlyList<string> GetDefaultSelectedSpellIds()
        {
            return SplitIds(DefaultSelectedSpellIdsRaw);
        }

        private static IReadOnlyList<string> SplitIds(string raw)
        {
            return string.IsNullOrWhiteSpace(raw)
                ? Array.Empty<string>()
                : raw.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(id => id.Trim())
                    .Where(id => id.Length > 0)
                    .Distinct(StringComparer.Ordinal)
                    .ToList();
        }
    }
}
