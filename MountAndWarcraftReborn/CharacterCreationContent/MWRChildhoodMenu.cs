using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.CharacterCreationContent;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace MWRMode.CharacterCreationContent
{
    public class MWRChildhoodMenu : CharacterCreationCampaignBehavior, ICharacterCreationContentHandler
    {
        public string GetPlayerChildhoodAgeEquipmentId(CharacterCreationManager characterCreationManager, string parentOccupationType, string cultureId, bool isFemale)
        {
            return "player_char_creation_childhood_age_" + cultureId + "_" + parentOccupationType + "_MWR_" + (isFemale ? "f" : "m");
        }

        public List<NarrativeMenuCharacterArgs> GetChildhoodMenuNarrativeMenuCharacterArgs(CultureObject culture, string occupationType, CharacterCreationManager characterCreationManager)
        {
            List<NarrativeMenuCharacterArgs> list = new List<NarrativeMenuCharacterArgs>();
            string playerChildhoodAgeEquipmentId = this.GetPlayerChildhoodAgeEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId, Hero.MainHero.IsFemale);
            list.Add(new NarrativeMenuCharacterArgs("player_childhood_character", 7, playerChildhoodAgeEquipmentId, "act_childhood_schooled", "spawnpoint_player_1", "", "", null, true, CharacterObject.PlayerCharacter.IsFemale));
            return list;
        }
        public void AddChildhoodMenu(CharacterCreationManager characterCreationManager)
        {
            List<NarrativeMenuCharacter> list = new List<NarrativeMenuCharacter>();
            BodyProperties bodyProperties = CharacterObject.PlayerCharacter.GetBodyProperties(CharacterObject.PlayerCharacter.Equipment, -1);
            bodyProperties = FaceGen.GetBodyPropertiesWithAge(ref bodyProperties, 7f);
            list.Add(new NarrativeMenuCharacter("player_childhood_character", bodyProperties, CharacterObject.PlayerCharacter.Race, CharacterObject.PlayerCharacter.IsFemale));
            NarrativeMenu narrativeMenu = new NarrativeMenu("narrative_childhood_menu", "narrative_parent_menu", "narrative_education_menu", new TextObject("{=8Yiwt1z6}Early Childhood", null), new TextObject("{=character_creation_content_16}As a child you were noted for...", null), list, new NarrativeMenu.GetNarrativeMenuCharacterArgsDelegate(this.GetChildhoodMenuNarrativeMenuCharacterArgs));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("childhood_leadership_option", new TextObject("{=kmM68Qx4}your leadership skills.", null), new TextObject("{=FfNwXtii}If the wolf pup gang of your early childhood had an alpha, it was definitely you. All the other kids followed your lead as you decided what to play and where to play, and led them in games and mischief.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetChildhoodLeadershipOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.ChildhoodLeadershipOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.ChildhoodLeadershipOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("childhood_brawn_option", new TextObject("{=5HXS8HEY}your brawn.", null), new TextObject("{=YKzuGc54}You were big, and other children looked to have you around in any scrap with children from a neighboring village. You pushed a plough and threw an axe like an adult.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetChildhoodBrawnOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.ChildhoodBrawnOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.ChildhoodBrawnOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("childhood_detail_option", new TextObject("{=QrYjPUEf}your attention to detail.", null), new TextObject("{=JUSHAPnu}You were quick on your feet and attentive to what was going on around you. Usually you could run away from trouble, though you could give a good account of yourself in a fight with other children if cornered.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetChildhoodDetailOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.ChildhoodDetailOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.ChildhoodDetailOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("childhood_smart_option", new TextObject("{=Y3UcaX74}your aptitude for numbers.", null), new TextObject("{=DFidSjIf}Most children around you had only the most rudimentary education, but you lingered after class to study letters and mathematics. You were fascinated by the marketplace - weights and measures, tallies and accounts, the chatter about profits and losses.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetChildhoodSmartOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.ChildhoodSmartOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.ChildhoodSmartOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("childhood_leader_option", new TextObject("{=GEYzLuwb}your way with people.", null), new TextObject("{=w2TEQq26}You were always attentive to other people, good at guessing their motivations. You studied how individuals were swayed, and tried out what you learned from adults on your friends.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetChildhoodLeaderOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.ChildhoodLeaderOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.ChildhoodLeaderOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("childhood_horse_option", new TextObject("{=MEgLE2kj}your skill with horses.", null), new TextObject("{=ngazFofr}You were always drawn to animals, and spent as much time as possible hanging out in the village stables. You could calm horses, and were sometimes called upon to break in new colts. You learned the basics of veterinary arts, much of which is applicable to humans as well.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetChildhoodHorseOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.ChildhoodHorseOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.ChildhoodHorseOptionOnSelect), null));
            characterCreationManager.AddNewMenu(narrativeMenu);
        }

        public void GetChildhoodLeadershipOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Leadership, DefaultSkills.Tactics };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Cunning, 1);
        }

        public bool ChildhoodLeadershipOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return true;
        }

        public void ChildhoodLeadershipOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            foreach (NarrativeMenuCharacter narrativeMenuCharacter in characterCreationManager.CurrentMenu.Characters)
            {
                if (narrativeMenuCharacter.StringId == "player_childhood_character")
                {
                    narrativeMenuCharacter.SetAnimationId("act_childhood_leader");
                }
            }
        }

        public void GetChildhoodBrawnOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.TwoHanded, DefaultSkills.Throwing };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Vigor, 1);
        }

        public bool ChildhoodBrawnOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return true;
        }

        public void ChildhoodBrawnOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            foreach (NarrativeMenuCharacter narrativeMenuCharacter in characterCreationManager.CurrentMenu.Characters)
            {
                if (narrativeMenuCharacter.StringId == "player_childhood_character")
                {
                    narrativeMenuCharacter.SetAnimationId("act_childhood_athlete");
                }
            }
        }

        public void GetChildhoodDetailOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Athletics, DefaultSkills.Bow };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Control, 1);
        }

        public bool ChildhoodDetailOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return true;
        }

        public void ChildhoodDetailOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            foreach (NarrativeMenuCharacter narrativeMenuCharacter in characterCreationManager.CurrentMenu.Characters)
            {
                if (narrativeMenuCharacter.StringId == "player_childhood_character")
                {
                    narrativeMenuCharacter.SetAnimationId("act_childhood_memory");
                }
            }
        }

        public void GetChildhoodSmartOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Engineering, DefaultSkills.Trade };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Intelligence, 1);
        }

        public bool ChildhoodSmartOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return true;
        }

        public void ChildhoodSmartOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            foreach (NarrativeMenuCharacter narrativeMenuCharacter in characterCreationManager.CurrentMenu.Characters)
            {
                if (narrativeMenuCharacter.StringId == "player_childhood_character")
                {
                    narrativeMenuCharacter.SetAnimationId("act_childhood_numbers");
                }
            }
        }

        public void GetChildhoodLeaderOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Charm, DefaultSkills.Leadership };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Social, 1);
        }

        public bool ChildhoodLeaderOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return true;
        }

        public void ChildhoodLeaderOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            foreach (NarrativeMenuCharacter narrativeMenuCharacter in characterCreationManager.CurrentMenu.Characters)
            {
                if (narrativeMenuCharacter.StringId == "player_childhood_character")
                {
                    narrativeMenuCharacter.SetAnimationId("act_childhood_manners");
                }
            }
        }

        public void GetChildhoodHorseOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Riding, DefaultSkills.Medicine };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Endurance, 1);
        }

        public bool ChildhoodHorseOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return true;
        }

        public void ChildhoodHorseOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            foreach (NarrativeMenuCharacter narrativeMenuCharacter in characterCreationManager.CurrentMenu.Characters)
            {
                if (narrativeMenuCharacter.StringId == "player_childhood_character")
                {
                    narrativeMenuCharacter.SetAnimationId("act_childhood_animals");
                }
            }
        }
    }
}