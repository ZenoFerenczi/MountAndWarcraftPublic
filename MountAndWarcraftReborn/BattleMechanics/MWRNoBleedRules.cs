using System;
using System.Collections.Generic;
using TaleWorlds.MountAndBlade;

namespace MountAndWarcraftReborn.BattleMechanics
{
    public static class MWRNoBleedRules
    {
        private static readonly HashSet<string> NoBleedCultureIds = new(StringComparer.OrdinalIgnoreCase)
        {
            "nord",
        };

        private static readonly HashSet<string> BleedExceptionTroopIds = new(StringComparer.OrdinalIgnoreCase)
        {
            // Add undead troop ids here if a specific troop should still bleed.
            // Example:
            // "my_special_undead_troop",
        };

        public static bool ShouldNotBleed(Agent agent)
        {
            if (agent?.Character == null)
            {
                return false;
            }

            if (BleedExceptionTroopIds.Contains(agent.Character.StringId))
            {
                return false;
            }

            string cultureId = agent.Character.Culture?.StringId;
            return cultureId != null && NoBleedCultureIds.Contains(cultureId);
        }
    }
}
