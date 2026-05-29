using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace MountAndWarcraftReborn.Magic
{
    [Serializable]
    public class MWRMagicAssignment
    {
        [XmlAttribute("character_id")]
        public string CharacterId { get; set; } = string.Empty;

        [XmlAttribute("class")]
        public MWRMagicClassId ClassId { get; set; } = MWRMagicClassId.None;

        [XmlAttribute("known_spell_ids")]
        public string KnownSpellIdsRaw { get; set; } = string.Empty;

        [XmlAttribute("selected_spell_ids")]
        public string SelectedSpellIdsRaw { get; set; } = string.Empty;

        public IReadOnlyList<string> GetKnownSpellIds()
        {
            return SplitIds(KnownSpellIdsRaw);
        }

        public IReadOnlyList<string> GetSelectedSpellIds()
        {
            return SplitIds(SelectedSpellIdsRaw);
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
