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
            if (p.HasPersonaStack(out var hediff_PersonaStack) && (p.Dead || p.Destroyed))
            {
                p = hediff_PersonaStack.PersonaData.GetDummyPawn;
            }
        }
    }
}

