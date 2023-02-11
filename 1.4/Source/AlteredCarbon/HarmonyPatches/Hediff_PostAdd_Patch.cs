using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(Hediff), "PostAdd")]
    public static class Hediff_PostAdd_Patch
    {
        public static void Prefix(Hediff __instance, DamageInfo? dinfo)
        {
            if (__instance.Part?.def == BodyPartDefOf.Neck && __instance is Hediff_MissingPart 
                && __instance.pawn.HasCorticalStack(out var hediff))
            {
                if (Rand.Chance(0.25f))
                {
                    hediff.SpawnStack();
                }
                if (!__instance.pawn.Dead)
                {
                    __instance.pawn.Kill(null);
                }
            }
        }
    }
}

