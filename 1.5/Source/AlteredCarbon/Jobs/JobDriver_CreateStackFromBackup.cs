using System.Collections.Generic;
using Verse.AI;

namespace AlteredCarbon
{
    public class JobDriver_CreateStackFromBackup : JobDriver
    {
        public const int RestoringDuration = 1000;
        public Building_NeuralEditor Building_NeuralMatrix => TargetA.Thing as Building_NeuralEditor;
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(TargetA, job);
        }
        public override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOn(() => !Building_NeuralMatrix.Powered || GameComponent_DigitalStorage.Instance.FirstPersonaStackToRestore(Map) is null);
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.B)
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
                    GameComponent_DigitalStorage.Instance.PerformStackRestoration(pawn, (TargetB.Thing as MindFrame).PersonaData);
                    job.targetB.Thing.Destroy();
                }
            };
        }
    }
}

