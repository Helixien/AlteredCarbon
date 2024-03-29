using HarmonyLib;
using System;
using Verse;
using Verse.AI;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(Pawn_PathFollower), "CostToMoveIntoCell", new Type[] { typeof(Pawn), typeof(IntVec3) })]
    public static class Pawn_PathFollower_CostToMoveIntoCell_Patch
    {
        public static void Postfix(Pawn pawn, IntVec3 c, ref float __result)
        {
            if (pawn.Map != null && pawn.Wears(AC_DefOf.AC_Apparel_DragoonHelmet) && pawn.CanPassOver(c))
            {
                __result = pawn.GetPawnBasePathCost(c);
            }
        }
    }
}