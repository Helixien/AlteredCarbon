using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    [HotSwappable]
    [HarmonyPatch(typeof(PawnRenderer), "LayingFacing")]
    public static class PawnRenderer_LayingFacing_Patch
    {
        public static bool Prefix(Pawn ___pawn, ref Rot4 __result)
        {
            if (___pawn.CurrentBed() is Building_SleeveCasket bed)
            {
                if (bed.Rotation == Rot4.North)
                {
                    __result = Rot4.South;
                    return false;
                }
            }
            return true;
        }
    }
}