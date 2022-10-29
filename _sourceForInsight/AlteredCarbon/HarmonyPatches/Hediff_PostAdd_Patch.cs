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
            if (__instance.Part?.def == BodyPartDefOf.Neck && __instance is Hediff_MissingPart)
            {
                var stackHediff = __instance.pawn.health.hediffSet.hediffs.FirstOrDefault((Hediff x) => x.def == AC_DefOf.VFEU_CorticalStack) as Hediff_CorticalStack;
                if (stackHediff != null)
                {
                    stackHediff.TryRecoverOrSpawnOnGround();
                    if (!__instance.pawn.Dead)
                    {
                        __instance.pawn.Kill(null);
                    }
                }
            }
        }
    }
}

