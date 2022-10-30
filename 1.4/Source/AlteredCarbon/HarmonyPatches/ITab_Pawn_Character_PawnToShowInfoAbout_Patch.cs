using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(ITab_Pawn_Character), "PawnToShowInfoAbout", MethodType.Getter)]
    public static class ITab_Pawn_Character_PawnToShowInfoAbout_Patch
    {
        public static bool Prefix(ref Pawn __result)
        {
            if (Find.Selector.SingleSelectedThing is CorticalStack stack && stack.PersonaData.ContainsInnerPersona)
            {
                __result = stack.PersonaData.GetDummyPawn;
                return false;
            }
            return true;
        }
    }
}

