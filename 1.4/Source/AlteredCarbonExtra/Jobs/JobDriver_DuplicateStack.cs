using System.Collections.Generic;
using Verse.AI;

namespace AlteredCarbon
{
    public class JobDriver_DuplicateStack : JobDriver
    {
        public const int DuplicateDuration = 1000;
        public Building_StackStorage Building_StackStorage => TargetA.Thing as Building_StackStorage;
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(TargetA, job);
        }
        public override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOn(() => !Building_StackStorage.CanDuplicateStack);
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
            yield return Toils_Haul.StartCarryThing(TargetIndex.B);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            Toil copyStack = Toils_General.Wait(DuplicateDuration, 0);
            copyStack.AddPreTickAction(() =>
            {
                pawn.rotationTracker.FaceCell(TargetThingA.Position);
            });
            ToilEffects.WithProgressBarToilDelay(copyStack, TargetIndex.A, false, -0.5f);
            ToilFailConditions.FailOnDespawnedNullOrForbidden<Toil>(copyStack, TargetIndex.A);
            yield return copyStack.WithEffect(AC_Extra_DefOf.VFEU_Hacking, TargetIndex.A);

            yield return new Toil
            {
                initAction = delegate ()
                {
                    Building_StackStorage.PerformStackDuplication(pawn);
                    TargetB.Thing.Destroy();
                }
            };
        }
    }
}

