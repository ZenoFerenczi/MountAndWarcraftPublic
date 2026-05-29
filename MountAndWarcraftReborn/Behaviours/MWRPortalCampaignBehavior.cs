#nullable enable
using System.Collections.Generic;
using System.Linq;
using MountAndWarcraftReborn.Portals;
using SandBox;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Core.ImageIdentifiers;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace MountAndWarcraftReborn.Behaviors
{
    public class MWRPortalCampaignBehavior : CampaignBehaviorBase
    {
        private const int MaxActivePortals = 4;
        private const int MaxRoamingPartiesPerPortal = 10;
        private const int MinRoamingPartySize = 80;
        private const int MaxRoamingPartySize = 120;
        private const float DoubledWaveChance = 0.20f;
        private const float PortalEncounterDistance = 1.1f;
        private const float PortalReentryCooldownHours = 6f;
        private const float PortalReentryDistanceBuffer = 0.5f;
        private const float DefaultPortalVisualScale = 0.085f;
        private const float DefaultPortalVisualZOffset = 0.35f;
        private const int VictoryGoldReward = 100000;
        private const float VictoryRenownReward = 100f;
        private const string PrimaryPortalPrefab = "mwr_dark_portal_map";
        private const string SecondaryPortalPrefab = "trollgate";
        private const string TertiaryPortalPrefab = "elf_gate1";
        private readonly HashSet<string> _createdPortalMapEntities = new HashSet<string>();
        private Dictionary<string, double> _portalLeaveCooldownUntilHours = new Dictionary<string, double>();

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
            CampaignEvents.OnAfterSessionLaunchedEvent.AddNonSerializedListener(this, OnAfterSessionLaunched);
            CampaignEvents.WeeklyTickEvent.AddNonSerializedListener(this, OnWeeklyTick);
            CampaignEvents.DailyTickSettlementEvent.AddNonSerializedListener(this, OnDailyTickSettlement);
            CampaignEvents.HourlyTickPartyEvent.AddNonSerializedListener(this, OnHourlyTickParty);
            CampaignEvents.AiHourlyTickEvent.AddNonSerializedListener(this, OnAiHourlyTick);
            CampaignEvents.OnMissionEndedEvent.AddNonSerializedListener(this, OnMissionEnded);
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("_portalLeaveCooldownUntilHours", ref _portalLeaveCooldownUntilHours);
        }

        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            AddPortalMenus(starter);
            EnsurePortalMapEntities();

            foreach (MWRPortalSiteComponent portal in GetPortalSiteComponents())
            {
                if (portal.IsClosedPermanently)
                {
                    portal.IsActive = false;
                }

                if (portal.IsBattleUnderway && !HasBattleDefenderParty(portal.Settlement))
                {
                    portal.IsBattleUnderway = false;
                }
            }
        }

        private void OnAfterSessionLaunched(CampaignGameStarter starter)
        {
            EnsurePortalMapEntities();
        }

        private void OnWeeklyTick()
        {
            List<MWRPortalSiteComponent> activePortals = GetPortalSiteComponents()
                .Where(portal => portal.IsActive)
                .ToList();

            if (activePortals.Count >= MaxActivePortals)
            {
                return;
            }

            List<MWRPortalSiteComponent> dormantPortals = GetPortalSiteComponents()
                .Where(portal => !portal.IsActive && !portal.IsClosedPermanently)
                .ToList();

            if (dormantPortals.Count == 0)
            {
                return;
            }

            MWRPortalSiteComponent chosenPortal = dormantPortals[MBRandom.RandomInt(dormantPortals.Count)];
            chosenPortal.IsActive = true;
            chosenPortal.IsBattleUnderway = false;

            InformationManager.DisplayMessage(
                new InformationMessage($"{chosenPortal.Settlement.Name} tears open and begins spilling undead raiders into the world."));
        }

        private void OnDailyTickSettlement(Settlement settlement)
        {
            if (settlement.SettlementComponent is not MWRPortalSiteComponent portal ||
                !portal.IsActive ||
                portal.IsBattleUnderway ||
                portal.IsClosedPermanently)
            {
                return;
            }

            if (CountActivePortalRaiders(settlement) >= MaxRoamingPartiesPerPortal)
            {
                return;
            }

            SpawnPortalRaider(portal);
        }

        private void OnAiHourlyTick(MobileParty party, PartyThinkParams thinkParams)
        {
            if (party?.PartyComponent is MWRPortalRaidingPartyComponent component)
            {
                component.HourlyTickAI(thinkParams);
            }
        }

        private void OnHourlyTickParty(MobileParty party)
        {
            if (party != MobileParty.MainParty)
            {
                return;
            }

            if (party.MapEvent != null ||
                party.CurrentSettlement != null ||
                Campaign.Current?.CurrentMenuContext != null)
            {
                return;
            }

            Settlement? nearbyPortal = GetNearbyPortalForPlayer(party);
            if (nearbyPortal == null)
            {
                return;
            }

            EnsurePortalSettlementEncounter(nearbyPortal);
        }

        private void OnMissionEnded(IMission missionObject)
        {
            MWRPortalSiteComponent? portal = GetPortalSiteComponents().FirstOrDefault(site => site.IsBattleUnderway);
            if (portal == null)
            {
                return;
            }

            portal.IsBattleUnderway = false;
            CleanupBattleDefenders(portal.Settlement);

            Mission? mission = missionObject as Mission;
            bool playerWon = mission?.MissionResult != null &&
                             mission.MissionResult.BattleResolved &&
                             mission.MissionResult.PlayerVictory;

            if (!playerWon)
            {
                InformationManager.ShowInquiry(
                    new InquiryData(
                        "Portal Assault Failed",
                        "The defenders forced you back and the portal remains open.",
                        true,
                        false,
                        "OK",
                        null,
                        null,
                        null));
                return;
            }

            ClosePortal(portal);
            AwardVictoryRewards(portal);
        }

        private void AddPortalMenus(CampaignGameStarter starter)
        {
            starter.AddGameMenu("mwr_portal_site_menu", "{PORTAL_SITE_DESCRIPTION}", PortalMenuInit);
            starter.AddGameMenuOption("mwr_portal_site_menu", "mwr_portal_attack", "{PORTAL_ATTACK_OPTION}", PortalBattleCondition, PortalBattleConsequence);
            starter.AddGameMenuOption(
                "mwr_portal_site_menu",
                "leave",
                "Leave...",
                args =>
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                    return true;
                },
                args =>
                {
                    if (Settlement.CurrentSettlement?.SettlementComponent is MWRPortalSiteComponent portal)
                    {
                        MarkPortalRecentlyLeft(portal.Settlement);
                    }

                    PlayerEncounter.Finish(true);
                },
                true);
        }

        private void PortalMenuInit(MenuCallbackArgs args)
        {
            if (Settlement.CurrentSettlement?.SettlementComponent is not MWRPortalSiteComponent portal)
            {
                MBTextManager.SetTextVariable("PORTAL_SITE_DESCRIPTION", "The air here is strangely still.");
                return;
            }

            string description = portal.IsClosedPermanently
                ? "The breach has been sealed. Frost still clings to the stones, but the portal is dead."
                : portal.IsActive
                    ? "A violent rift churns above the site. Undead raiders spill from it whenever it remains open."
                    : "The place lies dormant, but the ground is scarred by old necromantic power.";

            MBTextManager.SetTextVariable("PORTAL_SITE_DESCRIPTION", description);

            string backgroundMesh = string.IsNullOrWhiteSpace(portal.BackgroundMeshName)
                ? portal.WaitMeshName
                : portal.BackgroundMeshName;

            if (!string.IsNullOrWhiteSpace(backgroundMesh))
            {
                args.MenuContext.SetBackgroundMeshName(backgroundMesh);
            }
        }

        private bool PortalBattleCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.HostileAction;
            MBTextManager.SetTextVariable("PORTAL_ATTACK_OPTION", "Assault the portal");

            if (Settlement.CurrentSettlement?.SettlementComponent is not MWRPortalSiteComponent portal)
            {
                args.IsEnabled = false;
                return true;
            }

            if (!portal.IsActive)
            {
                args.IsEnabled = false;
                args.Tooltip = new TextObject("The portal is dormant.");
                return true;
            }

            if (portal.IsBattleUnderway)
            {
                args.IsEnabled = false;
                args.Tooltip = new TextObject("A battle is already underway here.");
                return true;
            }

            if (Hero.MainHero.IsWounded)
            {
                args.IsEnabled = false;
                args.Tooltip = new TextObject("You are wounded.");
                return true;
            }

            if (portal.SpawnClan == null || portal.BattleSpawnTemplate == null)
            {
                args.IsEnabled = false;
                args.Tooltip = new TextObject("The portal defenders could not be prepared.");
                return true;
            }

            return true;
        }

        private void PortalBattleConsequence(MenuCallbackArgs args)
        {
            if (Settlement.CurrentSettlement?.SettlementComponent is not MWRPortalSiteComponent portal)
            {
                return;
            }

            Clan? spawnClan = portal.SpawnClan;
            PartyTemplateObject? spawnTemplate = portal.BattleSpawnTemplate;
            if (spawnClan == null || spawnTemplate == null)
            {
                InformationManager.DisplayMessage(new InformationMessage("The portal flickers, but no defenders answer its call."));
                return;
            }

            CleanupBattleDefenders(portal.Settlement);

            MobileParty defenderParty = MWRPortalRaidingPartyComponent.CreatePortalParty(
                CreateUniquePartyId($"{portal.HomeSiteId}_defender"),
                portal.Settlement,
                $"{portal.Settlement.Name} Defenders",
                spawnTemplate,
                spawnClan,
                portal.BattlePartySize,
                true);

            portal.IsBattleUnderway = true;

            PlayerEncounter.RestartPlayerEncounter(defenderParty.Party, PartyBase.MainParty, false);
            if (PlayerEncounter.Battle == null)
            {
                PlayerEncounter.StartBattle();
                PlayerEncounter.Update();
            }

            CampaignMission.OpenBattleMission(portal.BattleSceneName, false);
        }

        private void SpawnPortalRaider(MWRPortalSiteComponent portal)
        {
            Clan? spawnClan = portal.SpawnClan;
            PartyTemplateObject? spawnTemplate = portal.SpawnTemplate;
            if (spawnClan == null || spawnTemplate == null)
            {
                return;
            }

            int minRoamingPartySize = portal.MinRoamingPartySize > 0
                ? portal.MinRoamingPartySize
                : MinRoamingPartySize;
            int maxRoamingPartySize = portal.MaxRoamingPartySize > 0
                ? portal.MaxRoamingPartySize
                : MaxRoamingPartySize;

            if (maxRoamingPartySize < minRoamingPartySize)
            {
                maxRoamingPartySize = minRoamingPartySize;
            }

            int targetSize = MBRandom.RandomInt(minRoamingPartySize, maxRoamingPartySize + 1);
            if (MBRandom.RandomFloat <= DoubledWaveChance)
            {
                targetSize *= 2;
            }

            MobileParty raiderParty = MWRPortalRaidingPartyComponent.CreatePortalParty(
                CreateUniquePartyId($"{portal.HomeSiteId}_raider"),
                portal.Settlement,
                $"{portal.Settlement.Name} Raiders",
                spawnTemplate,
                spawnClan,
                targetSize,
                false);

            if (raiderParty.PartyComponent is MWRPortalRaidingPartyComponent raiderComponent)
            {
                raiderComponent.TargetSettlement = FindInitialTarget(portal.Settlement, spawnClan);
            }
        }

        private static Settlement? FindInitialTarget(Settlement homeSettlement, Clan spawnClan)
        {
            Vec2 origin = homeSettlement.Position.ToVec2();

            Settlement? bestVillage = Settlement.All
                .Where(settlement => settlement != null && settlement.MapFaction != null)
                .Where(settlement => settlement != homeSettlement && settlement.IsVillage)
                .Where(settlement => settlement.MapFaction.Culture?.StringId != spawnClan.Culture?.StringId)
                .Where(settlement => !settlement.IsRaided && !settlement.IsUnderRaid)
                .OrderBy(settlement => origin.DistanceSquared(settlement.Position.ToVec2()))
                .FirstOrDefault();

            if (bestVillage != null)
            {
                return bestVillage;
            }

            return Settlement.All
                .Where(settlement => settlement != null && settlement.MapFaction != null)
                .Where(settlement => settlement != homeSettlement && (settlement.IsTown || settlement.IsCastle))
                .Where(settlement => settlement.MapFaction.Culture?.StringId != spawnClan.Culture?.StringId)
                .OrderBy(settlement => origin.DistanceSquared(settlement.Position.ToVec2()))
                .FirstOrDefault();
        }

        private void ClosePortal(MWRPortalSiteComponent portal)
        {
            portal.IsActive = false;
            portal.IsClosedPermanently = true;
            CleanupPortalRaiders(portal.Settlement);

            InformationManager.DisplayMessage(
                new InformationMessage($"{portal.Settlement.Name} is sealed and its raiding parties collapse with it."));
        }

        private void AwardVictoryRewards(MWRPortalSiteComponent portal)
        {
            GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, VictoryGoldReward, false);
            GainRenownAction.Apply(Hero.MainHero, VictoryRenownReward);

            List<ItemObject> rewardItems = portal.RewardItemIds
                .Select(id => MBObjectManager.Instance.GetObject<ItemObject>(id))
                .Where(item => item != null)
                .Cast<ItemObject>()
                .GroupBy(item => item.StringId)
                .Select(group => group.First())
                .OrderBy(_ => MBRandom.RandomFloat)
                .Take(4)
                .ToList();

            if (rewardItems.Count == 0)
            {
                InformationManager.DisplayMessage(
                    new InformationMessage($"You seal the portal and claim {VictoryGoldReward} gold and {VictoryRenownReward} renown."));
                return;
            }

            List<InquiryElement> rewardChoices = rewardItems
                .Select(item => new InquiryElement(item, item.Name.ToString(), new ItemImageIdentifier(item), true, item.Name.ToString()))
                .ToList();

            MultiSelectionInquiryData inquiry = new MultiSelectionInquiryData(
                "Portal Sealed",
                $"Choose one relic from the ruins. You also gain {VictoryGoldReward} gold and {VictoryRenownReward} renown.",
                rewardChoices,
                false,
                1,
                1,
                "Claim",
                null,
                OnRewardClaimed,
                null);

            MBInformationManager.ShowMultiSelectionInquiry(inquiry);
        }

        private void OnRewardClaimed(List<InquiryElement> selectedRewards)
        {
            if (selectedRewards.Count == 0 || selectedRewards[0].Identifier is not ItemObject item)
            {
                return;
            }

            MobileParty? mainParty = Hero.MainHero.PartyBelongedTo;
            if (mainParty?.Party?.ItemRoster == null)
            {
                return;
            }

            mainParty.Party.ItemRoster.AddToCounts(item, 1);
            InformationManager.DisplayMessage(new InformationMessage($"{item.Name} has been added to your inventory."));
        }

        private static IEnumerable<MWRPortalSiteComponent> GetPortalSiteComponents()
        {
            return MWRPortalSiteHelper.GetAllPortalSites()
                .Select(settlement => settlement.SettlementComponent)
                .OfType<MWRPortalSiteComponent>();
        }

        private static int CountActivePortalRaiders(Settlement homeSettlement)
        {
            return MobileParty.All.Count(party =>
                party?.PartyComponent is MWRPortalRaidingPartyComponent component &&
                !component.IsBattleDefender &&
                component.HomeSettlement == homeSettlement);
        }

        private static void CleanupPortalRaiders(Settlement homeSettlement)
        {
            List<MobileParty> raiderParties = MobileParty.All
                .Where(party =>
                    party?.PartyComponent is MWRPortalRaidingPartyComponent component &&
                    component.HomeSettlement == homeSettlement)
                .ToList();

            foreach (MobileParty party in raiderParties)
            {
                if (party.MapEvent == null)
                {
                    DestroyPartyAction.Apply(null, party);
                }
            }
        }

        private static void CleanupBattleDefenders(Settlement homeSettlement)
        {
            List<MobileParty> battleDefenders = MobileParty.All
                .Where(party =>
                    party?.PartyComponent is MWRPortalRaidingPartyComponent component &&
                    component.IsBattleDefender &&
                    component.HomeSettlement == homeSettlement)
                .ToList();

            foreach (MobileParty party in battleDefenders)
            {
                if (party.MapEvent == null)
                {
                    DestroyPartyAction.Apply(null, party);
                }
            }
        }

        private static bool HasBattleDefenderParty(Settlement homeSettlement)
        {
            return MobileParty.All.Any(party =>
                party?.PartyComponent is MWRPortalRaidingPartyComponent component &&
                component.IsBattleDefender &&
                component.HomeSettlement == homeSettlement);
        }

        private static string CreateUniquePartyId(string prefix)
        {
            string candidate = prefix;
            int suffix = 1;

            while (MobileParty.All.Any(party => party != null && party.StringId == candidate))
            {
                candidate = $"{prefix}_{suffix}";
                suffix++;
            }

            return candidate;
        }

        private Settlement? GetNearbyPortalForPlayer(MobileParty playerParty)
        {
            Vec2 playerPosition = playerParty.Position.ToVec2();

            return MWRPortalSiteHelper.GetAllPortalSites()
                .Where(settlement => settlement != null)
                .Where(settlement =>
                {
                    float encounterDistance = GetEncounterDistance(settlement);
                    return playerPosition.DistanceSquared(settlement.Position.ToVec2()) <= encounterDistance * encounterDistance;
                })
                .Where(settlement => !IsPortalOnReentryCooldown(settlement, playerPosition))
                .OrderBy(settlement => playerPosition.DistanceSquared(settlement.Position.ToVec2()))
                .FirstOrDefault();
        }

        private static void EnsurePortalSettlementEncounter(Settlement settlement)
        {
            if (PlayerEncounter.Current != null && PlayerEncounter.EncounterSettlement != settlement)
            {
                PlayerEncounter.Finish(false);
            }

            if (PlayerEncounter.Current != null &&
                PlayerEncounter.EncounterSettlement == settlement &&
                PlayerEncounter.LocationEncounter == null)
            {
                PlayerEncounter.EnterSettlement();
                return;
            }

            EnterSettlementAction.ApplyForParty(MobileParty.MainParty, settlement);

            if (PlayerEncounter.Current == null || PlayerEncounter.EncounterSettlement != settlement)
            {
                EncounterManager.StartSettlementEncounter(MobileParty.MainParty, settlement);
            }
        }

        private void EnsurePortalMapEntities()
        {
            if (Campaign.Current?.MapSceneWrapper is not MapScene mapScene || mapScene.Scene == null)
            {
                return;
            }

            foreach (Settlement settlement in MWRPortalSiteHelper.GetAllPortalSites())
            {
                if (!_createdPortalMapEntities.Add(settlement.StringId))
                {
                    continue;
                }

                CreatePortalMapEntity(mapScene, settlement);
            }
        }

        private static void CreatePortalMapEntity(MapScene mapScene, Settlement settlement)
        {
            Vec3 position = settlement.GetPositionAsVec3();
            MatrixFrame frame = new MatrixFrame(Mat3.Identity, position);

            GameEntity rootEntity = GameEntity.CreateEmpty(mapScene.Scene, true);
            rootEntity.Name = settlement.StringId;
            rootEntity.SetGlobalFrame(frame);

            AddPortalVisual(rootEntity, mapScene, settlement);
            AddPortalMarker(rootEntity, mapScene, settlement);
        }

        private static void AddPortalVisual(GameEntity rootEntity, MapScene mapScene, Settlement settlement)
        {
            GameEntity? visualEntity = GetPortalPrefabCandidates(settlement)
                .Select(prefabName => TryInstantiatePortalPrefab(mapScene, prefabName))
                .FirstOrDefault(entity => entity != null);

            if (visualEntity == null)
            {
                return;
            }

            visualEntity.Name = settlement.StringId;
            MatrixFrame visualFrame = new MatrixFrame(
                Mat3.Identity,
                new Vec3(0f, 0f, GetPortalVisualZOffset(settlement)));
            float visualScale = GetPortalVisualScale(settlement);
            visualFrame.Scale(new Vec3(visualScale, visualScale, visualScale));
            visualEntity.SetFrame(ref visualFrame);
            rootEntity.AddChild(visualEntity);
        }

        private static float GetPortalVisualScale(Settlement settlement)
        {
            if (settlement.SettlementComponent is MWRPortalSiteComponent portal && portal.PortalVisualScale > 0f)
            {
                return portal.PortalVisualScale;
            }

            return DefaultPortalVisualScale;
        }

        private static float GetPortalVisualZOffset(Settlement settlement)
        {
            if (settlement.SettlementComponent is MWRPortalSiteComponent portal)
            {
                return portal.PortalVisualZOffset;
            }

            return DefaultPortalVisualZOffset;
        }

        private static float GetEncounterDistance(Settlement settlement)
        {
            if (settlement.SettlementComponent is MWRPortalSiteComponent portal && portal.EncounterDistance > 0f)
            {
                return portal.EncounterDistance;
            }

            return PortalEncounterDistance;
        }

        private void MarkPortalRecentlyLeft(Settlement settlement)
        {
            if (settlement == null)
            {
                return;
            }

            _portalLeaveCooldownUntilHours[settlement.StringId] = CampaignTime.HoursFromNow(PortalReentryCooldownHours).ToHours;
        }

        private bool IsPortalOnReentryCooldown(Settlement settlement, Vec2 playerPosition)
        {
            if (!_portalLeaveCooldownUntilHours.TryGetValue(settlement.StringId, out double cooldownUntilHours))
            {
                return false;
            }

            float encounterDistance = GetEncounterDistance(settlement);
            float releaseDistance = encounterDistance + PortalReentryDistanceBuffer;
            if (playerPosition.DistanceSquared(settlement.Position.ToVec2()) > releaseDistance * releaseDistance)
            {
                _portalLeaveCooldownUntilHours.Remove(settlement.StringId);
                return false;
            }

            if (CampaignTime.Now.ToHours >= cooldownUntilHours)
            {
                _portalLeaveCooldownUntilHours.Remove(settlement.StringId);
                return false;
            }

            return true;
        }

        private static IEnumerable<string> GetPortalPrefabCandidates(Settlement settlement)
        {
            if (settlement.SettlementComponent is MWRPortalSiteComponent portal &&
                !string.IsNullOrWhiteSpace(portal.PortalPrefabId))
            {
                yield return portal.PortalPrefabId;
            }

            if (!(settlement.SettlementComponent is MWRPortalSiteComponent component &&
                  string.Equals(component.PortalPrefabId, PrimaryPortalPrefab, global::System.StringComparison.OrdinalIgnoreCase)))
            {
                yield return PrimaryPortalPrefab;
            }

            yield return SecondaryPortalPrefab;
            yield return TertiaryPortalPrefab;
        }

        private static GameEntity? TryInstantiatePortalPrefab(MapScene mapScene, string prefabName)
        {
            try
            {
                return GameEntity.Instantiate(mapScene.Scene, prefabName, false);
            }
            catch
            {
                return null;
            }
        }

        private static void AddPortalMarker(GameEntity rootEntity, MapScene mapScene, Settlement settlement)
        {
            GameEntity markerEntity = GameEntity.CreateEmpty(mapScene.Scene, true);
            markerEntity.Name = settlement.StringId;

            Decal? markerDecal = Decal.CreateDecal();
            if (markerDecal == null)
            {
                return;
            }

            Material material = Material.GetFromResource("decal_city_circle_a");
            markerDecal.SetMaterial(material);
            markerDecal.SetFactor1Linear(4281663744U);
            mapScene.Scene.AddDecalInstance(markerDecal, "editor_set", false);
            markerEntity.AddComponent(markerDecal);

            MatrixFrame markerFrame = new MatrixFrame(Mat3.Identity, new Vec3(0f, 0f, -0.2f));
            markerFrame.Scale(new Vec3(10f, 10f, 1f));
            markerEntity.SetFrame(ref markerFrame);
            rootEntity.AddChild(markerEntity);
        }
    }
}
