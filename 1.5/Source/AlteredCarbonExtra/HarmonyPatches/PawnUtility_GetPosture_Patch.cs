﻿using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon
{

    [HarmonyPatch(typeof(PawnUtility), nameof(PawnUtility.GetPosture))]
    public static class PawnUtility_GetPosture_Patch
    {
        public static void Postfix(Pawn p, ref PawnPosture __result)
        {
            if (__result == PawnPosture.LayingMask && (p.ParentHolder is Corpse corpse && corpse.ParentHolder is Building_SleeveGrower 
                || p.ParentHolder is Building_SleeveGrower))
            {
                __result = PawnPosture.Standing;
            }
        }
    }
}
