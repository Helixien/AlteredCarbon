using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(HediffComp_Immunizable), "SeverityChangePerDay")]
    public static class HediffComp_Immunizable_SeverityChangePerDay_Patch
    {
        public static bool Prefix(HediffComp_Immunizable __instance)
        {
            if (__instance.Pawn.HasHediff(AC_DefOf.AC_CryptoStasis) && __instance.FullyImmune is false)
            {
                return false;
            }
            return true;
        }
    }
}

