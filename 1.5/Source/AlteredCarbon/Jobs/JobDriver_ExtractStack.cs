using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace AlteredCarbon
{
    public class JobDriver_ExtractStack : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }
        public override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell);
            Toil extractStack = Toils_General.Wait(120, 0);
            ToilEffects.WithProgressBarToilDelay(extractStack, TargetIndex.A, false, -0.5f);
            ToilFailConditions.FailOnDespawnedNullOrForbidden<Toil>(extractStack, TargetIndex.A);
            ToilFailConditions.FailOnCannotTouch<Toil>(extractStack, TargetIndex.A, PathEndMode.OnCell);
            yield return extractStack;
            yield return new Toil
            {
                initAction = delegate ()
                {
                    Corpse corpse = (Corpse)TargetThingA;
                    if (corpse.InnerPawn.HasNeuralStack(out var hediff))
                    {
                        hediff.SpawnStack(placeMode: ThingPlaceMode.Direct);
                        BodyPartRecord head = corpse.InnerPawn.health.hediffSet.GetNotMissingParts().FirstOrDefault((BodyPartRecord x) => x.def == BodyPartDefOf.Head);
                        if (head != null)
                        {
                            Hediff_MissingPart hediff_MissingPart = (Hediff_MissingPart)HediffMaker.MakeHediff(HediffDefOf.MissingBodyPart, corpse.InnerPawn, head);
                            hediff_MissingPart.lastInjury = HediffDefOf.SurgicalCut;
                            hediff_MissingPart.IsFresh = true;
                            corpse.InnerPawn.health.AddHediff(hediff_MissingPart);
                        }
                        if (pawn.Map.designationManager.DesignationOn(corpse)?.def == AC_DefOf.AC_ExtractStackDesignation)
                        {
                            pawn.Map.designationManager.TryRemoveDesignationOn(corpse, AC_DefOf.AC_ExtractStackDesignation);
                        }
                    }
                }
            };
        }
    }
}

