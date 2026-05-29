using HarmonyLib;
using MountAndWarcraftReborn.GameManagers;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.MountAndBlade;

namespace MountAndWarcraftReborn.Patches.MainMenu
{
    [HarmonyPatch(typeof(Module), nameof(Module.GetInitialStateOptions))]
    public static class MWRMainMenuOptionsPatch
    {
        [HarmonyPostfix]
        private static void Postfix(ref IEnumerable<InitialStateOption> __result)
        {
            List<InitialStateOption> options = __result?.ToList() ?? new List<InitialStateOption>();
            int sandboxIndex = options.FindIndex(option => option.Id == "SandBoxNewGame");
            if (sandboxIndex < 0)
            {
                return;
            }

            InitialStateOption sandboxOption = options[sandboxIndex];
            options[sandboxIndex] = new InitialStateOption(
                sandboxOption.Id,
                sandboxOption.Name,
                sandboxOption.OrderIndex,
                OnSandboxClicked,
                sandboxOption.IsDisabledAndReason,
                sandboxOption.EnabledHint,
                sandboxOption.IsHidden);

            __result = options.OrderBy(option => option.OrderIndex).ToList();
        }

        private static void OnSandboxClicked()
        {
            MBGameManager.StartNewGame(new MWRSandboxGameManager());
        }
    }
}
