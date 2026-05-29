#nullable enable
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using BannerType = TaleWorlds.Core.Banner;

namespace MountAndWarcraftReborn.Patches.Banner
{
    [HarmonyPatch(typeof(BannerType), nameof(BannerType.TryGetBannerDataFromCode))]
    public static class Banner_TryGetBannerDataFromCode_Patch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction instruction in instructions)
            {
                if (LoadsVanillaBannerIconLimit(instruction))
                {
                    yield return new CodeInstruction(OpCodes.Ldc_I4, MWRBannerPatchSettings.MaxBannerIconCount);
                    continue;
                }

                yield return instruction;
            }
        }

        private static bool LoadsVanillaBannerIconLimit(CodeInstruction instruction)
        {
            if (instruction.opcode == OpCodes.Ldc_I4 && instruction.operand is int intValue)
            {
                return intValue == 32;
            }

            if (instruction.opcode == OpCodes.Ldc_I4_S)
            {
                if (instruction.operand is sbyte sbyteValue)
                {
                    return sbyteValue == 32;
                }

                if (instruction.operand is byte byteValue)
                {
                    return byteValue == 32;
                }

                if (instruction.operand is int operandValue)
                {
                    return operandValue == 32;
                }
            }

            return false;
        }
    }
}
