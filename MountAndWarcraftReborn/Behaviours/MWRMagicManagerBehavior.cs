using System;
using System.Collections.Generic;
using System.Linq;
using MountAndWarcraftReborn.Magic;
using MountAndWarcraftReborn.Magic.Campaign;
using SandBox.View.Map;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.ScreenSystem;

namespace MountAndWarcraftReborn.Behaviors
{
    public class MWRMagicManagerBehavior : CampaignBehaviorBase
    {
        private const int MaxSelectedSpells = 4;
        private const bool EnableMainHeroCharacterCreationMagicSelection = true;

        private Dictionary<string, MWRMagicInfo> _heroMagicInfos = new(StringComparer.Ordinal);

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
            ScreenManager.OnPushScreen -= OnPushScreen;
            ScreenManager.OnPushScreen += OnPushScreen;
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("_heroMagicInfos", ref _heroMagicInfos);
        }

        public MWRMagicInfo GetOrCreateMagicInfo(Hero hero)
        {
            if (hero == null)
            {
                throw new ArgumentNullException(nameof(hero));
            }

            MWRMagicDataManager.EnsureLoaded();

            if (!_heroMagicInfos.TryGetValue(hero.StringId, out MWRMagicInfo info))
            {
                info = CreateInitialMagicInfo(hero);
                _heroMagicInfos[hero.StringId] = info;
            }

            EnsureHeroStarterKit(hero, info);
            SyncMana(hero, info);
            return info;
        }

        public MWRMagicClassId GetMagicClass(Hero hero)
        {
            return GetOrCreateMagicInfo(hero).MagicClassId;
        }

        public float GetMana(Hero hero)
        {
            return GetOrCreateMagicInfo(hero).CurrentMana;
        }

        public float GetMaxMana(Hero hero)
        {
            MWRMagicClassDefinition? definition = MWRMagicDataManager.GetClassDefinition(GetMagicClass(hero));
            return definition?.BaseMaxMana ?? 0f;
        }

        public float GetManaRegenPerHour(Hero hero)
        {
            MWRMagicClassDefinition? definition = MWRMagicDataManager.GetClassDefinition(GetMagicClass(hero));
            return definition?.ManaRegenPerHour ?? 0f;
        }

        public IReadOnlyList<string> GetKnownSpellIds(Hero hero)
        {
            return GetOrCreateMagicInfo(hero).KnownSpellIds;
        }

        public IReadOnlyList<MWRMagicSpellTemplate> GetKnownSpellTemplates(Hero hero)
        {
            return GetKnownSpellIds(hero)
                .Select(MWRMagicDataManager.GetSpellTemplate)
                .Where(template => template != null)
                .Cast<MWRMagicSpellTemplate>()
                .ToList();
        }

        public IReadOnlyList<string> GetSelectedSpellIds(Hero hero)
        {
            return GetOrCreateMagicInfo(hero).SelectedSpellIds;
        }

        public IReadOnlyList<MWRMagicSpellTemplate> GetSelectedSpellTemplates(Hero hero)
        {
            return GetSelectedSpellIds(hero)
                .Select(MWRMagicDataManager.GetSpellTemplate)
                .Where(template => template != null)
                .Cast<MWRMagicSpellTemplate>()
                .ToList();
        }

        public bool HasSpell(Hero hero, string spellId)
        {
            return !string.IsNullOrWhiteSpace(spellId) &&
                   GetOrCreateMagicInfo(hero).KnownSpellIds.Contains(spellId, StringComparer.Ordinal);
        }

        public void SetHeroClass(Hero hero, MWRMagicClassId classId)
        {
            if (hero == null)
            {
                return;
            }

            MWRMagicInfo info = GetOrCreateMagicInfo(hero);
            info.MagicClassId = classId;
            info.StarterKitApplied = false;
            info.KnownSpellIds.Clear();
            info.SelectedSpellIds.Clear();
            info.CurrentMana = 0f;
            info.LastManaSyncHours = (float)CampaignTime.Now.ToHours;
            EnsureHeroStarterKit(hero, info);
        }

        public void SetSelectedSpellIds(Hero hero, IEnumerable<string> selectedSpellIds)
        {
            if (hero == null)
            {
                return;
            }

            MWRMagicInfo info = GetOrCreateMagicInfo(hero);
            List<string> sanitized = selectedSpellIds?
                .Where(id => !string.IsNullOrWhiteSpace(id) && info.KnownSpellIds.Contains(id, StringComparer.Ordinal))
                .Distinct(StringComparer.Ordinal)
                .Take(MaxSelectedSpells)
                .ToList() ?? new List<string>();

            if (sanitized.Count == 0 && info.KnownSpellIds.Count > 0)
            {
                sanitized.AddRange(info.KnownSpellIds.Take(MaxSelectedSpells));
            }

            info.SelectedSpellIds.Clear();
            info.SelectedSpellIds.AddRange(sanitized);
        }

        public void AddMana(Hero hero, float amount)
        {
            if (hero == null)
            {
                return;
            }

            MWRMagicInfo info = GetOrCreateMagicInfo(hero);
            float maxMana = GetMaxMana(hero);
            info.CurrentMana = Math.Max(0f, Math.Min(maxMana, info.CurrentMana + amount));
            info.LastManaSyncHours = (float)CampaignTime.Now.ToHours;
        }

        public bool TrySpendMana(Hero hero, float amount)
        {
            if (hero == null)
            {
                return false;
            }

            MWRMagicInfo info = GetOrCreateMagicInfo(hero);
            if (info.CurrentMana + 0.001f < amount)
            {
                return false;
            }

            info.CurrentMana = Math.Max(0f, info.CurrentMana - amount);
            info.LastManaSyncHours = (float)CampaignTime.Now.ToHours;
            return true;
        }

        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            MWRMagicDataManager.EnsureLoaded();

            if (MWRCharacterCreationMagicSelection.HasPendingSelection && Hero.MainHero != null)
            {
                if (EnableMainHeroCharacterCreationMagicSelection)
                {
                    SetHeroClass(Hero.MainHero, MWRCharacterCreationMagicSelection.PendingClassId);
                }

                MWRCharacterCreationMagicSelection.Clear();
            }
        }

        private static void OnPushScreen(ScreenBase pushedScreen)
        {
            if (pushedScreen is not MapScreen mapScreen || Hero.MainHero == null)
            {
                return;
            }

            if (Campaign.Current?.GetCampaignBehavior<MWRMagicManagerBehavior>() == null)
            {
                return;
            }

            if (mapScreen.GetMapView<MountAndWarcraftReborn.Magic.UI.MWRMagicMapView>() == null)
            {
                mapScreen.AddMapView<MountAndWarcraftReborn.Magic.UI.MWRMagicMapView>();
            }
        }

        private MWRMagicInfo CreateInitialMagicInfo(Hero hero)
        {
            MWRMagicInfo info = new MWRMagicInfo
            {
                MagicClassId = ResolveInitialClass(hero),
                LastManaSyncHours = (float)CampaignTime.Now.ToHours
            };

            EnsureHeroStarterKit(hero, info);
            return info;
        }

        private MWRMagicClassId ResolveInitialClass(Hero hero)
        {
            if (hero == Hero.MainHero)
            {
                if (EnableMainHeroCharacterCreationMagicSelection &&
                    MWRCharacterCreationMagicSelection.HasPendingSelection)
                {
                    return MWRCharacterCreationMagicSelection.PendingClassId;
                }

                return MWRMagicClassId.None;
            }

            return MWRMagicDataManager.GetAssignment(hero.CharacterObject?.StringId ?? string.Empty)?.ClassId
                   ?? MWRMagicClassId.None;
        }

        private void EnsureHeroStarterKit(Hero hero, MWRMagicInfo info)
        {
            if (info.StarterKitApplied)
            {
                return;
            }

            MWRMagicClassId classId = info.MagicClassId;
            if (classId == MWRMagicClassId.None)
            {
                info.CurrentMana = 0f;
                info.LastManaSyncHours = (float)CampaignTime.Now.ToHours;
                info.StarterKitApplied = true;
                return;
            }

            MWRMagicClassDefinition? classDefinition = MWRMagicDataManager.GetClassDefinition(classId);
            if (classDefinition == null)
            {
                info.MagicClassId = MWRMagicClassId.None;
                info.CurrentMana = 0f;
                info.LastManaSyncHours = (float)CampaignTime.Now.ToHours;
                info.StarterKitApplied = true;
                return;
            }

            IReadOnlyList<string> starterSpells = classDefinition.GetStarterSpellIds();
            IReadOnlyList<string> defaultSelected = classDefinition.GetDefaultSelectedSpellIds();
            MWRMagicAssignment? assignment = MWRMagicDataManager.GetAssignment(hero.CharacterObject?.StringId ?? string.Empty);

            if (assignment != null && assignment.GetKnownSpellIds().Count > 0)
            {
                starterSpells = assignment.GetKnownSpellIds();
            }

            if (assignment != null && assignment.GetSelectedSpellIds().Count > 0)
            {
                defaultSelected = assignment.GetSelectedSpellIds();
            }

            info.KnownSpellIds.Clear();
            info.KnownSpellIds.AddRange(starterSpells.Where(id => MWRMagicDataManager.GetSpellTemplate(id) != null));

            info.SelectedSpellIds.Clear();
            IEnumerable<string> selectedSpells = defaultSelected.Count > 0 ? defaultSelected : info.KnownSpellIds;
            info.SelectedSpellIds.AddRange(selectedSpells
                .Where(id => info.KnownSpellIds.Contains(id, StringComparer.Ordinal))
                .Distinct(StringComparer.Ordinal)
                .Take(MaxSelectedSpells));

            if (info.SelectedSpellIds.Count == 0 && info.KnownSpellIds.Count > 0)
            {
                info.SelectedSpellIds.AddRange(info.KnownSpellIds.Take(MaxSelectedSpells));
            }

            info.CurrentMana = classDefinition.BaseMaxMana;
            info.LastManaSyncHours = (float)CampaignTime.Now.ToHours;
            info.StarterKitApplied = true;
        }

        private void SyncMana(Hero hero, MWRMagicInfo info)
        {
            MWRMagicClassDefinition? classDefinition = MWRMagicDataManager.GetClassDefinition(info.MagicClassId);
            if (classDefinition == null)
            {
                info.CurrentMana = 0f;
                info.LastManaSyncHours = (float)CampaignTime.Now.ToHours;
                return;
            }

            float currentHours = (float)CampaignTime.Now.ToHours;
            float elapsedHours = Math.Max(0f, currentHours - info.LastManaSyncHours);
            if (elapsedHours <= 0f)
            {
                return;
            }

            float maxMana = classDefinition.BaseMaxMana;
            float regeneratedMana = elapsedHours * classDefinition.ManaRegenPerHour;
            info.CurrentMana = Math.Min(maxMana, info.CurrentMana + regeneratedMana);
            info.LastManaSyncHours = currentHours;
        }
    }
}
