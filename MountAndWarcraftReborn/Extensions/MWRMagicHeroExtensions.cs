using System.Collections.Generic;
using MountAndWarcraftReborn.Behaviors;
using MountAndWarcraftReborn.Magic;
using MountAndWarcraftReborn.Magic.Campaign;
using TaleWorlds.CampaignSystem;

namespace MountAndWarcraftReborn.Extensions
{
    public static class MWRMagicHeroExtensions
    {
        public static MWRMagicInfo? GetMagicInfo(this Hero hero)
        {
            return hero == null || Campaign.Current == null
                ? null
                : Campaign.Current.GetCampaignBehavior<MWRMagicManagerBehavior>()?.GetOrCreateMagicInfo(hero);
        }

        public static MWRMagicClassId GetMagicClass(this Hero hero)
        {
            return hero == null || Campaign.Current == null
                ? MWRMagicClassId.None
                : Campaign.Current.GetCampaignBehavior<MWRMagicManagerBehavior>()?.GetMagicClass(hero) ?? MWRMagicClassId.None;
        }

        public static float GetMana(this Hero hero)
        {
            return hero == null || Campaign.Current == null
                ? 0f
                : Campaign.Current.GetCampaignBehavior<MWRMagicManagerBehavior>()?.GetMana(hero) ?? 0f;
        }

        public static float GetMaxMana(this Hero hero)
        {
            return hero == null || Campaign.Current == null
                ? 0f
                : Campaign.Current.GetCampaignBehavior<MWRMagicManagerBehavior>()?.GetMaxMana(hero) ?? 0f;
        }

        public static float GetManaRegenPerHour(this Hero hero)
        {
            return hero == null || Campaign.Current == null
                ? 0f
                : Campaign.Current.GetCampaignBehavior<MWRMagicManagerBehavior>()?.GetManaRegenPerHour(hero) ?? 0f;
        }

        public static IReadOnlyList<string> GetKnownSpellIds(this Hero hero)
        {
            return hero == null || Campaign.Current == null
                ? System.Array.Empty<string>()
                : Campaign.Current.GetCampaignBehavior<MWRMagicManagerBehavior>()?.GetKnownSpellIds(hero) ?? System.Array.Empty<string>();
        }

        public static IReadOnlyList<string> GetSelectedSpellIds(this Hero hero)
        {
            return hero == null || Campaign.Current == null
                ? System.Array.Empty<string>()
                : Campaign.Current.GetCampaignBehavior<MWRMagicManagerBehavior>()?.GetSelectedSpellIds(hero) ?? System.Array.Empty<string>();
        }

        public static IReadOnlyList<MWRMagicSpellTemplate> GetKnownSpellTemplates(this Hero hero)
        {
            return hero == null || Campaign.Current == null
                ? System.Array.Empty<MWRMagicSpellTemplate>()
                : Campaign.Current.GetCampaignBehavior<MWRMagicManagerBehavior>()?.GetKnownSpellTemplates(hero) ?? System.Array.Empty<MWRMagicSpellTemplate>();
        }

        public static IReadOnlyList<MWRMagicSpellTemplate> GetSelectedSpellTemplates(this Hero hero)
        {
            return hero == null || Campaign.Current == null
                ? System.Array.Empty<MWRMagicSpellTemplate>()
                : Campaign.Current.GetCampaignBehavior<MWRMagicManagerBehavior>()?.GetSelectedSpellTemplates(hero) ?? System.Array.Empty<MWRMagicSpellTemplate>();
        }

        public static bool HasSpell(this Hero hero, string spellId)
        {
            return hero != null &&
                   Campaign.Current?.GetCampaignBehavior<MWRMagicManagerBehavior>()?.HasSpell(hero, spellId) == true;
        }

        public static void AddMana(this Hero hero, float amount)
        {
            Campaign.Current?.GetCampaignBehavior<MWRMagicManagerBehavior>()?.AddMana(hero, amount);
        }
    }
}
