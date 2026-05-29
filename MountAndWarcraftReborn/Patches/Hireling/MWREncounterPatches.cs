using HarmonyLib;
using MountAndWarcraftReborn.Behaviors;
using MountAndWarcraftReborn.CampaignMechanics.PostBattleLoot;
using MountAndWarcraftReborn.Extensions;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace MountAndWarcraftReborn.Patches.Hireling
{
    [HarmonyPatch]
    public static class MWREncounterPatches
    {
        private const string HirelingMenuId = "mwr_hireling_menu";
        private const string HirelingBattleMenuId = "mwr_hireling_battle_menu";

        [HarmonyPrefix]
        [HarmonyPatch(typeof(VillageHostileActionCampaignBehavior), "village_raid_game_menu_init")]
        public static bool VillageRaidGameMenuInitPrefix(MenuCallbackArgs args)
        {
            if (PlayerEncounter.EncounterSettlement != null)
            {
                return true;
            }

            if (!Hero.MainHero.IsEnlisted())
            {
                return true;
            }

            if (Hero.MainHero.IsEnlisted())
            {
                Hero lord = Hero.MainHero.GetEnlistingHero();
                var settlement = lord?.CurrentSettlement;
                MBTextManager.SetTextVariable("VILLAGE_NAME", settlement?.Name ?? new TextObject("{=mwr_hireling_unknown_village}unknown settlement"), false);
                return false;
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(VillageHostileActionCampaignBehavior), "wait_menu_start_raiding_on_condition")]
        public static bool WaitMenuStartRaidingOnConditionPrefix(MenuCallbackArgs args, ref bool __result)
        {
            if (!Hero.MainHero.IsEnlisted())
            {
                return true;
            }

            __result = false;
            GameMenu.SwitchToMenu(HirelingMenuId);
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameMenu), "ActivateGameMenu")]
        public static bool ActivateGameMenuPrefix(ref string menuId)
        {
            if (menuId != "encounter" || !Hero.MainHero.IsEnlisted())
            {
                return true;
            }

            if (MWRHirelingCampaignBehavior.IsStartingBattle)
            {
                return true;
            }

            PlayerEncounter currentEncounter = PlayerEncounter.Current;
            if (currentEncounter?.IsJoinedBattle == true && currentEncounter.EncounterState != PlayerEncounterState.End)
            {
                return true;
            }

            if (currentEncounter != null
                && currentEncounter.EncounterState == PlayerEncounterState.End
                && PlayerEncounter.EncounterSettlement == null)
            {
                PlayerEncounter.Finish(false);
            }

            MapEvent playerMapEvent = Hero.MainHero.PartyBelongedTo?.MapEvent;
            BattleSideEnum? playerMapEventSide = Hero.MainHero.PartyBelongedTo?.MapEventSide?.MissionSide;

            bool hasJoinableHirelingBattle =
                MWRHirelingCampaignBehavior.IsJoinableHirelingMapEvent(playerMapEvent, playerMapEventSide) ||
                MWRHirelingCampaignBehavior.HasJoinableBattleForCurrentEnlistment();

            bool hasPendingNativeCleanup = MWRHirelingCampaignBehavior.HasPendingNativeEncounterCleanup();

            if (currentEncounter?.IsJoinedBattle == true && (hasJoinableHirelingBattle || hasPendingNativeCleanup))
            {
                return true;
            }

            if (hasJoinableHirelingBattle)
            {
                menuId = HirelingBattleMenuId;
            }
            else if (currentEncounter != null && hasPendingNativeCleanup)
            {
                return true;
            }
            else
            {
                menuId = HirelingMenuId;
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameMenu), "SwitchToMenu")]
        public static bool SwitchToMenuPrefix(ref string menuId)
        {
            if (!Hero.MainHero.IsEnlisted() || !MWRHirelingCampaignBehavior.InPostBattleTransition)
            {
                return true;
            }

            if (menuId != "menu_settlement_taken"
                && menuId != "menu_settlement_taken_player_leader"
                && menuId != "menu_settlement_taken_player_army_member"
                && menuId != "menu_settlement_taken_player_participant")
            {
                return true;
            }

            menuId = HirelingMenuId;
            MWRHirelingCampaignBehavior.MarkHirelingWaitMenuShown();
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerEncounter), "Finish")]
        public static bool PlayerEncounterFinishPrefix()
        {
            if (!Hero.MainHero.IsEnlisted() || !MWRHirelingCampaignBehavior.InPostBattleTransition)
            {
                return true;
            }

            if (PlayerEncounter.EncounterSettlement == null)
            {
                return true;
            }

            MWRHirelingCampaignBehavior hirelingBehavior = Campaign.Current?.GetCampaignBehavior<MWRHirelingCampaignBehavior>();
            MobileParty enlistingLordParty = hirelingBehavior?.EnlistingLord?.PartyBelongedTo;

            bool hasActiveHirelingBattle =
                MWRHirelingCampaignBehavior.IsOngoingHirelingMapEvent(MapEvent.PlayerMapEvent) ||
                MWRHirelingCampaignBehavior.IsOngoingHirelingMapEvent(Hero.MainHero.PartyBelongedTo?.MapEvent) ||
                MWRHirelingCampaignBehavior.IsOngoingHirelingMapEvent(enlistingLordParty?.MapEvent);

            return !hasActiveHirelingBattle;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MapEvent), "GetMemberRosterReceivingLootShare")]
        public static void GetMemberRosterPostfix(TroopRoster __result)
        {
            if (__result != null && PendingLootedTroopManager.HasPendingModifications)
            {
                PendingLootedTroopManager.ApplyMemberModifications(__result);
                PendingLootedTroopManager.ConsumeMemberModifications();
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MapEvent), "GetPrisonerRosterReceivingLootShare")]
        public static void GetPrisonerRosterPostfix(TroopRoster __result)
        {
            if (__result != null && PendingLootedTroopManager.HasPendingModifications)
            {
                PendingLootedTroopManager.ApplyPrisonerModifications(__result);
                PendingLootedTroopManager.ConsumePrisonerModifications();
            }
        }
    }
}
