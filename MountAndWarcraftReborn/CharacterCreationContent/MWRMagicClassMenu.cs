using System.Collections.Generic;
using MountAndWarcraftReborn.Magic;
using MountAndWarcraftReborn.Magic.Campaign;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.CharacterCreationContent;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace MWRMode.CharacterCreationContent
{
    public class MWRMagicClassMenu : CharacterCreationCampaignBehavior, ICharacterCreationContentHandler
    {
        public void AddMagicClassSelectionMenu(CharacterCreationManager characterCreationManager)
        {
            BodyProperties bodyProperties = MWRCharacterCreationFaceGenHelper.GetAgedPreviewBodyProperties(
                characterCreationManager,
                characterCreationManager.CharacterCreationContent.StartingAge);

            List<NarrativeMenuCharacter> characters = new List<NarrativeMenuCharacter>
            {
                new NarrativeMenuCharacter(
                    "player_magic_class_selection_character",
                    bodyProperties,
                    MWRCharacterCreationFaceGenHelper.GetPreviewRace(characterCreationManager),
                    CharacterObject.PlayerCharacter.IsFemale)
            };

            NarrativeMenu narrativeMenu = new NarrativeMenu(
                "narrative_magic_class_selection_menu",
                "narrative_age_selection_menu",
                string.Empty,
                new TextObject("{=mwr_magic_class_title}Magic Discipline"),
                new TextObject("{=mwr_magic_class_desc}If your hero walks a magical path, choose it now. You can also begin as a non-caster and ignore magic entirely."),
                characters,
                GetMagicClassNarrativeCharacters);

            AddOption(narrativeMenu, "none", "No Magic", "Remain a non-caster for now.", MWRMagicClassId.None);
            AddOption(narrativeMenu, "mage", "Mage", "Arcane battlefield caster focused on destructive spellcraft.", MWRMagicClassId.Mage);
            AddOption(narrativeMenu, "priest", "Priest", "Holy caster focused on healing and light magic.", MWRMagicClassId.Priest);
            AddOption(narrativeMenu, "shaman", "Shaman", "Elemental battlemage who balances lightning and restorative magic.", MWRMagicClassId.Shaman);
            AddOption(narrativeMenu, "warlock", "Warlock", "Fel and shadow caster who drains life from enemies.", MWRMagicClassId.Warlock);

            characterCreationManager.AddNewMenu(narrativeMenu);
        }

        private List<NarrativeMenuCharacterArgs> GetMagicClassNarrativeCharacters(CultureObject culture, string occupationType, CharacterCreationManager characterCreationManager)
        {
            string playerEquipmentId = "player_char_creation_" +
                                       characterCreationManager.CharacterCreationContent.SelectedCulture.StringId +
                                       "_" +
                                       characterCreationManager.CharacterCreationContent.SelectedTitleType +
                                       "_MWR_" +
                                       (Hero.MainHero.IsFemale ? "f" : "m");

            return new List<NarrativeMenuCharacterArgs>
            {
                new NarrativeMenuCharacterArgs(
                    "player_magic_class_selection_character",
                    characterCreationManager.CharacterCreationContent.StartingAge,
                    playerEquipmentId,
                    "act_inventory_idle_start",
                    "spawnpoint_player_1",
                    string.Empty,
                    string.Empty,
                    null,
                    true,
                    CharacterObject.PlayerCharacter.IsFemale)
            };
        }

        private static void AddOption(
            NarrativeMenu narrativeMenu,
            string optionId,
            string title,
            string description,
            MWRMagicClassId classId)
        {
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption(
                $"magic_class_{optionId}_option",
                new TextObject(title),
                new TextObject(description),
                GetOptionArgs,
                AlwaysAvailable,
                manager => OnSelect(manager, classId),
                manager => OnConsequence(manager, classId)));
        }

        private static void GetOptionArgs(NarrativeMenuOptionArgs args)
        {
        }

        private static bool AlwaysAvailable(CharacterCreationManager characterCreationManager)
        {
            return true;
        }

        private static void OnSelect(CharacterCreationManager characterCreationManager, MWRMagicClassId classId)
        {
            MWRCharacterCreationMagicSelection.SetPendingClass(classId);
        }

        private static void OnConsequence(CharacterCreationManager characterCreationManager, MWRMagicClassId classId)
        {
            MWRCharacterCreationMagicSelection.SetPendingClass(classId);
        }
    }
}
