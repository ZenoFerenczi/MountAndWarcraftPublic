using HarmonyLib;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.Tableaus;

namespace MWRMode.Patches.CharacterCreation
{
    [HarmonyPatch(typeof(CharacterTableau), nameof(CharacterTableau.SetRace))]
    public static class CharacterTableau_SetRace_Patch
    {
        [HarmonyPostfix]
        private static void Postfix(CharacterTableau __instance)
        {
            var agentVisuals =
                AccessTools.Field(typeof(CharacterTableau), "_agentVisuals")?.GetValue(__instance) as AgentVisuals;
            agentVisuals?.Reset();

            var oldAgentVisuals =
                AccessTools.Field(typeof(CharacterTableau), "_oldAgentVisuals")?.GetValue(__instance) as AgentVisuals;
            oldAgentVisuals?.Reset();

            AccessTools.Method(typeof(CharacterTableau), "InitializeAgentVisuals")
                ?.Invoke(__instance, new object[] { });
        }
    }
}