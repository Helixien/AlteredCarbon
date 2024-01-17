using HarmonyLib;
using Verse;

namespace AlteredCarbon
{
    [HarmonyBefore("legodude17.mvcf")]
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.TryGetAttackVerb))]
    public static class Pawn_TryGetAttackVerb_Patch
    {
        public static void Postfix(Pawn __instance, ref Verb __result, Thing target)
        {
            var comp = __result?.HediffCompSource as HediffComp_MeleeWeapon;
            if (comp is null)
            {
                foreach (var hediff in __instance.health.hediffSet.hediffs)
                {
                    comp = hediff.TryGetComp<HediffComp_MeleeWeapon>();
                    if (comp != null)
                    {
                        foreach (var verb in comp.VerbTracker.AllVerbs)
                        {
                            if (verb.IsStillUsableBy(__instance) && (target is null || verb.IsUsableOn(target) && verb.CanHitTarget(target)))
                            {
                                __result = verb;
                                return;
                            }
                        }
                    }
                }
            }
        }
    }
}

