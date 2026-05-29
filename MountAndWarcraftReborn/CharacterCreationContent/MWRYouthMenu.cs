using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.CharacterCreationContent;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace MWRMode.CharacterCreationContent
{
    public class MWRYouthMenu : CharacterCreationCampaignBehavior, ICharacterCreationContentHandler
    {
        public string GetPlayerEquipmentId(CharacterCreationManager characterCreationManager, string occupationType, string cultureId, bool isFemale)
        {
            return "player_char_creation_" + cultureId + "_" + occupationType + "_MWR_" + (isFemale ? "f" : "m");
        }

        public List<NarrativeMenuCharacterArgs> GetYouthMenuNarrativeMenuCharacterArgs(CultureObject culture, string occupationType, CharacterCreationManager characterCreationManager)
        {
            if (string.IsNullOrEmpty(characterCreationManager.CharacterCreationContent.SelectedTitleType))
            {
                characterCreationManager.CharacterCreationContent.SelectedTitleType = "guard";
            }
            List<NarrativeMenuCharacterArgs> list = new List<NarrativeMenuCharacterArgs>();
            string playerEquipmentId = this.GetPlayerEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedTitleType, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId, Hero.MainHero.IsFemale);
            list.Add(new NarrativeMenuCharacterArgs("player_youth_character", 17, playerEquipmentId, "act_childhood_schooled", "spawnpoint_player_1", "", "", null, true, CharacterObject.PlayerCharacter.IsFemale));
            MBEquipmentRoster @object = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(playerEquipmentId);
            ItemObject item = @object.DefaultEquipment[EquipmentIndex.ArmorItemEndSlot].Item;
            list.Add(new NarrativeMenuCharacterArgs("narrative_character_horse", -1, "", "act_inventory_idle_start", "spawnpoint_mount_1", @object.DefaultEquipment[EquipmentIndex.ArmorItemEndSlot].Item.StringId, @object.DefaultEquipment[EquipmentIndex.HorseHarness].Item.StringId, MountCreationKey.GetRandomMountKey(item, CharacterObject.PlayerCharacter.GetMountKeySeed()), false, false));
            return list;
        }

        public void AddYouthMenu(CharacterCreationManager characterCreationManager)
        {
            TextObject description = CharacterObject.PlayerCharacter.IsFemale ? new TextObject("{=5kbeAC7k}In wartorn Azeroth, especially in frontier or tribal areas, some women as well as men learn to fight from an early age. You...", null) : new TextObject("{=F7OO5SAa}As a youngster growing up in Azeroth, war was never too far away. You...", null);
            BodyProperties bodyProperties = CharacterObject.PlayerCharacter.GetBodyProperties(CharacterObject.PlayerCharacter.Equipment, -1);
            bodyProperties = FaceGen.GetBodyPropertiesWithAge(ref bodyProperties, 17f);
            List<NarrativeMenuCharacter> list = new List<NarrativeMenuCharacter>();
            list.Add(new NarrativeMenuCharacter("player_youth_character", bodyProperties, CharacterObject.PlayerCharacter.Race, CharacterObject.PlayerCharacter.IsFemale));
            list.Add(new NarrativeMenuCharacter("narrative_character_horse"));
            NarrativeMenu narrativeMenu = new NarrativeMenu("narrative_youth_menu", "narrative_education_menu", "narrative_adulthood_menu", new TextObject("{=ok8lSW6M}Youth", null), description, list, new NarrativeMenu.GetNarrativeMenuCharacterArgsDelegate(this.GetYouthMenuNarrativeMenuCharacterArgs));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("youth_staff_first_option", new TextObject("{=CITG915d}joined a commander's staff.", null), new TextObject("{=wNHqFlDL}You were chosen by your superior officer to serve an imperial strategos as a courier. You were not given major responsibilities - mostly carrying messages and tending to his horse - but it did give you a chance to see how campaigns were planned and men were deployed in battle.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetYouthStaffOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.YouthStaffOneOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.YouthStaffOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("youth_staff_second_option", new TextObject("{=CITG915d}joined a commander's staff.", null), new TextObject("{=ANbNblaH}You were picked as the courier of the commander of the local forces. You were not given major responsibilities - mostly carrying messages and tending to his horse - but it did give you a chance to see how campaigns were planned and men were deployed in battle.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetYouthStaffOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.YouthStaffTwoOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.YouthStaffOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("youth_groom_option", new TextObject("{=bhE2i6OU}served as a baron's groom.", null), new TextObject("{=i3k7YtA8}You were chosen by a knight to accompany a minor baron of the Vlandian kingdom. You were not given major responsibilities - mostly carrying messages and tending to his horse - but it did give you a chance to see how campaigns were planned and men were deployed in battle.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetYouthGroomOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.YouthGroomOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.YouthGroomOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("youth_servant_first_option", new TextObject("{=F2bgujPo}were a chieftain's servant.", null), new TextObject("{=AXWO4C69}Your were choosen among others to accompany a chieftain of your people. You were not given major responsibilities - mostly carrying messages and tending to his horse - but it did give you a chance to see how campaigns were planned and men were deployed in battle.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetYouthServantOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.YouthServantOneOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.YouthServantOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("youth_servant_second_option", new TextObject("{=F2bgujPo}were a chieftain's servant.", null), new TextObject("{=neMCgMZM}Local wise man picked you to become the messenger of a chieftain of your people. You were not given major responsibilities - mostly carrying messages and tending to his horse - but it did give you a chance to see how campaigns were planned and men were deployed in battle.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetYouthServantOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.YouthServantTwoOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.YouthServantOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("youth_cavalry_option", new TextObject("{=h2KnarLL}trained with the cavalry.", null), new TextObject("{=7cHsIMLP}You could never have bought the equipment on your own, but you were a good enough rider so that the local lord lent you a horse and equipment. You joined the armored cavalry, training with the lance.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetYouthCavalryOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.YouthCavalryOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.YouthCavalryOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("youth_hearth_option", new TextObject("{=zsC2t5Hb}trained with the hearth guard.", null), new TextObject("{=RmbWW6Bm}You were a big and imposing enough youth that the chief's guard allowed you to train alongside them, in preparation to join them some day.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetYouthHearthOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.YouthHearthOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.YouthHearthOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("youth_guard_high_register_option", new TextObject("{=aTncHUfL}stood guard with the garrisons.", null), new TextObject("{=63TAYbkx}Urban troops spend much of their time guarding the town walls. Most of their training was in missile weapons, especially useful during sieges.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetYouthGuardHighRegisterOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.YouthGuardHighRegisterOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.YouthGuardHighRegisterOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("youth_guard_low_register_option", new TextObject("{=aTncHUfL}stood guard with the garrisons.", null), new TextObject("{=oR58iNDz}Urban troops spend much of their time guarding the town walls. Most of their training was in missile weapons.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetYouthGuardLowRegisterOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.YouthGuardLowRegisterOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.YouthGuardLowRegisterOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("youth_guard_garrisons_register_option", new TextObject("{=aTncHUfL}stood guard with the garrisons.", null), new TextObject("{=e6lINjFg}The garrisons spent most of their time guarding the town walls, and their training focused largely on missile weapons.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetYouthGuardGarrisonRegisterOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.YouthGuardGarrisonRegisterOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.YouthGuardGarrisonRegisterOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("youth_guard_empire_register_option", new TextObject("{=aTncHUfL}stood guard with the garrisons.", null), new TextObject("{=oR58iNDz}Urban troops spend much of their time guarding the town walls. Most of their training was in missile weapons.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetYouthGuardEmpireRegisterOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.YouthGuardEmpireRegisterOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.YouthGuardEmpireRegisterOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("youth_rider_high_register_option", new TextObject("{=VlXOgIX6}rode with the scouts.", null), new TextObject("{=888lmJqs}All of Azeroth's kingdoms recognize the value of good light cavalry. You were a good enough rider so that your village pitched in to buy you a small mount so you could fulfill their levy obligations.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetYouthRiderHighRegisterOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.YouthRiderHighRegisterOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.YouthRiderHighRegisterOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("youth_rider_low_register_option", new TextObject("{=VlXOgIX6}rode with the scouts.", null), new TextObject("{=sYuN6hPD}All of Azeroth's kingdoms recognize the value of good light cavalry. You were a good enough rider so that your village pitched in to buy you a small mount so you could fulfill their levy obligations.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetYouthRiderLowRegisterOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.YouthRiderLowRegisterOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.YouthRiderLowRegisterOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("youth_infantry_option", new TextObject("{=a8arFSra}trained with the infantry.", null), new TextObject("{=afH90aNs}Levy armed with spear and shield, drawn from smallholding farmers, have always been the backbone of most armies of Calradia.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetYouthInfantryOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.YouthInfantryOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.YouthInfantryOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("youth_skirmisher_option", new TextObject("{=oMbOIPc9}joined the skirmishers.", null), new TextObject("{=bXAg5w19}Younger recruits, or those of a slighter build, or those too poor to buy shield and armor tend to join the skirmishers. Fighting with bow and javelin, they try to stay out of reach of the main enemy forces.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetYouthSkirmisherOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.YouthSkirmisherOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.YouthSkirmisherOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("youth_kern_option", new TextObject("{=cDWbwBwI}joined the kern.", null), new TextObject("{=tTb28jyU}Many Battanians fight as kern, versatile troops who could both harass the enemy line with their javelins or join in the final screaming charge once it weakened.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetYouthKernOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.YouthKernOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.YouthKernOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("youth_camp_option", new TextObject("{=GFUggps8}marched with the camp followers.", null), new TextObject("{=64rWqBLN}You avoided service with one of the main forces of your realm's armies, but followed instead in the train - the troops' wives, lovers and servants, and those who make their living by caring for, entertaining, or cheating the soldiery.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetYouthCampOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.YouthCampOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.YouthCampOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("youth_envoys_guard_first_option", new TextObject("{=YmPlLGXb}served as an envoy's guard", null), new TextObject("{=qPamcCkA}Your family arranged for you to accompany an envoy. You were not given major responsibilities - mostly carrying arms and trying to look imposing. - but it did give you a chance to travel a lot and socialise and see the world.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetEnvoysGuardFirstOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.EnvoysGuardFirstOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.EnvoysGuardFirstOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("youth_envoys_guard_second_option", new TextObject("{=YmPlLGXb}served as an envoy's guard", null), new TextObject("{=VYU1nEHP}Your family arranged for you to accompany an envoy. You were not given major responsibilities but it did give you a chance to travel and socialise and see a bit of the world.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetEnvoysGuardSecondOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.EnvoysGuardSecondOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.EnvoysGuardSecondOptionOnSelect), null));
            characterCreationManager.AddNewMenu(narrativeMenu);
        }

        public void GetYouthStaffOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Steward, DefaultSkills.Tactics };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Cunning, 1);
        }

        public bool YouthStaffOneOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "empire" || characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "nord";
        }

        public bool YouthStaffTwoOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "aserai";
        }

        public void YouthStaffOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            characterCreationManager.CharacterCreationContent.SelectedTitleType = "retainer";
            string playerEquipmentId = this.GetPlayerEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedTitleType, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId, Hero.MainHero.IsFemale);
            foreach (NarrativeMenuCharacter narrativeMenuCharacter in characterCreationManager.CurrentMenu.Characters)
            {
                if (narrativeMenuCharacter.StringId == "player_youth_character")
                {
                    narrativeMenuCharacter.SetAnimationId("act_childhood_decisive");
                    narrativeMenuCharacter.SetEquipment(Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(playerEquipmentId));
                }
            }
        }

        public void GetYouthGroomOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Charm, DefaultSkills.Tactics };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Social, 1);
        }

        public bool YouthGroomOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "vlandia";
        }

        public void YouthGroomOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            characterCreationManager.CharacterCreationContent.SelectedTitleType = "retainer";
            string playerEquipmentId = this.GetPlayerEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedTitleType, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId, Hero.MainHero.IsFemale);
            foreach (NarrativeMenuCharacter narrativeMenuCharacter in characterCreationManager.CurrentMenu.Characters)
            {
                if (narrativeMenuCharacter.StringId == "player_youth_character")
                {
                    narrativeMenuCharacter.SetAnimationId("act_childhood_sharp");
                    narrativeMenuCharacter.SetEquipment(Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(playerEquipmentId));
                }
            }
        }

        public void GetYouthServantOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Steward, DefaultSkills.Tactics };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Cunning, 1);
        }

        public bool YouthServantOneOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "khuzait";
        }

        public bool YouthServantTwoOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "battania";
        }

        public void YouthServantOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            characterCreationManager.CharacterCreationContent.SelectedTitleType = "retainer";
            string playerEquipmentId = this.GetPlayerEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedTitleType, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId, Hero.MainHero.IsFemale);
            foreach (NarrativeMenuCharacter narrativeMenuCharacter in characterCreationManager.CurrentMenu.Characters)
            {
                if (narrativeMenuCharacter.StringId == "player_youth_character")
                {
                    narrativeMenuCharacter.SetAnimationId("act_childhood_ready");
                    narrativeMenuCharacter.SetEquipment(Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(playerEquipmentId));
                }
            }
        }

        public void GetYouthCavalryOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Riding, DefaultSkills.Polearm };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Endurance, 1);
        }

        public bool YouthCavalryOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "vlandia";
        }

        public void YouthCavalryOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            characterCreationManager.CharacterCreationContent.SelectedTitleType = "mercenary";
            string playerEquipmentId = this.GetPlayerEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedTitleType, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId, Hero.MainHero.IsFemale);
            foreach (NarrativeMenuCharacter narrativeMenuCharacter in characterCreationManager.CurrentMenu.Characters)
            {
                if (narrativeMenuCharacter.StringId == "player_youth_character")
                {
                    narrativeMenuCharacter.SetAnimationId("act_childhood_apprentice");
                    narrativeMenuCharacter.SetEquipment(Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(playerEquipmentId));
                }
            }
        }

        public void GetYouthHearthOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Riding, DefaultSkills.Polearm };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Endurance, 1);
        }

        public bool YouthHearthOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "sturgia" || characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "battania";
        }

        public void YouthHearthOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            characterCreationManager.CharacterCreationContent.SelectedTitleType = "mercenary";
            string playerEquipmentId = this.GetPlayerEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedTitleType, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId, Hero.MainHero.IsFemale);
            foreach (NarrativeMenuCharacter narrativeMenuCharacter in characterCreationManager.CurrentMenu.Characters)
            {
                if (narrativeMenuCharacter.StringId == "player_youth_character")
                {
                    narrativeMenuCharacter.SetAnimationId("act_childhood_athlete");
                    narrativeMenuCharacter.SetEquipment(Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(playerEquipmentId));
                }
            }
        }

        public void GetYouthGuardHighRegisterOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Crossbow, DefaultSkills.Engineering };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Intelligence, 1);
        }

        public bool YouthGuardHighRegisterOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "vlandia";
        }

        public void YouthGuardHighRegisterOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            characterCreationManager.CharacterCreationContent.SelectedTitleType = "guard";
            string playerEquipmentId = this.GetPlayerEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedTitleType, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId, Hero.MainHero.IsFemale);
            foreach (NarrativeMenuCharacter narrativeMenuCharacter in characterCreationManager.CurrentMenu.Characters)
            {
                if (narrativeMenuCharacter.StringId == "player_youth_character")
                {
                    narrativeMenuCharacter.SetAnimationId("act_childhood_vibrant");
                    narrativeMenuCharacter.SetEquipment(Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(playerEquipmentId));
                }
            }
        }

        public void GetYouthGuardLowRegisterOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Bow, DefaultSkills.Engineering };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Intelligence, 1);
        }

        public bool YouthGuardLowRegisterOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "sturgia";
        }

        public void YouthGuardLowRegisterOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            characterCreationManager.CharacterCreationContent.SelectedTitleType = "guard";
            string playerEquipmentId = this.GetPlayerEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedTitleType, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId, Hero.MainHero.IsFemale);
            foreach (NarrativeMenuCharacter narrativeMenuCharacter in characterCreationManager.CurrentMenu.Characters)
            {
                if (narrativeMenuCharacter.StringId == "player_youth_character")
                {
                    narrativeMenuCharacter.SetAnimationId("act_childhood_sharp");
                    narrativeMenuCharacter.SetEquipment(Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(playerEquipmentId));
                }
            }
        }

        public void GetYouthGuardGarrisonRegisterOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Bow, DefaultSkills.Engineering };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Intelligence, 1);
        }

        public bool YouthGuardGarrisonRegisterOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "battania" || characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "khuzait" || characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "aserai";
        }

        public void YouthGuardGarrisonRegisterOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            characterCreationManager.CharacterCreationContent.SelectedTitleType = "guard";
            string playerEquipmentId = this.GetPlayerEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedTitleType, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId, Hero.MainHero.IsFemale);
            foreach (NarrativeMenuCharacter narrativeMenuCharacter in characterCreationManager.CurrentMenu.Characters)
            {
                if (narrativeMenuCharacter.StringId == "player_youth_character")
                {
                    narrativeMenuCharacter.SetAnimationId("act_childhood_sharp");
                    narrativeMenuCharacter.SetEquipment(Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(playerEquipmentId));
                }
            }
        }

        public void GetYouthGuardEmpireRegisterOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Crossbow, DefaultSkills.Engineering };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Intelligence, 1);
        }

        public bool YouthGuardEmpireRegisterOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "empire";
        }

        public void YouthGuardEmpireRegisterOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            characterCreationManager.CharacterCreationContent.SelectedTitleType = "guard";
            string playerEquipmentId = this.GetPlayerEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedTitleType, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId, Hero.MainHero.IsFemale);
            foreach (NarrativeMenuCharacter narrativeMenuCharacter in characterCreationManager.CurrentMenu.Characters)
            {
                if (narrativeMenuCharacter.StringId == "player_youth_character")
                {
                    narrativeMenuCharacter.SetAnimationId("act_childhood_sharp");
                    narrativeMenuCharacter.SetEquipment(Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(playerEquipmentId));
                }
            }
        }

        public void GetYouthRiderHighRegisterOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Riding, DefaultSkills.Bow };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Endurance, 1);
        }

        public bool YouthRiderHighRegisterOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "empire" || characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "khuzait";
        }

        public void YouthRiderHighRegisterOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            characterCreationManager.CharacterCreationContent.SelectedTitleType = "hunter";
            string playerEquipmentId = this.GetPlayerEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedTitleType, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId, Hero.MainHero.IsFemale);
            foreach (NarrativeMenuCharacter narrativeMenuCharacter in characterCreationManager.CurrentMenu.Characters)
            {
                if (narrativeMenuCharacter.StringId == "player_youth_character")
                {
                    narrativeMenuCharacter.SetAnimationId("act_sturgia_mp_warrior_axe");
                    narrativeMenuCharacter.SetEquipment(Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(playerEquipmentId));
                }
            }
        }

        public void GetYouthRiderLowRegisterOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Riding, DefaultSkills.Bow };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Endurance, 1);
        }

        public bool YouthRiderLowRegisterOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "aserai" || characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "sturgia";
        }

        public void YouthRiderLowRegisterOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            characterCreationManager.CharacterCreationContent.SelectedTitleType = "hunter";
            string playerEquipmentId = this.GetPlayerEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedTitleType, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId, Hero.MainHero.IsFemale);
            foreach (NarrativeMenuCharacter narrativeMenuCharacter in characterCreationManager.CurrentMenu.Characters)
            {
                if (narrativeMenuCharacter.StringId == "player_youth_character")
                {
                    narrativeMenuCharacter.SetAnimationId("act_sturgia_mp_huskarl_idle");
                    narrativeMenuCharacter.SetEquipment(Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(playerEquipmentId));
                }
            }
        }

        public void GetYouthInfantryOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Polearm, DefaultSkills.OneHanded };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Vigor, 1);
        }

        public bool YouthInfantryOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "empire" || characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "vlandia" || characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "khuzait" || characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "aserai" || characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "battania" || characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "sturgia";
        }

        public void YouthInfantryOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            characterCreationManager.CharacterCreationContent.SelectedTitleType = "infantry";
            string playerEquipmentId = this.GetPlayerEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedTitleType, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId, Hero.MainHero.IsFemale);
            foreach (NarrativeMenuCharacter narrativeMenuCharacter in characterCreationManager.CurrentMenu.Characters)
            {
                if (narrativeMenuCharacter.StringId == "player_youth_character")
                {
                    narrativeMenuCharacter.SetAnimationId("act_childhood_fierce");
                    narrativeMenuCharacter.SetEquipment(Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(playerEquipmentId));
                }
            }
        }

        public void GetYouthSkirmisherOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Throwing, DefaultSkills.OneHanded };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Control, 1);
        }

        public bool YouthSkirmisherOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "empire" || characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "vlandia" || characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "khuzait" || characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "aserai" || characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "sturgia";
        }

        public void YouthSkirmisherOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            characterCreationManager.CharacterCreationContent.SelectedTitleType = "skirmisher";
            string playerEquipmentId = this.GetPlayerEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedTitleType, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId, Hero.MainHero.IsFemale);
            foreach (NarrativeMenuCharacter narrativeMenuCharacter in characterCreationManager.CurrentMenu.Characters)
            {
                if (narrativeMenuCharacter.StringId == "player_youth_character")
                {
                    narrativeMenuCharacter.SetAnimationId("act_childhood_fox");
                    narrativeMenuCharacter.SetEquipment(Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(playerEquipmentId));
                }
            }
        }

        public void GetYouthKernOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Throwing, DefaultSkills.OneHanded };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Control, 1);
        }

        public bool YouthKernOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "battania";
        }

        public void YouthKernOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            characterCreationManager.CharacterCreationContent.SelectedTitleType = "kern";
            string playerEquipmentId = this.GetPlayerEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedTitleType, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId, Hero.MainHero.IsFemale);
            foreach (NarrativeMenuCharacter narrativeMenuCharacter in characterCreationManager.CurrentMenu.Characters)
            {
                if (narrativeMenuCharacter.StringId == "player_youth_character")
                {
                    narrativeMenuCharacter.SetAnimationId("act_childhood_apprentice");
                    narrativeMenuCharacter.SetEquipment(Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(playerEquipmentId));
                }
            }
        }

        public void GetYouthCampOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Roguery, DefaultSkills.Throwing };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Cunning, 1);
        }

        public bool YouthCampOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "vlandia" || characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "sturgia";
        }

        public void YouthCampOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            characterCreationManager.CharacterCreationContent.SelectedTitleType = "bard";
            string playerEquipmentId = this.GetPlayerEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedTitleType, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId, Hero.MainHero.IsFemale);
            foreach (NarrativeMenuCharacter narrativeMenuCharacter in characterCreationManager.CurrentMenu.Characters)
            {
                if (narrativeMenuCharacter.StringId == "player_youth_character")
                {
                    narrativeMenuCharacter.SetAnimationId("act_childhood_militia");
                    narrativeMenuCharacter.SetEquipment(Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(playerEquipmentId));
                }
            }
        }

        public void GetEnvoysGuardFirstOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Charm, DefaultSkills.Scouting };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Social, 1);
        }

        public void GetEnvoysGuardSecondOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Charm, DefaultSkills.Scouting };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Social, 1);
        }

        public bool EnvoysGuardFirstOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "empire" || characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "khuzait";
        }

        public bool EnvoysGuardSecondOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "battania" || characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "aserai";
        }

        public void EnvoysGuardFirstOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            characterCreationManager.CharacterCreationContent.SelectedTitleType = "guard";
            string playerEquipmentId = this.GetPlayerEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedTitleType, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId, Hero.MainHero.IsFemale);
            foreach (NarrativeMenuCharacter narrativeMenuCharacter in characterCreationManager.CurrentMenu.Characters)
            {
                if (narrativeMenuCharacter.StringId == "player_youth_character")
                {
                    narrativeMenuCharacter.SetAnimationId("act_childhood_sharp");
                    narrativeMenuCharacter.SetEquipment(Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(playerEquipmentId));
                }
            }
        }

        public void EnvoysGuardSecondOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            characterCreationManager.CharacterCreationContent.SelectedTitleType = "guard";
            string playerEquipmentId = this.GetPlayerEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedTitleType, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId, Hero.MainHero.IsFemale);
            foreach (NarrativeMenuCharacter narrativeMenuCharacter in characterCreationManager.CurrentMenu.Characters)
            {
                if (narrativeMenuCharacter.StringId == "player_youth_character")
                {
                    narrativeMenuCharacter.SetAnimationId("act_childhood_sharp");
                    narrativeMenuCharacter.SetEquipment(Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(playerEquipmentId));
                }
            }
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