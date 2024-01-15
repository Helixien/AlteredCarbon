using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;

namespace AlteredCarbon
{
    [HarmonyPatch]
    public static class CompBiocodable_UnCode_Patch
    {
        [HarmonyTargetMethods]
        public static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(CompBladelinkWeapon), nameof(CompBladelinkWeapon.UnCode));
            yield return AccessTools.Method(typeof(CompBiocodable), nameof(CompBiocodable.UnCode));
        }

        public static bool Prefix(CompBiocodable __instance)
        {
            if (__instance.CodedPawn != null && __instance.CodedPawn.HasStackInsideOrOutside())
            {
                return false;
            }
            return true;
        }
    }
}

