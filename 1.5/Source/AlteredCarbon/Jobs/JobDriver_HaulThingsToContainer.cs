using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace AlteredCarbon
{
    public class JobDriver_HaulThingsToContainer : JobDriver_HaulToContainer
    {
        public override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOnDestroyedNullOrForbidden(TargetIndex.B);
            this.FailOn(delegate
            {
                ThingOwner thingOwner = Container.TryGetInnerInteractableThingOwner();
                if (thingOwner != null && !thingOwner.CanAcceptAnyOf(ThingToCarry))
                {
                    return true;
                }
                IHaulDestination haulDestination = Container as IHaulDestination;
                return (haulDestination != null && !haulDestination.Accepts(ThingToCarry)) ? true : false;
            });
            Toil getToHaulTarget = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
            Toil uninstallIfMinifiable = Toils_Construct.UninstallIfMinifiable(TargetIndex.A).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
            Toil startCarryingThing = Toils_Haul.StartCarryThing(TargetIndex.A, putRemainderInQueue: false, subtractNumTakenFromJobCount: true);
            Toil jumpIfAlsoCollectingNextTarget = Toils_Haul.JumpIfAlsoCollectingNextTargetInQueue(getToHaulTarget, TargetIndex.A);
            Toil carryToContainer = Toils_Haul.CarryHauledThingToContainer();
            yield return Toils_Jump.JumpIf(jumpIfAlsoCollectingNextTarget, () => pawn.IsCarryingThing(ThingToCarry));
            yield return getToHaulTarget;
            yield return uninstallIfMinifiable;
            yield return startCarryingThing;
            yield return jumpIfAlsoCollectingNextTarget;
            yield return carryToContainer;
            yield return Toils_Goto.MoveOffTargetBlueprint(TargetIndex.B);
            Toil toil = Toils_General.Wait(Duration, TargetIndex.B);
            toil.WithProgressBarToilDelay(TargetIndex.B);
            EffecterDef workEffecter = WorkEffecter;
            if (workEffecter != null)
            {
                toil.WithEffect(workEffecter, TargetIndex.B);
            }
            SoundDef workSustainer = WorkSustainer;
            if (workSustainer != null)
            {
                toil.PlaySustainerOrSound(workSustainer);
            }
            Thing destThing = job.GetTarget(TargetIndex.B).Thing;
            ModifyPrepareToil(toil);
            yield return toil;
            yield return Toils_Construct.MakeSolidThingFromBlueprintIfNecessary(TargetIndex.B, TargetIndex.C);
            yield return Toils_Haul.DepositHauledThingInContainer(TargetIndex.B, TargetIndex.C, delegate
            {
                var comp = Container.TryGetComp<CompThingContainer>();
                if (comp != null)
                {
                    MoteMaker.ThrowText(Container.DrawPos, pawn.Map, "InsertedThing".Translate($"{comp.innerContainer.Count} / " +
                        $"{comp.Props.stackLimit}"));
                }

            });
            yield return Toils_Haul.JumpToCarryToNextContainerIfPossible(carryToContainer, TargetIndex.C);
        }
    }
}

