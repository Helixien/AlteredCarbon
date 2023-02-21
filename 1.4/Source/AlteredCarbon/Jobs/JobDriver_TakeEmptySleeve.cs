using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace AlteredCarbon
{
    public class JobDriver_TakeEmptySleeve : JobDriver
    {
        protected Pawn Takee => (Pawn)job.GetTarget(TargetIndex.A).Thing;
        protected Building_Bed DropBed => (Building_Bed)job.GetTarget(TargetIndex.B).Thing;
        private bool TakeeRescued
        {
            get
            {
                if (Takee.RaceProps.Humanlike && job.def != JobDefOf.Arrest && !Takee.IsPrisonerOfColony)
                {
                    if (Takee.ageTracker.CurLifeStage.alwaysDowned)
                    {
                        return HealthAIUtility.ShouldSeekMedicalRest(Takee);
                    }
                    return true;
                }
                return false;
            }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (pawn.Reserve(Takee, job, 1, -1, null, errorOnFailed))
            {
                return pawn.Reserve(DropBed, job, DropBed.SleepingSlotsCount, 0, null, errorOnFailed);
            }
            return false;
        }

        public override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOnDestroyedOrNull(TargetIndex.B);
            this.FailOnAggroMentalStateAndHostile(TargetIndex.A);
            yield return Toils_Bed.ClaimBedIfNonMedical(TargetIndex.B, TargetIndex.A);
            Toil goToTakee = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.A).FailOnDespawnedNullOrForbidden(TargetIndex.B)
                .FailOn(() => job.def == JobDefOf.Arrest && !Takee.CanBeArrestedBy(pawn))
                .FailOn(() => !pawn.CanReach(DropBed, PathEndMode.OnCell, Danger.Deadly))
                .FailOn(() => !Takee.Downed)
                .FailOnSomeonePhysicallyInteracting(TargetIndex.A);
            yield return goToTakee;
            Toil startCarrying = Toils_Haul.StartCarryThing(TargetIndex.A);
            startCarrying.FailOnBedNoLongerUsable(TargetIndex.B, TargetIndex.A);
            Toil goToBed = Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch).FailOn(() => !pawn.IsCarryingPawn(Takee));
            goToBed.FailOnBedNoLongerUsable(TargetIndex.B, TargetIndex.A);
            yield return Toils_Jump.JumpIf(goToBed, () => pawn.IsCarryingPawn(Takee));
            yield return startCarrying;
            yield return goToBed;
            yield return Toils_Reserve.Release(TargetIndex.B);
            yield return Toils_Bed.TuckIntoBed(DropBed, pawn, Takee, TakeeRescued);
        }
    }
}