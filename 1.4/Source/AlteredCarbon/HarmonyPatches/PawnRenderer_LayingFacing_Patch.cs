using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(PawnRenderer), "LayingFacing")]
    public static class PawnRenderer_LayingFacing_Patch
    {
        public static bool Prefix(Pawn ___pawn, ref Rot4 __result)
        {
            if (___pawn.CurrentBed() is Building_SleeveCasket bed)
            {
                __result = bed.Rotation;
                return false;
            }
            return true;
        }
    }
}