using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(Dialog_BillConfig), "DoWindowContents")]
    public static class Dialog_BillConfig_DoWindowContents_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var codes = codeInstructions.ToList();
            var workAmountTotalGet = AccessTools.Method(typeof(RecipeDef), nameof(RecipeDef.WorkAmountTotal));
            var billField = AccessTools.Field(typeof(Dialog_BillConfig), "bill");
            foreach (var code in codes)
            {
                yield return code;
                if (code.Calls(workAmountTotalGet))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, billField);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Dialog_BillConfig_DoWindowContents_Patch), nameof(GetWorkAmount)));
                }
            }
        }

        public static float GetWorkAmount(float workAmount, Bill bill)
        {
            if (bill is Bill_EditStack bill_EditStack)
            {
                return bill_EditStack.GetWorkAmount();
            }
            return workAmount;
        }
    }
}

