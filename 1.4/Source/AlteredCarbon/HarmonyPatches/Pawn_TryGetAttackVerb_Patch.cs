using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
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
                var verbs = new List<VerbEntry>();
                foreach (var hediff in __instance.health.hediffSet.hediffs)
                {
                    comp = hediff.TryGetComp<HediffComp_MeleeWeapon>();
                    if (comp != null)
                    {
                        foreach (var verb in comp.VerbTracker.AllVerbs)
                        {
                            if (verb.IsStillUsableBy(__instance) && (target is null || verb.IsUsableOn(target) && verb.CanHitTarget(target)))
                            {
                                verbs.Add(new VerbEntry(verb, __instance));
                            }
                        }
                    }
                }

                if (verbs.TryRandomElementByWeight((VerbEntry ve) => ve.GetSelectionWeight(target), out var result))
                {
                    __result = result.verb;
                }
            }
        }
    }
}

