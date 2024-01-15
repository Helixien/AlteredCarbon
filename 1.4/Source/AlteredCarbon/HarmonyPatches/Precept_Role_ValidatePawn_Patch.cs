using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(Precept_Role), "ValidatePawn")]
    public static class Precept_Role_ValidatePawn_Patch
    {
        public static void Prefix(ref Pawn p)
        {
            if (p.HasCorticalStack(out var hediff_CorticalStack) && (p.Dead || p.Destroyed))
            {
                p = hediff_CorticalStack.PersonaData.GetDummyPawn;
            }
        }
    }
}

