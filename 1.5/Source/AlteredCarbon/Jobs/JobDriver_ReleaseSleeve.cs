using System.Collections.Generic;
using Verse.AI;

namespace AlteredCarbon
{
    public class JobDriver_ReleaseSleeve : JobDriver
    {
        public JobDriver_ReleaseSleeve()
        {
            rotateToFace = TargetIndex.A;
        }
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }
        public override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Reserve.Reserve(TargetIndex.A, 1, -1, null);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            Toils_General.Wait(120).WithProgressBarToilDelay(TargetIndex.A).FailOnDespawnedOrNull(TargetIndex.A)
                .FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
            yield return new Toil
            {
                initAction = delegate ()
                {
                    Building_SleeveGestator grower = (Building_SleeveGestator)TargetA.Thing;
                    grower.EjectContents();
                }
            };
            yield break;
        }
    }
}

