using HarmonyLib;
using TaleWorlds.MountAndBlade.GauntletUI.BodyGenerator;

namespace MWRMode.Patches.CharacterCreation
{
    [HarmonyPatch(typeof(BodyGeneratorView), "OnFinalize")]
    public static class BodyGeneratorView_OnFinalize_RaceSelectorReset_Patch
    {
        [HarmonyPostfix]
        private static void Postfix()
        {
            FaceGenVM_RaceSelector_Patch.ResetCache();
        }
    }
}