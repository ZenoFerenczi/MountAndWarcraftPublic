using HarmonyLib;
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
    public class MWREducationMenu : CharacterCreationCampaignBehavior, ICharacterCreationContentHandler
    {
        public string GetPlayerEducationAgeEquipmentId(CharacterCreationManager characterCreationManager, string parentOccupationType, string cultureId, bool isFemale)
        {
            return "player_char_creation_education_age_" + cultureId + "_" + parentOccupationType + "_MWR_" + (isFemale ? "f" : "m");
        }

        public string GetPlayerEquipmentId(CharacterCreationManager characterCreationManager, string occupationType, string cultureId, bool isFemale)
        {
            return "player_char_creation_" + cultureId + "_" + occupationType + "_MWR_" + (isFemale ? "f" : "m");
        }

        public List<NarrativeMenuCharacterArgs> GetEducationMenuNarrativeMenuCharacterArgs(CultureObject culture, string occupationType, CharacterCreationManager characterCreationManager)
        {
            List<NarrativeMenuCharacterArgs> list = new List<NarrativeMenuCharacterArgs>();
            string playerEducationAgeEquipmentId = this.GetPlayerEducationAgeEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId, Hero.MainHero.IsFemale);
            list.Add(new NarrativeMenuCharacterArgs("player_education_character", 12, playerEducationAgeEquipmentId, "act_childhood_schooled", "spawnpoint_player_1", "", "", null, true, CharacterObject.PlayerCharacter.IsFemale));
            return list;
        }

        new public void AddEducationMenu(CharacterCreationManager characterCreationManager)
        {
            BodyProperties bodyProperties = CharacterObject.PlayerCharacter.GetBodyProperties(CharacterObject.PlayerCharacter.Equipment, -1);
            bodyProperties = FaceGen.GetBodyPropertiesWithAge(ref bodyProperties, 12f);
            List<NarrativeMenuCharacter> list = new List<NarrativeMenuCharacter>();
            list.Add(new NarrativeMenuCharacter("player_education_character", bodyProperties, CharacterObject.PlayerCharacter.Race, CharacterObject.PlayerCharacter.IsFemale));
            NarrativeMenu narrativeMenu = new NarrativeMenu("narrative_education_menu", "narrative_childhood_menu", "narrative_youth_menu", new TextObject("{=rcoueCmk}Adolescence", null), new TextObject("{=WYvnWcXQ}Like all village children you helped out in the fields. You also...", null), list, new NarrativeMenu.GetNarrativeMenuCharacterArgsDelegate(this.GetEducationMenuNarrativeMenuCharacterArgs));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("education_herder_option", new TextObject("{=RKVNvimC}herded the sheep.", null), new TextObject("{=KfaqPpbK}You went with other fleet-footed youths to take the villages' sheep, goats or cattle to graze in pastures near the village. You were in charge of chasing down stray beasts, and always kept a big stone on hand to be hurled at lurking predators if necessary.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetEducationHerderOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.EducationHerderOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.EducationHerderOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("education_smith_option", new TextObject("{=bTKiN0hr}worked in the village smithy.", null), new TextObject("{=y6j1bJTH}You were apprenticed to the local smith. You learned how to heat and forge metal, hammering for hours at a time until your muscles ached.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetEducationSmithOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.EducationSmithOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.EducationSmithOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("education_engineer_option", new TextObject("{=tI8ZLtoA}repaired projects.", null), new TextObject("{=6LFj919J}You helped dig wells, rethatch houses, and fix broken plows. You learned about the basics of construction, as well as what it takes to keep a farming community prosperous.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetEducationEngineerOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.EducationEngineerOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.EducationEngineerOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("education_doctor_option", new TextObject("{=TRwgSLD2}gathered herbs in the wild.", null), new TextObject("{=9ks4u5cH}You were sent by the village healer up into the hills to look for useful medicinal plants. You learned which herbs healed wounds or brought down a fever, and how to find them.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetEducationDoctorOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.EducationDoctorOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.EducationDoctorOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("education_hunter_option", new TextObject("{=T7m7ReTq}hunted small game.", null), new TextObject("{=RuvSk3QT}You accompanied a local hunter as he went into the wilderness, helping him set up traps and catch small animals.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetEducationHunterOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.EducationHunterOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.EducationHunterOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("education_merchant_option", new TextObject("{=qAbMagWq}sold product at the market.", null), new TextObject("{=DIgsfYfz}You took your family's goods to the nearest town to sell your produce and buy supplies. It was hard work, but you enjoyed the hubbub of the marketplace.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetEducationMerchantOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.EducationMerchantOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.EducationMerchantOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("education_watcher_option", new TextObject("{=go7Yu7KS}watched the militia training.", null), new TextObject("{=qnqdEJOv}You watched the town's watch practice shooting and perfect their plans to defend the walls in case of a siege.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetEducationWatcherOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.EducationWatcherOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.EducationWatcherOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("education_ganger_option", new TextObject("{=gAjvAGTa}hung out with the gangs in the alleys.", null), new TextObject("{=1SUTcF0J}The gang leaders who kept watch over the slums of Azarothian cities were always in need of poor youth to run messages and back them up in turf wars, while thrill-seeking merchants' sons and daughters sometimes slummed it in their company as well.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetEducationGangerOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.EducationGangerOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.EducationGangerOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("education_docker_option", new TextObject("{=QVVCgajg}helped at building sites.", null), new TextObject("{=bhdkegZ4}All towns had their share of projects that were constantly in need of both skilled and unskilled labor. You learned how hoists and scaffolds were constructed, how planks and stones were hewn and fitted, and other skills.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetEducationDockerOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.EducationDockerOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.EducationDockerOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("education_marketer_option", new TextObject("{=JTsv6PFe}worked in the markets and caravanserais.", null), new TextObject("{=rmMcwSn8}You helped your family handle their business affairs, going down to the marketplace to make purchases and oversee the arrival of caravans.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetEducationMarketerOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.EducationMarketerOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.EducationMarketerOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("education_tutor_option", new TextObject("{=EMVojYzW}studied with your public tutor.", null), new TextObject("{=hXl25avg}Your family arranged for a public tutor and you took full advantage, reading voraciously on history, mathematics, and philosophy and discussing what you read with your tutor and classmates.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetEducationTutorOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.EducationTutorOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.EducationTutorOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("education_horser_option", new TextObject("{=hin3iA2D}cared for the horses.", null), new TextObject("{=Ghz90npw}Your family owned a few horses at the town stables and you took charge of their care. Many evenings you would take them out beyond the walls and gallup through the fields, racing other youth.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetEducationPoorHorserOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.EducationPoorHorserOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.EducationPoorHorserOptionOnSelect), null));
            characterCreationManager.AddNewMenu(narrativeMenu);
        }

        public void GetEducationHerderOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Athletics, DefaultSkills.Throwing };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Control, 1);
        }

        public bool EducationHerderOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return !MWREducationMenu.CharacterOccupationTypes.IsUrbanOccupation(characterCreationManager.CharacterCreationContent.SelectedParentOccupation);
        }

        public void EducationHerderOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            foreach (NarrativeMenuCharacter narrativeMenuCharacter in characterCreationManager.CurrentMenu.Characters)
            {
                if (narrativeMenuCharacter.StringId == "player_education_character")
                {
                    narrativeMenuCharacter.SetAnimationId("act_childhood_streets");
                    narrativeMenuCharacter.SetLeftHandItem("");
                    narrativeMenuCharacter.SetRightHandItem("carry_bostaff_rogue1");
                    break;
                }
            }
        }

        public void GetEducationSmithOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.TwoHanded, DefaultSkills.Crafting };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Vigor, 1);
        }

        public bool EducationSmithOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return !MWREducationMenu.CharacterOccupationTypes.IsUrbanOccupation(characterCreationManager.CharacterCreationContent.SelectedParentOccupation);
        }

        public void EducationSmithOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            foreach (NarrativeMenuCharacter narrativeMenuCharacter in characterCreationManager.CurrentMenu.Characters)
            {
                if (narrativeMenuCharacter.StringId == "player_education_character")
                {
                    narrativeMenuCharacter.SetAnimationId("act_childhood_militia");
                    narrativeMenuCharacter.SetLeftHandItem("");
                    narrativeMenuCharacter.SetRightHandItem("peasant_hammer_1_t1");
                    break;
                }
            }
        }

        public void GetEducationEngineerOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Crafting, DefaultSkills.Engineering };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Intelligence, 1);
        }

        public bool EducationEngineerOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return !MWREducationMenu.CharacterOccupationTypes.IsUrbanOccupation(characterCreationManager.CharacterCreationContent.SelectedParentOccupation);
        }

        public void EducationEngineerOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            foreach (NarrativeMenuCharacter narrativeMenuCharacter in characterCreationManager.CurrentMenu.Characters)
            {
                if (narrativeMenuCharacter.StringId == "player_education_character")
                {
                    narrativeMenuCharacter.SetAnimationId("act_childhood_grit");
                    narrativeMenuCharacter.SetLeftHandItem("");
                    narrativeMenuCharacter.SetRightHandItem("carry_hammer");
                    break;
                }
            }
        }

        public void GetEducationDoctorOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Medicine, DefaultSkills.Scouting };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Endurance, 1);
        }

        public bool EducationDoctorOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return !MWREducationMenu.CharacterOccupationTypes.IsUrbanOccupation(characterCreationManager.CharacterCreationContent.SelectedParentOccupation);
        }

        public void EducationDoctorOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            foreach (NarrativeMenuCharacter narrativeMenuCharacter in characterCreationManager.CurrentMenu.Characters)
            {
                if (narrativeMenuCharacter.StringId == "player_education_character")
                {
                    narrativeMenuCharacter.SetAnimationId("act_childhood_peddlers");
                    narrativeMenuCharacter.SetLeftHandItem("");
                    narrativeMenuCharacter.SetRightHandItem("_to_carry_bd_basket_a");
                    break;
                }
            }
        }

        public void GetEducationHunterOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Bow, DefaultSkills.Tactics };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Cunning, 1);
        }

        public bool EducationHunterOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return !MWREducationMenu.CharacterOccupationTypes.IsUrbanOccupation(characterCreationManager.CharacterCreationContent.SelectedParentOccupation);
        }

        public void EducationHunterOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            foreach (NarrativeMenuCharacter narrativeMenuCharacter in characterCreationManager.CurrentMenu.Characters)
            {
                if (narrativeMenuCharacter.StringId == "player_education_character")
                {
                    narrativeMenuCharacter.SetAnimationId("act_childhood_sharp");
                    narrativeMenuCharacter.SetLeftHandItem("");
                    narrativeMenuCharacter.SetRightHandItem("composite_bow");
                    break;
                }
            }
        }

        public void GetEducationMerchantOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Trade, DefaultSkills.Charm };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Social, 1);
        }

        public bool EducationMerchantOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return !MWREducationMenu.CharacterOccupationTypes.IsUrbanOccupation(characterCreationManager.CharacterCreationContent.SelectedParentOccupation);
        }

        public void EducationMerchantOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            foreach (NarrativeMenuCharacter narrativeMenuCharacter in characterCreationManager.CurrentMenu.Characters)
            {
                if (narrativeMenuCharacter.StringId == "player_education_character")
                {
                    narrativeMenuCharacter.SetAnimationId("act_childhood_peddlers_2");
                    narrativeMenuCharacter.SetLeftHandItem("");
                    narrativeMenuCharacter.SetRightHandItem("_to_carry_bd_fabric_c");
                    break;
                }
            }
        }

        public void GetEducationWatcherOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Polearm, DefaultSkills.Tactics };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Control, 1);
        }

        public bool EducationWatcherOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return MWREducationMenu.CharacterOccupationTypes.IsUrbanOccupation(characterCreationManager.CharacterCreationContent.SelectedParentOccupation);
        }

        public void EducationWatcherOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            foreach (NarrativeMenuCharacter narrativeMenuCharacter in characterCreationManager.CurrentMenu.Characters)
            {
                if (narrativeMenuCharacter.StringId == "player_education_character")
                {
                    narrativeMenuCharacter.SetAnimationId("act_childhood_fox");
                    narrativeMenuCharacter.SetLeftHandItem("");
                    narrativeMenuCharacter.SetRightHandItem("");
                    break;
                }
            }
        }

        public void GetEducationGangerOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Roguery, DefaultSkills.OneHanded };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Cunning, 1);
        }

        public bool EducationGangerOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return MWREducationMenu.CharacterOccupationTypes.IsUrbanOccupation(characterCreationManager.CharacterCreationContent.SelectedParentOccupation);
        }

        public void EducationGangerOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            foreach (NarrativeMenuCharacter narrativeMenuCharacter in characterCreationManager.CurrentMenu.Characters)
            {
                if (narrativeMenuCharacter.StringId == "player_education_character")
                {
                    narrativeMenuCharacter.SetAnimationId("act_childhood_athlete");
                    narrativeMenuCharacter.SetLeftHandItem("");
                    narrativeMenuCharacter.SetRightHandItem("");
                    break;
                }
            }
        }

        public void GetEducationDockerOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Athletics, DefaultSkills.Crafting };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Vigor, 1);
        }

        public bool EducationDockerOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return MWREducationMenu.CharacterOccupationTypes.IsUrbanOccupation(characterCreationManager.CharacterCreationContent.SelectedParentOccupation);
        }

        public void EducationDockerOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            foreach (NarrativeMenuCharacter narrativeMenuCharacter in characterCreationManager.CurrentMenu.Characters)
            {
                if (narrativeMenuCharacter.StringId == "player_education_character")
                {
                    narrativeMenuCharacter.SetAnimationId("act_childhood_peddlers");
                    narrativeMenuCharacter.SetLeftHandItem("");
                    narrativeMenuCharacter.SetRightHandItem("_to_carry_bd_basket_a");
                    break;
                }
            }
        }

        public void GetEducationMarketerOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Trade, DefaultSkills.Charm };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Social, 1);
        }

        public bool EducationMarketerOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return MWREducationMenu.CharacterOccupationTypes.IsUrbanOccupation(characterCreationManager.CharacterCreationContent.SelectedParentOccupation);
        }

        public void EducationMarketerOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            foreach (NarrativeMenuCharacter narrativeMenuCharacter in characterCreationManager.CurrentMenu.Characters)
            {
                if (narrativeMenuCharacter.StringId == "player_education_character")
                {
                    narrativeMenuCharacter.SetAnimationId("act_childhood_manners");
                    narrativeMenuCharacter.SetLeftHandItem("");
                    narrativeMenuCharacter.SetRightHandItem("");
                    break;
                }
            }
        }

        public void GetEducationTutorOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Engineering, DefaultSkills.Leadership };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Intelligence, 1);
        }

        public bool EducationTutorOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return MWREducationMenu.CharacterOccupationTypes.IsUrbanOccupation(characterCreationManager.CharacterCreationContent.SelectedParentOccupation);
        }

        public void EducationTutorOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            foreach (NarrativeMenuCharacter narrativeMenuCharacter in characterCreationManager.CurrentMenu.Characters)
            {
                if (narrativeMenuCharacter.StringId == "player_education_character")
                {
                    narrativeMenuCharacter.SetAnimationId("act_childhood_book");
                    narrativeMenuCharacter.SetLeftHandItem("character_creation_notebook");
                    narrativeMenuCharacter.SetRightHandItem("");
                    break;
                }
            }
        }

        public void GetEducationPoorHorserOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Riding, DefaultSkills.Steward };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Endurance, 1);
        }

        public bool EducationPoorHorserOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return MWREducationMenu.CharacterOccupationTypes.IsUrbanOccupation(characterCreationManager.CharacterCreationContent.SelectedParentOccupation);
        }

        public void EducationPoorHorserOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            foreach (NarrativeMenuCharacter narrativeMenuCharacter in characterCreationManager.CurrentMenu.Characters)
            {
                if (narrativeMenuCharacter.StringId == "player_education_character")
                {
                    narrativeMenuCharacter.SetAnimationId("act_childhood_peddlers_2");
                    narrativeMenuCharacter.SetLeftHandItem("");
                    narrativeMenuCharacter.SetRightHandItem("_to_carry_bd_fabric_c");
                    break;
                }
            }
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

        public readonly IReadOnlyDictionary<string, string> _occupationToEquipmentMapping = new Dictionary<string, string>
    {
        {
            "retainer",
            "retainer"
        },
        {
            "bard",
            "bard"
        },
        {
            "hunter",
            "hunter"
        },
        {
            "farmer",
            "farmer"
        },
        {
            "herder",
            "herder"
        },
        {
            "healer",
            "healer"
        },
        {
            "mercenary",
            "mercenary"
        },
        {
            "infantry",
            "infantry"
        },
        {
            "skirmisher",
            "skirmisher"
        },
        {
            "kern",
            "kern"
        },
        {
            "guard",
            "guard"
        },
        {
            "retainer_urban",
            "retainer"
        },
        {
            "mercenary_urban",
            "mercenary"
        },
        {
            "merchant_urban",
            "merchant"
        },
        {
            "vagabond_urban",
            "vagabond"
        },
        {
            "artisan_urban",
            "artisan"
        },
        {
            "physician_urban",
            "physician"
        },
        {
            "healer_urban",
            "healer"
        },
        {
            "bard_urban",
            "bard"
        }
};

        public const int ChildhoodAge = 7;

        public const int EducationAge = 12;

        public const int YouthAge = 17;

        public const int AccomplishmentAge = 20;

        public const int ParentAge = 33;

        public const int YoungAdultAge = 20;

        public const int AdultAge = 30;

        public const int MiddleAge = 40;

        public const int ElderAge = 50;

        new public const int FocusToAddYouthStart = 2;

        new public const int FocusToAddAdultStart = 4;

        new public const int FocusToAddMiddleAgedStart = 6;

        new public const int FocusToAddElderlyStart = 8;

        new public const int AttributeToAddYouthStart = 1;

        new public const int AttributeToAddAdultStart = 2;

        new public const int AttributeToAddMiddleAgedStart = 3;

        new public const int AttributeToAddElderlyStart = 4;

        new public const string MotherNarrativeCharacterStringId = "mother_character";

        new public const string FatherNarrativeCharacterStringId = "father_character";

        new public const string PlayerChildhoodCharacterStringId = "player_childhood_character";

        new public const string PlayerEducationCharacterStringId = "player_education_character";

        new public const string PlayerYouthCharacterStringId = "player_youth_character";

        new public const string PlayerAdulthoodCharacterStringId = "player_adulthood_character";

        new public const string PlayerAgeSelectionCharacterStringId = "player_age_selection_character";

        new public const string HorseNarrativeCharacterStringId = "narrative_character_horse";

        public int _focusToAdd = 1;

        public int _skillLevelToAdd = 10;

        public int _attributeLevelToAdd = 1;

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