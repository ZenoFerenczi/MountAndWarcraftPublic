using HarmonyLib;
using TaleWorlds.CampaignSystem.CharacterCreationContent;

namespace MWRMode.Patches.CharacterCreation
{
    [HarmonyPatch(typeof(NarrativeMenuCharacter), "SetRightHandItem")]
    internal static class NarrativeMenuCharacter_SetRightHandItem_Patch
    {
        private static bool Prefix(NarrativeMenuCharacter __instance)
        {
            return true;
        }
    }

    [HarmonyPatch(typeof(NarrativeMenuCharacter), "SetLeftHandItem")]
    internal static class NarrativeMenuCharacter_SetLeftHandItem_Patch
    {
        private static bool Prefix(NarrativeMenuCharacter __instance)
        {
            return true;
        }
    }
}
