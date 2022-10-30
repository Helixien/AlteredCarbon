using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(Pawn), "Kill")]
    public class Pawn_Kill_Patch
    {
        public static void Prefix(out Caravan __state, Pawn __instance, DamageInfo? dinfo, Hediff exactCulprit = null)
        {
            __state = null;
            try
            {
                if (dinfo.HasValue && dinfo.Value.Def == DamageDefOf.Crush && dinfo.Value.Category == DamageInfo.SourceCategory.Collapse)
                {
                    return;
                }
                if (__instance != null && (__instance.HasStack() || __instance.IsEmptySleeve()))
                {
                    __instance.DisableKilledEffects();
                }
                var stackHediff = __instance.health.hediffSet.GetFirstHediffOfDef(AC_DefOf.VFEU_CorticalStack) as Hediff_CorticalStack;
                if (stackHediff != null)
                {
                    if (dinfo.HasValue && dinfo.Value.Def.ExternalViolenceFor(__instance))
                    {
                        stackHediff.PersonaData.diedFromCombat = true;
                    }
                    LessonAutoActivator.TeachOpportunity(AC_DefOf.VFEU_DeadPawnWithStack, __instance, OpportunityType.Important);
                    AlteredCarbonManager.Instance.deadPawns.Add(__instance);
                    __state = __instance.GetCaravan();
                }
                if (AlteredCarbonManager.Instance.StacksIndex.TryGetValue(__instance.thingIDNumber, out var corticalStack))
                {
                    if (LookTargets_Patch.targets.TryGetValue(__instance, out var targets))
                    {
                        foreach (var target in targets)
                        {
                            target.targets.Remove(__instance);
                            target.targets.Add(corticalStack);
                        }
                    }
                }
            }
            catch { };
        }
        public static void Postfix(Caravan __state, Pawn __instance, DamageInfo? dinfo, Hediff exactCulprit = null)
        {
            if (__state != null && __state.PawnsListForReading.Any())
            {
                var stackHediff = __instance.health.hediffSet.GetFirstHediffOfDef(AC_DefOf.VFEU_CorticalStack) as Hediff_CorticalStack;
                if (stackHediff.def.spawnThingOnRemoved != null)
                {
                    var corticalStackThing = ThingMaker.MakeThing(stackHediff.def.spawnThingOnRemoved) as CorticalStack;
                    if (stackHediff.PersonaData.ContainsInnerPersona)
                    {
                        corticalStackThing.PersonaData.CopyDataFrom(stackHediff.PersonaData);
                    }
                    else
                    {
                        corticalStackThing.PersonaData.CopyPawn(__instance);
                    }
                    AlteredCarbonManager.Instance.RegisterStack(corticalStackThing);
                    AlteredCarbonManager.Instance.RegisterSleeve(__instance, corticalStackThing.PersonaData.stackGroupID);
                    CaravanInventoryUtility.GiveThing(__state, corticalStackThing);
                }
                var head = __instance.health.hediffSet.GetNotMissingParts().FirstOrDefault((BodyPartRecord x) => x.def == BodyPartDefOf.Head);
                if (head != null)
                {
                    Hediff_MissingPart hediff_MissingPart = (Hediff_MissingPart)HediffMaker.MakeHediff(HediffDefOf.MissingBodyPart, __instance, head);
                    hediff_MissingPart.lastInjury = HediffDefOf.SurgicalCut;
                    hediff_MissingPart.IsFresh = true;
                    __instance.health.AddHediff(hediff_MissingPart);
                }
                __instance.health.RemoveHediff(stackHediff);
            }
        }

    }
}

