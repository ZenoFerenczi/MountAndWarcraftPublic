#nullable enable
using HarmonyLib;
using System.Reflection;

namespace MountAndWarcraftReborn.Patches.Banner
{
    [HarmonyPatch]
    public static class BannerEditorView_HandleUserInput_Patch
    {
        private static MethodBase? _targetMethod;

        public static bool Prepare()
        {
            _targetMethod = AccessTools.Method("SandBox.GauntletUI.BannerEditor.BannerEditorView:HandleUserInput");
            return _targetMethod != null;
        }

        public static MethodBase TargetMethod()
        {
            return _targetMethod!;
        }

        [HarmonyPostfix]
        public static void Postfix(object __instance)
        {
            MWRBannerClipboardHelper.HandleInput(__instance);
        }
    }
}
