using System.Collections.Generic;
using Verse.AI;

namespace AlteredCarbon
{
    public class JobDriver_CancelRepurposingBody : JobDriver
    {
        public Building_SleeveGestator Building_Incubator => TargetA.Thing as Building_SleeveGestator;
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(TargetA, job);
        }
        public override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOn(() => Building_Incubator.incubatorState != IncubatorState.ToBeCanceled);
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            Toil doWork = Toils_General.Wait(60, 0);
            doWork.AddPreTickAction(() =>
            {
                pawn.rotationTracker.FaceCell(TargetThingA.Position);
            });
            ToilEffects.WithProgressBarToilDelay(doWork, TargetIndex.A, false, -0.5f);
            ToilFailConditions.FailOnDespawnedNullOrForbidden<Toil>(doWork, TargetIndex.A);
            yield return doWork;
            yield return new Toil
            {
                initAction = delegate ()
                {
                    Building_Incubator.CancelBodyRepurposing();
                }
            };
        }
    }
}

