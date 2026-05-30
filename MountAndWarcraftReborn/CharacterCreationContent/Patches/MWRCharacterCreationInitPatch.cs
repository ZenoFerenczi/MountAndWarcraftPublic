using HarmonyLib;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.CharacterCreationContent;
using TaleWorlds.Localization;
using MWRMode.CharacterCreationContent;

namespace MWRMode.Patches.CharacterCreation
{
    [HarmonyPatch(typeof(CharacterCreationCampaignBehavior), nameof(CharacterCreationCampaignBehavior.InitializeData))]
    public static class MWRCharacterCreationInitPatch
    {
        private const bool EnablePlayerMagicSelectionMenu = true;

        [HarmonyPrefix]
        private static bool Prefix(CharacterCreationManager characterCreationManager)
        {
            characterCreationManager.CharacterCreationContent.ChangeReviewPageDescription(
                new TextObject("{=W6pKpEoT}You prepare to set off for a grand adventure in Azeroth! Here is your character. Continue if you are ready, or go back to make changes."));

            var parentsMenu = new MWRParentsMenu();
            var childhoodMenu = new MWRChildhoodMenu();
            var educationMenu = new MWREducationMenu();
            var youthMenu = new MWRYouthMenu();
            var adulthoodMenu = new MWRAdulthoodMenu();
            var ageMenu = new MWRAgeMenu();

            parentsMenu.AddParentsMenu(characterCreationManager);
            childhoodMenu.AddChildhoodMenu(characterCreationManager);
            educationMenu.AddEducationMenu(characterCreationManager);
            youthMenu.AddYouthMenu(characterCreationManager);
            adulthoodMenu.AddAdulthoodMenu(characterCreationManager);
            ageMenu.AddAgeSelectionMenu(characterCreationManager);
            if (EnablePlayerMagicSelectionMenu)
            {
                var magicClassMenu = new MWRMagicClassMenu();
                magicClassMenu.AddMagicClassSelectionMenu(characterCreationManager);
            }

            return false;
        }
    }
}
