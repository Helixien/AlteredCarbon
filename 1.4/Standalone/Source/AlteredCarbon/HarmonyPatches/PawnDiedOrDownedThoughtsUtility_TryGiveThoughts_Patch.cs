using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(PawnDiedOrDownedThoughtsUtility), "TryGiveThoughts", new Type[] { typeof(Pawn), typeof(DamageInfo?), typeof(PawnDiedOrDownedThoughtsKind) } )]
    internal static class PawnDiedOrDownedThoughtsUtility_TryGiveThoughts_Patch
    {
        private static bool Prefix(Pawn victim, DamageInfo? dinfo, PawnDiedOrDownedThoughtsKind thoughtsKind)
        {
            if (victim.IsEmptySleeve())
            {
                return false;
            }
            return true;
        }
    }
}

