using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace AlteredCarbon
{
    public class JobDriver_HaulCorpseToGestation : JobDriver_HaulToContainer
    {
        public Building_SleeveGestator Building_Incubator => TargetA.Thing as Building_SleeveGestator;
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return TargetB.IsValid ? pawn.Reserve(TargetA, job) && pawn.Reserve(TargetB, job) : pawn.Reserve(TargetA, job);
        }
        public override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOn(() => Building_Incubator.incubatorState != IncubatorState.ToBeActivated);
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            yield return Toils_General.DoAtomic(delegate
            {
                job.count = 1;
            });
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch).FailOnDespawnedNullOrForbidden(TargetIndex.B)
                .FailOnSomeonePhysicallyInteracting(TargetIndex.B);
            yield return Toils_Haul.StartCarryThing(TargetIndex.B, false, true, false).FailOnDestroyedNullOrForbidden(TargetIndex.B);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            yield return Toils_General.Wait(10).FailOnDestroyedNullOrForbidden(TargetIndex.B)
                .FailOnDestroyedNullOrForbidden(TargetIndex.A)
                .FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell).WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
            yield return new Toil
            {
                initAction = delegate
                {
                    var corspe = TargetB.Thing as Corpse;
                    corspe.Strip();
                    Building_Incubator.PutCorpseForRepurposing(corspe);
                    Building_Incubator.incubatorState = IncubatorState.Growing;
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            Toil doWork = Toils_General.Wait(60, 0);
            doWork.AddPreTickAction(() =>
            {
                pawn.rotationTracker.FaceCell(TargetThingA.Position);
            });
            doWork.WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
            doWork.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            yield return doWork;
        }
    }
}

