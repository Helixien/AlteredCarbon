using HarmonyLib;
using RimWorld;
using System.Linq;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(TerrorUtility), "GetTerrorLevel")]
    public static class TerrorUtility_GetTerrorLevel_Patch
    {
        public static void Postfix(Pawn pawn, ref float __result)
        {
            if (pawn.health.hediffSet.hediffs.OfType<Hediff_CortexOverseer>().Any(x => x.activated))
            {
                __result = 1f;
            }
        }
    }
}

