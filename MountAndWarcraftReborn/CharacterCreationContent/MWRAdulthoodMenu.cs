using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.CharacterCreationContent;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace MWRMode.CharacterCreationContent
{
    public class MWRAdulthoodMenu : CharacterCreationCampaignBehavior, ICharacterCreationContentHandler
    {
        public string GetPlayerEquipmentId(CharacterCreationManager characterCreationManager, string occupationType, string cultureId, bool isFemale)
        {
            return "player_char_creation_" + cultureId + "_" + occupationType + "_MWR_" + (isFemale ? "f" : "m");
        }


        public List<NarrativeMenuCharacterArgs> GetAdultMenuNarrativeMenuCharacterArgs(CultureObject culture, string occupationType, CharacterCreationManager characterCreationManager)
        {
            List<NarrativeMenuCharacterArgs> list = new List<NarrativeMenuCharacterArgs>();
            string playerEquipmentId = this.GetPlayerEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedTitleType, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId, Hero.MainHero.IsFemale);
            list.Add(new NarrativeMenuCharacterArgs("player_adulthood_character", 20, playerEquipmentId, "act_childhood_schooled", "spawnpoint_player_1", "", "", null, true, CharacterObject.PlayerCharacter.IsFemale));
            MBEquipmentRoster @object = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(playerEquipmentId);
            ItemObject item = @object.DefaultEquipment[EquipmentIndex.ArmorItemEndSlot].Item;
            list.Add(new NarrativeMenuCharacterArgs("narrative_character_horse", -1, "", "act_horse_stand_1", "spawnpoint_mount_1", @object.DefaultEquipment[EquipmentIndex.ArmorItemEndSlot].Item.StringId, @object.DefaultEquipment[EquipmentIndex.HorseHarness].Item.StringId, MountCreationKey.GetRandomMountKey(item, CharacterObject.PlayerCharacter.GetMountKeySeed()), false, false));
            return list;
        }

        public void AddAdulthoodMenu(CharacterCreationManager characterCreationManager)
        {
            BodyProperties bodyProperties = CharacterObject.PlayerCharacter.GetBodyProperties(CharacterObject.PlayerCharacter.Equipment, -1);
            bodyProperties = FaceGen.GetBodyPropertiesWithAge(ref bodyProperties, 20f);
            List<NarrativeMenuCharacter> list = new List<NarrativeMenuCharacter>();
            list.Add(new NarrativeMenuCharacter("player_adulthood_character", bodyProperties, CharacterObject.PlayerCharacter.Race, CharacterObject.PlayerCharacter.IsFemale));
            list.Add(new NarrativeMenuCharacter("narrative_character_horse"));
            MBTextManager.SetTextVariable("EXP_VALUE", 10);
            NarrativeMenu narrativeMenu = new NarrativeMenu("narrative_adulthood_menu", "narrative_youth_menu", "narrative_age_selection_menu", new TextObject("{=MafIe9yI}Young Adulthood", null), new TextObject("{=4WYY0X59}Before you set out for a life of adventure, your biggest achievement was...", null), list, new NarrativeMenu.GetNarrativeMenuCharacterArgsDelegate(this.GetAdultMenuNarrativeMenuCharacterArgs));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("adulthood_defeated_enemy_option", new TextObject("{=8bwpVpgy}you defeated an enemy in battle.", null), new TextObject("{=1IEroJKs}Not everyone who musters for the levy marches to war, and not everyone who goes on campaign sees action. You did both, and you also took down an enemy warrior in direct one-to-one combat, in the full view of your comrades.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetAdulthoodDefeatedEnemyOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.AdulthoodDefeatedEnemyOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.AdulthoodDefeatedEnemyOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("adulthood_manhunt_option", new TextObject("{=mP3uFbcq}you led a successful manhunt.", null), new TextObject("{=4f5xwzX0}When your community needed to organize a posse to pursue horse thieves, you were the obvious choice. You hunted down the raiders, surrounded them and forced their surrender, and took back your stolen property.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetAdulthoodManhuntOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.AdulthoodManhuntOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.AdulthoodManhuntOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("adulthood_caravan_leader_option", new TextObject("{=wfbtS71d}you led a caravan.", null), new TextObject("{=joRHKCkm}Your family needed someone trustworthy to take a caravan to a neighboring town. You organized supplies, ensured a constant watch to keep away bandits, and brought it safely to its destination.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetAdulthoodCaravanLeaderOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.AdulthoodCaravanLeaderOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.AdulthoodCaravanLeaderOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("adulthood_saved_village_option", new TextObject("{=x1HTX5hq}you saved your village from a flood.", null), new TextObject("{=bWlmGDf3}When a sudden storm caused the local stream to rise suddenly, your neighbors needed quick-thinking leadership. You provided it, directing them to build levees to save their homes.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetAdulthoodSavedVillageOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.AdulthoodSavedVillageOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.AdulthoodSavedVillageOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("adulthood_saved_city_option", new TextObject("{=s8PNllPN}you saved your city quarter from a fire.", null), new TextObject("{=ZAGR6PYc}When a sudden blaze broke out in a back alley, your neighbors needed quick-thinking leadership and you provided it. You organized a bucket line to the nearest well, putting the fire out before any homes were lost.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetAdulthoodSavedCityOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.AdulthoodSavedCityOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.AdulthoodSavedCityOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("adulthood_workshop_option", new TextObject("{=xORjDTal}you invested some money in a workshop.", null), new TextObject("{=PyVqDLBu}Your parents didn't give you much money, but they did leave just enough for you to secure a loan against a larger amount to build a small workshop. You paid back what you borrowed, and sold your enterprise for a profit.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetAdulthoodWorkshopOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.AdulthoodWorkshopOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.AdulthoodWorkshopOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("adulthood_investor_option", new TextObject("{=xKXcqRJI}you invested some money in land.", null), new TextObject("{=cbF9jdQo}Your parents didn't give you much money, but they did leave just enough for you to purchase a plot of unused land at the edge of the village. You cleared away rocks and dug an irrigation ditch, raised a few seasons of crops, than sold it for a considerable profit.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetAdulthoodInvestorOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.AdulthoodInvestorOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.AdulthoodInvestorOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("adulthood_hunter_option", new TextObject("{=TbNRtUjb}you hunted a dangerous animal.", null), new TextObject("{=I3PcdaaL}Wolves and bears are a constant menace to the flocks of Azeroth. You went with a group of your fellow villages and fired the missile that brought down the beast.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetAdulthoodHunterOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.AdulthoodHunterOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.AdulthoodHunterOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("adulthood_siege_survivor_option", new TextObject("{=WbHfGCbd}you survived a siege.", null), new TextObject("{=FhZPjhli}Your hometown was briefly placed under siege, and you were called to defend the walls. Everyone did their part to repulse the enemy assault, and everyone is justly proud of what they endured.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetAdulthoodSiegeSurvivorOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.AdulthoodSiegeSurvivorOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.AdulthoodSiegeSurvivorOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("adulthood_escapade_high_register_option", new TextObject("{=kNXet6Um}you had a famous escapade in town.", null), new TextObject("{=DjeAJtix}Maybe it was a love affair, or maybe you cheated at dice, or maybe you just chose your words poorly when drinking with a dangerous crowd. Anyway, on one of your trips into town you got into the kind of trouble from which only a quick tongue or quick feet get you out alive.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetAdulthoodEscapadeHighRegisterOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.AdulthoodEscapadeHighRegisterOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.AdulthoodEscapadeHighRegisterOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("adulthood_escapade_low_register_option", new TextObject("{=qlOuiKXj}you had a famous escapade.", null), new TextObject("{=lD5Ob3R4}Maybe it was a love affair, or maybe you cheated at dice, or maybe you just chose your words poorly when drinking with a dangerous crowd. Anyway, you got into the kind of trouble from which only a quick tongue or quick feet get you out alive.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetAdulthoodEscapadeLowRegisterOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.AdulthoodEscapadeLowRegisterOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.AdulthoodEscapadeLowRegisterOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("adulthood_nice_person_option", new TextObject("{=Yqm0Dics}you treated people well.", null), new TextObject("{=dDmcqTzb}Yours wasn't the kind of reputation that local legends are made of, but it was the kind that wins you respect among those around you. You were consistently fair and honest in your business dealings and helpful to those in trouble. In doing so, you got a sense of what made people tick.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetAdulthoodNicePersonOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.AdulthoodNicePersonOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.AdulthoodNicePersonOptionOnSelect), null));
            characterCreationManager.AddNewMenu(narrativeMenu);
        }

        public void GetAdulthoodDefeatedEnemyOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.OneHanded, DefaultSkills.TwoHanded };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Vigor, 1);
            TraitObject[] affectedTraits = new TraitObject[] { DefaultTraits.Valor };
            args.SetAffectedTraits(affectedTraits);
            args.SetLevelToTraits(1);
            args.SetRenownToAdd(20);
        }

        public bool AdulthoodDefeatedEnemyOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return true;
        }

        public void AdulthoodDefeatedEnemyOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            foreach (NarrativeMenuCharacter narrativeMenuCharacter in characterCreationManager.CurrentMenu.Characters)
            {
                if (narrativeMenuCharacter.StringId == "player_adulthood_character")
                {
                    narrativeMenuCharacter.SetAnimationId("act_childhood_athlete");
                }
            }
        }

        public void GetAdulthoodManhuntOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Tactics, DefaultSkills.Leadership };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Cunning, 1);
            TraitObject[] affectedTraits = new TraitObject[] { DefaultTraits.Calculating };
            args.SetAffectedTraits(affectedTraits);
            args.SetLevelToTraits(1);
            args.SetRenownToAdd(10);
        }

        public bool AdulthoodManhuntOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return !MWRAdulthoodMenu.CharacterOccupationTypes.IsUrbanOccupation(characterCreationManager.CharacterCreationContent.SelectedParentOccupation) && (characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "vlandia" || characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "empire" || characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "aserai" || characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "battania" || characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "khuzait");
        }

        public void AdulthoodManhuntOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            foreach (NarrativeMenuCharacter narrativeMenuCharacter in characterCreationManager.CurrentMenu.Characters)
            {
                if (narrativeMenuCharacter.StringId == "player_adulthood_character")
                {
                    narrativeMenuCharacter.SetAnimationId("act_battania_mp_clan_warrior_shieldperk_idle");
                }
            }
        }

        public void GetAdulthoodCaravanLeaderOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Trade, DefaultSkills.Leadership };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Cunning, 1);
            TraitObject[] affectedTraits = new TraitObject[] { DefaultTraits.Calculating };
            args.SetAffectedTraits(affectedTraits);
            args.SetLevelToTraits(1);
            args.SetRenownToAdd(10);
        }

        public bool AdulthoodCaravanLeaderOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return MWRAdulthoodMenu.CharacterOccupationTypes.IsUrbanOccupation(characterCreationManager.CharacterCreationContent.SelectedParentOccupation) && (characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "vlandia" || characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "sturgia" || characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "empire" || characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "aserai" || characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "khuzait" || characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "nord");
        }

        public void AdulthoodCaravanLeaderOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            foreach (NarrativeMenuCharacter narrativeMenuCharacter in characterCreationManager.CurrentMenu.Characters)
            {
                if (narrativeMenuCharacter.StringId == "player_adulthood_character")
                {
                    narrativeMenuCharacter.SetAnimationId("act_childhood_ready_handshield");
                }
            }
        }

        public void GetAdulthoodSavedVillageOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Tactics, DefaultSkills.Leadership };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Cunning, 1);
            TraitObject[] affectedTraits = new TraitObject[] { DefaultTraits.Valor };
            args.SetAffectedTraits(affectedTraits);
            args.SetLevelToTraits(1);
            args.SetRenownToAdd(10);
        }

        public bool AdulthoodSavedVillageOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return !MWRAdulthoodMenu.CharacterOccupationTypes.IsUrbanOccupation(characterCreationManager.CharacterCreationContent.SelectedParentOccupation) && (characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "sturgia" || characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "nord");
        }

        public void AdulthoodSavedVillageOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            foreach (NarrativeMenuCharacter narrativeMenuCharacter in characterCreationManager.CurrentMenu.Characters)
            {
                if (narrativeMenuCharacter.StringId == "player_adulthood_character")
                {
                    narrativeMenuCharacter.SetAnimationId("act_drafted_to_war_pose");
                }
            }
        }

        public void GetAdulthoodSavedCityOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Tactics, DefaultSkills.Leadership };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Cunning, 1);
            TraitObject[] affectedTraits = new TraitObject[] { DefaultTraits.Calculating };
            args.SetAffectedTraits(affectedTraits);
            args.SetLevelToTraits(1);
            args.SetRenownToAdd(10);
        }

        public bool AdulthoodSavedCityOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return MWRAdulthoodMenu.CharacterOccupationTypes.IsUrbanOccupation(characterCreationManager.CharacterCreationContent.SelectedParentOccupation) && characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "battania";
        }

        public void AdulthoodSavedCityOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            foreach (NarrativeMenuCharacter narrativeMenuCharacter in characterCreationManager.CurrentMenu.Characters)
            {
                if (narrativeMenuCharacter.StringId == "player_adulthood_character")
                {
                    narrativeMenuCharacter.SetAnimationId("act_childhood_vibrant");
                }
            }
        }

        public void GetAdulthoodWorkshopOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Trade, DefaultSkills.Crafting };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Intelligence, 1);
            TraitObject[] affectedTraits = new TraitObject[] { DefaultTraits.Calculating };
            args.SetAffectedTraits(affectedTraits);
            args.SetLevelToTraits(1);
            args.SetRenownToAdd(10);
        }

        public bool AdulthoodWorkshopOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return MWRAdulthoodMenu.CharacterOccupationTypes.IsUrbanOccupation(characterCreationManager.CharacterCreationContent.SelectedParentOccupation);
        }

        public void AdulthoodWorkshopOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            foreach (NarrativeMenuCharacter narrativeMenuCharacter in characterCreationManager.CurrentMenu.Characters)
            {
                if (narrativeMenuCharacter.StringId == "player_adulthood_character")
                {
                    narrativeMenuCharacter.SetAnimationId("act_childhood_decisive");
                }
            }
        }
        public void GetAdulthoodInvestorOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Trade, DefaultSkills.Crafting };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Intelligence, 1);
            TraitObject[] affectedTraits = new TraitObject[] { DefaultTraits.Calculating };
            args.SetAffectedTraits(affectedTraits);
            args.SetLevelToTraits(1);
            args.SetRenownToAdd(10);
        }

        public bool AdulthoodInvestorOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return !MWRAdulthoodMenu.CharacterOccupationTypes.IsUrbanOccupation(characterCreationManager.CharacterCreationContent.SelectedParentOccupation);
        }

        public void AdulthoodInvestorOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            foreach (NarrativeMenuCharacter narrativeMenuCharacter in characterCreationManager.CurrentMenu.Characters)
            {
                if (narrativeMenuCharacter.StringId == "player_adulthood_character")
                {
                    narrativeMenuCharacter.SetAnimationId("act_childhood_decisive");
                }
            }
        }

        public void GetAdulthoodHunterOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Polearm, DefaultSkills.Bow };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Control, 1);
            TraitObject[] affectedTraits = new TraitObject[] { DefaultTraits.Valor };
            args.SetAffectedTraits(affectedTraits);
            args.SetLevelToTraits(1);
            args.SetRenownToAdd(5);
        }

        public bool AdulthoodHunterOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return !MWRAdulthoodMenu.CharacterOccupationTypes.IsUrbanOccupation(characterCreationManager.CharacterCreationContent.SelectedParentOccupation);
        }

        public void AdulthoodHunterOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            foreach (NarrativeMenuCharacter narrativeMenuCharacter in characterCreationManager.CurrentMenu.Characters)
            {
                if (narrativeMenuCharacter.StringId == "player_adulthood_character")
                {
                    narrativeMenuCharacter.SetAnimationId("act_childhood_tough");
                }
            }
        }

        public void GetAdulthoodSiegeSurvivorOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Bow, DefaultSkills.Crossbow };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Control, 1);
            args.SetRenownToAdd(5);
        }

        public bool AdulthoodSiegeSurvivorOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return MWRAdulthoodMenu.CharacterOccupationTypes.IsUrbanOccupation(characterCreationManager.CharacterCreationContent.SelectedParentOccupation);
        }

        public void AdulthoodSiegeSurvivorOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            foreach (NarrativeMenuCharacter narrativeMenuCharacter in characterCreationManager.CurrentMenu.Characters)
            {
                if (narrativeMenuCharacter.StringId == "player_adulthood_character")
                {
                    narrativeMenuCharacter.SetAnimationId("act_childhood_tough");
                }
            }
        }

        public void GetAdulthoodEscapadeHighRegisterOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Athletics, DefaultSkills.Roguery };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Endurance, 1);
            TraitObject[] affectedTraits = new TraitObject[] { DefaultTraits.Valor };
            args.SetAffectedTraits(affectedTraits);
            args.SetLevelToTraits(1);
            args.SetRenownToAdd(5);
        }

        public bool AdulthoodEscapadeHighRegisterOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return !MWRAdulthoodMenu.CharacterOccupationTypes.IsUrbanOccupation(characterCreationManager.CharacterCreationContent.SelectedParentOccupation);
        }

        public void AdulthoodEscapadeHighRegisterOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            foreach (NarrativeMenuCharacter narrativeMenuCharacter in characterCreationManager.CurrentMenu.Characters)
            {
                if (narrativeMenuCharacter.StringId == "player_adulthood_character")
                {
                    narrativeMenuCharacter.SetAnimationId("act_childhood_clever");
                }
            }
        }

        public void GetAdulthoodEscapadeLowRegisterOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Athletics, DefaultSkills.Roguery };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Endurance, 1);
            TraitObject[] affectedTraits = new TraitObject[] { DefaultTraits.Valor };
            args.SetAffectedTraits(affectedTraits);
            args.SetLevelToTraits(1);
            args.SetRenownToAdd(5);
        }

        public bool AdulthoodEscapadeLowRegisterOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return MWRAdulthoodMenu.CharacterOccupationTypes.IsUrbanOccupation(characterCreationManager.CharacterCreationContent.SelectedParentOccupation);
        }

        public void AdulthoodEscapadeLowRegisterOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            foreach (NarrativeMenuCharacter narrativeMenuCharacter in characterCreationManager.CurrentMenu.Characters)
            {
                if (narrativeMenuCharacter.StringId == "player_adulthood_character")
                {
                    narrativeMenuCharacter.SetAnimationId("act_childhood_clever");
                }
            }
        }

        public void GetAdulthoodNicePersonOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Charm, DefaultSkills.Steward };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Social, 1);
            TraitObject[] affectedTraits = new TraitObject[] { DefaultTraits.Mercy, DefaultTraits.Generosity, DefaultTraits.Honor };
            args.SetAffectedTraits(affectedTraits);
            args.SetLevelToTraits(1);
            args.SetRenownToAdd(5);
        }

        public bool AdulthoodNicePersonOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return true;
        }

        public void AdulthoodNicePersonOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            foreach (NarrativeMenuCharacter narrativeMenuCharacter in characterCreationManager.CurrentMenu.Characters)
            {
                if (narrativeMenuCharacter.StringId == "player_adulthood_character")
                {
                    narrativeMenuCharacter.SetAnimationId("act_childhood_manners");
                }
            }
        }

        public static class CharacterOccupationTypes
        {
            public static bool IsUrbanOccupation(string occupation)
            {
                return occupation == "retainer_urban" || occupation == "mercenary_urban" || occupation == "merchant_urban" || occupation == "vagabond_urban" || occupation == "artisan_urban" || occupation == "physician_urban" || occupation == "healer_urban" || occupation == "bard_urban";
            }

            public const string Retainer = "retainer";

            public const string Bard = "bard";

            public const string Hunter = "hunter";

            public const string Farmer = "farmer";

            public const string Herder = "herder";

            public const string Healer = "healer";

            public const string Mercenary = "mercenary";

            public const string Infantry = "infantry";

            public const string Skirmisher = "skirmisher";

            public const string Kern = "kern";

            public const string Guard = "guard";

            public const string RetainerUrban = "retainer_urban";

            public const string MercenaryUrban = "mercenary_urban";

            public const string MerchantUrban = "merchant_urban";

            public const string VagabondUrban = "vagabond_urban";

            public const string ArtisanUrban = "artisan_urban";

            public const string PhysicianUrban = "physician_urban";

            public const string HealerUrban = "healer_urban";

            public const string BardUrban = "bard_urban";
        }
    }
}