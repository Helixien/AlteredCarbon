using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon;

[HarmonyPatch(typeof(Pawn_RelationsTracker), "SecondaryRomanceChanceFactor")]
public class Pawn_RelationsTracker_SecondaryRomanceChanceFactor_Patch
{
    public static void Postfix(Pawn otherPawn, ref float __result)
    {
        if (otherPawn.health.hediffSet.HasHediff(AC_DefOf.AC_VoiceSynthesizer))
        {
            __result *= 1.15f;
        }
    }
}