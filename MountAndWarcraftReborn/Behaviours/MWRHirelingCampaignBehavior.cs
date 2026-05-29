using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using MountAndWarcraftReborn.CampaignMechanics.PostBattleLoot;
using MountAndWarcraftReborn.Extensions;
using MountAndWarcraftReborn.Hirelings;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace MountAndWarcraftReborn.Behaviors
{
    public class MWRHirelingCampaignBehavior : CampaignBehaviorBase
    {
        private const string HirelingMenuId = "mwr_hireling_menu";
        private const string HirelingBattleMenuId = "mwr_hireling_battle_menu";
        private const float MinimumServeDays = 25f;
        private const float RatioPartyAgainstEnemyStrength = 3f;
        private const int StrictJoinedCleanupDelayTicks = 120;
        private const int FallbackJoinedCleanupDelayTicks = 360;

        private MWRHirelingActivities _activities;

        private float _durationInDays;
        private bool _hirelingEnlisted;
        private Hero _hirelingEnlistingLord;
        private bool _hirelingLordIsFightingWithoutPlayer;
        private bool _pauseModeToggle;
        private int _manuallyFoughtBattles;
        private bool _startBattle;
        private bool _siegeBattleMissionStarted;
        private bool _inPostBattleTransition;
        private bool _hirelingWaitMenuShown;
        private int _deadJoinedEncounterCleanupTicks;
        private MapEvent _joinedHirelingCleanupBattle;
        private float _entryServiceTimeStamp;
        private SkillObject _currentTrainedSkill;
        private int _currentActivityIndex;
        private bool _enlistInquiryDeclined;
        private int _lastPaidDay;

        public float DurationInDays => _durationInDays;

        public int ManuallyFoughtBattles => _manuallyFoughtBattles;

        public Hero EnlistingLord => _hirelingEnlistingLord;

        public static bool IsStartingBattle =>
            Campaign.Current?.GetCampaignBehavior<MWRHirelingCampaignBehavior>()?._startBattle ?? false;

        public static bool InPostBattleTransition =>
            Campaign.Current?.GetCampaignBehavior<MWRHirelingCampaignBehavior>()?._inPostBattleTransition ?? false;

        internal static void MarkHirelingWaitMenuShown()
        {
            MWRHirelingCampaignBehavior behavior = Campaign.Current?.GetCampaignBehavior<MWRHirelingCampaignBehavior>();
            if (behavior != null)
            {
                behavior._hirelingWaitMenuShown = true;
            }
        }

        public bool IsEnlisted()
        {
            return _hirelingEnlisted;
        }

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, Initialize);
            CampaignEvents.TickEvent.AddNonSerializedListener(this, OnTick);
            CampaignEvents.SettlementEntered.AddNonSerializedListener(this, EnlistingLordPartyEntersSettlement);
            CampaignEvents.OnSettlementLeftEvent.AddNonSerializedListener(this, OnPartyLeavesSettlement);
            CampaignEvents.OnPlayerBattleEndEvent.AddNonSerializedListener(this, ControlPlayerLoot);
            CampaignEvents.MapEventEnded.AddNonSerializedListener(this, MapEventEnded);
            CampaignEvents.GameMenuOpened.AddNonSerializedListener(this, MenuOpened);
            CampaignEvents.GameMenuOptionSelectedEvent.AddNonSerializedListener(this, ContinueTimeAfterLeftSettlementWhileEnlisted);
            CampaignEvents.WeeklyTickEvent.AddNonSerializedListener(this, WeeklyRenownGain);
            CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, SkillGain);
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, OnDailyTick);
            CampaignEvents.OnClanChangedKingdomEvent.AddNonSerializedListener(this, LeaveKingdomEvent);
            CampaignEvents.RaidCompletedEvent.AddNonSerializedListener(this, OnRaidCompleted);
            CampaignEvents.OnQuarterDailyPartyTick.AddNonSerializedListener(this, IgnoreHirelingPartyRefresh);
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("_hireling_enlisted", ref _hirelingEnlisted);
            dataStore.SyncData("_hireling_enlisting_lord", ref _hirelingEnlistingLord);
            dataStore.SyncData("_hireling_entry_service_timestamp", ref _entryServiceTimeStamp);
            dataStore.SyncData("_hireling_manually_fought_battles", ref _manuallyFoughtBattles);
            dataStore.SyncData("_hireling_duration_in_days", ref _durationInDays);
            dataStore.SyncData("_hireling_current_activity_index", ref _currentActivityIndex);
            dataStore.SyncData("_hireling_in_post_battle_transition", ref _inPostBattleTransition);
            dataStore.SyncData("_hireling_joined_cleanup_battle", ref _joinedHirelingCleanupBattle);
            dataStore.SyncData("_hireling_dead_joined_cleanup_ticks", ref _deadJoinedEncounterCleanupTicks);
            dataStore.SyncData("_hireling_last_paid_day", ref _lastPaidDay);
        }

        private void Initialize(CampaignGameStarter campaignGameStarter)
        {
            _activities ??= new MWRHirelingActivities();
            InitializeDialogs(campaignGameStarter);
            SetupHirelingMenu(campaignGameStarter);
            SetupBattleMenu(campaignGameStarter);
        }

        private bool SanityCheck()
        {
            Hero dialogPartner = Campaign.Current?.ConversationManager?.OneToOneConversationHero;
            return dialogPartner != null
                && Clan.PlayerClan?.MapFaction == Clan.PlayerClan
                && MobileParty.MainParty?.Army == null
                && dialogPartner.IsPartyLeader;
        }

        private bool QuitCondition()
        {
            return Campaign.Current?.ConversationManager?.OneToOneConversationHero == _hirelingEnlistingLord
                && IsEnlisted()
                && _durationInDays > MinimumServeDays;
        }

        private void InitializeDialogs(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddPlayerLine(
                "mwr_hireling_offer_service",
                "lord_talk_speak_diplomacy_2",
                "mwr_hireling_explain",
                "{=mwr_hireling_offer_sword}I am offering my sword.",
                () => SanityCheck() && !IsEnlisted() && MWRHirelingHelpers.HirelingServiceConditions(),
                null);

            campaignGameStarter.AddDialogLine(
                "mwr_hireling_explain",
                "mwr_hireling_explain",
                "mwr_hireling_decide_player",
                "{=mwr_hireling_explain_wrapper}{HIRELING_EXPLAIN_TEXT}",
                null,
                () => MBTextManager.SetTextVariable(
                    "HIRELING_EXPLAIN_TEXT",
                    MWRHirelingHelpers.GetExplainText(Campaign.Current.ConversationManager.OneToOneConversationHero),
                    false),
                200);

            campaignGameStarter.AddPlayerLine(
                "mwr_hireling_accept",
                "mwr_hireling_decide_player",
                "mwr_hireling_prompt",
                "{=mwr_hireling_accept_text}I accept, my lord.",
                MWRHirelingHelpers.HirelingServiceConditions,
                () => DisplayPrompt(EnlistPlayer));

            campaignGameStarter.AddPlayerLine(
                "mwr_hireling_think",
                "mwr_hireling_decide_player",
                "lord_pretalk",
                "{=mwr_hireling_think_about_it}I need time to think.",
                null,
                null);

            campaignGameStarter.AddDialogLine(
                "mwr_hireling_prompt",
                "mwr_hireling_prompt",
                "mwr_hireling_decision",
                "{=mwr_hireling_ellipsis}...",
                null,
                null);

            campaignGameStarter.AddPlayerLine(
                "mwr_hireling_decision_decline",
                "mwr_hireling_decision",
                "lord_pretalk",
                "{=mwr_hireling_think_about_it}I need time to think.",
                () => _enlistInquiryDeclined,
                null);

            campaignGameStarter.AddDialogLine(
                "mwr_hireling_decision",
                "mwr_hireling_decision",
                "end",
                "{=mwr_hireling_decision_wrapper}{HIRELING_DECISION_TEXT}",
                null,
                () => MBTextManager.SetTextVariable(
                    "HIRELING_DECISION_TEXT",
                    MWRHirelingHelpers.GetDecisionText(Campaign.Current.ConversationManager.OneToOneConversationHero),
                    false));

            campaignGameStarter.AddPlayerLine(
                "mwr_hireling_quit",
                "lord_talk_speak_diplomacy_2",
                "mwr_hireling_quit_sure",
                "{=mwr_hireling_quit_service}I would like to end my service.",
                QuitCondition,
                null);

            campaignGameStarter.AddDialogLine(
                "mwr_hireling_quit_sure",
                "mwr_hireling_quit_sure",
                "mwr_hireling_quit_choice",
                "{=mwr_hireling_are_you_sure}Are you sure?",
                null,
                null);

            campaignGameStarter.AddPlayerLine(
                "mwr_hireling_quit_yes",
                "mwr_hireling_quit_choice",
                "mwr_hireling_quit_response",
                "{=mwr_hireling_yes_leave}Yes, I want to leave.",
                null,
                null);

            campaignGameStarter.AddPlayerLine(
                "mwr_hireling_quit_no",
                "mwr_hireling_quit_choice",
                "lord_pretalk",
                "{=mwr_hireling_think_about_it}I need time to think.",
                null,
                null);

            campaignGameStarter.AddDialogLine(
                "mwr_hireling_quit_response",
                "mwr_hireling_quit_response",
                "end",
                "{=mwr_hireling_quit_response_wrapper}{HIRELING_QUIT_TEXT}",
                null,
                () =>
                {
                    MBTextManager.SetTextVariable(
                        "HIRELING_QUIT_TEXT",
                        MWRHirelingHelpers.GetQuitText(Campaign.Current.ConversationManager.OneToOneConversationHero),
                        false);
                    LeaveLordPartyAction();
                });
        }

        private void SetupHirelingMenu(CampaignGameStarter campaignGameStarter)
        {
            TextObject infoText = new TextObject("{=mwr_hireling_menu_text}{ENLISTING_TEXT}");

            AddBackOption(campaignGameStarter, "town");
            AddBackOption(campaignGameStarter, "castle");
            AddBackOption(campaignGameStarter, "village");

            campaignGameStarter.AddWaitGameMenu(
                HirelingMenuId,
                infoText.Value,
                party_wait_talk_to_other_members_on_init,
                wait_on_condition,
                null,
                wait_on_tick,
                GameMenu.MenuAndOptionType.WaitMenuHideProgressAndHoursOption);

            campaignGameStarter.AddGameMenuOption(
                HirelingMenuId,
                "enter_settlement",
                "{=mwr_hireling_enter_settlement}Enter the settlement",
                args =>
                {
                    if (!IsEnlisted())
                    {
                        return false;
                    }

                    args.optionLeaveType = GameMenuOption.LeaveType.Continue;
                    return _hirelingEnlistingLord?.PartyBelongedTo?.CurrentSettlement != null;
                },
                args =>
                {
                    Settlement settlement = _hirelingEnlistingLord.PartyBelongedTo.CurrentSettlement;
                    if (settlement == null || settlement.SiegeEvent != null)
                    {
                        return;
                    }

                    EnsureHirelingSettlementEncounter(settlement);

                    if (settlement.IsTown)
                    {
                        GameMenu.SwitchToMenu("town");
                    }
                    else if (settlement.IsVillage)
                    {
                        GameMenu.SwitchToMenu("village");
                    }
                    else if (settlement.IsCastle)
                    {
                        GameMenu.SwitchToMenu("castle");
                    }
                },
                true);

            campaignGameStarter.AddGameMenuOption(
                HirelingMenuId,
                "pause_time_option",
                "{=mwr_hireling_pause_time}Pause time: {PAUSE_ONOFF}",
                args =>
                {
                    MBTextManager.SetTextVariable("PAUSE_ONOFF", _pauseModeToggle ? "{=mwr_hireling_on}On" : "{=mwr_hireling_off}Off", false);
                    return true;
                },
                PauseModeToggle);

            campaignGameStarter.AddGameMenuOption(
                HirelingMenuId,
                "talk_to_lord",
                "{=mwr_hireling_talk_to_lord}Talk to your lord",
                args =>
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.Continue;
                    return _hirelingEnlistingLord?.PartyBelongedTo != null;
                },
                args => StartDialog());

            for (int index = 0; index < 5; index++)
            {
                int localIndex = index;
                campaignGameStarter.AddGameMenuOption(
                    HirelingMenuId,
                    $"activity_{index}",
                    $"{{=mwr_hireling_activity_{index}}}{{HIRELINGACTIVITYTEXT{index}}}",
                    args => HoverActivity(localIndex, args),
                    args => ToggleActivity(localIndex, args));
            }

            campaignGameStarter.AddGameMenuOption(
                HirelingMenuId,
                "hireling_leave",
                "{=mwr_hireling_desert}Desert",
                args =>
                {
                    TextObject info = new TextObject("{=mwr_hireling_desert_warning}This will damage your standing with {FACTION}. Serve for {MINIMUM_DAYS} days and speak to your enlisting lord to leave cleanly.");
                    info.SetTextVariable("FACTION", _hirelingEnlistingLord?.MapFaction?.Name ?? new TextObject("{=mwr_hireling_unknown_faction}this faction"));
                    info.SetTextVariable("MINIMUM_DAYS", MinimumServeDays);
                    args.Tooltip = info;
                    args.optionLeaveType = GameMenuOption.LeaveType.Escape;
                    return true;
                },
                args => LeaveEnlistingParty(HirelingMenuId),
                true);
        }

        private void SetupBattleMenu(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddGameMenu(HirelingBattleMenuId, "{BATTLE_INFO}", hireling_battle_menu_on_init, GameMenu.MenuOverlayType.Encounter);

            campaignGameStarter.AddGameMenuOption(
                HirelingBattleMenuId,
                "hireling_join_battle",
                "{=mwr_hireling_join_battle}Join battle",
                hireling_battle_menu_join_battle_on_condition,
                JoinBattleConsequence,
                false,
                4);

            campaignGameStarter.AddGameMenuOption(
                HirelingBattleMenuId,
                "hireling_avoid_combat",
                "{=mwr_hireling_avoid_combat}Avoid combat",
                hireling_battle_menu_avoid_combat_on_condition,
                AvoidCombatConsequence,
                false,
                4);

            campaignGameStarter.AddGameMenuOption(
                HirelingBattleMenuId,
                "hireling_flee",
                "{=mwr_hireling_flee}Flee",
                hireling_battle_menu_desert_on_condition,
                args => LeaveEnlistingParty(HirelingBattleMenuId, true),
                false,
                4);
        }

        private void AddBackOption(CampaignGameStarter campaignGameStarter, string menuId)
        {
            campaignGameStarter.AddGameMenuOption(
                menuId,
                $"mwr_hireling_back_{menuId}",
                "{=mwr_hireling_back}Back",
                args =>
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                    return IsEnlisted();
                },
                args => GameMenu.SwitchToMenu(HirelingMenuId),
                true);
        }

        private void DisplayPrompt(Action enlistPlayer)
        {
            TextObject title = new TextObject("{=mwr_hireling_prompt_title}Swear service");
            TextObject explanation = new TextObject("{=mwr_hireling_prompt_text}You will ride with this lord's host until you leave his service. Your party will merge into his, you will follow his campaign, and you will be paid for loyal service.");
            _enlistInquiryDeclined = false;

            InquiryData inquiry = new InquiryData(
                title.ToString(),
                explanation.ToString(),
                true,
                true,
                new TextObject("{=mwr_hireling_accept_short}Accept").ToString(),
                new TextObject("{=mwr_hireling_decline_short}Decline").ToString(),
                enlistPlayer,
                () => _enlistInquiryDeclined = true);

            InformationManager.ShowInquiry(inquiry);
        }

        private void StartDialog()
        {
            if (_hirelingEnlistingLord?.PartyBelongedTo == null)
            {
                return;
            }

            ConversationCharacterData characterData = new ConversationCharacterData(_hirelingEnlistingLord.CharacterObject, _hirelingEnlistingLord.PartyBelongedTo.Party);
            ConversationCharacterData playerData = new ConversationCharacterData(Hero.MainHero.CharacterObject, Hero.MainHero.PartyBelongedTo.Party);
            Campaign.Current.CurrentConversationContext = ConversationContext.Default;
            Campaign.Current.ConversationManager.OpenMapConversation(playerData, characterData);
        }

        private void SetActivities()
        {
            List<MWRHirelingActivityDefinition> activities = GetCurrentActivities();
            for (int i = 0; i < 5; i++)
            {
                TextObject value = new TextObject("{=mwr_hireling_activity_missing}Unavailable");
                if (i < activities.Count)
                {
                    value = activities[i].MenuText;
                    if (_currentActivityIndex == i)
                    {
                        value = new TextObject($"[{value}]");
                    }
                }

                MBTextManager.SetTextVariable($"HIRELINGACTIVITYTEXT{i}", value, false);
            }
        }

        private bool HoverActivity(int index, MenuCallbackArgs args)
        {
            List<MWRHirelingActivityDefinition> activities = GetCurrentActivities();
            if (index >= activities.Count)
            {
                return false;
            }

            args.Tooltip = activities[index].TooltipText;
            return true;
        }

        private void ToggleActivity(int index, MenuCallbackArgs args)
        {
            List<MWRHirelingActivityDefinition> activities = GetCurrentActivities();
            if (index >= activities.Count)
            {
                return;
            }

            _currentActivityIndex = index;
            _currentTrainedSkill = activities[index].Skill;
            SetActivities();
            args.Tooltip = activities[index].TooltipText;
            args.MenuContext.Refresh();

            if (IsCurrentHirelingBattleJoinable(GetCurrentHirelingBattleParty()))
            {
                GameMenu.ActivateGameMenu(HirelingBattleMenuId);
            }
        }

        private List<MWRHirelingActivityDefinition> GetCurrentActivities()
        {
            _activities ??= new MWRHirelingActivities();
            string cultureId = Hero.MainHero?.Culture?.StringId;
            return _activities.GetActivities(cultureId);
        }

        private void PauseModeToggle(MenuCallbackArgs args)
        {
            _pauseModeToggle = !_pauseModeToggle;
            MBTextManager.SetTextVariable("PAUSE_ONOFF", _pauseModeToggle ? "{=mwr_hireling_on}On" : "{=mwr_hireling_off}Off", false);
            args.MenuContext.Refresh();
        }

        private void ContinueTimeAfterLeftSettlementWhileEnlisted(GameMenu menu, GameMenuOption option)
        {
            if (!_hirelingEnlisted || option == null)
            {
                return;
            }

            if (option.IdString == "town_leave" || option.IdString == "castle_leave" || option.IdString == "village_leave")
            {
                GameMenu.ActivateGameMenu(HirelingMenuId);
                Campaign.Current.TimeControlMode = CampaignTimeControlMode.StoppableFastForward;
            }
        }

        private void IgnoreHirelingPartyRefresh(MobileParty mobileParty)
        {
            if (_hirelingEnlisted && MobileParty.MainParty == mobileParty)
            {
                MobileParty.MainParty.IgnoreForHours(8f);
            }
        }

        private void WeeklyRenownGain()
        {
            if (!Hero.MainHero.IsEnlisted())
            {
                return;
            }

            int gain = 5 + Hero.MainHero.Clan.Tier;
            Hero.MainHero.Clan.AddRenown(gain);
        }

        private void SkillGain()
        {
            if (!_hirelingEnlisted || Hero.MainHero.IsWounded)
            {
                return;
            }

            List<MWRHirelingActivityDefinition> activities = GetCurrentActivities();
            if (activities.Count == 0)
            {
                return;
            }

            if (_currentActivityIndex < 0 || _currentActivityIndex >= activities.Count)
            {
                _currentActivityIndex = 0;
            }

            _currentTrainedSkill ??= activities[_currentActivityIndex].Skill;
            if (_currentTrainedSkill != null)
            {
                Hero.MainHero.AddSkillXp(_currentTrainedSkill, 25f);
            }
        }

        private void OnDailyTick()
        {
            if (!_hirelingEnlisted || _hirelingEnlistingLord == null)
            {
                return;
            }

            int currentDay = (int)MathF.Floor(CampaignTime.Now.ToDays);
            if (_lastPaidDay >= currentDay)
            {
                return;
            }

            int wage = MWRHirelingHelpers.CalculateDailyWage(Hero.MainHero, _manuallyFoughtBattles, _durationInDays);
            if (wage > 0)
            {
                GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, wage, false);
            }

            _lastPaidDay = currentDay;
        }

        private void LeaveKingdomEvent(Clan clan, Kingdom kingdom, Kingdom newKingdom, ChangeKingdomAction.ChangeKingdomActionDetail detail, bool showNotification)
        {
            if (clan == Clan.PlayerClan && IsEnlisted())
            {
                LeaveLordPartyAction();
            }
        }

        private void JoinBattleConsequence(MenuCallbackArgs args)
        {
            while (Campaign.Current.CurrentMenuContext != null)
            {
                GameMenu.ExitToLast();
            }

            MobileParty eventAlliedParty = GetCurrentHirelingBattleParty();
            if (eventAlliedParty == null)
            {
                return;
            }

            MapEvent mapEvent = eventAlliedParty.MapEvent;
            MapEventSide alliedMapEventSide = GetCurrentHirelingBattleSide(eventAlliedParty);
            PartyBase enemyLeaderBase = alliedMapEventSide?.OtherSide?.LeaderParty;
            if (mapEvent == null || alliedMapEventSide == null || enemyLeaderBase == null)
            {
                return;
            }

            MobileParty playerParty = MobileParty.MainParty;
            playerParty.MapEventSide = alliedMapEventSide;

            if (mapEvent.IsSiegeAssault)
            {
                playerParty.BesiegerCamp = eventAlliedParty.BesiegerCamp
                    ?? alliedMapEventSide.LeaderParty?.MobileParty?.BesiegerCamp;
                playerParty.CurrentSettlement = eventAlliedParty.CurrentSettlement
                    ?? alliedMapEventSide.LeaderParty?.MobileParty?.CurrentSettlement
                    ?? mapEvent.MapEventSettlement;
                Game.Current.AfterTick -= InitializeSiegeBattle;
                _siegeBattleMissionStarted = true;
                _startBattle = true;
            }
            else
            {
                playerParty.BesiegerCamp = null;
                playerParty.CurrentSettlement = null;
                _startBattle = true;
                EncounterManager.StartPartyEncounter(PartyBase.MainParty, enemyLeaderBase);
            }

            _hirelingLordIsFightingWithoutPlayer = false;
        }

        private void AvoidCombatConsequence(MenuCallbackArgs args)
        {
            _hirelingLordIsFightingWithoutPlayer = true;
            _startBattle = false;
            _joinedHirelingCleanupBattle = null;
            _deadJoinedEncounterCleanupTicks = 0;

            MobileParty playerParty = MobileParty.MainParty;
            playerParty.MapEventSide = null;
            playerParty.BesiegerCamp = null;
            playerParty.CurrentSettlement = null;

            args.MenuContext.GameMenu.StartWait();
        }

        private void OnRaidCompleted(BattleSideEnum side, RaidEventComponent component)
        {
            if (!_hirelingEnlisted || component == null || !component.IsPlayerMapEvent)
            {
                return;
            }

            _hirelingWaitMenuShown = false;
            if (HasPendingNativeHirelingEncounterCleanup())
            {
                return;
            }

            bool hasAnySettlementState =
                PlayerEncounter.EncounterSettlement != null ||
                MobileParty.MainParty.CurrentSettlement != null ||
                Settlement.CurrentSettlement != null;

            if (!hasAnySettlementState)
            {
                return;
            }

            while (Campaign.Current.CurrentMenuContext != null)
            {
                GameMenu.ExitToLast();
            }

            if (PlayerEncounter.EncounterSettlement != null)
            {
                PlayerEncounter.LeaveSettlement();
            }
            else if (MobileParty.MainParty.CurrentSettlement != null)
            {
                LeaveSettlementAction.ApplyForParty(MobileParty.MainParty);
                PartyBase.MainParty.SetVisualAsDirty();
            }

            if (PlayerEncounter.Current != null
                && PlayerEncounter.EncounterSettlement == null
                && PlayerEncounter.Current.EncounterState == PlayerEncounterState.End)
            {
                PlayerEncounter.Finish(false);
            }

            if (PlayerEncounter.LocationEncounter != null)
            {
                PlayerEncounter.LocationEncounter = null;
            }

            GameMenu.ActivateGameMenu(HirelingMenuId);
        }

        private float GetEnlistingLordEventStrengthRatio(MobileParty lordParty)
        {
            MapEventSide mapEventSide = GetCurrentHirelingBattleSide(lordParty);
            if (lordParty?.MapEvent == null || mapEventSide == null)
            {
                return 1f;
            }

            BattleSideEnum side = mapEventSide.MissionSide;
            lordParty.MapEvent.GetStrengthsRelativeToParty(side, out float enlistingLordStrength, out float enemyStrength);
            return enemyStrength > 0f ? enlistingLordStrength / enemyStrength : 1f;
        }

        private bool IsHirelingSettlementEncounterSafe(Settlement settlement)
        {
            return settlement != null
                && !_inPostBattleTransition
                && settlement.SiegeEvent == null
                && settlement.Party.MapEvent == null;
        }

        private void EnsureHirelingSettlementEncounter(Settlement settlement)
        {
            if (settlement == null)
            {
                return;
            }

            if (PlayerEncounter.Current != null && PlayerEncounter.EncounterSettlement != settlement)
            {
                _inPostBattleTransition = false;
                PlayerEncounter.Finish(false);
            }

            if (PlayerEncounter.Current != null
                && PlayerEncounter.EncounterSettlement == settlement
                && PlayerEncounter.LocationEncounter == null)
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

        private MobileParty GetCurrentHirelingBattleParty()
        {
            MobileParty lordParty = _hirelingEnlistingLord?.PartyBelongedTo;
            if (lordParty == null)
            {
                return null;
            }

            if (lordParty.MapEvent != null)
            {
                return lordParty;
            }

            MobileParty armyOwnerParty = lordParty.Army?.ArmyOwner?.PartyBelongedTo;
            if (armyOwnerParty?.MapEvent == null)
            {
                return null;
            }

            bool lordAlreadyInArmyOwnerMapEvent = armyOwnerParty.MapEvent.InvolvedParties.Any(x => x == lordParty.Party);
            if (lordAlreadyInArmyOwnerMapEvent)
            {
                return armyOwnerParty;
            }

            if (!armyOwnerParty.MapEvent.IsSiegeAssault)
            {
                return null;
            }

            bool sameBesiegerCamp = lordParty.BesiegerCamp != null && lordParty.BesiegerCamp == armyOwnerParty.BesiegerCamp;
            bool sameDefendingSettlement = lordParty.CurrentSettlement != null
                && lordParty.CurrentSettlement == armyOwnerParty.CurrentSettlement
                && lordParty.CurrentSettlement == armyOwnerParty.MapEvent.MapEventSettlement;

            if ((sameBesiegerCamp || sameDefendingSettlement)
                && armyOwnerParty.MapEvent.InvolvedParties.Any(x => x == lordParty.Party))
            {
                return armyOwnerParty;
            }

            return null;
        }

        private static MapEventSide ResolveMapEventSide(MapEvent mapEvent, PartyBase party)
        {
            if (mapEvent == null || party == null)
            {
                return null;
            }

            if (mapEvent.AttackerSide?.Parties.Any(x => x.Party == party) == true)
            {
                return mapEvent.AttackerSide;
            }

            if (mapEvent.DefenderSide?.Parties.Any(x => x.Party == party) == true)
            {
                return mapEvent.DefenderSide;
            }

            return null;
        }

        private static MapEventSide GetCurrentHirelingBattleSide(MobileParty battleParty)
        {
            if (battleParty?.MapEvent == null)
            {
                return null;
            }

            return battleParty.MapEventSide ?? ResolveMapEventSide(battleParty.MapEvent, battleParty.Party);
        }

        private bool HasExternalRaidInterrupter(MapEvent mapEvent)
        {
            IEnumerable<PartyBase> defaultSettlementDefenders = mapEvent.MapEventSettlement.GetInvolvedPartiesForEventType(mapEvent.EventType);
            return mapEvent.DefenderSide.Parties.Any(defenderParty => !defaultSettlementDefenders.Contains(defenderParty.Party));
        }

        internal static bool IsJoinableHirelingMapEvent(MapEvent mapEvent, BattleSideEnum? alliedSide)
        {
            if (mapEvent == null || alliedSide == null || mapEvent.HasWinner)
            {
                return false;
            }

            MapEventSide alliedMapEventSide = mapEvent.GetMapEventSide(alliedSide.Value);
            MapEventSide enemyMapEventSide = alliedMapEventSide?.OtherSide;
            if (alliedMapEventSide == null || enemyMapEventSide?.LeaderParty == null)
            {
                return false;
            }

            bool alliedHasHealthyTroops = alliedMapEventSide.Parties.Any(x => x.Party.NumberOfHealthyMembers > 0);
            bool enemyHasHealthyTroops = enemyMapEventSide.Parties.Any(x => x.Party.NumberOfHealthyMembers > 0);
            if (!alliedHasHealthyTroops || !enemyHasHealthyTroops)
            {
                return false;
            }

            return mapEvent.HasTroopsOnBothSides() || mapEvent.IsSiegeAssault;
        }

        internal static bool IsOngoingHirelingMapEvent(MapEvent mapEvent)
        {
            return mapEvent != null && !mapEvent.HasWinner;
        }

        internal static bool HasPendingNativeEncounterCleanup()
        {
            return Campaign.Current?.GetCampaignBehavior<MWRHirelingCampaignBehavior>()?.HasPendingNativeHirelingEncounterCleanup() ?? false;
        }

        internal static bool HasActiveNativeJoinedBattleEncounter()
        {
            return Campaign.Current?.GetCampaignBehavior<MWRHirelingCampaignBehavior>()?.HasActiveNativeHirelingJoinedBattleEncounter() ?? false;
        }

        internal static bool HasJoinableBattleForCurrentEnlistment()
        {
            return Campaign.Current?.GetCampaignBehavior<MWRHirelingCampaignBehavior>()?.HasJoinableHirelingBattle() ?? false;
        }

        private bool HasJoinableHirelingBattle()
        {
            MapEvent playerMapEvent = Hero.MainHero.PartyBelongedTo?.MapEvent;
            BattleSideEnum? playerMapEventSide = Hero.MainHero.PartyBelongedTo?.MapEventSide?.MissionSide;

            if (IsJoinableHirelingMapEvent(playerMapEvent, playerMapEventSide))
            {
                return true;
            }

            return IsCurrentHirelingBattleJoinable(GetCurrentHirelingBattleParty());
        }

        private bool HasPendingNativeHirelingEncounterCleanup()
        {
            PlayerEncounter currentEncounter = PlayerEncounter.Current;
            MapEvent currentBattle = PlayerEncounter.Battle;

            return _inPostBattleTransition
                && currentEncounter != null
                && PlayerEncounter.EncounterSettlement == null
                && (currentEncounter.EncounterState == PlayerEncounterState.End
                    || (currentBattle != null && currentBattle.HasWinner));
        }

        private bool HasActiveNativeHirelingJoinedBattleEncounter()
        {
            PlayerEncounter currentEncounter = PlayerEncounter.Current;
            MapEvent currentBattle = PlayerEncounter.Battle;

            return currentEncounter?.IsJoinedBattle == true
                && currentBattle != null
                && !currentBattle.HasWinner
                && PlayerEncounter.EncounterSettlement == null
                && currentEncounter.EncounterState != PlayerEncounterState.End;
        }

        private bool IsCurrentHirelingBattleJoinable(MobileParty battleParty)
        {
            MapEventSide battleSide = GetCurrentHirelingBattleSide(battleParty);
            return battleSide != null
                && IsJoinableHirelingMapEvent(battleParty.MapEvent, battleSide.MissionSide);
        }

        private void MenuOpened(MenuCallbackArgs obj)
        {
            string menuId = obj?.MenuContext?.GameMenu?.StringId;
            if (string.IsNullOrEmpty(menuId))
            {
                return;
            }

            if (IsEnlisted() && menuId == "encounter" && !_startBattle)
            {
                if (HasActiveNativeHirelingJoinedBattleEncounter() || HasPendingNativeHirelingEncounterCleanup())
                {
                    return;
                }

                if (PlayerEncounter.Current?.IsJoinedBattle == true && HasJoinableHirelingBattle())
                {
                    return;
                }

                GameMenu.SwitchToMenu(HirelingMenuId);
                _hirelingWaitMenuShown = true;
                return;
            }

            if (_startBattle && menuId == "join_encounter")
            {
                MobileParty battleParty = GetCurrentHirelingBattleParty();
                MapEventSide battleSide = GetCurrentHirelingBattleSide(battleParty);
                if (battleSide == null)
                {
                    return;
                }

                PlayerEncounter.JoinBattle(battleSide.MissionSide);
                _joinedHirelingCleanupBattle = PlayerEncounter.Battle ?? battleParty.MapEvent;
                _deadJoinedEncounterCleanupTicks = 0;
                GameMenu.SwitchToMenu("encounter");
                return;
            }

            if (_startBattle && menuId == "encounter")
            {
                if (_siegeBattleMissionStarted)
                {
                    MobileParty battleParty = GetCurrentHirelingBattleParty();
                    MapEventSide battleSide = GetCurrentHirelingBattleSide(battleParty);
                    if (battleSide != null)
                    {
                        PlayerEncounter.JoinBattle(battleSide.MissionSide);
                        _joinedHirelingCleanupBattle = PlayerEncounter.Battle ?? battleParty.MapEvent;
                        _deadJoinedEncounterCleanupTicks = 0;
                    }

                    _siegeBattleMissionStarted = false;
                }

                _startBattle = false;
                if (Hero.MainHero.PartyBelongedTo.MapEvent != null)
                {
                    MenuHelper.EncounterAttackConsequence(obj);
                }
                else
                {
                    _startBattle = true;
                }
            }
        }

        private void LeaveEnlistingParty(string menuToReturn, bool desertion = false)
        {
            if (!desertion)
            {
                desertion = _durationInDays < MinimumServeDays;
            }

            string desertText = desertion
                ? new TextObject("{=mwr_hireling_desert_consequence}This will harm your relations with the entire faction.").ToString()
                : string.Empty;

            InquiryData inquiry = new InquiryData(
                new TextObject("{=mwr_hireling_abandon_party_title}Abandon Party").ToString(),
                new TextObject("{=mwr_hireling_abandon_party_text}Are you sure you want to abandon the party? {DESERT_TEXT}")
                    .SetTextVariable("DESERT_TEXT", desertText)
                    .ToString(),
                true,
                true,
                new TextObject("{=mwr_hireling_yes}Yes").ToString(),
                new TextObject("{=mwr_hireling_no}No").ToString(),
                () =>
                {
                    if (desertion && _hirelingEnlistingLord?.MapFaction != null)
                    {
                        ChangeCrimeRatingAction.Apply(_hirelingEnlistingLord.MapFaction, 55f);
                        if (_hirelingEnlistingLord.Clan?.Kingdom != null)
                        {
                            foreach (Clan clan in _hirelingEnlistingLord.Clan.Kingdom.Clans)
                            {
                                if (!clan.IsUnderMercenaryService)
                                {
                                    ChangeRelationAction.ApplyPlayerRelation(clan.Leader, -10);
                                }
                            }
                        }
                    }

                    GameMenu.ExitToLast();
                    LeaveLordPartyAction();
                },
                () => GameMenu.ActivateGameMenu(menuToReturn));

            InformationManager.ShowInquiry(inquiry);
        }

        public void LeaveLordPartyAction()
        {
            _startBattle = false;
            _siegeBattleMissionStarted = false;
            _inPostBattleTransition = false;
            _hirelingLordIsFightingWithoutPlayer = false;
            _hirelingWaitMenuShown = false;
            _joinedHirelingCleanupBattle = null;
            _deadJoinedEncounterCleanupTicks = 0;
            Game.Current.AfterTick -= InitializeSiegeBattle;

            _hirelingEnlisted = false;
            _hirelingEnlistingLord = null;

            PlayerEncounter.Current?.RosterToReceiveLootItems?.Clear();
            PendingLootedTroopManager.ResetAllPendingState();

            if (PlayerEncounter.Current != null)
            {
                PlayerEncounter.Finish();
            }

            if (Settlement.CurrentSettlement != null)
            {
                if (PlayerEncounter.EncounterSettlement != null)
                {
                    PlayerEncounter.LeaveSettlement();
                }

                if (MobileParty.MainParty.CurrentSettlement != null)
                {
                    LeaveSettlementAction.ApplyForParty(MobileParty.MainParty);
                }
            }

            UndoDiplomacy();
            ShowPlayerParty();

            _durationInDays = 0f;
            _manuallyFoughtBattles = 0;
            _currentTrainedSkill = null;
            _currentActivityIndex = 0;
            _lastPaidDay = 0;
        }

        private void InitializeSiegeBattle(float tick)
        {
            if (!_hirelingEnlisted || !_siegeBattleMissionStarted || MobileParty.MainParty == null)
            {
                return;
            }

            MapEvent mainPartyMapEvent = MobileParty.MainParty.MapEvent;
            if (mainPartyMapEvent == null || mainPartyMapEvent.StringId == null)
            {
                return;
            }

            StartBattleAction.Apply(PartyBase.MainParty, mainPartyMapEvent.DefenderSide.LeaderParty);
            _siegeBattleMissionStarted = false;
            Game.Current.AfterTick -= InitializeSiegeBattle;
        }

        private bool hireling_battle_menu_join_battle_on_condition(MenuCallbackArgs args)
        {
            return !Hero.MainHero.IsWounded && IsCurrentHirelingBattleJoinable(GetCurrentHirelingBattleParty());
        }

        private bool hireling_battle_menu_desert_on_condition(MenuCallbackArgs args)
        {
            return _hirelingEnlistingLord?.CurrentSettlement == null;
        }

        private bool hireling_battle_menu_avoid_combat_on_condition(MenuCallbackArgs args)
        {
            if (_hirelingEnlistingLord == null)
            {
                return false;
            }

            MobileParty lordParty = GetCurrentHirelingBattleParty();
            if (lordParty?.MapEvent == null || !IsCurrentHirelingBattleJoinable(lordParty))
            {
                return false;
            }

            if (Hero.MainHero.IsWounded)
            {
                return true;
            }

            float partyStrength = GetEnlistingLordEventStrengthRatio(lordParty);
            return partyStrength > RatioPartyAgainstEnemyStrength;
        }

        private bool wait_on_condition(MenuCallbackArgs args)
        {
            return true;
        }

        private void wait_on_tick(MenuCallbackArgs args, CampaignTime time)
        {
            if (!_hirelingEnlisted || _hirelingEnlistingLord?.PartyBelongedTo == null)
            {
                while (Campaign.Current.CurrentMenuContext != null)
                {
                    GameMenu.ExitToLast();
                }

                return;
            }

            if (args.MenuContext?.GameMenu == null)
            {
                return;
            }

            TextObject menuText = new TextObject("{=mwr_hireling_maintext}{ENLISTING_LORD} leads the host.{newline}Days in service: {ENLISTING_DURATION}{newline}Battles fought: {HIRELING_BATTLE_COUNT}{ENLISTING_ARMY}");
            menuText.SetTextVariable("ENLISTING_LORD", _hirelingEnlistingLord.Name);
            menuText.SetTextVariable("ENLISTING_DURATION", $"{_durationInDays:0.0}");
            menuText.SetTextVariable("HIRELING_BATTLE_COUNT", _manuallyFoughtBattles);
            menuText.SetTextVariable("ENLISTING_ARMY", BuildStatusInfoText());
            MBTextManager.SetTextVariable("ENLISTING_TEXT", menuText, false);

            if (_hirelingEnlistingLord.MapFaction?.Culture?.EncounterBackgroundMesh != null)
            {
                args.MenuContext.SetBackgroundMeshName(_hirelingEnlistingLord.MapFaction.Culture.EncounterBackgroundMesh);
            }
        }

        private void hireling_battle_menu_on_init(MenuCallbackArgs args)
        {
            MobileParty battleParty = GetCurrentHirelingBattleParty();
            if (battleParty?.MapEvent == null)
            {
                MBTextManager.SetTextVariable("BATTLE_INFO", new TextObject("{=mwr_hireling_battle_fallback}Your lord engages in battle."), false);
                return;
            }

            MapEvent mapEvent = battleParty.MapEvent;
            MapEventSide lordSide = GetCurrentHirelingBattleSide(battleParty);
            MapEventSide enemySide = lordSide?.OtherSide;

            TextObject battleTypeText = new TextObject("{=mwr_hireling_battle_type}Battle");
            if (mapEvent.IsSiegeAssault)
            {
                bool isDefender = lordSide?.MissionSide == BattleSideEnum.Defender;
                battleTypeText = isDefender
                    ? new TextObject("{=mwr_hireling_siege_defense}Siege Defense")
                    : new TextObject("{=mwr_hireling_siege_assault}Siege Assault");
            }

            string enemyLeader = enemySide?.LeaderParty?.LeaderHero?.Name?.ToString()
                ?? enemySide?.LeaderParty?.Name?.ToString()
                ?? new TextObject("{=mwr_hireling_unknown_enemy}Unknown").ToString();

            int allyStrength = lordSide?.RecalculateMemberCountOfSide() ?? 0;
            int enemyStrength = enemySide?.RecalculateMemberCountOfSide() ?? 0;

            TextObject oddsText;
            if (enemyStrength == 0)
            {
                oddsText = new TextObject("{=mwr_hireling_certain_victory}Certain Victory");
            }
            else
            {
                float ratio = (float)allyStrength / enemyStrength;
                oddsText = ratio switch
                {
                    >= 2.0f => new TextObject("{=mwr_hireling_odds_overwhelming_advantage}Overwhelming Advantage"),
                    >= 1.5f => new TextObject("{=mwr_hireling_odds_strong_advantage}Strong Advantage"),
                    >= 1.1f => new TextObject("{=mwr_hireling_odds_slight_advantage}Slight Advantage"),
                    >= 0.9f => new TextObject("{=mwr_hireling_odds_even}Even Odds"),
                    >= 0.66f => new TextObject("{=mwr_hireling_odds_slight_disadvantage}Slight Disadvantage"),
                    >= 0.5f => new TextObject("{=mwr_hireling_odds_strong_disadvantage}Strong Disadvantage"),
                    _ => new TextObject("{=mwr_hireling_odds_overwhelming_disadvantage}Overwhelming Disadvantage")
                };
            }

            TextObject enemyText = new TextObject("{=mwr_hireling_enemy_line}Enemy: {ENEMY_LEADER}");
            enemyText.SetTextVariable("ENEMY_LEADER", enemyLeader);

            TextObject oddsLine = new TextObject("{=mwr_hireling_odds_line}Odds: {ODDS}");
            oddsLine.SetTextVariable("ODDS", oddsText);

            TextObject battleInfo = new TextObject("{=mwr_hireling_battle_info}{BATTLE_TYPE}{newline}{ENEMY_LINE}{newline}{ODDS_LINE}");
            battleInfo.SetTextVariable("BATTLE_TYPE", battleTypeText);
            battleInfo.SetTextVariable("ENEMY_LINE", enemyText);
            battleInfo.SetTextVariable("ODDS_LINE", oddsLine);
            MBTextManager.SetTextVariable("BATTLE_INFO", battleInfo, false);
        }

        private void ControlPlayerLoot(MapEvent mapEvent)
        {
            if (IsEnlisted() && mapEvent.PlayerSide == mapEvent.WinningSide)
            {
                if (!_hirelingLordIsFightingWithoutPlayer)
                {
                    _manuallyFoughtBattles++;
                }

                PlayerEncounter.Current?.RosterToReceiveLootItems?.Clear();
                PendingLootedTroopManager.ClearPendingMembers();
                PendingLootedTroopManager.ClearPendingPrisoners();
            }

            _hirelingWaitMenuShown = false;
        }

        private void OnPartyLeavesSettlement(MobileParty mobileParty, Settlement settlement)
        {
            if (!_hirelingEnlisted || _hirelingEnlistingLord == null)
            {
                return;
            }

            bool enlistingLordLeftSettlement = _hirelingEnlistingLord.PartyBelongedTo == mobileParty;
            bool mainPartyLeftSettlement = MobileParty.MainParty == mobileParty && mobileParty.CurrentSettlement == null;
            if (!enlistingLordLeftSettlement && !mainPartyLeftSettlement)
            {
                return;
            }

            while (Campaign.Current.CurrentMenuContext != null)
            {
                GameMenu.ExitToLast();
            }

            if (enlistingLordLeftSettlement)
            {
                if (MobileParty.MainParty.CurrentSettlement != null)
                {
                    LeaveSettlementAction.ApplyForParty(MobileParty.MainParty);
                }

                PartyBase.MainParty.SetVisualAsDirty();
                _hirelingWaitMenuShown = false;
                GameMenu.ActivateGameMenu(HirelingMenuId);
                return;
            }

            if (PlayerEncounter.Current != null
                && PlayerEncounter.EncounterSettlement == null
                && PlayerEncounter.Current.EncounterState == PlayerEncounterState.End)
            {
                PlayerEncounter.Finish(false);
            }

            if (PlayerEncounter.LocationEncounter != null)
            {
                PlayerEncounter.LocationEncounter = null;
            }

            PartyBase.MainParty.SetVisualAsDirty();
            _hirelingWaitMenuShown = false;
            GameMenu.ActivateGameMenu(HirelingMenuId);
        }

        private void EnlistingLordPartyEntersSettlement(MobileParty mobileParty, Settlement settlement, Hero partyHero)
        {
            if (!_hirelingEnlisted || _hirelingEnlistingLord == null || _hirelingEnlistingLord.PartyBelongedTo != mobileParty)
            {
                return;
            }

            if (!settlement.IsTown)
            {
                _hirelingWaitMenuShown = false;
                GameMenu.ActivateGameMenu(HirelingMenuId);
                _hirelingWaitMenuShown = true;
                return;
            }

            if (MobileParty.MainParty.CurrentSettlement == settlement && PlayerEncounter.EncounterSettlement == settlement)
            {
                return;
            }

            if (!IsHirelingSettlementEncounterSafe(settlement))
            {
                GameMenu.SwitchToMenu(HirelingMenuId);
                if (_pauseModeToggle)
                {
                    Campaign.Current.TimeControlMode = CampaignTimeControlMode.Stop;
                }

                return;
            }

            EnsureHirelingSettlementEncounter(settlement);
            _inPostBattleTransition = false;
            GameMenu.SwitchToMenu(HirelingMenuId);

            if (_pauseModeToggle)
            {
                Campaign.Current.TimeControlMode = CampaignTimeControlMode.Stop;
            }
        }

        private void MapEventEnded(MapEvent mapEvent)
        {
            if (_hirelingEnlistingLord == null || !IsEnlisted())
            {
                return;
            }

            if (!GetEnlistingLordIsInMapEvent(mapEvent) && !mapEvent.IsPlayerMapEvent)
            {
                return;
            }

            if (_hirelingEnlistingLord.PartyBelongedTo == null)
            {
                return;
            }

            _inPostBattleTransition = true;
            PlayerEncounter.Current?.RosterToReceiveLootItems?.Clear();
            PendingLootedTroopManager.ResetAllPendingState();
            _hirelingLordIsFightingWithoutPlayer = false;

            bool endedPlayerEncounterForThisBattle =
                PlayerEncounter.Current != null &&
                PlayerEncounter.EncounterSettlement == null &&
                PlayerEncounter.Battle == mapEvent;

            if (endedPlayerEncounterForThisBattle && PlayerEncounter.Current.IsJoinedBattle != true)
            {
                PlayerEncounter.Finish(false);
                _hirelingWaitMenuShown = false;
                return;
            }

            bool waitingForNativeEncounterCleanup = mapEvent.IsPlayerMapEvent && HasPendingNativeHirelingEncounterCleanup();
            if (waitingForNativeEncounterCleanup)
            {
                _hirelingWaitMenuShown = false;
            }
            else
            {
                GameMenu.ActivateGameMenu(HirelingMenuId);
                _hirelingWaitMenuShown = true;
            }
        }

        private bool TryCleanupDeadJoinedHirelingEncounter()
        {
            PlayerEncounter currentEncounter = PlayerEncounter.Current;
            MapEvent currentBattle = PlayerEncounter.Battle;
            string currentMenuId = Campaign.Current.CurrentMenuContext?.GameMenu?.StringId;

            bool isTrackedJoinedHirelingBattle = _joinedHirelingCleanupBattle != null && currentBattle == _joinedHirelingCleanupBattle;
            bool hasTrackedJoinedHirelingEncounter =
                !_startBattle &&
                currentEncounter?.IsJoinedBattle == true &&
                PlayerEncounter.EncounterSettlement == null &&
                isTrackedJoinedHirelingBattle;

            if (!hasTrackedJoinedHirelingEncounter)
            {
                if (currentEncounter == null || currentBattle != _joinedHirelingCleanupBattle)
                {
                    _joinedHirelingCleanupBattle = null;
                }

                _deadJoinedEncounterCleanupTicks = 0;
                return false;
            }

            if (HasJoinableHirelingBattle() || currentMenuId != HirelingMenuId)
            {
                _deadJoinedEncounterCleanupTicks = 0;
                return false;
            }

            bool nativeCleanupIsStillPending = HasPendingNativeHirelingEncounterCleanup();
            _deadJoinedEncounterCleanupTicks++;

            bool canStrictlyCleanupEndedJoinedEncounter =
                _inPostBattleTransition &&
                currentBattle != null &&
                currentBattle.HasWinner &&
                currentEncounter.EncounterState == PlayerEncounterState.End &&
                currentBattle.PlayerSide == currentBattle.WinningSide &&
                !nativeCleanupIsStillPending;

            if (canStrictlyCleanupEndedJoinedEncounter && _deadJoinedEncounterCleanupTicks >= StrictJoinedCleanupDelayTicks)
            {
                PlayerEncounter.Finish(false);
                _hirelingWaitMenuShown = false;
                _joinedHirelingCleanupBattle = null;
                _deadJoinedEncounterCleanupTicks = 0;
                return true;
            }

            if (_deadJoinedEncounterCleanupTicks >= FallbackJoinedCleanupDelayTicks)
            {
                PlayerEncounter.Finish(false);
                _hirelingWaitMenuShown = false;
                _joinedHirelingCleanupBattle = null;
                _deadJoinedEncounterCleanupTicks = 0;
                return true;
            }

            return false;
        }

        private void OnTick(float dt)
        {
            if (_hirelingEnlisted && _hirelingEnlistingLord?.PartyBelongedTo != null)
            {
                MobileParty playerParty = MobileParty.MainParty;
                bool hasDetachedPersistedBattleState =
                    PlayerEncounter.Current == null &&
                    !HasPendingNativeHirelingEncounterCleanup() &&
                    !HasJoinableHirelingBattle() &&
                    (playerParty.MapEventSide != null || playerParty.BesiegerCamp != null);

                if ((_inPostBattleTransition || hasDetachedPersistedBattleState)
                    && !HasPendingNativeHirelingEncounterCleanup()
                    && !HasJoinableHirelingBattle()
                    && PlayerEncounter.Current == null)
                {
                    playerParty.MapEventSide = null;
                    playerParty.BesiegerCamp = null;
                    playerParty.CurrentSettlement = null;
                    _inPostBattleTransition = false;
                }

                CampaignTime campaignStart = Campaign.Current.Models.CampaignTimeModel.CampaignStartTime;
                _durationInDays = campaignStart.ElapsedDaysUntilNow - _entryServiceTimeStamp;

                var currentMenuContext = Campaign.Current.CurrentMenuContext;
                if (currentMenuContext?.GameMenu?.StringId == HirelingMenuId)
                {
                    GameMenu hirelingMenu = Campaign.Current.GameMenuManager.GetGameMenu(HirelingMenuId);
                    hirelingMenu.RunOnTick(currentMenuContext, dt);
                }

                if (TryCleanupDeadJoinedHirelingEncounter())
                {
                    return;
                }

                bool waitingForNativeEncounterCleanup =
                    HasActiveNativeHirelingJoinedBattleEncounter() ||
                    HasPendingNativeHirelingEncounterCleanup();

                if (!_hirelingWaitMenuShown && !waitingForNativeEncounterCleanup)
                {
                    GameMenu.ActivateGameMenu(HirelingMenuId);
                    _hirelingWaitMenuShown = true;
                    SetActivities();
                    Campaign.Current.CurrentMenuContext?.Refresh();
                }

                HidePlayerParty();
                PartyBase.MainParty.MobileParty.Position = _hirelingEnlistingLord.PartyBelongedTo.Position;

                MobileParty battleParty = GetCurrentHirelingBattleParty();
                if (battleParty?.MapEvent == null)
                {
                    return;
                }

                MapEvent mapEvent = battleParty.MapEvent;
                bool currentHirelingBattleIsJoinable = IsCurrentHirelingBattleJoinable(battleParty);
                if (!currentHirelingBattleIsJoinable)
                {
                    if (currentMenuContext?.GameMenu?.StringId == HirelingBattleMenuId)
                    {
                        GameMenu.ActivateGameMenu(HirelingMenuId);
                        _hirelingWaitMenuShown = true;
                    }

                    return;
                }

                if (mapEvent.IsRaid || mapEvent.IsForcingSupplies || mapEvent.IsForcingVolunteers)
                {
                    bool lordIsOnRaidSide = mapEvent.AttackerSide.Parties.Any(x => x.Party == _hirelingEnlistingLord.PartyBelongedTo.Party);
                    if (lordIsOnRaidSide && !HasExternalRaidInterrupter(mapEvent))
                    {
                        return;
                    }
                }

                if (!_hirelingLordIsFightingWithoutPlayer)
                {
                    GameMenu.ActivateGameMenu(HirelingBattleMenuId);
                }
            }
            else if (_hirelingEnlisted && _hirelingEnlistingLord?.PartyBelongedTo == null)
            {
                LeaveLordPartyAction();
            }
        }

        private void EnlistPlayer()
        {
            _hirelingEnlistingLord = Campaign.Current.ConversationManager.OneToOneConversationHero;
            if (_hirelingEnlistingLord?.PartyBelongedTo == null || _hirelingEnlistingLord.Clan?.Kingdom == null)
            {
                return;
            }

            HidePlayerParty();
            DisbandParty();
            MobileParty.MainParty.IgnoreForHours(8f);

            ChangeKingdomAction.ApplyByJoinFactionAsMercenary(Clan.PlayerClan, _hirelingEnlistingLord.Clan.Kingdom, default, 25, false);

            while (Campaign.Current.CurrentMenuContext != null)
            {
                GameMenu.ExitToLast();
            }

            if (PlayerEncounter.Current != null && PlayerEncounter.EncounterSettlement == null)
            {
                PlayerEncounter.Finish(false);
            }

            _hirelingEnlisted = true;
            _startBattle = false;
            _siegeBattleMissionStarted = false;
            _inPostBattleTransition = false;
            _hirelingLordIsFightingWithoutPlayer = false;
            _hirelingWaitMenuShown = false;
            _currentTrainedSkill = null;
            _currentActivityIndex = 0;
            _manuallyFoughtBattles = 0;
            _lastPaidDay = (int)MathF.Floor(CampaignTime.Now.ToDays);

            if (Clan.PlayerClan.Influence != 0f)
            {
                ChangeClanInfluenceAction.Apply(Clan.PlayerClan, -Clan.PlayerClan.Influence);
            }

            SetActivities();
            _entryServiceTimeStamp = Campaign.Current.Models.CampaignTimeModel.CampaignStartTime.ElapsedDaysUntilNow;
            GameMenu.ActivateGameMenu(HirelingMenuId);
        }

        private void UndoDiplomacy()
        {
            ChangeKingdomAction.ApplyByLeaveKingdomAsMercenary(Hero.MainHero.Clan, false);
        }

        private void ShowPlayerParty()
        {
            PartyBase.MainParty.MobileParty.IsVisible = true;
        }

        private void HidePlayerParty()
        {
            PartyBase.MainParty.MobileParty.IsVisible = false;
        }

        private void DisbandParty()
        {
            if (MobileParty.MainParty.MemberRoster.TotalManCount <= 1)
            {
                return;
            }

            List<TroopRosterElement> transferRoster = new List<TroopRosterElement>();
            foreach (TroopRosterElement troopRosterElement in MobileParty.MainParty.MemberRoster.GetTroopRoster())
            {
                if (troopRosterElement.Character != Hero.MainHero.CharacterObject && troopRosterElement.Character.HeroObject == null)
                {
                    transferRoster.Add(troopRosterElement);
                }
            }

            foreach (TroopRosterElement troopRosterElement in transferRoster)
            {
                MobileParty.MainParty.MemberRoster.AddToCounts(troopRosterElement.Character, -troopRosterElement.Number);
                EnlistingLord.PartyBelongedTo.MemberRoster.AddToCounts(troopRosterElement.Character, troopRosterElement.Number);
            }
        }

        private bool GetEnlistingLordIsInMapEvent(MapEvent mapEvent)
        {
            PartyBase lordParty = _hirelingEnlistingLord?.PartyBelongedTo?.Party;
            return lordParty != null && mapEvent?.InvolvedParties.Any(involvedParty => involvedParty == lordParty) == true;
        }

        private void party_wait_talk_to_other_members_on_init(MenuCallbackArgs args)
        {
        }

        private string BuildStatusInfoText()
        {
            List<string> lines = new List<string>();
            MobileParty lordParty = _hirelingEnlistingLord.PartyBelongedTo;

            if (lordParty.Army != null)
            {
                TextObject armyText = new TextObject("{=mwr_hireling_serving_in_army}{newline}Serving in {ARMY_NAME}");
                armyText.SetTextVariable("ARMY_NAME", lordParty.Army.Name);
                lines.Add(armyText.ToString());
            }

            var siegeEvent = lordParty.BesiegerCamp?.SiegeEvent ?? lordParty.Army?.ArmyOwner?.PartyBelongedTo?.BesiegerCamp?.SiegeEvent;
            if (siegeEvent != null)
            {
                TextObject siegeText = new TextObject("{=mwr_hireling_besieging}{newline}Besieging {SETTLEMENT_NAME}");
                siegeText.SetTextVariable("SETTLEMENT_NAME", siegeEvent.BesiegedSettlement.Name);
                lines.Add(siegeText.ToString());
            }

            MapEvent lordMapEvent = lordParty.MapEvent;
            if (lordMapEvent != null && (lordMapEvent.IsRaid || lordMapEvent.IsForcingSupplies || lordMapEvent.IsForcingVolunteers))
            {
                var raidSettlement = lordMapEvent.MapEventSettlement;
                if (raidSettlement != null)
                {
                    TextObject raidText = new TextObject("{=mwr_hireling_raiding}{newline}Raiding {VILLAGE_NAME}");
                    raidText.SetTextVariable("VILLAGE_NAME", raidSettlement.Name);
                    lines.Add(raidText.ToString());
                }
            }

            return string.Concat(lines);
        }
    }
}
