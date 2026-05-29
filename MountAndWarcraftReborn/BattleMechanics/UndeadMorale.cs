using HarmonyLib;
using TaleWorlds.MountAndBlade;
using TaleWorlds.DotNet;

namespace MountAndWarcraftReborn.BattleMechanics
{
    public static class MWRUndeadMoraleHelper
    {
        private const string UndeadCultureId = "nord";

        public static bool IsUndeadAgent(Agent agent)
        {
            if (agent == null || agent.Character == null || agent.Character.Culture == null)
                return false;

            return agent.Character.Culture.StringId == UndeadCultureId;
        }
    }

}
