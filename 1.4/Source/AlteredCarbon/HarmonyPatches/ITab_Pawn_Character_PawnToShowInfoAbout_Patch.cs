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
        public static bool Prefix(ref Pawn __result)
        {
            if (Find.Selector.SingleSelectedThing is CorticalStack stack && stack.PersonaData.ContainsInnerPersona)
            {
                __result = stack.PersonaData.GetDummyPawn;
                if (Time.frameCount - lastTimeUpdated >= 60)
                {
                    lastTimeUpdated = Time.frameCount;
                    stack.PersonaData.RefreshDummyPawn();
                }
                return false;
            }
            return true;
        }
    }
}

