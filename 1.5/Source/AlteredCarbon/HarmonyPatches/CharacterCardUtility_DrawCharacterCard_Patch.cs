using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(CharacterCardUtility), "DrawCharacterCard")]
    public static class CharacterCardUtility_DrawCharacterCard_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var deadInfo = AccessTools.PropertyGetter(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.Dead));
            var shouldPreventButtonsInfo = AccessTools.Method(typeof(CharacterCardUtility_DrawCharacterCard_Patch), "ShouldPreventButtons"); 
            var codes = codeInstructions.ToList();
            for (int i = 0; i < codes.Count; i++ )
            {
                var code = codes[i];
                yield return code;
                if (i > 0 && codes[i - 1].Calls(deadInfo))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Call, shouldPreventButtonsInfo);
                    yield return new CodeInstruction(OpCodes.Brtrue, code.operand);
                }
            }
        }

        public static bool ShouldPreventButtons(Pawn pawn)
        {
            return NeuralData.lastDummyPawn == pawn;
        }
    }
}

