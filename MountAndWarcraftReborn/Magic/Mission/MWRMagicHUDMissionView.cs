using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.ScreenSystem;

namespace MountAndWarcraftReborn.Magic.Mission
{
    [DefaultView]
    public class MWRMagicHUDMissionView : MissionView
    {
        private const bool EnableBattleMagicUi = true;
        private const bool EnableHudMovie = true;
        private const bool EnableRadialMovie = true;
        private const bool EnableMainAgentHook = true;
        private const bool EnableMissionTickLogic = true;
        private GauntletLayer? _hudLayer;
        private GauntletLayer? _radialLayer;
        private MWRMagicHUDVM? _hudVm;
        private MWRMagicRadialSelectionVM? _radialVm;
        private bool _radialOpen;
        private bool _isInitializedForMainAgent;
        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            if (!EnableBattleMagicUi)
            {
                return;
            }

            if (EnableMainAgentHook)
            {
                TaleWorlds.MountAndBlade.Mission.Current.OnMainAgentChanged += OnMainAgentChanged;
            }
            if (EnableHudMovie)
            {
                _hudVm = new MWRMagicHUDVM();
                _hudLayer = new GauntletLayer("GauntletLayer", 100);
                _hudLayer.LoadMovie("MWRMagicHUD", _hudVm);
                MissionScreen.AddLayer(_hudLayer);
            }

            if (EnableRadialMovie)
            {
                _radialVm = new MWRMagicRadialSelectionVM();
                _radialLayer = new GauntletLayer("GauntletLayer", 99, true);
                _radialLayer.LoadMovie("MWRMagicRadialSelection", _radialVm);
                MissionScreen.AddLayer(_radialLayer);
            }
        }

        public override void OnMissionScreenFinalize()
        {
            if (!EnableBattleMagicUi)
            {
                base.OnMissionScreenFinalize();
                return;
            }

            if (EnableMainAgentHook && TaleWorlds.MountAndBlade.Mission.Current != null)
            {
                TaleWorlds.MountAndBlade.Mission.Current.OnMainAgentChanged -= OnMainAgentChanged;
            }

            base.OnMissionScreenFinalize();
        }

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            if (!EnableBattleMagicUi || !EnableMissionTickLogic)
            {
                return;
            }

            MWRMagicAbilityManagerMissionLogic? logic = Mission.GetMissionBehavior<MWRMagicAbilityManagerMissionLogic>();
            Agent? mainAgent = Agent.Main;
            MWRMagicAgentComponent? component = mainAgent?.GetComponent<MWRMagicAgentComponent>();

            if (mainAgent == null || component == null || logic == null || mainAgent.State != AgentState.Active)
            {
                if (_hudVm != null)
                {
                    _hudVm.Refresh(null, null, null);
                }

                if (_radialVm != null)
                {
                    _radialVm.IsVisible = false;
                    _radialVm.Refresh(null, null, _ => { });
                }

                CloseRadial(logic);
                return;
            }

            if (!_isInitializedForMainAgent)
            {
                InitializeForMainAgent(mainAgent, component);
            }

            if (_hudVm != null)
            {
                _hudVm.Refresh(mainAgent, component, logic);
            }

            if (_radialVm != null)
            {
                _radialVm.IsVisible = _radialOpen;
                if (_radialOpen)
                {
                    _radialVm.Refresh(component, logic, index => OnSelectSpell(logic, mainAgent, index));
                }
            }

            if (TaleWorlds.InputSystem.Input.IsKeyPressed(InputKey.Q))
            {
                if (logic.CurrentState == MWRMagicAbilityModeState.Targeting)
                {
                    logic.CancelTargeting();
                    CloseRadial(logic);
                }
                else
                {
                    ToggleRadial(logic);
                }
            }

            if (TaleWorlds.InputSystem.Input.IsKeyPressed(InputKey.F))
            {
                CloseRadial(logic);
                if (!logic.TryCastSelectedSpell(mainAgent, out string failureReason) &&
                    !string.IsNullOrWhiteSpace(failureReason))
                {
                    InformationManager.DisplayMessage(new InformationMessage(failureReason));
                }
            }

            HandleNumberSelection(logic, mainAgent);
        }

        private void OnMainAgentChanged(Agent oldAgent)
        {
            _isInitializedForMainAgent = false;

            Agent? mainAgent = Agent.Main;
            MWRMagicAgentComponent? component = mainAgent?.GetComponent<MWRMagicAgentComponent>();
            if (mainAgent != null && component != null)
            {
                InitializeForMainAgent(mainAgent, component);
            }
            else
            {
                _hudVm?.InitializeSpells(null);
                _radialVm?.InitializeItems(null, null, _ => { });
            }
        }

        private void InitializeForMainAgent(Agent mainAgent, MWRMagicAgentComponent component)
        {
            _hudVm?.InitializeSpells(component);
            _radialVm?.InitializeItems(component, Mission.GetMissionBehavior<MWRMagicAbilityManagerMissionLogic>(), index => OnSelectSpell(Mission.GetMissionBehavior<MWRMagicAbilityManagerMissionLogic>(), mainAgent, index));
            _isInitializedForMainAgent = true;
        }

        private void HandleNumberSelection(MWRMagicAbilityManagerMissionLogic logic, Agent mainAgent)
        {
            if (TaleWorlds.InputSystem.Input.IsKeyPressed(InputKey.D1))
            {
                OnSelectSpell(logic, mainAgent, 0);
            }
            else if (TaleWorlds.InputSystem.Input.IsKeyPressed(InputKey.D2))
            {
                OnSelectSpell(logic, mainAgent, 1);
            }
            else if (TaleWorlds.InputSystem.Input.IsKeyPressed(InputKey.D3))
            {
                OnSelectSpell(logic, mainAgent, 2);
            }
            else if (TaleWorlds.InputSystem.Input.IsKeyPressed(InputKey.D4))
            {
                OnSelectSpell(logic, mainAgent, 3);
            }
        }

        private void OnSelectSpell(MWRMagicAbilityManagerMissionLogic? logic, Agent? mainAgent, int index)
        {
            if (logic == null || mainAgent == null)
            {
                return;
            }

            logic.SelectSpell(mainAgent, index);
            CloseRadial(logic);
        }

        private void ToggleRadial(MWRMagicAbilityManagerMissionLogic? logic)
        {
            if (_radialOpen)
            {
                CloseRadial(logic);
            }
            else
            {
                _radialOpen = true;
                logic?.SetQuickMenuOpen(true);
                if (_radialLayer != null)
                {
                    _radialLayer.IsFocusLayer = true;
                    ScreenManager.TrySetFocus(_radialLayer);
                }
            }
        }

        private void CloseRadial(MWRMagicAbilityManagerMissionLogic? logic)
        {
            _radialOpen = false;
            logic?.SetQuickMenuOpen(false);
            if (_radialLayer != null)
            {
                _radialLayer.IsFocusLayer = false;
                ScreenManager.TryLoseFocus(_radialLayer);
            }
        }

    }
}
