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
                var verb = GetMeleeWeaponVerb(__instance, target);
                if (verb != null)
                {
                    __result = verb;
                }
            }
        }

        public static Verb GetMeleeWeaponVerb(Pawn __instance, Thing target)
        {
            var verbs = new List<VerbEntry>();
            foreach (var hediff in __instance.health.hediffSet.hediffs)
            {
                var comp = hediff.TryGetComp<HediffComp_MeleeWeapon>();
                if (comp != null)
                {
                    foreach (var verb in comp.VerbTracker.AllVerbs)
                    {
                        if (__instance.IsUsable(target, verb))
                        {
                            verbs.Add(new VerbEntry(verb, __instance));
                        }
                    }
                }
            }

            if (verbs.TryRandomElementByWeight((VerbEntry ve) => ve.GetSelectionWeight(target), out var result))
            {
                return result.verb;
            }
            return null;
        }

        public static bool IsUsable(this Pawn __instance, Thing target, Verb verb)
        {
            return verb.IsStillUsableBy(__instance) && (target is null || verb.IsUsableOn(target) && verb.CanHitTarget(target));
        }
    }
}

