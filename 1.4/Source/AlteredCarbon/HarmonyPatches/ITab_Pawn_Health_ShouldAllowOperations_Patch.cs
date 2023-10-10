using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(ITab_Pawn_Health), "ShouldAllowOperations")]
    public static class ITab_Pawn_Health_ShouldAllowOperations_Patch
    {
        public static void Postfix(ref bool __result, ITab_Pawn_Health __instance)
        {
            if (!__result && __instance.PawnForHealth.Dead is false && __instance.PawnForHealth.IsEmptySleeve()) 
            {
                __result = true;
            }
        }
    }
}

