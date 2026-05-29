using HarmonyLib;
using MountAndWarcraftReborn.Components.Models;
using MountAndWarcraftReborn.Portals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MWRMode.Behaviors;
using MountAndWarcraftReborn.Behaviors;
using MWRMode.Components.Models;
using MWRMode.Mechanics.Undead;
using SandBox.GameComponents;
using SandBox.Missions.MissionLogics;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ComponentInterfaces;
using MountAndWarcraftReborn.BattleMechanics;
using MountAndWarcraftReborn.BattleMechanics.Music;
using MountAndWarcraftReborn.Magic.Mission;

namespace MountAndWarcraftReborn
{
    public class SubModule : MBSubModuleBase
    {
        private const string HarmonyId = "MountAndWarcraftCore";

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            var harmony = new Harmony(HarmonyId);
            PatchNamespace(harmony, Assembly.GetExecutingAssembly(), "MWRMode.Patches.CharacterCreation");
            PatchNamespace(harmony, Assembly.GetExecutingAssembly(), "MWRMode.Cultures");
            PatchSpecificType(harmony, typeof(AddOnlyOurCulturesPatch));
            PatchNamespace(harmony, Assembly.GetExecutingAssembly(), "MountAndWarcraftReborn.Patches.MainMenu");
            PatchNamespace(harmony, Assembly.GetExecutingAssembly(), "MountAndWarcraftReborn.Patches.Movies");
            PatchNamespace(harmony, Assembly.GetExecutingAssembly(), "MountAndWarcraftReborn.Patches.Settlement");
            PatchNamespace(harmony, Assembly.GetExecutingAssembly(), "MountAndWarcraftReborn.Patches.Banner");
            PatchNamespace(harmony, Assembly.GetExecutingAssembly(), "MountAndWarcraftReborn.Patches.Hireling");
            PatchNamespace(harmony, Assembly.GetExecutingAssembly(), "MountAndWarcraftReborn.Patches.Magic");
            PatchNamespace(harmony, Assembly.GetExecutingAssembly(), "MountAndWarcraftReborn.Patches.TroopWeight");
            PatchNamespace(harmony, Assembly.GetExecutingAssembly(), "MWRMode.Patches.TroopWeight");
        }

        public override void OnMissionBehaviorInitialize(Mission mission)
        {
            base.OnMissionBehaviorInitialize(mission);

            mission.AddMissionBehavior(new MWRUndeadMoraleMissionLogic());
            mission.AddMissionBehavior(new MWRHirelingBattleMissionLogic());
            mission.AddMissionBehavior(new MWRMagicAbilityManagerMissionLogic());
            mission.AddMissionBehavior(new MWRMagicHUDMissionView());
        }
        protected override void OnSubModuleUnloaded()
        {
            base.OnSubModuleUnloaded();

        }

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            base.OnBeforeInitialModuleScreenSetAsRoot();
            
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarter)
        {
            base.OnGameStart(game, gameStarter);

            if (gameStarter is not CampaignGameStarter gs)
            {
                return;
            }

            ReplaceModel<PartySizeLimitModel>(gs, new CustomPartySizeLimitModel());
            ReplaceModel<CharacterStatsModel>(gs, new MWRCharacterStatsModel());
            ReplaceModel<PartyWageModel>(gs, new MWRPartyWageModel());
            ReplaceModel<PrisonerRecruitmentCalculationModel>(gs, new MWRPrisonerRecruitmentCalculationModel());
            ReplaceModel<MobilePartyFoodConsumptionModel>(gs, new MWRMobilePartyFoodConsumptionModel());
            ReplaceModel<CombatSimulationModel>(gs, new MWRCombatSimulationModel());
            ReplaceModel<AgentApplyDamageModel>(gs, new MWRAgentApplyDamageModel());
            ReplaceModel<AgentStatCalculateModel>(gs, new MWRAgentStatCalculateModel());
            ReplaceModel<SettlementMilitiaModel>(gs, new MWRSettlementMilitiaModel());
            ReplaceModel<EncounterGameMenuModel>(gs, new MWREncounterGameMenuModel());
            ReplaceModel<MarriageModel>(gs, new MWRMarriageModel());
            ReplaceModel<PartySpeedModel>(gs, new MWRPartySpeedModel(GetExistingModel<PartySpeedModel>(gs)));
            ReplaceModel<ArmyManagementCalculationModel>(gs, new MWRArmyManagementCalculationModel(GetExistingModel<ArmyManagementCalculationModel>(gs)));
            gs.AddModel(new MWRBattleMoraleModel());
            gs.AddModel(new MWRDamageParticleModel());

            gs.AddBehavior(new MWRSettlementCultureConversionBehavior());
            gs.AddBehavior(new MWRUndeadWarBehavior());
            gs.AddBehavior(new MWRFactionDiscontinuationBehavior());
            gs.AddBehavior(new MWRPortalCampaignBehavior());
            gs.AddBehavior(new MWRPatrolTemplateSizeCampaignBehavior());
            gs.AddBehavior(new MWRMagicManagerBehavior());
            gs.AddBehavior(new MWRBattleResultMusicCampaignBehavior());
            gs.AddBehavior(new MWRHirelingCampaignBehavior());
            gs.AddBehavior(new MWRPartyCultureCleanupBehavior());
            gs.AddBehavior(new MWRHeroHealthInitializeBehavior());
            gs.AddBehavior(new MWRUndeadRaiseDeadCampaignBehavior());
            gs.AddBehavior(new MWRUndeadRaiseDeadAiCampaignBehavior());

            ReplaceModel<DiplomacyModel>(gs, new MWRDiplomacyModel());
            ReplaceModel<VolunteerModel>(gs, new MWRVolunteerModel());

            CampaignOptions.IsLifeDeathCycleDisabled = true;
        }

        public override void BeginGameStart(Game game)
        {
            base.BeginGameStart(game);

            if (game.GameType is Campaign)
            {
                game.ObjectManager.RegisterType<MWRPortalSiteComponent>("PortalSite", "Components", 230U, true);
            }
        }

        private static void PatchNamespace(Harmony harmony, Assembly assembly, string namespacePrefix)
        {
            string[] excludedTypeNames =
            {
                "NarrativeMenuCharacter_SetAnimationId_Patch",
                "NarrativeMenuCharacter_SetEquipment_Patch",
                "NarrativeMenuCharacter_SetRightHandItem_Patch",
                "NarrativeMenuCharacter_SetLeftHandItem_Patch",
                "CharacterPreviewRaceFixPatches"
            };

            foreach (Type type in assembly.GetTypes()
                         .Where(t => t.Namespace != null &&
                                     t.Namespace.StartsWith(namespacePrefix, StringComparison.Ordinal) &&
                                     t.GetCustomAttributes(typeof(HarmonyPatch), true).Length > 0 &&
                                     !excludedTypeNames.Contains(t.Name, StringComparer.Ordinal)))
            {
                harmony.CreateClassProcessor(type).Patch();
            }
        }

        private static void PatchSpecificType(Harmony harmony, Type type)
        {
            if (type == null || type.GetCustomAttributes(typeof(HarmonyPatch), true).Length == 0)
                return;

            harmony.CreateClassProcessor(type).Patch();
        }

        private static void ReplaceModel<TBaseModel>(CampaignGameStarter gameStarter, GameModel replacementModel)
            where TBaseModel : GameModel
        {
            var models = (List<GameModel>)gameStarter.Models;
            for (int i = models.Count - 1; i >= 0; i--)
            {
                if (models[i] is TBaseModel)
                {
                    models.RemoveAt(i);
                }
            }

            gameStarter.AddModel(replacementModel);
        }

        private static TBaseModel GetExistingModel<TBaseModel>(CampaignGameStarter gameStarter)
            where TBaseModel : GameModel
        {
            var models = (List<GameModel>)gameStarter.Models;
            for (int i = models.Count - 1; i >= 0; i--)
            {
                if (models[i] is TBaseModel model)
                {
                    return model;
                }
            }

            return null;
        }
    }
}
