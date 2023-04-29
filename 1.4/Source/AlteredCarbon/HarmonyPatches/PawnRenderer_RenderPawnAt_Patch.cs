using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    [HotSwappable]
    [HarmonyPatch(typeof(PawnRenderer), "RenderPawnAt")]
    public static class PawnRenderer_RenderPawnAt_Patch
    {
        public static void UseDynamicDrawIfNeeded(Pawn pawn, ref bool flag)
        {
            if (pawn.CurrentBed() is Building_SleeveCasket)
            {
                flag = false;
            }
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            bool found = false;
            var codes = codeInstructions.ToList();
            foreach (var code in codes)
            {
                yield return code;
                if (!found && code.opcode == OpCodes.Stloc_3)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PawnRenderer), "pawn"));
                    yield return new CodeInstruction(OpCodes.Ldloca, 3);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PawnRenderer_RenderPawnAt_Patch), nameof(UseDynamicDrawIfNeeded)));
                    found = true;
                }
            }
            if (!found)
            {
                Log.Error("AlteredCarbon failed to transpile PawnRenderer.RenderPawnAt");
            }
        }

    }
}