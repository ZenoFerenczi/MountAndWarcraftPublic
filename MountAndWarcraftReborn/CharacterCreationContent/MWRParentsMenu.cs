using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.CharacterCreationContent;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace MWRMode.CharacterCreationContent
{
    public class MWRParentsMenu : CharacterCreationCampaignBehavior, ICharacterCreationContentHandler
    {
        /// <summary>
        /// I've split the different steps of the backstory options to make it easier to work with, if you want it to be ina single file : https://github.com/Sh1ny4/CharacterCreationRedone/blob/bc3fd947cef49e8275fa729928d9743b5ea64abe/CharacterCreationRedone/CharacterCreationOptions/CharacterCreationRedoneVanilla.cs
        /// a lot has changed in the 1.3 update so here is what I found :
        /// it is now possible to add options without this patching, as seen in the war sails DLC. this is allowed by having a narrative menu ID. Check the DLC code to see how to implement it since it is not the idea behind this mod
        /// 
        /// Each menu option has 4 inputs : Condition, Args, OnSelect and Consequences
        ///     Condition input : allows you to limit which options are available, can be the cultures, for the parents to be noble, having a specifc trait, being a woman, etc
        ///     Args  : contains what will be affected by your choice like focus, skill level, attributes, traits, etc
        ///     OnSelect : mostly used to change what is displayed like the equipement and the animation
        ///     Consequences : is optional, it can be used to change what isn't available in args. I have used it to have the player be part of a kingdom, increase the clan level, change the gold, have a companion or give the player a criminal rating
        /// 
        /// A lot more can be done with this, like having a section that is purely a starting gear choice and each option costing a certain amount or having a menu option that allow you to select in which place to spawn
        /// 
        /// </summary>

        public string GetMotherEquipmentId(CharacterCreationManager characterCreationManager, string occupationType, string cultureId)
        {
            return "mother_char_creation_" + occupationType + "_MWR_" + cultureId;
        }

        public string GetFatherEquipmentId(CharacterCreationManager characterCreationManager, string occupationType, string cultureId)
        {
            return "father_char_creation_" + occupationType + "_MWR_" + cultureId;
        }
        public List<NarrativeMenuCharacterArgs> GetParentMenuNarrativeMenuCharacterArgs(CultureObject culture, string occupationType, CharacterCreationManager characterCreationManager)
        {
            return new List<NarrativeMenuCharacterArgs>
            {
                new NarrativeMenuCharacterArgs("mother_character", 33, "mother_char_creation_none_MWR_" + characterCreationManager.CharacterCreationContent.SelectedCulture.StringId, "act_character_creation_female_default_standing", "spawnpoint_player_1", "", "", null, true, true),
                new NarrativeMenuCharacterArgs("father_character", 33, "father_char_creation_none_MWR_" + characterCreationManager.CharacterCreationContent.SelectedCulture.StringId, "act_character_creation_male_default_standing", "spawnpoint_player_1", "", "", null, true, false)
            };
        }

        public void AddParentsMenu(CharacterCreationManager characterCreationManager)
        {
            List<NarrativeMenuCharacter> list = new List<NarrativeMenuCharacter>();
            BodyProperties bodyProperties2;
            BodyProperties bodyProperties;
            FaceGen.GenerateParentKey(bodyProperties = (bodyProperties2 = CharacterObject.PlayerCharacter.GetBodyProperties(CharacterObject.PlayerCharacter.Equipment, -1)), CharacterObject.PlayerCharacter.Race, ref bodyProperties2, ref bodyProperties);
            bodyProperties2 = new BodyProperties(new DynamicBodyProperties(33f, 0.3f, 0.2f), bodyProperties2.StaticProperties);
            bodyProperties = new BodyProperties(new DynamicBodyProperties(33f, 0.5f, 0.5f), bodyProperties.StaticProperties);
            list.Add(new NarrativeMenuCharacter("mother_character", bodyProperties2, CharacterObject.PlayerCharacter.Race, true));
            list.Add(new NarrativeMenuCharacter("father_character", bodyProperties, CharacterObject.PlayerCharacter.Race, false));
            NarrativeMenu narrativeMenu = new NarrativeMenu("narrative_parent_menu", "start", "narrative_childhood_menu", new TextObject("{=b4lDDcli}Family", null), new TextObject("{=XgFU1pCx}You were born into a family of...", null), list, new NarrativeMenu.GetNarrativeMenuCharacterArgsDelegate(this.GetParentMenuNarrativeMenuCharacterArgs));

            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("aserai_kinsfolk_option", new TextObject("{=Sw8OxnNr}Chieftain's Bodyguards", null), new TextObject("{=MFrIHJZM}Your family was born from a smaller offshoot of the chief's tribe. Your father's land gave him enough income to afford a raptor but he was not quite wealthy enough to buy the armor needed to join the heavier raptor riders. He fought as one of the scouts for which the jungle is famous.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetAseraiKinsfolkNarrativeOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.AseraiKinsfolkNarrativeOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.AseraiKinsfolkNarrativeOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("aserai_slave_option", new TextObject("{=ngFVgwDD}Warrior-slaves", null), new TextObject("{=GsPC2MgU}Your father was part of one of the slave-bodyguards maintained by the troll chiefs. He fought by his master's side and was freed - perhaps for an act of valor, or perhaps he paid for his freedom with his share of the spoils of battle.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetAseraiSlaveNarrativeOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.AseraiSlaveNarrativeOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.AseraiSlaveNarrativeOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("aserai_physician_option", new TextObject("{=bgy8LVvY}Physicians", null), new TextObject("{=BhQlmQoj}Your family were respected physicians in an jungle town. They set bones and cured the sick, and their skills were in much demand. They were respected in the higher echelons of society too.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetAseraiPhysicianNarrativeOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.AseraiPhysicianNarrativeOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.AseraiPhysicianNarrativeOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("aserai_farmer_option", new TextObject("{=g31pXuqi}Jungle farmers", null), new TextObject("{=5P0KqBAw}Your family tilled the soil in Stranglethorn. Your father was a member of the main foot levy of his tribe, fighting with his kinsmen under the chief's banner.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetAseraiFarmerNarrativeOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.AseraiFarmerNarrativeOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.AseraiFarmerNarrativeOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("aserai_herder_option", new TextObject("{=EEedqolz}Nomads", null), new TextObject("{=PKhcPbBX}Your family were part of a nomadic tribe, crisscrossing the junlge from the Vile Reef to the Wild Shore.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetAseraiHerderNarrativeOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.AseraiHerderNarrativeOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.AseraiHerderNarrativeOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("aserai_artisan_option", new TextObject("{=tRIrbTvv}Thugs", null), new TextObject("{=6bUSbsKC}Your father worked for a local chief, one of the strongmen who keep order in the poorer quarters of the town. He resolved disputes over land, dice and insults, imposing his authority with the chief's traditional staff.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetAseraiArtisanNarrativeOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.AseraiArtisanNarrativeOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.AseraiArtisanNarrativeOptionOnSelect), null));

            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("battania_retainer_option", new TextObject("{=GeNKQlHR}Members of the Silvermoon Guard", null), new TextObject("{=j2py5Rv5}Your family were the trusted kinfolk of the High King, and sat at his table in his great hall. Your father assisted the king in running the affairs of the kingdom and trained alongside the High Elven elite.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetBattaniaRetainerNarrativeOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.BattaniaRetainerNarrativeOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.BattaniaRetainerNarrativeOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("battania_healer_option", new TextObject("{=AeBzTj6w}Healers", null), new TextObject("{=j6py5Rv5}Your parents were healers who gathered herbs and treated the sick. As a living reservoir of High Elven tradition, they were also asked to adjudicate many disputes between the Houses.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetBattaniaHealerNarrativeOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.BattaniaHealerNarrativeOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.BattaniaHealerNarrativeOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("battania_farmer_option", new TextObject("{=tGEStbxb}Citizens", null), new TextObject("{=WchH8bS2}Your family were middle-ranking members of a High Elven House, who tilled their own land. Your mother fought with the Farstriders, joining in skirmishes and ambushes on bands of trolls.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetBattaniaFarmerNarrativeOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.BattaniaFarmerNarrativeOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.BattaniaFarmerNarrativeOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("battania_artisan_option", new TextObject("{=BCU6RezA}Smiths", null), new TextObject("{=kg9YtrOg}Your family were smiths, a revered profession among the High Elves. They crafted everything from fine filigree jewelry in geometric designs to the well-balanced longswords favored by the High Elven aristocracy.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetBattaniaArtisanNarrativeOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.BattaniaArtisanNarrativeOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.BattaniaArtisanNarrativeOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("battania_hunter_option", new TextObject("{=7eWmU2mF}Foresters", null), new TextObject("{=7jBroUUQ}Your family had little land of their own, so they earned their living from the woods, hunting and trapping. They taught you from an early age that skills like finding game trails and killing an animal with one shot could make the difference between eating and starvation.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetBattaniaHunterNarrativeOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.BattaniaHunterNarrativeOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.BattaniaHunterNarrativeOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("battania_bard_option", new TextObject("{=SpJqhEEh}Bards", null), new TextObject("{=aVzcyhhy}Your father was a bard, drifting from lord's hall to lord's hall making his living singing the praises of one High Elven aristocrat and mocking his enemies, then going to his enemy's hall and doing the reverse. You learned from him that a clever tongue could spare you from a life toiling in the fields, if you kept your wits about you.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetBattaniaBardNarrativeOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.BattaniaBardNarrativeOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.BattaniaBardNarrativeOptionOnSelect), null));

            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("empire_lanlord_option", new TextObject("{=InN5ZZt3}A landlord's retainers", null), new TextObject("{=ivKl4mV2}Your father was a trusted knight of the local lord. He rode with the lord's cavalry, fighting as an armored knight.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetEmpireLandlordNarrativeOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.EmpireLandlordNarrativeOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.EmpireLandlordNarrativeOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("empire_merchant_option", new TextObject("{=651FhzdR}Urban merchants", null), new TextObject("{=FQntPChs}Your family were merchants one of the main cities of the kingdom. They sometimes organized caravans to nearby towns and discussed issues in the town council.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetEmpireUrbanNarrativeOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.EmpireUrbanNarrativeOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.EmpireUrbanNarrativeOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("empire_farmer_option", new TextObject("{=sb4gg8Ak}Freeholders", null), new TextObject("{=09z8Q08f}Your family were small farmers with just enough land to feed themselves and make a small profit. People like them were the pillars of the rural economy, as well as the backbone of the militia.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetEmpireFarmerNarrativeOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.EmpireFarmerNarrativeOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.EmpireFarmerNarrativeOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("empire_artisan_option", new TextObject("{=v48N6h1t}Urban artisans", null), new TextObject("{=ueCm5y1C}Your family owned their own workshop in a city, making goods from raw materials brought in from the countryside. Your father played an active if minor role in the town council, and also served in the militia.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetEmpireArtisanNarrativeOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.EmpireArtisanNarrativeOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.EmpireArtisanNarrativeOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("empire_hunter_option", new TextObject("{=7eWmU2mF}Foresters", null), new TextObject("{=yRFSzSDZ}Your family lived in a village, but did not own their own land. Instead, your father supplemented paid jobs with long trips in the woods, hunting and trapping, always keeping a wary eye for the lord's game wardens.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetEmpireHunterNarrativeOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.EmpireHunterNarrativeOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.EmpireHunterNarrativeOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("empire_vagabond_option", new TextObject("{=aEke8dSb}Urban vagabonds", null), new TextObject("{=Jvf6K7TZYour family numbered among the many poor migrants living in the slums that grow up outside the walls of cities, making whatever money they could froma variety of odd jobs. Sometimes they did service for one of the city's many criminal gangs, and you had an early look at the dark side of life.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetEmpireVagabondNarrativeOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.EmpireVagabondNarrativeOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.EmpireVagabondNarrativeOptionOnSelect), null));

            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("khuzait_retainer_option", new TextObject("{=FVaRDe2a}Warchiefs", null), new TextObject("{=jAs3kDXh}Your family were trusted lieutenants of the Warchief, and shared his meals in the chieftain's tent. Your father assisted the Warchief in running the affairs of the clan and fought in the center of the Horde battle line.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetKhuzaitRetainerNarrativeOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.KhuzaitRetainerNarrativeOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.KhuzaitRetainerNarrativeOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("khuzait_merhant_option", new TextObject("{=TkgLEDRM}Quartermasters", null), new TextObject("{=qPg3IDiq}Your father was a quartermaster during the Horde's invasion of Azeroth, providing cruicial goods and resources for the war effort.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetKhuzaitMerchantNarrativeOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.KhuzaitMerchantNarrativeOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.KhuzaitMerchantNarrativeOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("khuzait_mercenary_option", new TextObject("{=tGEStbxb}Clanspeople", null), new TextObject("{=URgZ4ai4}Your family were middle-ranking members of one of the Horde's clans. Your father had some herds of his own, but was not rich. Whe the Horde was summoned to battle, he fought as a grunt in the battle line.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetKhuzaitHerderNarrativeOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.KhuzaitHerderNarrativeOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.KhuzaitHerderNarrativeOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("khuzait_farmer_option", new TextObject("{=gQ2tAvCz}PigFarmers", null), new TextObject("{=5QSGoRFj}Your family maintained a small farm, raising hogs and boars to provide meat for the Horde's warriors.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetKhuzaitFarmerNarrativeOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.KhuzaitFarmerNarrativeOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.KhuzaitFarmerNarrativeOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("khuzait_healer_option", new TextObject("{=vfhVveLW}Shamans", null), new TextObject("{=WOKNhaG2}Your family were guardians of the sacred traditions of the Horde, channeling the spirits of nature and the ancestors. They tended the sick and dispensed wisdom, resolving disputes and providing practical advice.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetKhuzaitHealerNarrativeOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.KhuzaitHealerNarrativeOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.KhuzaitHealerNarrativeOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("khuzait_herder_option", new TextObject("{=Xqba1Obq}Nomads", null), new TextObject("{=9aoQYpZs}Your family's clan never pledged its loyalty to the Warchief and never settled down. They remain some of the finest trackers and scouts, as the ability to spot an enemy coming and move quickly is often all that protects their herds from their neighbors' predations.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetKhuzaitNomadHerderNarrativeOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.KhuzaitNomadHerderNarrativeOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.KhuzaitNomadHerderNarrativeOptionOnSelect), null));

            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("sturgia_companion_option", new TextObject("{=mc78FEbA}Mountain Guards", null), new TextObject("{=hob3WVkU}Your father was a member of the Mountain Guard, the king's own personal guard. He sat at the king's table in the great hall, oversaw the king's estates, and stood by his side in the center of the shield wall in battle.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetSturgiaCompanionNarrativeOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.SturgiaCompanionNarrativeOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.SturgiaCompanionNarrativeOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("sturgia_trader_option", new TextObject("{=HqzVBfpl}Urban traders", null), new TextObject("{=bjVMtW3W}Your family were merchants who lived in Ironforge, organizing the shipment of the north's bounty of furs, honey and other goods to faraway lands.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetSturgiaTraderNarrativeOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.SturgiaTraderNarrativeOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.SturgiaTraderNarrativeOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("sturgia_farmer_option", new TextObject("{=zrpqSWSh}Free farmers", null), new TextObject("{=Mcd3ZyKq}Your family had just enough land to feed themselves and make a small profit. People like them were the pillars of the kingdom's economy, as well as the backbone of the levy.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetSturgiaFarmerNarrativeOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.SturgiaFarmerNarrativeOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.SturgiaFarmerNarrativeOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("sturgia_artisan_option", new TextObject("{=v48N6h1t}Urban artisans", null), new TextObject("{=ueCm5y1C}Your family owned their own workshop in a city, making goods from raw materials brought in from the countryside. Your father played an active if minor role in the town council, and also served in the militia.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetSturgiaArtisanNarrativeOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.SturgiaArtisanNarrativeOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.SturgiaArtisanNarrativeOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("sturgia_hunter_option", new TextObject("{=YcnK0Thk}Hunters", null), new TextObject("{=WyZ2UtFF}Your family had no taste for the authority of the boyars. They made their living deep in the woods, slashing and burning fields which they tended for a year or two before moving on. They hunted and trapped fox, hare, ermine, and other fur-bearing animals.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetSturgiaHunterNarrativeOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.SturgiaHunterNarrativeOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.SturgiaHunterNarrativeOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("sturgia_vagabond_option", new TextObject("{=TPoK3GSj}Vagabonds", null), new TextObject("{=2SDWhGmQ}Your family numbered among the poor migrants living in the slums that grow up outside the walls of the river cities, making whatever money they could from a variety of odd jobs. Sometimes they did services for one of the region's many criminal gangs.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetSturgiaVagabondNarrativeOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.SturgiaVagabondNarrativeOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.SturgiaVagabondNarrativeOptionOnSelect), null));

            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("vlandia_retainer_option", new TextObject("{=2TptWc4m}A baron's retainers", null), new TextObject("{=0Suu1Q9q}Your father was a bailif for the local lord. He looked after his liege's estates, resolved disputes in the village, and helped train the village levy. He rode with the lord's cavalry, fighting as an armored knight.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetVlandiaRetainerNarrativeOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.VlandiaRetainerNarrativeOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.VlandiaRetainerNarrativeOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("vlandia_merchant_option", new TextObject("{=651FhzdR}Urban merchants", null), new TextObject("{=qNZFkxJb}Your family were merchants one of the main cities of Lordaeron. They organized caravans to nearby towns and were active in the local merchant's guild.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetVlandiaMerchantNarrativeOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.VlandiaMerchantNarrativeOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.VlandiaMerchantNarrativeOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("vlandia_farmer_option", new TextObject("{=RDfXuVxT}Freeholders", null), new TextObject("{=BLZ4mdhb}Your family were small farmers with just enough land to feed themselves and make a small profit. People like them were the pillars of Lordaeron's economy, as well as the backbone of the levy.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetVlandiaFarmerNarrativeOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.VlandiaFarmerNarrativeOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.VlandiaFarmerNarrativeOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("vlandia_blacksmith_option", new TextObject("{=p2KIhGbE}Urban blacksmith", null), new TextObject("{=btsMpRcA}Your family owned a smithy in a city. Your father played an active if minor role in the town council, and also served in the militia.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetVlandiaBlacksmithNarrativeOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.VlandiaBlacksmithNarrativeOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.VlandiaBlacksmithNarrativeOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("vlandia_hunter_option", new TextObject("{=YcnK0Thk}Hunters", null), new TextObject("{=yRFSzSDZ}Your family lived in a village, but did not own their own land. Instead, your father supplemented paid jobs with long trips in the woods, hunting and trapping, always keeping a wary eye for the lord's game wardens.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetVlandiaHunterNarrativeOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.VlandiaHunterNarrativeOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.VlandiaHunterNarrativeOptionOnSelect), null));
            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("vlandia_mercenary_option", new TextObject("{=ipQP6aVi}Mercenaries", null), new TextObject("{=yYhX6JQC}Your father joined one of the Azeroth's many mercenary companies, composed of men who got such a taste for war in their lord's service that they never took well to peace. Their crossbowmen were much valued across Azeroth. Your mother was a camp follower, taking you along in the wake of bloody campaigns.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetVlandiaMercenaryNarrativeOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.VlandiaMercenaryNarrativeOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.VlandiaMercenaryNarrativeOptionOnSelect), null));

            narrativeMenu.AddNarrativeMenuOption(new NarrativeMenuOption("nord_lanlord_option", new TextObject("{=InN5ZZt3}Lich Kings Minion", null), new TextObject("{=ivKl4mV2}You were raised by the Lich King himself as one of his trusted Death Knights and gifted immense power for your loyal service. You are charged with conquering the lands of Azeroth and expanding the reach of the Lich King beyond his icy domain.", null), new GetNarrativeMenuOptionArgsDelegate(this.GetNordLandlordNarrativeOptionArgs), new NarrativeMenuOptionOnConditionDelegate(this.NordLandlordNarrativeOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(this.NordLandlordNarrativeOptionOnSelect), null));

            characterCreationManager.AddNewMenu(narrativeMenu);
        }

        public void GetEmpireLandlordNarrativeOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Riding, DefaultSkills.Polearm };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Vigor, 1);
        }

        public bool EmpireLandlordNarrativeOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "empire";
        }

        public void EmpireLandlordNarrativeOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            characterCreationManager.CharacterCreationContent.SetParentOccupation("retainer");
            string motherEquipmentId = this.GetMotherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            string fatherEquipmentId = this.GetFatherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            MBEquipmentRoster @object = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(motherEquipmentId);
            MBEquipmentRoster object2 = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(fatherEquipmentId);
            string motherAnimation = "act_character_creation_female_default_side_to_side_1";
            string fatherAnimation = "act_character_creation_male_default_side_to_side_1";
            this.UpdateParentEquipment(characterCreationManager, @object, object2, motherAnimation, fatherAnimation);
        }

        public void GetEmpireUrbanNarrativeOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Trade, DefaultSkills.Charm };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Social, 1);
        }

        public bool EmpireUrbanNarrativeOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "empire";
        }

        public void EmpireUrbanNarrativeOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            characterCreationManager.CharacterCreationContent.SetParentOccupation("merchant_urban");
            string motherEquipmentId = this.GetMotherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            string fatherEquipmentId = this.GetFatherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            MBEquipmentRoster @object = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(motherEquipmentId);
            MBEquipmentRoster object2 = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(fatherEquipmentId);
            string motherAnimation = "act_character_creation_female_default_mother_front";
            string fatherAnimation = "act_character_creation_male_default_mother_front";
            this.UpdateParentEquipment(characterCreationManager, @object, object2, motherAnimation, fatherAnimation);
        }

        public void GetEmpireFarmerNarrativeOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Athletics, DefaultSkills.Polearm };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Endurance, 1);
        }

        public bool EmpireFarmerNarrativeOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "empire";
        }

        public void EmpireFarmerNarrativeOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            characterCreationManager.CharacterCreationContent.SetParentOccupation("farmer");
            string motherEquipmentId = this.GetMotherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            string fatherEquipmentId = this.GetFatherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            MBEquipmentRoster @object = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(motherEquipmentId);
            MBEquipmentRoster object2 = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(fatherEquipmentId);
            string motherAnimation = "act_character_creation_female_default_father_sitting";
            string fatherAnimation = "act_character_creation_male_default_father_sitting";
            this.UpdateParentEquipment(characterCreationManager, @object, object2, motherAnimation, fatherAnimation);
        }

        public void GetEmpireArtisanNarrativeOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Crafting, DefaultSkills.Crossbow };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Intelligence, 1);
        }

        public bool EmpireArtisanNarrativeOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "empire";
        }

        public void EmpireArtisanNarrativeOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            characterCreationManager.CharacterCreationContent.SetParentOccupation("artisan_urban");
            string motherEquipmentId = this.GetMotherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            string fatherEquipmentId = this.GetFatherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            MBEquipmentRoster @object = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(motherEquipmentId);
            MBEquipmentRoster object2 = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(fatherEquipmentId);
            string motherAnimation = "act_character_creation_female_default_side_to_side_2";
            string fatherAnimation = "act_character_creation_male_default_side_to_side_2";
            this.UpdateParentEquipment(characterCreationManager, @object, object2, motherAnimation, fatherAnimation);
        }

        public void GetEmpireHunterNarrativeOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Scouting, DefaultSkills.Bow };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Control, 1);
        }

        public bool EmpireHunterNarrativeOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "empire";
        }

        public void EmpireHunterNarrativeOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            characterCreationManager.CharacterCreationContent.SetParentOccupation("hunter");
            string motherEquipmentId = this.GetMotherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            string fatherEquipmentId = this.GetFatherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            MBEquipmentRoster @object = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(motherEquipmentId);
            MBEquipmentRoster object2 = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(fatherEquipmentId);
            string motherAnimation = "act_character_creation_female_default_side_to_side_3";
            string fatherAnimation = "act_character_creation_male_default_side_to_side_3";
            this.UpdateParentEquipment(characterCreationManager, @object, object2, motherAnimation, fatherAnimation);
        }

        public void GetEmpireVagabondNarrativeOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Roguery, DefaultSkills.Throwing };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Cunning, 1);
        }

        public bool EmpireVagabondNarrativeOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "empire";
        }

        public void EmpireVagabondNarrativeOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            characterCreationManager.CharacterCreationContent.SetParentOccupation("vagabond_urban");
            string motherEquipmentId = this.GetMotherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            string fatherEquipmentId = this.GetFatherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            MBEquipmentRoster @object = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(motherEquipmentId);
            MBEquipmentRoster object2 = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(fatherEquipmentId);
            string motherAnimation = "act_character_creation_female_default_hugging";
            string fatherAnimation = "act_character_creation_male_default_hugging";
            this.UpdateParentEquipment(characterCreationManager, @object, object2, motherAnimation, fatherAnimation);
        }

        new public void UpdateParentEquipment(CharacterCreationManager characterCreationManager, MBEquipmentRoster motherEquipment, MBEquipmentRoster fatherEquipment, string motherAnimation, string fatherAnimation)
        {
            foreach (NarrativeMenuCharacter narrativeMenuCharacter in characterCreationManager.CurrentMenu.Characters)
            {
                if (narrativeMenuCharacter.StringId.Equals("mother_character"))
                {
                    narrativeMenuCharacter.SetEquipment(motherEquipment);
                    narrativeMenuCharacter.SetAnimationId(motherAnimation);
                }
                if (narrativeMenuCharacter.StringId.Equals("father_character"))
                {
                    narrativeMenuCharacter.SetEquipment(fatherEquipment);
                    narrativeMenuCharacter.SetAnimationId(fatherAnimation);
                }
            }
        }

        public void GetVlandiaRetainerNarrativeOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Riding, DefaultSkills.Polearm };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Social, 1);
        }

        public bool VlandiaRetainerNarrativeOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "vlandia";
        }

        public void VlandiaRetainerNarrativeOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            characterCreationManager.CharacterCreationContent.SetParentOccupation("retainer");
            string motherEquipmentId = this.GetMotherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            string fatherEquipmentId = this.GetFatherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            MBEquipmentRoster @object = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(motherEquipmentId);
            MBEquipmentRoster object2 = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(fatherEquipmentId);
            string motherAnimation = "act_character_creation_female_default_side_to_side_1";
            string fatherAnimation = "act_character_creation_male_default_side_to_side_1";
            this.UpdateParentEquipment(characterCreationManager, @object, object2, motherAnimation, fatherAnimation);
        }

        public void GetVlandiaMerchantNarrativeOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Trade, DefaultSkills.Charm };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Intelligence, 1);
        }

        public bool VlandiaMerchantNarrativeOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "vlandia";
        }

        public void VlandiaMerchantNarrativeOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            characterCreationManager.CharacterCreationContent.SetParentOccupation("merchant_urban");
            string motherEquipmentId = this.GetMotherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            string fatherEquipmentId = this.GetFatherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            MBEquipmentRoster @object = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(motherEquipmentId);
            MBEquipmentRoster object2 = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(fatherEquipmentId);
            string motherAnimation = "act_character_creation_female_default_mother_front";
            string fatherAnimation = "act_character_creation_male_default_mother_front";
            this.UpdateParentEquipment(characterCreationManager, @object, object2, motherAnimation, fatherAnimation);
        }

        public void GetVlandiaFarmerNarrativeOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Polearm, DefaultSkills.Crossbow };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Endurance, 1);
        }

        public bool VlandiaFarmerNarrativeOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "vlandia";
        }

        public void VlandiaFarmerNarrativeOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            characterCreationManager.CharacterCreationContent.SetParentOccupation("farmer");
            string motherEquipmentId = this.GetMotherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            string fatherEquipmentId = this.GetFatherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            MBEquipmentRoster @object = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(motherEquipmentId);
            MBEquipmentRoster object2 = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(fatherEquipmentId);
            string motherAnimation = "act_character_creation_female_default_father_sitting";
            string fatherAnimation = "act_character_creation_male_default_father_sitting";
            this.UpdateParentEquipment(characterCreationManager, @object, object2, motherAnimation, fatherAnimation);
        }

        public void GetVlandiaBlacksmithNarrativeOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Crafting, DefaultSkills.TwoHanded };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Vigor, 1);
        }

        public bool VlandiaBlacksmithNarrativeOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "vlandia";
        }

        public void VlandiaBlacksmithNarrativeOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            characterCreationManager.CharacterCreationContent.SetParentOccupation("artisan_urban");
            string motherEquipmentId = this.GetMotherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            string fatherEquipmentId = this.GetFatherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            MBEquipmentRoster @object = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(motherEquipmentId);
            MBEquipmentRoster object2 = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(fatherEquipmentId);
            string motherAnimation = "act_character_creation_female_default_side_to_side_2";
            string fatherAnimation = "act_character_creation_male_default_side_to_side_2";
            this.UpdateParentEquipment(characterCreationManager, @object, object2, motherAnimation, fatherAnimation);
        }

        public void GetVlandiaHunterNarrativeOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Scouting, DefaultSkills.Crossbow };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Control, 1);
        }

        public bool VlandiaHunterNarrativeOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "vlandia";
        }

        public void VlandiaHunterNarrativeOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            characterCreationManager.CharacterCreationContent.SetParentOccupation("hunter");
            string motherEquipmentId = this.GetMotherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            string fatherEquipmentId = this.GetFatherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            MBEquipmentRoster @object = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(motherEquipmentId);
            MBEquipmentRoster object2 = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(fatherEquipmentId);
            string motherAnimation = "act_character_creation_female_default_side_to_side_3";
            string fatherAnimation = "act_character_creation_male_default_side_to_side_3";
            this.UpdateParentEquipment(characterCreationManager, @object, object2, motherAnimation, fatherAnimation);
        }

        public void GetVlandiaMercenaryNarrativeOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Roguery, DefaultSkills.Crossbow };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Cunning, 1);
        }

        public bool VlandiaMercenaryNarrativeOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "vlandia";
        }

        public void VlandiaMercenaryNarrativeOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            characterCreationManager.CharacterCreationContent.SetParentOccupation("merchant_urban");
            string motherEquipmentId = this.GetMotherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            string fatherEquipmentId = this.GetFatherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            MBEquipmentRoster @object = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(motherEquipmentId);
            MBEquipmentRoster object2 = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(fatherEquipmentId);
            string motherAnimation = "act_character_creation_female_default_hugging";
            string fatherAnimation = "act_character_creation_male_default_hugging";
            this.UpdateParentEquipment(characterCreationManager, @object, object2, motherAnimation, fatherAnimation);
        }

        public void GetSturgiaCompanionNarrativeOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Riding, DefaultSkills.TwoHanded };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Social, 1);
        }

        public bool SturgiaCompanionNarrativeOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "sturgia";
        }

        public void SturgiaCompanionNarrativeOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            characterCreationManager.CharacterCreationContent.SetParentOccupation("retainer");
            string motherEquipmentId = this.GetMotherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            string fatherEquipmentId = this.GetFatherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            MBEquipmentRoster @object = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(motherEquipmentId);
            MBEquipmentRoster object2 = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(fatherEquipmentId);
            string motherAnimation = "act_character_creation_female_default_side_to_side_1";
            string fatherAnimation = "act_character_creation_male_default_side_to_side_1";
            this.UpdateParentEquipment(characterCreationManager, @object, object2, motherAnimation, fatherAnimation);
        }

        public void GetSturgiaTraderNarrativeOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Trade, DefaultSkills.Tactics };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Cunning, 1);
        }

        public bool SturgiaTraderNarrativeOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "sturgia";
        }

        public void SturgiaTraderNarrativeOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            characterCreationManager.CharacterCreationContent.SetParentOccupation("merchant_urban");
            string motherEquipmentId = this.GetMotherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            string fatherEquipmentId = this.GetFatherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            MBEquipmentRoster @object = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(motherEquipmentId);
            MBEquipmentRoster object2 = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(fatherEquipmentId);
            string motherAnimation = "act_character_creation_female_default_mother_front";
            string fatherAnimation = "act_character_creation_male_default_mother_front";
            this.UpdateParentEquipment(characterCreationManager, @object, object2, motherAnimation, fatherAnimation);
        }

        public void GetSturgiaFarmerNarrativeOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Athletics, DefaultSkills.Polearm };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Endurance, 1);
        }

        public bool SturgiaFarmerNarrativeOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "sturgia";
        }

        public void SturgiaFarmerNarrativeOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            characterCreationManager.CharacterCreationContent.SetParentOccupation("farmer");
            string motherEquipmentId = this.GetMotherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            string fatherEquipmentId = this.GetFatherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            MBEquipmentRoster @object = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(motherEquipmentId);
            MBEquipmentRoster object2 = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(fatherEquipmentId);
            string motherAnimation = "act_character_creation_female_default_father_sitting";
            string fatherAnimation = "act_character_creation_male_default_father_sitting";
            this.UpdateParentEquipment(characterCreationManager, @object, object2, motherAnimation, fatherAnimation);
        }

        public void GetSturgiaArtisanNarrativeOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Crafting, DefaultSkills.OneHanded };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Intelligence, 1);
        }

        public bool SturgiaArtisanNarrativeOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "sturgia";
        }

        public void SturgiaArtisanNarrativeOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            characterCreationManager.CharacterCreationContent.SetParentOccupation("artisan_urban");
            string motherEquipmentId = this.GetMotherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            string fatherEquipmentId = this.GetFatherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            MBEquipmentRoster @object = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(motherEquipmentId);
            MBEquipmentRoster object2 = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(fatherEquipmentId);
            string motherAnimation = "act_character_creation_female_default_side_to_side_2";
            string fatherAnimation = "act_character_creation_male_default_side_to_side_2";
            this.UpdateParentEquipment(characterCreationManager, @object, object2, motherAnimation, fatherAnimation);
        }

        public void GetSturgiaHunterNarrativeOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Scouting, DefaultSkills.Bow };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Vigor, 1);
        }

        public bool SturgiaHunterNarrativeOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "sturgia";
        }

        public void SturgiaHunterNarrativeOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            characterCreationManager.CharacterCreationContent.SetParentOccupation("hunter");
            string motherEquipmentId = this.GetMotherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            string fatherEquipmentId = this.GetFatherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            MBEquipmentRoster @object = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(motherEquipmentId);
            MBEquipmentRoster object2 = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(fatherEquipmentId);
            string motherAnimation = "act_character_creation_female_default_side_to_side_3";
            string fatherAnimation = "act_character_creation_male_default_side_to_side_3";
            this.UpdateParentEquipment(characterCreationManager, @object, object2, motherAnimation, fatherAnimation);
        }

        public void GetSturgiaVagabondNarrativeOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Roguery, DefaultSkills.Throwing };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Control, 1);
        }

        public bool SturgiaVagabondNarrativeOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "sturgia";
        }

        public void SturgiaVagabondNarrativeOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            characterCreationManager.CharacterCreationContent.SetParentOccupation("vagabond_urban");
            string motherEquipmentId = this.GetMotherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            string fatherEquipmentId = this.GetFatherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            MBEquipmentRoster @object = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(motherEquipmentId);
            MBEquipmentRoster object2 = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(fatherEquipmentId);
            string motherAnimation = "act_character_creation_female_default_hugging";
            string fatherAnimation = "act_character_creation_male_default_hugging";
            this.UpdateParentEquipment(characterCreationManager, @object, object2, motherAnimation, fatherAnimation);
        }


        public void GetAseraiKinsfolkNarrativeOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Riding, DefaultSkills.Throwing };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Social, 1);
        }

        public bool AseraiKinsfolkNarrativeOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "aserai";
        }

        public void AseraiKinsfolkNarrativeOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            characterCreationManager.CharacterCreationContent.SetParentOccupation("retainer");
            string motherEquipmentId = this.GetMotherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            string fatherEquipmentId = this.GetFatherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            MBEquipmentRoster @object = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(motherEquipmentId);
            MBEquipmentRoster object2 = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(fatherEquipmentId);
            string motherAnimation = "act_character_creation_female_default_side_to_side_1";
            string fatherAnimation = "act_character_creation_male_default_side_to_side_1";
            this.UpdateParentEquipment(characterCreationManager, @object, object2, motherAnimation, fatherAnimation);
        }

        public void GetAseraiSlaveNarrativeOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Riding, DefaultSkills.Polearm };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Vigor, 1);
        }

        public bool AseraiSlaveNarrativeOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "aserai";
        }

        public void AseraiSlaveNarrativeOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            characterCreationManager.CharacterCreationContent.SetParentOccupation("mercenary_urban");
            string motherEquipmentId = this.GetMotherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            string fatherEquipmentId = this.GetFatherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            MBEquipmentRoster @object = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(motherEquipmentId);
            MBEquipmentRoster object2 = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(fatherEquipmentId);
            string motherAnimation = "act_character_creation_female_default_mother_front";
            string fatherAnimation = "act_character_creation_male_default_mother_front";
            this.UpdateParentEquipment(characterCreationManager, @object, object2, motherAnimation, fatherAnimation);
        }

        public void GetAseraiPhysicianNarrativeOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Medicine, DefaultSkills.Charm };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Intelligence, 1);
        }

        public bool AseraiPhysicianNarrativeOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "aserai";
        }

        public void AseraiPhysicianNarrativeOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            characterCreationManager.CharacterCreationContent.SetParentOccupation("physician_urban");
            string motherEquipmentId = this.GetMotherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            string fatherEquipmentId = this.GetFatherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            MBEquipmentRoster @object = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(motherEquipmentId);
            MBEquipmentRoster object2 = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(fatherEquipmentId);
            string motherAnimation = "act_character_creation_female_default_father_sitting";
            string fatherAnimation = "act_character_creation_male_default_father_sitting";
            this.UpdateParentEquipment(characterCreationManager, @object, object2, motherAnimation, fatherAnimation);
        }

        public void GetAseraiFarmerNarrativeOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Athletics, DefaultSkills.OneHanded };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Endurance, 1);
        }

        public bool AseraiFarmerNarrativeOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "aserai";
        }

        public void AseraiFarmerNarrativeOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            characterCreationManager.CharacterCreationContent.SetParentOccupation("farmer");
            string motherEquipmentId = this.GetMotherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            string fatherEquipmentId = this.GetFatherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            MBEquipmentRoster @object = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(motherEquipmentId);
            MBEquipmentRoster object2 = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(fatherEquipmentId);
            string motherAnimation = "act_character_creation_female_default_side_to_side_2";
            string fatherAnimation = "act_character_creation_male_default_side_to_side_2";
            this.UpdateParentEquipment(characterCreationManager, @object, object2, motherAnimation, fatherAnimation);
        }

        public void GetAseraiHerderNarrativeOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Scouting, DefaultSkills.Bow };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Cunning, 1);
        }

        public bool AseraiHerderNarrativeOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "aserai";
        }

        public void AseraiHerderNarrativeOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            characterCreationManager.CharacterCreationContent.SetParentOccupation("herder");
            string motherEquipmentId = this.GetMotherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            string fatherEquipmentId = this.GetFatherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            MBEquipmentRoster @object = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(motherEquipmentId);
            MBEquipmentRoster object2 = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(fatherEquipmentId);
            string motherAnimation = "act_character_creation_female_default_side_to_side_3";
            string fatherAnimation = "act_character_creation_male_default_side_to_side_3";
            this.UpdateParentEquipment(characterCreationManager, @object, object2, motherAnimation, fatherAnimation);
        }

        public void GetAseraiArtisanNarrativeOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Roguery, DefaultSkills.Polearm };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Control, 1);
        }

        public bool AseraiArtisanNarrativeOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "aserai";
        }

        public void AseraiArtisanNarrativeOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            characterCreationManager.CharacterCreationContent.SetParentOccupation("artisan_urban");
            string motherEquipmentId = this.GetMotherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            string fatherEquipmentId = this.GetFatherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            MBEquipmentRoster @object = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(motherEquipmentId);
            MBEquipmentRoster object2 = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(fatherEquipmentId);
            string motherAnimation = "act_character_creation_female_default_hugging";
            string fatherAnimation = "act_character_creation_male_default_hugging";
            this.UpdateParentEquipment(characterCreationManager, @object, object2, motherAnimation, fatherAnimation);
        }

        public void GetBattaniaRetainerNarrativeOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.TwoHanded, DefaultSkills.Bow };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Vigor, 1);
        }

        public bool BattaniaRetainerNarrativeOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "battania";
        }

        public void BattaniaRetainerNarrativeOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            characterCreationManager.CharacterCreationContent.SetParentOccupation("retainer_urban");
            string motherEquipmentId = this.GetMotherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            string fatherEquipmentId = this.GetFatherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            MBEquipmentRoster @object = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(motherEquipmentId);
            MBEquipmentRoster object2 = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(fatherEquipmentId);
            string motherAnimation = "act_character_creation_female_default_side_to_side_1";
            string fatherAnimation = "act_character_creation_male_default_side_to_side_1";
            this.UpdateParentEquipment(characterCreationManager, @object, object2, motherAnimation, fatherAnimation);
        }

        public void GetBattaniaHealerNarrativeOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Medicine, DefaultSkills.Charm };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Intelligence, 1);
        }

        public bool BattaniaHealerNarrativeOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "battania";
        }

        public void BattaniaHealerNarrativeOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            characterCreationManager.CharacterCreationContent.SetParentOccupation("healer");
            string motherEquipmentId = this.GetMotherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            string fatherEquipmentId = this.GetFatherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            MBEquipmentRoster @object = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(motherEquipmentId);
            MBEquipmentRoster object2 = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(fatherEquipmentId);
            string motherAnimation = "act_character_creation_female_default_mother_front";
            string fatherAnimation = "act_character_creation_male_default_mother_front";
            this.UpdateParentEquipment(characterCreationManager, @object, object2, motherAnimation, fatherAnimation);
        }

        public void GetBattaniaFarmerNarrativeOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Athletics, DefaultSkills.Throwing };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Control, 1);
        }

        public bool BattaniaFarmerNarrativeOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "battania";
        }

        public void BattaniaFarmerNarrativeOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            characterCreationManager.CharacterCreationContent.SetParentOccupation("farmer");
            string motherEquipmentId = this.GetMotherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            string fatherEquipmentId = this.GetFatherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            MBEquipmentRoster @object = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(motherEquipmentId);
            MBEquipmentRoster object2 = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(fatherEquipmentId);
            string motherAnimation = "act_character_creation_female_default_father_sitting";
            string fatherAnimation = "act_character_creation_male_default_father_sitting";
            this.UpdateParentEquipment(characterCreationManager, @object, object2, motherAnimation, fatherAnimation);
        }

        public void GetBattaniaArtisanNarrativeOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Crafting, DefaultSkills.TwoHanded };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Endurance, 1);
        }

        public bool BattaniaArtisanNarrativeOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "battania";
        }

        public void BattaniaArtisanNarrativeOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            characterCreationManager.CharacterCreationContent.SetParentOccupation("artisan_urban");
            string motherEquipmentId = this.GetMotherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            string fatherEquipmentId = this.GetFatherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            MBEquipmentRoster @object = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(motherEquipmentId);
            MBEquipmentRoster object2 = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(fatherEquipmentId);
            string motherAnimation = "act_character_creation_female_default_side_to_side_2";
            string fatherAnimation = "act_character_creation_male_default_side_to_side_2";
            this.UpdateParentEquipment(characterCreationManager, @object, object2, motherAnimation, fatherAnimation);
        }

        public void GetBattaniaHunterNarrativeOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Scouting, DefaultSkills.Tactics };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Cunning, 1);
        }

        public bool BattaniaHunterNarrativeOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "battania";
        }

        public void BattaniaHunterNarrativeOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            characterCreationManager.CharacterCreationContent.SetParentOccupation("hunter");
            string motherEquipmentId = this.GetMotherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            string fatherEquipmentId = this.GetFatherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            MBEquipmentRoster @object = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(motherEquipmentId);
            MBEquipmentRoster object2 = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(fatherEquipmentId);
            string motherAnimation = "act_character_creation_female_default_side_to_side_3";
            string fatherAnimation = "act_character_creation_male_default_side_to_side_3";
            this.UpdateParentEquipment(characterCreationManager, @object, object2, motherAnimation, fatherAnimation);
        }

        public void GetBattaniaBardNarrativeOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Roguery, DefaultSkills.Charm };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Social, 1);
        }

        public bool BattaniaBardNarrativeOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "battania";
        }

        public void BattaniaBardNarrativeOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            characterCreationManager.CharacterCreationContent.SetParentOccupation("bard_urban");
            string motherEquipmentId = this.GetMotherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            string fatherEquipmentId = this.GetFatherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            MBEquipmentRoster @object = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(motherEquipmentId);
            MBEquipmentRoster object2 = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(fatherEquipmentId);
            string motherAnimation = "act_character_creation_female_default_hugging";
            string fatherAnimation = "act_character_creation_male_default_hugging";
            this.UpdateParentEquipment(characterCreationManager, @object, object2, motherAnimation, fatherAnimation);
        }

        public void GetKhuzaitRetainerNarrativeOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Riding, DefaultSkills.Polearm };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Endurance, 1);
        }

        public bool KhuzaitRetainerNarrativeOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "khuzait";
        }

        public void KhuzaitRetainerNarrativeOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            characterCreationManager.CharacterCreationContent.SetParentOccupation("retainer_urban");
            string motherEquipmentId = this.GetMotherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            string fatherEquipmentId = this.GetFatherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            MBEquipmentRoster @object = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(motherEquipmentId);
            MBEquipmentRoster object2 = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(fatherEquipmentId);
            string motherAnimation = "act_character_creation_female_default_side_to_side_1";
            string fatherAnimation = "act_character_creation_male_default_side_to_side_1";
            this.UpdateParentEquipment(characterCreationManager, @object, object2, motherAnimation, fatherAnimation);
        }

        public void GetKhuzaitMerchantNarrativeOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Trade, DefaultSkills.Charm };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Social, 1);
        }

        public bool KhuzaitMerchantNarrativeOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "khuzait";
        }

        public void KhuzaitMerchantNarrativeOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            characterCreationManager.CharacterCreationContent.SetParentOccupation("merchant_urban");
            string motherEquipmentId = this.GetMotherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            string fatherEquipmentId = this.GetFatherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            MBEquipmentRoster @object = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(motherEquipmentId);
            MBEquipmentRoster object2 = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(fatherEquipmentId);
            string motherAnimation = "act_character_creation_female_default_mother_front";
            string fatherAnimation = "act_character_creation_male_default_mother_front";
            this.UpdateParentEquipment(characterCreationManager, @object, object2, motherAnimation, fatherAnimation);
        }

        public void GetKhuzaitHerderNarrativeOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Bow, DefaultSkills.Riding };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Control, 1);
        }

        public bool KhuzaitHerderNarrativeOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "khuzait";
        }

        public void KhuzaitHerderNarrativeOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            characterCreationManager.CharacterCreationContent.SetParentOccupation("herder");
            string motherEquipmentId = this.GetMotherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            string fatherEquipmentId = this.GetFatherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            MBEquipmentRoster @object = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(motherEquipmentId);
            MBEquipmentRoster object2 = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(fatherEquipmentId);
            string motherAnimation = "act_character_creation_female_default_father_sitting";
            string fatherAnimation = "act_character_creation_male_default_father_sitting";
            this.UpdateParentEquipment(characterCreationManager, @object, object2, motherAnimation, fatherAnimation);
        }

        public void GetKhuzaitFarmerNarrativeOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Polearm, DefaultSkills.Throwing };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Vigor, 1);
        }

        public bool KhuzaitFarmerNarrativeOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "khuzait";
        }

        public void KhuzaitFarmerNarrativeOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            characterCreationManager.CharacterCreationContent.SetParentOccupation("farmer");
            string motherEquipmentId = this.GetMotherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            string fatherEquipmentId = this.GetFatherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            MBEquipmentRoster @object = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(motherEquipmentId);
            MBEquipmentRoster object2 = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(fatherEquipmentId);
            string motherAnimation = "act_character_creation_female_default_side_to_side_2";
            string fatherAnimation = "act_character_creation_male_default_side_to_side_2";
            this.UpdateParentEquipment(characterCreationManager, @object, object2, motherAnimation, fatherAnimation);
        }

        public void GetKhuzaitHealerNarrativeOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Medicine, DefaultSkills.Charm };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Intelligence, 1);
        }

        public bool KhuzaitHealerNarrativeOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "khuzait";
        }

        public void KhuzaitHealerNarrativeOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            characterCreationManager.CharacterCreationContent.SetParentOccupation("healer_urban");
            string motherEquipmentId = this.GetMotherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            string fatherEquipmentId = this.GetFatherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            MBEquipmentRoster @object = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(motherEquipmentId);
            MBEquipmentRoster object2 = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(fatherEquipmentId);
            string motherAnimation = "act_character_creation_female_default_side_to_side_3";
            string fatherAnimation = "act_character_creation_male_default_side_to_side_3";
            this.UpdateParentEquipment(characterCreationManager, @object, object2, motherAnimation, fatherAnimation);
        }

        public void GetKhuzaitNomadHerderNarrativeOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Scouting, DefaultSkills.Riding };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(1);
            args.SetLevelToSkills(10);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Cunning, 1);
        }

        public bool KhuzaitNomadHerderNarrativeOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "khuzait";
        }

        public void KhuzaitNomadHerderNarrativeOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            characterCreationManager.CharacterCreationContent.SetParentOccupation("herder");
            string motherEquipmentId = this.GetMotherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            string fatherEquipmentId = this.GetFatherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            MBEquipmentRoster @object = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(motherEquipmentId);
            MBEquipmentRoster object2 = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(fatherEquipmentId);
            string motherAnimation = "act_character_creation_female_default_hugging";
            string fatherAnimation = "act_character_creation_male_default_hugging";
            this.UpdateParentEquipment(characterCreationManager, @object, object2, motherAnimation, fatherAnimation);
        }


        public void GetNordLandlordNarrativeOptionArgs(NarrativeMenuOptionArgs args)
        {
            SkillObject[] affectedSkills = new SkillObject[] { DefaultSkills.Riding, DefaultSkills.Polearm, DefaultSkills.OneHanded, DefaultSkills.TwoHanded, DefaultSkills.Bow, DefaultSkills.Crossbow, DefaultSkills.Throwing, DefaultSkills.Athletics };
            args.SetAffectedSkills(affectedSkills);
            args.SetFocusToSkills(4);
            args.SetLevelToSkills(150);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Vigor, 1);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Endurance, 1);
            args.SetLevelToAttribute(DefaultCharacterAttributes.Control, 1);
        }
        public bool NordLandlordNarrativeOptionOnCondition(CharacterCreationManager characterCreationManager)
        {
            return characterCreationManager.CharacterCreationContent.SelectedCulture.StringId == "nord";
        }

        public void NordLandlordNarrativeOptionOnSelect(CharacterCreationManager characterCreationManager)
        {
            characterCreationManager.CharacterCreationContent.SetParentOccupation("retainer");
            string motherEquipmentId = this.GetMotherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            string fatherEquipmentId = this.GetFatherEquipmentId(characterCreationManager, characterCreationManager.CharacterCreationContent.SelectedParentOccupation, characterCreationManager.CharacterCreationContent.SelectedCulture.StringId);
            MBEquipmentRoster @object = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(motherEquipmentId);
            MBEquipmentRoster object2 = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(fatherEquipmentId);
            string motherAnimation = "act_character_creation_female_default_side_to_side_1";
            string fatherAnimation = "act_character_creation_male_default_side_to_side_1";
            this.UpdateParentEquipment(characterCreationManager, @object, object2, motherAnimation, fatherAnimation);
        }

    }
}