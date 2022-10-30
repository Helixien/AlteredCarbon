using HarmonyLib;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(PawnNameColorUtility), "PawnNameColorOf")]
    public static class PawnNameColorUtility_PawnNameColorOf_Patch
    {
        private static void Postfix(ref Color __result, Pawn pawn)
        {
            if (pawn.IsEmptySleeve())
            {
                __result = Color.green;
            }
        }
    }
}

