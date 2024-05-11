using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(Pawn_MeleeVerbs), "TryGetMeleeVerb")]
    public static class Pawn_MeleeVerbs_TryGetMeleeVerb_Patch
    {
        public static bool Prefix(Pawn_MeleeVerbs __instance, Thing target, ref Verb __result)
        {
            if (__instance.pawn?.equipment?.Primary?.def.IsMeleeWeapon ?? false)
            {
                return true;
            }
            if (__instance.curMeleeVerb != null && __instance.curMeleeVerb.HediffCompSource is HediffComp_MeleeWeapon hediffComp)
            {
                if (target is null)
                {
                    if (Find.TickManager.TicksGame >= __instance.curMeleeVerbUpdateTick + 60)
                    {
                        var verb = GetMeleeWeaponVerb(__instance.pawn, __instance.curMeleeVerbTarget, hediffComp);
                        if (verb != null)
                        {
                            __instance.SetCurMeleeVerb(verb, __instance.curMeleeVerbTarget);
                            __result = verb;
                            return false;
                        }
                    }
                    __result = __instance.curMeleeVerb;
                    return false;
                }
                if (Find.TickManager.TicksGame >= __instance.curMeleeVerbUpdateTick + 60 || target != null && target != __instance.curMeleeVerbTarget
                    || !__instance.curMeleeVerb.IsStillUsableBy(__instance.pawn) || !__instance.curMeleeVerb.IsUsableOn(target))
                {
                    Verb verb;
                    if (target == __instance.curMeleeVerbTarget)
                    {
                        verb = GetMeleeWeaponVerb(__instance.pawn, target, hediffComp);
                        if (verb != null)
                        {
                            __instance.SetCurMeleeVerb(verb, target);
                            __result = verb;
                            return false;
                        }
                        else
                        {
                            verb = GetMeleeWeaponVerb(__instance.pawn, target);
                            if (verb != null)
                            {
                                __instance.SetCurMeleeVerb(verb, target);
                                __result = verb;
                                return false;
                            }
                        }
                    }
                    else
                    {
                        verb = GetMeleeWeaponVerb(__instance.pawn, target);
                        if (verb != null)
                        {
                            __instance.SetCurMeleeVerb(verb, target);
                            __result = verb;
                            return false;
                        }
                    }
                }
                else
                {
                    //if (Find.Selector.SelectedPawns.Contains(__instance.pawn))
                    //    Log.Message("Keeping verb: " + __instance.curMeleeVerb + " for " + target + " - " + __instance.curMeleeVerbTarget);
                    __result = __instance.curMeleeVerb;
                    return false;
                }
            }
            else
            {
                var verb = GetMeleeWeaponVerb(__instance.pawn, target);
               if (verb != null)
                {
                    __instance.SetCurMeleeVerb(verb, target);
                    __result = verb;
                    return false;
                }
            }
            //if (Find.Selector.SelectedPawns.Contains(__instance.pawn))
            //    Log.Message("Keeping verb 2: " + __instance.curMeleeVerb + " for " + target + " - " + __instance.curMeleeVerbTarget);

            return true;
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
                            verbs.Add(new VerbEntry(verb, __instance, comp.VerbTracker.AllVerbs, 1));
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

        public static Verb GetMeleeWeaponVerb(Pawn __instance, Thing target, HediffComp_MeleeWeapon comp)
        {
            var verbs = new List<VerbEntry>();
            foreach (var verb in comp.VerbTracker.AllVerbs)
            {
                if (__instance.IsUsable(target, verb))
                {
                    verbs.Add(new VerbEntry(verb, __instance, comp.VerbTracker.AllVerbs, 1));
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
            return verb.IsStillUsableBy(__instance) && (target is null || verb.IsUsableOn(target));
        }
    }
}

