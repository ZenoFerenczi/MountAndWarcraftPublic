using HarmonyLib;
using MWRMode.CharacterCreationContent;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterCreationContent;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.MountAndBlade.ViewModelCollection.FaceGenerator;

namespace MWRMode.Patches.CharacterCreation
{
    [HarmonyPatch]
    public static class FaceGenVM_RaceSelector_Patch
    {
        private static SelectorVM<SelectorItemVM>? _filteredRaceSelector;
        private static List<int>? _filteredRaceIds;
        private static string? _lastCultureId;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FaceGenVM), nameof(FaceGenVM.RaceSelector), MethodType.Getter)]
        private static void Post_RaceSelector(FaceGenVM __instance, ref SelectorVM<SelectorItemVM> __result)
        {
            if (__result?.ItemList == null || __result.ItemList.Count == 0)
                return;

            if (!(GameStateManager.Current?.ActiveState is CharacterCreationState))
                return;

            Hero mainHero = Hero.MainHero;
            if (mainHero?.Culture == null)
                return;

            string cultureId = mainHero.Culture.StringId ?? string.Empty;
            if (string.IsNullOrEmpty(cultureId))
                return;

            CheckFilteredRaceSelector(__result, cultureId);

            if (_filteredRaceSelector != null)
            {
                __result = _filteredRaceSelector;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(FaceGenVM), nameof(FaceGenVM.SetBodyProperties))]
        private static void Pre_SetBodyProperties(
            FaceGenVM __instance,
            BodyProperties bodyProperties,
            bool ignoreDebugValues,
            ref int race,
            int gender = -1,
            bool recordChange = false)
        {
            if (_filteredRaceSelector == null || _filteredRaceIds == null || _filteredRaceIds.Count == 0)
                return;

            if (!(GameStateManager.Current?.ActiveState is CharacterCreationState))
                return;

            Hero mainHero = Hero.MainHero;
            if (mainHero?.Culture == null)
                return;

            string cultureId = mainHero.Culture.StringId ?? string.Empty;
            if (string.IsNullOrEmpty(cultureId))
                return;

            if (!MWRRaceRules.IsRaceAllowedForCulture(cultureId, race))
            {
                race = MWRRaceRules.GetFallbackRaceForCulture(cultureId);
            }

            if (__instance.RaceSelector.ItemList.Count != _filteredRaceSelector.ItemList.Count)
            {
                SetRaceSelectorField(__instance, _filteredRaceSelector);
            }

            __instance.RaceSelector.SelectedIndex = GetFilteredIndexFromActualRace(race, cultureId);
        }

        private static void CheckFilteredRaceSelector(SelectorVM<SelectorItemVM> originalSelector, string cultureId)
        {
            if (_filteredRaceSelector != null &&
                _filteredRaceIds != null &&
                _lastCultureId == cultureId)
            {
                return;
            }

            _lastCultureId = cultureId;
            _filteredRaceIds = new List<int>();
            List<string> allowedRaceNames = new List<string>();

            for (int raceId = 0; raceId < originalSelector.ItemList.Count; raceId++)
            {
                if (!MWRRaceRules.IsRaceAllowedForCulture(cultureId, raceId))
                    continue;

                _filteredRaceIds.Add(raceId);
                allowedRaceNames.Add(GetSelectorItemText(originalSelector.ItemList[raceId]));
            }

            if (_filteredRaceIds.Count == 0)
            {
                _filteredRaceSelector = null;
                return;
            }

            int currentRace = Hero.MainHero?.CharacterObject?.Race ?? 0;
            int selectedFilteredIndex = GetFilteredIndexFromActualRace(currentRace, cultureId);

            Action<SelectorVM<SelectorItemVM>>? originalOnChange =
                AccessTools.Field(typeof(SelectorVM<SelectorItemVM>), "_onChange")
                    ?.GetValue(originalSelector) as Action<SelectorVM<SelectorItemVM>>;

            Action<SelectorVM<SelectorItemVM>> wrappedOnChange = selector =>
            {
                if (selector == null || _filteredRaceIds == null || _filteredRaceIds.Count == 0)
                    return;

                int filteredIndex = selector.SelectedIndex;
                if (filteredIndex < 0 || filteredIndex >= _filteredRaceIds.Count)
                    return;

                int actualRaceId = _filteredRaceIds[filteredIndex];

                originalSelector.SelectedIndex = actualRaceId;
                originalOnChange?.Invoke(originalSelector);
            };

            _filteredRaceSelector = new SelectorVM<SelectorItemVM>(
                allowedRaceNames,
                selectedFilteredIndex,
                wrappedOnChange);
        }

        private static int GetFilteredIndexFromActualRace(int actualRaceId, string cultureId)
        {
            if (_filteredRaceIds == null || _filteredRaceIds.Count == 0)
                return 0;

            for (int i = 0; i < _filteredRaceIds.Count; i++)
            {
                if (_filteredRaceIds[i] == actualRaceId)
                    return i;
            }

            int fallbackRace = MWRRaceRules.GetFallbackRaceForCulture(cultureId);

            for (int i = 0; i < _filteredRaceIds.Count; i++)
            {
                if (_filteredRaceIds[i] == fallbackRace)
                    return i;
            }

            return 0;
        }

        private static void SetRaceSelectorField(FaceGenVM vm, SelectorVM<SelectorItemVM> selector)
        {
            if (vm == null || selector == null)
                return;

            try
            {
                Traverse.Create(vm).Field("RaceSelector").SetValue(selector);
                return;
            }
            catch
            {
            }

            try
            {
                Traverse.Create(vm).Property("RaceSelector").SetValue(selector);
                return;
            }
            catch
            {
            }

            var backingField = AccessTools.Field(typeof(FaceGenVM), "<RaceSelector>k__BackingField");
            backingField?.SetValue(vm, selector);
        }

        private static string GetSelectorItemText(SelectorItemVM item)
        {
            if (item == null)
                return string.Empty;

            var prop = AccessTools.Property(typeof(SelectorItemVM), "StringItem");
            if (prop != null)
            {
                object? value = prop.GetValue(item, null);
                if (value is string text && !string.IsNullOrWhiteSpace(text))
                    return text;
            }

            return item.ToString();
        }

        public static void ResetCache()
        {
            _filteredRaceSelector = null;
            _filteredRaceIds = null;
            _lastCultureId = null;
        }
    }
}