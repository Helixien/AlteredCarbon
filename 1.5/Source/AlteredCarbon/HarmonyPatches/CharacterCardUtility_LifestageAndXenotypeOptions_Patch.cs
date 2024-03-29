using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(CharacterCardUtility), "LifestageAndXenotypeOptions")]
    public static class CharacterCardUtility_LifestageAndXenotypeOptions_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var codes = codeInstructions.ToList();
            var get_AllDefsInfo = AccessTools.PropertyGetter(typeof(DefDatabase<XenotypeDef>), "AllDefs");
            foreach (var code in codes)
            {
                yield return code;
                if (code.Calls(get_AllDefsInfo))
                {
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(CharacterCardUtility_LifestageAndXenotypeOptions_Patch),
                        nameof(FilterXenotypes)));
                }
            }
        }

        public static IEnumerable<XenotypeDef> FilterXenotypes(IEnumerable<XenotypeDef> list)
        {
            return list.Where(x => x != AC_DefOf.AC_Sleeveliner);
        }
    }
}

