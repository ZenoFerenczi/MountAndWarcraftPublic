using HarmonyLib;
using MWRMode.CharacterCreationContent;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterCreationContent;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.Core;

namespace MWRMode.Patches.CharacterCreation
{
    [HarmonyPatch(typeof(CharacterObject), nameof(CharacterObject.UpdatePlayerCharacterBodyProperties))]
    public static class CharacterObject_UpdatePlayerCharacterBodyProperties_Patch
    {
        private static void Prefix(ref BodyProperties properties, ref int race, bool isFemale)
        {
            if (GameStateManager.Current?.ActiveState is CharacterCreationState)
                return;

            Hero mainHero = Hero.MainHero;
            if (mainHero == null || mainHero.Culture == null)
                return;

            string cultureId = mainHero.Culture.StringId ?? string.Empty;

            if (MWRRaceRules.IsRaceAllowedForCulture(cultureId, race))
                return;

            int fallbackRace = MWRRaceRules.GetFallbackRaceForCulture(cultureId);
            race = fallbackRace;

            if (MWRRaceRules.TryGetDefaultBodyForRace(fallbackRace, isFemale, out BodyProperties fallbackProperties))
            {
                properties = fallbackProperties;
            }
        }
    }
}
