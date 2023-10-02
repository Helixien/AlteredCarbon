using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{
    [HotSwappable]
    [HarmonyPatch(typeof(ITab_Pawn_Character), "PawnToShowInfoAbout", MethodType.Getter)]
    public static class ITab_Pawn_Character_PawnToShowInfoAbout_Patch
    {
        public static int lastTimeUpdated;
        public static Pawn lastPawn;
        public static bool Prefix(ref Pawn __result)
        {
            if (Find.Selector.SingleSelectedThing is CorticalStack stack && stack.PersonaData.ContainsInnerPersona)
            {
                if (__result != lastPawn)
                {
                    lastPawn = __result;
                    stack.PersonaData.RefreshDummyPawn();
                }
                else if (Time.frameCount - lastTimeUpdated >= 60)
                {
                    lastTimeUpdated = Time.frameCount;
                    stack.PersonaData.RefreshDummyPawn();
                }
                __result = stack.PersonaData.GetDummyPawn;
                return false;
            }
            return true;
        }
    }
}

