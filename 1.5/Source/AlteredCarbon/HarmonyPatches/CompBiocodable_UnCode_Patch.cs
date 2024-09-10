using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch]
    public static class CompBiocodable_UnCode_Patch
    {
        public static Pawn disableKillEffect;
        [HarmonyTargetMethods]
        public static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(CompBladelinkWeapon), nameof(CompBladelinkWeapon.UnCode));
            yield return AccessTools.Method(typeof(CompBiocodable), nameof(CompBiocodable.UnCode));
        }

        public static bool Prefix(CompBiocodable __instance)
        {
            if (__instance.CodedPawn != null && disableKillEffect == __instance.CodedPawn)
            {
                return false;
            }
            return true;
        }
    }
}

