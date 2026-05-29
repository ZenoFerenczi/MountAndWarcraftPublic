using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace MWRMode.CharacterCreationContent
{
    public static class MWRRaceRules
    {
        private static readonly Dictionary<string, HashSet<int>> AllowedRacesByCulture = new Dictionary<string, HashSet<int>>
        {
            // Replace these race IDs with your real ones
            { "empire", new HashSet<int> { 0 } },       // humans only
            { "aserai", new HashSet<int> { 5 } },   // orcs + goblins
            { "nord", new HashSet<int> { 0 } },         // undead only
            { "khuzait", new HashSet<int> { 2 } },         // undead only
            { "battania", new HashSet<int> { 4 } },         // high elves
            { "sturgia", new HashSet<int> { 6 } },         // undead only
            { "vlandia", new HashSet<int> { 0 } },         // undead only
        };

        private static readonly Dictionary<int, string> DefaultMaleBodyByRace = new Dictionary<int, string>
        {
            { 0, "<BodyProperties version='4' age='25.84' weight='0.5000' build='0.5000' key='001CB80CC000300D7C7664876753888A7577866254C69643C4B647398C95A0370077760307A7497300000000000000000000000000000000000000003AF47002'/>" },
            { 1, "<BodyProperties version='4' age='25.84' weight='0.7000' build='0.7000' key='001CB80CC000300D7C7664876753888A7577866254C69643C4B647398C95A0370077760307A7497300000000000000000000000000000000000000003AF47002'/>" },
            { 2, "<BodyProperties version='4' age='25.84' weight='0.3000' build='0.3000' key='001CB80CC000300D7C7664876753888A7577866254C69643C4B647398C95A0370077760307A7497300000000000000000000000000000000000000003AF47002'/>" },
            { 3, "<BodyProperties version='4' age='25.84' weight='0.5000' build='0.5000' key='0028C80FC000100DBA756445533377873CD1833B3101B44A21C3C5347CA32C260F7776F20BBC35E8000000000000000000000000000000000000000042F41002'/>" },
            { 4, "<BodyProperties version='4' age='25.84' weight='0.5000' build='0.5000' key='001CB80CC000300D7C7664876753888A7577866254C69643C4B647398C95A0370077760307A7497300000000000000000000000000000000000000003AF47002'/>" },
            { 5, "<BodyProperties version='4' age='25.84' weight='0.5000' build='0.5000' key='0028C80FC000100DBA756445533377873CD1833B3101B44A21C3C5347CA32C260F7776F20BBC35E8000000000000000000000000000000000000000042F41002'/>" },
            { 6, "<BodyProperties version='4' age='25.84' weight='0.5000' build='0.5000' key='0028C80FC000100DBA756445533377873CD1833B3101B44A21C3C5347CA32C260F7776F20BBC35E8000000000000000000000000000000000000000042F41002'/>" },
            { 7, "<BodyProperties version='4' age='25.84' weight='0.5000' build='0.5000' key='0028C80FC000100DBA756445533377873CD1833B3101B44A21C3C5347CA32C260F7776F20BBC35E8000000000000000000000000000000000000000042F41002'/>" },

        };

        private static readonly Dictionary<int, string> DefaultFemaleBodyByRace = new Dictionary<int, string>
        {
            { 0, "<BodyProperties version='4' age='25.84' weight='0.5000' build='0.5000' key='001CB80CC000300D7C7664876753888A7577866254C69643C4B647398C95A0370077760307A7497300000000000000000000000000000000000000003AF47002'/>" },
            { 1, "<BodyProperties version='4' age='25.84' weight='0.7000' build='0.7000' key='001CB80CC000300D7C7664876753888A7577866254C69643C4B647398C95A0370077760307A7497300000000000000000000000000000000000000003AF47002'/>" },
            { 2, "<BodyProperties version='4' age='25.84' weight='0.3000' build='0.3000' key='001CB80CC000300D7C7664876753888A7577866254C69643C4B647398C95A0370077760307A7497300000000000000000000000000000000000000003AF47002'/>" },
            { 3, "<BodyProperties version='4' age='25.84' weight='0.5000' build='0.5000' key='0028C80FC000100DBA756445533377873CD1833B3101B44A21C3C5347CA32C260F7776F20BBC35E8000000000000000000000000000000000000000042F41002'/>" },
            { 4, "<BodyProperties version='4' age='25.84' weight='0.5000' build='0.5000' key='001CB80CC000300D7C7664876753888A7577866254C69643C4B647398C95A0370077760307A7497300000000000000000000000000000000000000003AF47002'/>" },
            { 5, "<BodyProperties version='4' age='25.84' weight='0.5000' build='0.5000' key='0028C80FC000100DBA756445533377873CD1833B3101B44A21C3C5347CA32C260F7776F20BBC35E8000000000000000000000000000000000000000042F41002'/>" },
            { 6, "<BodyProperties version='4' age='25.84' weight='0.5000' build='0.5000' key='0028C80FC000100DBA756445533377873CD1833B3101B44A21C3C5347CA32C260F7776F20BBC35E8000000000000000000000000000000000000000042F41002'/>" },
            { 7, "<BodyProperties version='4' age='25.84' weight='0.5000' build='0.5000' key='0028C80FC000100DBA756445533377873CD1833B3101B44A21C3C5347CA32C260F7776F20BBC35E8000000000000000000000000000000000000000042F41002'/>" },

        };

        public static bool IsRaceAllowedForCulture(string cultureId, int raceId)
        {
            if (string.IsNullOrEmpty(cultureId))
                return true;

            if (!AllowedRacesByCulture.TryGetValue(cultureId, out HashSet<int> allowed))
                return true;

            return allowed.Contains(raceId);
        }

        public static int GetFallbackRaceForCulture(string cultureId)
        {
            if (!AllowedRacesByCulture.TryGetValue(cultureId, out HashSet<int> allowed) || allowed.Count == 0)
                return 0;

            foreach (int race in allowed)
                return race;

            return 0;
        }

        public static bool TryGetDefaultBodyForRace(int raceId, bool isFemale, out BodyProperties properties)
        {
            properties = default;

            string bodyString;
            bool found = isFemale
                ? DefaultFemaleBodyByRace.TryGetValue(raceId, out bodyString)
                : DefaultMaleBodyByRace.TryGetValue(raceId, out bodyString);

            if (!found || string.IsNullOrEmpty(bodyString))
                return false;

            return BodyProperties.FromString(bodyString, out properties);
        }
    }
}
