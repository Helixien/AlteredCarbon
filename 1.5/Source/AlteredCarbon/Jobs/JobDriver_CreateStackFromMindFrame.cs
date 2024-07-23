using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace AlteredCarbon
{
    public class JobDriver_CreateStackFromMindFrame : JobDriver
    {
        public const int RestoringDuration = 1000;
        public Building_NeuralEditor Building_NeuralEditor => TargetA.Thing as Building_NeuralEditor;
        public MindFrame MindFrame => TargetC.Thing as MindFrame;
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(TargetA, job) && pawn.Reserve(TargetB, job) && (TargetC.HasThing is false || pawn.Reserve(TargetC, job));
        }

        public override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedNullOrForbidden(TargetIndex.A);
            this.FailOnDestroyedNullOrForbidden(TargetIndex.B);
            this.FailOn(() => !Building_NeuralEditor.Powered || Building_NeuralEditor.mindFrameToRestore 
            != MindFrame && MindFrame.CanAutoRestorePawn is false);
            if (TargetC.Thing.Spawned)
            {
                yield return Toils_Goto.GotoThing(TargetIndex.C, PathEndMode.ClosestTouch)
                .FailOnDespawnedNullOrForbidden(TargetIndex.C)
                .FailOnSomeonePhysicallyInteracting(TargetIndex.C);
                yield return Toils_Haul.StartCarryThing(TargetIndex.C);
                yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
                yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.A, null, storageMode: false);
                this.FailOnDestroyedNullOrForbidden(TargetIndex.C);
            }

            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch)
                .FailOnDespawnedNullOrForbidden(TargetIndex.B)
                .FailOnSomeonePhysicallyInteracting(TargetIndex.B);
            yield return Toils_Haul.StartCarryThing(TargetIndex.B);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            Toil restoreStack = Toils_General.Wait(RestoringDuration, 0);
            restoreStack.AddPreTickAction(() =>
            {
                pawn.rotationTracker.FaceCell(TargetThingA.Position);
            });
            ToilEffects.WithProgressBarToilDelay(restoreStack, TargetIndex.A, false, -0.5f);
            ToilFailConditions.FailOnDespawnedNullOrForbidden<Toil>(restoreStack, TargetIndex.A);
            yield return restoreStack.WithEffect(AC_DefOf.AC_Hacking, TargetIndex.A);
            yield return new Toil
            {
                initAction = delegate ()
                {
                    var mindFrame = TargetC.Thing as MindFrame;
                    Building_NeuralEditor.PerformStackRestoration(pawn, mindFrame, Building_NeuralEditor.ConnectedMatrix);
                    job.targetB.Thing.Destroy();
                }
            };
        }
    }
}

