using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.ScreenSystem;

namespace MountAndWarcraftReborn.Magic.UI
{
    [GameStateScreen(typeof(MWRSpellBookState))]
    public class MWRSpellBookScreen : ScreenBase, IGameStateListener
    {
        private readonly MWRSpellBookState _state;
        private GauntletLayer? _gauntletLayer;
        private MWRSpellBookVM? _vm;

        public MWRSpellBookScreen(MWRSpellBookState state)
        {
            _state = state;
            _state.RegisterListener(this);
        }

        protected override void OnFrameTick(float dt)
        {
            base.OnFrameTick(dt);
            LoadingWindow.DisableGlobalLoadingWindow();

            if (_gauntletLayer != null && _gauntletLayer.Input.IsHotKeyReleased("Exit"))
            {
                CloseScreen();
            }
        }

        void IGameStateListener.OnActivate()
        {
            _vm = new MWRSpellBookVM(CloseScreen);
            _gauntletLayer = new GauntletLayer("GauntletLayer", 1, true);
            _gauntletLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
            _gauntletLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericCampaignPanelsGameKeyCategory"));
            _gauntletLayer.LoadMovie("MWRSpellBook", _vm);
            _gauntletLayer.IsFocusLayer = true;
            AddLayer(_gauntletLayer);
            ScreenManager.TrySetFocus(_gauntletLayer);
        }

        void IGameStateListener.OnDeactivate()
        {
            if (_gauntletLayer == null)
            {
                return;
            }

            RemoveLayer(_gauntletLayer);
            _gauntletLayer.IsFocusLayer = false;
            ScreenManager.TryLoseFocus(_gauntletLayer);
        }

        void IGameStateListener.OnFinalize()
        {
            _gauntletLayer = null;
            _vm = null;
        }

        void IGameStateListener.OnInitialize()
        {
        }

        private static void CloseScreen()
        {
            Game.Current?.GameStateManager?.PopState(0);
        }
    }
}
