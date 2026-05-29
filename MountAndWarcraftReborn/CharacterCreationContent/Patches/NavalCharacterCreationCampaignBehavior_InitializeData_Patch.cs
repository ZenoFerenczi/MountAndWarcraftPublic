using HarmonyLib;
using NavalDLC.CampaignBehaviors;
using TaleWorlds.CampaignSystem.CharacterCreationContent;

namespace MWRMode.Patches.CharacterCreation
{
    [HarmonyPatch(typeof(NavalCharacterCreationCampaignBehavior), "InitializeData")]
    public static class NavalCharacterCreationCampaignBehavior_InitializeData_Patch
    {
        [HarmonyPrefix]
        private static bool Prefix(CharacterCreationManager characterCreationManager)
        {
            // Skip War Sails extra parent/childhood/education/youth character creation options.
            // Nord culture itself is still added earlier in InitializeCharacterCreationCultures().
            return false;
        }
    }
}