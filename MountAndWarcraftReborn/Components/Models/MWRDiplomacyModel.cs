using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.Localization;

namespace MWRMode.Components.Models
{
    public class MWRDiplomacyModel : DefaultDiplomacyModel
    {
        public override float GetScoreOfClanToJoinKingdom(Clan clan, Kingdom kingdom)
        {
            if (!CanClanJoinKingdom(clan, kingdom))
            {
                return float.MinValue;
            }

            return base.GetScoreOfClanToJoinKingdom(clan, kingdom);
        }

        public override float GetScoreOfKingdomToGetClan(Kingdom kingdom, Clan clan)
        {
            if (!CanClanJoinKingdom(clan, kingdom))
            {
                return float.MinValue;
            }

            return base.GetScoreOfKingdomToGetClan(kingdom, clan);
        }

        public override float GetScoreOfMercenaryToJoinKingdom(Clan clan, Kingdom kingdom)
        {
            if (!CanClanJoinKingdom(clan, kingdom))
            {
                return float.MinValue;
            }

            return base.GetScoreOfMercenaryToJoinKingdom(clan, kingdom);
        }

        public override float GetScoreOfKingdomToHireMercenary(Kingdom kingdom, Clan mercenaryClan)
        {
            if (!CanClanJoinKingdom(mercenaryClan, kingdom))
            {
                return float.MinValue;
            }

            return base.GetScoreOfKingdomToHireMercenary(kingdom, mercenaryClan);
        }

        public override float GetScoreOfDeclaringPeace(IFaction factionDeclaresPeace, IFaction factionDeclaredPeace)
        {
            if (IsUndeadFaction(factionDeclaresPeace) || IsUndeadFaction(factionDeclaredPeace))
            {
                return float.MinValue;
            }

            return base.GetScoreOfDeclaringPeace(factionDeclaresPeace, factionDeclaredPeace);
        }

        public override float GetScoreOfDeclaringPeaceForClan(
            IFaction factionDeclaresPeace,
            IFaction factionDeclaredPeace,
            Clan evaluatingClan,
            out TextObject reason,
            bool includeReason = false)
        {
            reason = TextObject.GetEmpty();

            if (IsUndeadFaction(factionDeclaresPeace) || IsUndeadFaction(factionDeclaredPeace))
            {
                return float.MinValue;
            }

            return base.GetScoreOfDeclaringPeaceForClan(
                factionDeclaresPeace,
                factionDeclaredPeace,
                evaluatingClan,
                out reason,
                includeReason);
        }

        private static bool IsUndeadFaction(IFaction faction)
        {
            return faction?.Culture?.StringId == "nord";
        }

        private static bool CanClanJoinKingdom(Clan clan, Kingdom kingdom)
        {
            string clanCultureId = clan?.Culture?.StringId ?? string.Empty;
            string kingdomCultureId = kingdom?.Culture?.StringId ?? string.Empty;

            if (string.IsNullOrWhiteSpace(clanCultureId) || string.IsNullOrWhiteSpace(kingdomCultureId))
            {
                return true;
            }

            if (clanCultureId == kingdomCultureId)
            {
                return true;
            }

            if (!MWRMode.CultureRules.IsKingdomCultureCompatible(clanCultureId, kingdomCultureId))
            {
                return false;
            }

            return !HasLivingKingdomOfCulture(clanCultureId);
        }

        private static bool HasLivingKingdomOfCulture(string cultureId)
        {
            if (string.IsNullOrWhiteSpace(cultureId))
            {
                return false;
            }

            foreach (Kingdom kingdom in Kingdom.All)
            {
                if (kingdom != null && !kingdom.IsEliminated && kingdom.Culture?.StringId == cultureId)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
