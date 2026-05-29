using TaleWorlds.Core;

namespace MountAndWarcraftReborn.Magic.UI
{
    public static class MWRMagicScreenHelper
    {
        public static void OpenSpellBook()
        {
            if (Game.Current?.GameStateManager == null)
            {
                return;
            }

            if (Game.Current.GameStateManager.ActiveState is MWRSpellBookState)
            {
                return;
            }

            MWRSpellBookState state = Game.Current.GameStateManager.CreateState<MWRSpellBookState>();
            Game.Current.GameStateManager.PushState(state);
        }
    }
}
