using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using System.Linq;

namespace MountAndWarcraftReborn.Hirelings
{
    public static class MWRHirelingHelpers
    {
        public static bool HirelingServiceConditions()
        {
            Hero dialogPartner = Campaign.Current?.ConversationManager?.OneToOneConversationHero;
            return CanServeLord(dialogPartner);
        }

        public static bool CanServeLord(Hero lord)
        {
            if (lord == null || lord == Hero.MainHero)
            {
                return false;
            }

            if (!lord.IsPartyLeader || lord.Clan?.Kingdom == null)
            {
                return false;
            }

            if (Clan.PlayerClan == null || MobileParty.MainParty == null)
            {
                return false;
            }

            if (Clan.PlayerClan.MapFaction != Clan.PlayerClan)
            {
                return false;
            }

            if (MobileParty.MainParty.Army != null)
            {
                return false;
            }

            if (lord.Clan == Clan.PlayerClan || lord.PartyBelongedTo == null)
            {
                return false;
            }

            return true;
        }

        public static TextObject GetExplainText(Hero lord)
        {
            string factionName = lord?.MapFaction?.Name?.ToString() ?? "my banners";
            TextObject text = new TextObject("{=mwr_hireling_explain}If you swear to serve under my banners, you will follow my host, share in our campaign, and earn your keep in battle.");
            text.SetTextVariable("FACTION_NAME", factionName);
            return text;
        }

        public static TextObject GetDecisionText(Hero lord)
        {
            TextObject text = new TextObject("{=mwr_hireling_decision}Very well. You serve with us now. Keep up with the host and do not shame my banners.");
            if (lord != null)
            {
                text.SetTextVariable("LORD_NAME", lord.Name);
            }

            return text;
        }

        public static TextObject GetQuitText(Hero lord)
        {
            TextObject text = new TextObject("{=mwr_hireling_quit_response}Then our arrangement is ended. See that you do not cross me again.");
            if (lord != null)
            {
                text.SetTextVariable("LORD_NAME", lord.Name);
            }

            return text;
        }

        public static int CalculateDailyWage(Hero hero, int battlesFought, float durationInDays)
        {
            if (hero == null)
            {
                return 0;
            }

            ExplainedNumber wage = new ExplainedNumber(25f * hero.Level);
            wage.AddFactor(0.10f * battlesFought);
            wage.AddFactor(-0.5f + durationInDays / 20f);

            int companionFactor = 0;
            if (hero.PartyBelongedTo != null)
            {
                companionFactor = hero.PartyBelongedTo.MemberRoster.GetTroopRoster()
                    .Count(x => x.Character.HeroObject != null && x.Character != hero.CharacterObject);
            }

            if (companionFactor > 0)
            {
                wage.AddFactor(companionFactor);
            }

            return (int)wage.ResultNumber;
        }
    }
}
