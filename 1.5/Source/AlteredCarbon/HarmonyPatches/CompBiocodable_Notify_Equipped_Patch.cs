using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(CompBiocodable), "Notify_Equipped")]
    public static class CompBiocodable_Notify_Equipped_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var methodInfo = AccessTools.Method(typeof(CompBiocodable_Notify_Equipped_Patch), nameof(ShouldBiocodeOnEquip));
            foreach (var instruction in instructions)
            {
                yield return instruction;

                if (instruction.opcode == OpCodes.Ldfld && instruction.operand.ToString().Contains("biocodeOnEquip"))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, methodInfo);
                }
            }
        }

        public static bool ShouldBiocodeOnEquip(bool shouldEquip, CompBiocodable comp)
        {
            if (CompBiocodable_PostExposeData_Patch.wasBiocoded.TryGet(comp, out var wasBiocoded) && wasBiocoded)
            {
                return true;
            }
            return shouldEquip;
        }
    }
}

