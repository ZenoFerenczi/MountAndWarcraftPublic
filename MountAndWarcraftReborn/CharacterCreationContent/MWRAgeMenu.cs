using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.CharacterCreationContent;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace MWRMode.CharacterCreationContent
{
    public class MWRAgeMenu : CharacterCreationCampaignBehavior, ICharacterCreationContentHandler
    {
        public string GetPlayerEquipmentId(CharacterCreationManager characterCreationManager, string occupationType, string cultureId, bool isFemale)
        {
            return "player_char_creation_" + cultureId + "_" + occupationType + "_MWR_" + (isFemale ? "f" : "m");
        }
        private List<NarrativeMenuCharacterArgs> GetAgeSelectionMenuNarrativeMenuCharacterArgs(CultureObject culture, string occupationType, CharacterCreationManager characterCreationManager)
        {
            List<NarrativeMenuCharacterArgs> list = new List<NarrativeMenuCharacterArgs>();
            string playerEquipmentId = this.GetPlayerEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedTitleType, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId, Hero.MainHero.IsFemale);
            list.Add(new NarrativeMenuCharacterArgs("player_age_selection_character", characterCreationManager.CharacterCreationContent.StartingAge, playerEquipmentId, "act_childhood_schooled", "spawnpoint_player_1", "", "", null, true, CharacterObject.PlayerCharacter.IsFemale));
            MBEquipmentRoster @object = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(playerEquipmentId);
            ItemObject item = @object.DefaultEquipment[EquipmentIndex.ArmorItemEndSlot].Item;
            list.Add(new NarrativeMenuCharacterArgs("narrative_character_horse", -1, "", "act_horse_stand_1", "spawnpoint_mount_1", @object.DefaultEquipment[EquipmentIndex.ArmorItemEndSlot].Item.StringId, @object.DefaultEquipment[EquipmentIndex.HorseHarness].Item.StringId, MountCreationKey.GetRandomMountKey(item, CharacterObject.PlayerCharacter.GetMountKeySeed()), false, false));
            return list;
        }
        public void AddAgeSelectionMenu(CharacterCreationManager characterCreationManager)
        {
            MBTextManager.SetTextVariable("EXP_VALUE", 10);
            BodyProperties bodyProperties = MWRCharacterCreationFaceGenHelper.GetAgedPreviewBodyProperties(
                characterCreationManager,
                (float)characterCreationManager.CharacterCreationContent.StartingAge);
            List<NarrativeMenuCharacter> list = new List<NarrativeMenuCharacter>();
            int previewRace = MWRCharacterCreationFaceGenHelper.GetPreviewRace(characterCreationManager);
            list.Add(new NarrativeMenuCharacter("player_age_selection_character", bodyProperties, previewRace, CharacterObject.PlayerCharacter.IsFemale));
            list.Add(new NarrativeMenuCharacter("narrative_character_horse"));
            NarrativeMenu narrativeMenu = new NarrativeMenu("narrative_age_selection_menu", "narrative_adulthood_menu", "narrative_magic_class_selection_menu", new TextObject("{=HDFEAYDk}Starting Age", null), new TextObject("{=VlOGrGSn}Your character started off on the adventuring path at the age of...", null), list, new NarrativeMenu.GetNarrativeMenuCharacterArgsDelegate(this.GetAgeSelectionMenuNarrativeMenuCharacterArgs));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("age_selection_young_adult_option", new TextObject("{=!}21", null), new TextObject("{=2k7adlh7}While lacking experience a bit, you are full with youthful energy, you are fully eager, for the long years of adventuring ahead.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetAgeSelectionYoungAdultAgeOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.AgeSelectionYoungAdultAgeOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.AgeSelectionYoungAdultAgeOptionOnSelect), new NarrativeMenuOptionOnConsequenceDelegate(this.AgeSelectionYoungAdultAgeOptionOnConsequence)));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("age_selection_adult_option", new TextObject("{=!}30", null), new TextObject("{=NUlVFRtK}You are at your prime, You still have some youthful energy but also have a substantial amount of experience under your belt. ", null), new GetNarrativeMenuOptionArgsDelegate(this.GetAgeSelectionAdultOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.AgeSelectionAdultOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.AgeSelectionAdultOptionOnSelect), new NarrativeMenuOptionOnConsequenceDelegate(this.AgeSelectionAdultOptionOnConsequence)));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("age_selection_middle_age_option", new TextObject("{=!}40", null), new TextObject("{=5MxTYApM}This is the right age for starting off, you have years of experience, and you are old enough for people to respect you and gather under your banner.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetAgeSelectionMiddleAgeOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.AgeSelectionMiddleAgeOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.AgeSelectionMiddleAgeOptionOnSelect), new NarrativeMenuOptionOnConsequenceDelegate(this.AgeSelectionMiddleAgeOptionOnConsequence)));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("age_selection_elder_option", new TextObject("{=!}50", null), new TextObject("{=ePD5Afvy}While you are past your prime, there is still enough time to go on that last big adventure for you. And you have all the experience you need to overcome anything!", null), new GetNarrativeMenuOptionArgsDelegate(this.GetAgeSelectionElderOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.AgeSelectionElderOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.AgeSelectionElderOptionOnSelect), new NarrativeMenuOptionOnConsequenceDelegate(this.AgeSelectionElderOptionOnConsequence)));
            characterCreationManager.AddNewMenu(narrativeMenu);
        }

        public void GetAgeSelectionYoungAdultAgeOptionArgs(NarrativeMenuOptionArgs args)
        {
            args.SetUnspentFocusToAdd(2);
            args.SetUnspentAttributeToAdd(1);
        }

        public bool AgeSelectionYoungAdultAgeOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return true;
        }

        public void AgeSelectionYoungAdultAgeOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            string playerEquipmentId = this.GetPlayerEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedTitleType, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId, Hero.MainHero.IsFemale);
            foreach (NarrativeMenuCharacter narrativeMenuCharacter in characterCreationManager.CurrentMenu.Characters)
            {
                if (narrativeMenuCharacter.StringId == "player_age_selection_character")
                {
                    narrativeMenuCharacter.SetAnimationId("act_childhood_focus");
                    narrativeMenuCharacter.ChangeAge(21f);
                    MBEquipmentRoster @object = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(playerEquipmentId);
                    if (@object == null)
                    {
                        Debug.FailedAssert("character creation menu character equipment should not be null!", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\CampaignBehaviors\\CharacterCreationCampaignBehavior.cs", "AgeSelectionYoungAdultAgeOptionOnSelect", 4884);
                        @object = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>("player_char_creation_default");
                    }
                    narrativeMenuCharacter.SetEquipment(@object);
                    break;
                }
            }
            characterCreationManager.CharacterCreationContent.StartingAge = 21;
            Hero.MainHero.SetBirthDay(CampaignTime.YearsFromNow(-21f));
        }

        public void AgeSelectionYoungAdultAgeOptionOnConsequence(CharacterCreationManager characterCreationManager)
        {
            characterCreationManager.CharacterCreationContent.StartingAge = 21;
            this.ApplyMainHeroEquipment(characterCreationManager);
        }

        public void GetAgeSelectionAdultOptionArgs(NarrativeMenuOptionArgs args)
        {
            args.SetUnspentFocusToAdd(4);
            args.SetUnspentAttributeToAdd(2);
        }

        public bool AgeSelectionAdultOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return true;
        }

        public void AgeSelectionAdultOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            string playerEquipmentId = this.GetPlayerEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedTitleType, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId, Hero.MainHero.IsFemale);
            foreach (NarrativeMenuCharacter narrativeMenuCharacter in characterCreationManager.CurrentMenu.Characters)
            {
                if (narrativeMenuCharacter.StringId == "player_age_selection_character")
                {
                    narrativeMenuCharacter.SetAnimationId("act_childhood_athlete");
                    narrativeMenuCharacter.ChangeAge(30f);
                    MBEquipmentRoster @object = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(playerEquipmentId);
                    if (@object == null)
                    {
                        Debug.FailedAssert("character creation menu character equipment should not be null!", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\CampaignBehaviors\\CharacterCreationCampaignBehavior.cs", "AgeSelectionAdultOptionOnSelect", 4934);
                        @object = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>("player_char_creation_default");
                    }
                    narrativeMenuCharacter.SetEquipment(@object);
                    break;
                }
            }
            characterCreationManager.CharacterCreationContent.StartingAge = 30;
            Hero.MainHero.SetBirthDay(CampaignTime.YearsFromNow(-30f));
        }

        public void AgeSelectionAdultOptionOnConsequence(CharacterCreationManager characterCreationManager)
        {
            characterCreationManager.CharacterCreationContent.StartingAge = 30;
            this.ApplyMainHeroEquipment(characterCreationManager);
        }

        public void GetAgeSelectionMiddleAgeOptionArgs(NarrativeMenuOptionArgs args)
        {
            args.SetUnspentFocusToAdd(6);
            args.SetUnspentAttributeToAdd(3);
        }

        public bool AgeSelectionMiddleAgeOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return true;
        }

        public void AgeSelectionMiddleAgeOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            string playerEquipmentId = this.GetPlayerEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedTitleType, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId, Hero.MainHero.IsFemale);
            foreach (NarrativeMenuCharacter narrativeMenuCharacter in characterCreationManager.CurrentMenu.Characters)
            {
                if (narrativeMenuCharacter.StringId == "player_age_selection_character")
                {
                    narrativeMenuCharacter.SetAnimationId("act_childhood_sharp");
                    narrativeMenuCharacter.ChangeAge(30f);
                    MBEquipmentRoster @object = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(playerEquipmentId);
                    if (@object == null)
                    {
                        Debug.FailedAssert("character creation menu character equipment should not be null!", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\CampaignBehaviors\\CharacterCreationCampaignBehavior.cs", "AgeSelectionMiddleAgeOptionOnSelect", 4984);
                        @object = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>("player_char_creation_default");
                    }
                    narrativeMenuCharacter.SetEquipment(@object);
                    break;
                }
            }
            characterCreationManager.CharacterCreationContent.StartingAge = 40;
            Hero.MainHero.SetBirthDay(CampaignTime.YearsFromNow(-40f));
        }

        public void AgeSelectionMiddleAgeOptionOnConsequence(CharacterCreationManager characterCreationManager)
        {
            characterCreationManager.CharacterCreationContent.StartingAge = 40;
            this.ApplyMainHeroEquipment(characterCreationManager);
        }

        public void GetAgeSelectionElderOptionArgs(NarrativeMenuOptionArgs args)
        {
            args.SetUnspentFocusToAdd(8);
            args.SetUnspentAttributeToAdd(4);
        }

        public bool AgeSelectionElderOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return true;
        }

        public void AgeSelectionElderOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            string playerEquipmentId = this.GetPlayerEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedTitleType, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId, Hero.MainHero.IsFemale);
            foreach (NarrativeMenuCharacter narrativeMenuCharacter in characterCreationManager.CurrentMenu.Characters)
            {
                if (narrativeMenuCharacter.StringId == "player_age_selection_character")
                {
                    narrativeMenuCharacter.SetAnimationId("act_childhood_tough");
                    narrativeMenuCharacter.ChangeAge(50f);
                    MBEquipmentRoster @object = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(playerEquipmentId);
                    if (@object == null)
                    {
                        Debug.FailedAssert("character creation menu character equipment should not be null!", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\CampaignBehaviors\\CharacterCreationCampaignBehavior.cs", "AgeSelectionElderOptionOnSelect", 5034);
                        @object = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>("player_char_creation_default");
                    }
                    narrativeMenuCharacter.SetEquipment(@object);
                    break;
                }
            }
            characterCreationManager.CharacterCreationContent.StartingAge = 50;
            Hero.MainHero.SetBirthDay(CampaignTime.YearsFromNow(-50f));
        }

        public void AgeSelectionElderOptionOnConsequence(CharacterCreationManager characterCreationManager)
        {
            characterCreationManager.CharacterCreationContent.StartingAge = 50;
            this.ApplyMainHeroEquipment(characterCreationManager);
        }

        public void ApplyMainHeroEquipment(CharacterCreationManager characterCreationManager)
        {
            NarrativeMenu narrativeMenuWithId = characterCreationManager.GetNarrativeMenuWithId("narrative_age_selection_menu");
            NarrativeMenuCharacter narrativeMenuCharacter = null;
            foreach (NarrativeMenuCharacter narrativeMenuCharacter2 in narrativeMenuWithId.Characters)
            {
                if (narrativeMenuCharacter2.StringId.Equals("player_age_selection_character"))
                {
                    narrativeMenuCharacter = narrativeMenuCharacter2;
                    break;
                }
            }
            CharacterObject.PlayerCharacter.Equipment.FillFrom(narrativeMenuCharacter.Equipment.DefaultEquipment, true);
            CharacterObject.PlayerCharacter.FirstCivilianEquipment.FillFrom(narrativeMenuCharacter.Equipment.GetRandomCivilianEquipment(), true);
        }
    }
}
