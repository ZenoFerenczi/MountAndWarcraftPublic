#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;

namespace MountAndWarcraftReborn.Portals
{
    public class MWRPortalSiteComponent : SettlementComponent
    {
        private const string DefaultBattleScene = "nord_battle_terrain_a";
        private const string DefaultSpawnTemplateId = "kingdom_hero_party_nord_template";
        private const string DefaultSpawnClanId = "clan_nord_1";
        private const string DefaultPortalPrefabId = "mwr_dark_portal_map";
        private const int DefaultMinRoamingPartySize = 80;
        private const int DefaultMaxRoamingPartySize = 120;
        private const float DefaultPortalVisualScale = 0.085f;
        private const float DefaultPortalVisualZOffset = 0.35f;
        private const float DefaultEncounterDistance = 1.1f;

        private string _battleSceneName = DefaultBattleScene;
        private string _spawnTemplateId = DefaultSpawnTemplateId;
        private string _battleSpawnTemplateId = string.Empty;
        private string _spawnClanId = DefaultSpawnClanId;
        private string _portalPrefabId = DefaultPortalPrefabId;
        private string _homeSiteId = string.Empty;
        private readonly List<string> _rewardItemIds = new List<string>();

        [SaveableProperty(1)]
        public bool IsActive { get; set; }

        [SaveableProperty(2)]
        public bool IsBattleUnderway { get; set; }

        [SaveableProperty(3)]
        public bool IsClosedPermanently { get; set; }

        [SaveableProperty(4)]
        public Clan? OwnerClan { get; set; }

        public int BattlePartySize { get; private set; } = 400;
        public int MinRoamingPartySize { get; private set; } = DefaultMinRoamingPartySize;
        public int MaxRoamingPartySize { get; private set; } = DefaultMaxRoamingPartySize;
        public float PortalVisualScale { get; private set; } = DefaultPortalVisualScale;
        public float PortalVisualZOffset { get; private set; } = DefaultPortalVisualZOffset;
        public float EncounterDistance { get; private set; } = DefaultEncounterDistance;

        public string BattleSceneName => _battleSceneName;

        public IReadOnlyList<string> RewardItemIds => _rewardItemIds;

        public string SpawnTemplateId => _spawnTemplateId;

        public string BattleSpawnTemplateId => string.IsNullOrWhiteSpace(_battleSpawnTemplateId)
            ? _spawnTemplateId
            : _battleSpawnTemplateId;

        public string SpawnClanId => _spawnClanId;

        public string PortalPrefabId => _portalPrefabId;

        public string HomeSiteId => string.IsNullOrWhiteSpace(_homeSiteId)
            ? Settlement?.StringId ?? string.Empty
            : _homeSiteId;

        public PartyTemplateObject? SpawnTemplate => MBObjectManager.Instance.GetObject<PartyTemplateObject>(_spawnTemplateId);

        public PartyTemplateObject? BattleSpawnTemplate => MBObjectManager.Instance.GetObject<PartyTemplateObject>(BattleSpawnTemplateId);

        public Clan? SpawnClan => Clan.All.FirstOrDefault(x => x.StringId == _spawnClanId)
            ?? MBObjectManager.Instance.GetObject<Clan>(_spawnClanId);

        public override IFaction MapFaction => (IFaction?)OwnerClan ?? (IFaction?)SpawnClan ?? Clan.PlayerClan;

        public override void OnInit()
        {
            base.OnInit();
            EnsureDefaults();
        }

        protected override void AfterLoad()
        {
            base.AfterLoad();
            EnsureDefaults();
        }

        public override void Deserialize(MBObjectManager objectManager, XmlNode node)
        {
            base.Deserialize(objectManager, node);

            if (node.Attributes == null)
            {
                EnsureDefaults();
                return;
            }

            if (node.Attributes["id"] != null)
            {
                _homeSiteId = node.Attributes["id"]!.Value;
            }

            if (node.Attributes["background_crop_position"] != null &&
                float.TryParse(node.Attributes["background_crop_position"]!.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out float cropPosition))
            {
                BackgroundCropPosition = cropPosition;
            }

            if (node.Attributes["background_mesh"] != null)
            {
                BackgroundMeshName = node.Attributes["background_mesh"]!.Value;
            }

            if (node.Attributes["wait_mesh"] != null)
            {
                WaitMeshName = node.Attributes["wait_mesh"]!.Value;
            }

            if (node.Attributes["battle_scene"] != null)
            {
                _battleSceneName = node.Attributes["battle_scene"]!.Value;
            }

            if (node.Attributes["battle_party_size"] != null &&
                int.TryParse(node.Attributes["battle_party_size"]!.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int battlePartySize))
            {
                BattlePartySize = Math.Max(1, battlePartySize);
            }

            if (node.Attributes["spawn_template"] != null)
            {
                _spawnTemplateId = node.Attributes["spawn_template"]!.Value;
            }

            if (node.Attributes["battle_spawn_template"] != null)
            {
                _battleSpawnTemplateId = node.Attributes["battle_spawn_template"]!.Value;
            }

            if (node.Attributes["spawn_clan"] != null)
            {
                _spawnClanId = node.Attributes["spawn_clan"]!.Value;
            }

            if (node.Attributes["portal_prefab"] != null)
            {
                _portalPrefabId = node.Attributes["portal_prefab"]!.Value;
            }

            if (node.Attributes["portal_visual_scale"] != null &&
                float.TryParse(node.Attributes["portal_visual_scale"]!.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out float portalVisualScale))
            {
                PortalVisualScale = Math.Max(0.001f, portalVisualScale);
            }

            if (node.Attributes["portal_visual_z_offset"] != null &&
                float.TryParse(node.Attributes["portal_visual_z_offset"]!.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out float portalVisualZOffset))
            {
                PortalVisualZOffset = portalVisualZOffset;
            }

            if (node.Attributes["encounter_distance"] != null &&
                float.TryParse(node.Attributes["encounter_distance"]!.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out float encounterDistance))
            {
                EncounterDistance = Math.Max(0.05f, encounterDistance);
            }

            if (node.Attributes["min_roaming_party_size"] != null &&
                int.TryParse(node.Attributes["min_roaming_party_size"]!.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int minRoamingPartySize))
            {
                MinRoamingPartySize = Math.Max(1, minRoamingPartySize);
            }

            if (node.Attributes["max_roaming_party_size"] != null &&
                int.TryParse(node.Attributes["max_roaming_party_size"]!.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int maxRoamingPartySize))
            {
                MaxRoamingPartySize = Math.Max(1, maxRoamingPartySize);
            }

            if (node.Attributes["reward_items"] != null)
            {
                _rewardItemIds.Clear();
                _rewardItemIds.AddRange(
                    node.Attributes["reward_items"]!.Value
                        .Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim())
                        .Where(x => !string.IsNullOrWhiteSpace(x)));
            }

            EnsureDefaults();
        }

        protected override void OnInventoryUpdated(ItemRosterElement item, int count)
        {
        }

        private void EnsureDefaults()
        {
            if (string.IsNullOrWhiteSpace(_battleSceneName))
            {
                _battleSceneName = DefaultBattleScene;
            }

            if (string.IsNullOrWhiteSpace(_spawnTemplateId))
            {
                _spawnTemplateId = DefaultSpawnTemplateId;
            }

            if (string.IsNullOrWhiteSpace(_battleSpawnTemplateId))
            {
                _battleSpawnTemplateId = _spawnTemplateId;
            }

            if (string.IsNullOrWhiteSpace(_spawnClanId))
            {
                _spawnClanId = DefaultSpawnClanId;
            }

            if (string.IsNullOrWhiteSpace(_portalPrefabId))
            {
                _portalPrefabId = DefaultPortalPrefabId;
            }

            if (MinRoamingPartySize <= 0)
            {
                MinRoamingPartySize = DefaultMinRoamingPartySize;
            }

            if (MaxRoamingPartySize <= 0)
            {
                MaxRoamingPartySize = DefaultMaxRoamingPartySize;
            }

            if (MaxRoamingPartySize < MinRoamingPartySize)
            {
                MaxRoamingPartySize = MinRoamingPartySize;
            }

            if (PortalVisualScale <= 0f)
            {
                PortalVisualScale = DefaultPortalVisualScale;
            }

            if (EncounterDistance <= 0f)
            {
                EncounterDistance = DefaultEncounterDistance;
            }

            if (string.IsNullOrWhiteSpace(_homeSiteId) && Settlement != null)
            {
                _homeSiteId = Settlement.StringId;
            }
        }
    }
}
