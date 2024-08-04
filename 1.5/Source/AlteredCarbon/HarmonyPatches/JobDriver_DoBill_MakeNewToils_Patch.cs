﻿using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Verse;
using Verse.AI;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(WorkGiver_DoBill), "TryFindBestIngredientsHelper")]
    public static class WorkGiver_DoBill_TryFindBestIngredientsHelper_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var method = AccessTools.Method(typeof(RegionTraverser), "BreadthFirstTraverse", new Type[]
            {typeof(Region), typeof(RegionEntryPredicate), typeof(RegionProcessor), typeof(int), typeof(RegionType)});
            foreach (var instruction in instructions)
            {
                if (instruction.Calls(method))
                {
                    yield return new CodeInstruction(OpCodes.Call,
                        AccessTools.Method(typeof(WorkGiver_DoBill_TryFindBestIngredientsHelper_Patch),
                        "AppendStacks"));
                }
                yield return instruction;
            }
        }

        public static void AppendStacks()
        {

        }
    }
    [HarmonyPatch(typeof(JobDriver_DoBill), "MakeNewToils")]
    public static class JobDriver_DoBill_MakeNewToils_Patch
    {
        public static IEnumerable<Toil> Postfix(IEnumerable<Toil> __result, JobDriver_DoBill __instance)
        {
            if (__instance.job.bill.recipe.Worker is Recipe_OperateOnPersonaStack)
            {
                var job = __instance.job;
                var pawn = __instance.pawn;
                Toil gotoBillGiver = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
                Toil toil = ToilMaker.MakeToil("MakeNewToils");
                toil.initAction = delegate
                {
                    if (job.targetQueueB != null && job.targetQueueB.Count == 1 && job.targetQueueB[0].Thing is UnfinishedThing unfinishedThing)
                    {
                        unfinishedThing.BoundBill = (Bill_ProductionWithUft)job.bill;
                    }
                    job.bill.Notify_DoBillStarted(pawn);
                };
                yield return toil;
                yield return Toils_Jump.JumpIf(gotoBillGiver, () => job.GetTargetQueue(TargetIndex.B).NullOrEmpty());
                foreach (Toil item in CollectIngredientsToils(__instance, TargetIndex.B, TargetIndex.A, TargetIndex.C, 
                    subtractNumTakenFromJobCount: false, failIfStackCountLessThanJobCount: true, 
                    __instance.BillGiver is Building_WorkTableAutonomous))
                {
                    yield return item;
                }
                yield return gotoBillGiver;
                yield return Toils_Recipe.MakeUnfinishedThingIfNeeded();
                var recipeWork = job.bill is Bill_RewriteStack ? DoRecipeWorkFixed() : Toils_Recipe.DoRecipeWork();
                yield return recipeWork.FailOnDespawnedNullOrForbiddenPlacedThings(TargetIndex.A).FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
                yield return Toils_Recipe.CheckIfRecipeCanFinishNow();
                yield return Toils_Recipe.FinishRecipeAndStartStoringProduct(TargetIndex.None);
            }
            else
            {
                if (__result != null)
                {
                    foreach (var toil in __result)
                    {
                        yield return toil;
                    }
                }
            }
        }

        public static IEnumerable<Toil> CollectIngredientsToils(JobDriver_DoBill jobdriver, TargetIndex ingredientInd, TargetIndex billGiverInd, 
            TargetIndex ingredientPlaceCellInd, bool subtractNumTakenFromJobCount = false, 
            bool failIfStackCountLessThanJobCount = true, bool placeInBillGiver = false)
        {

            var label = Toils_General.Do(delegate
            {
                var targetQueue = jobdriver.job.GetTargetQueue(ingredientInd);
                if (targetQueue.Any())
                {
                    var target = targetQueue[0].Thing;
                    if (target.Spawned is false)
                    {

                    }
                }
            });
            yield return label;

            Toil extract = Toils_JobTransforms.ExtractNextTargetFromQueue(ingredientInd);
            yield return extract;
            Toil jumpIfHaveTargetInQueue = Toils_Jump.JumpIfHaveTargetInQueue(ingredientInd, label);
            yield return JobDriver_DoBill.JumpIfTargetInsideBillGiver(jumpIfHaveTargetInQueue, ingredientInd, billGiverInd);
            Toil getToHaulTarget = Toils_Goto.GotoThing(ingredientInd, PathEndMode.ClosestTouch)
                .FailOnDespawnedNullOrForbidden(ingredientInd).FailOnSomeonePhysicallyInteracting(ingredientInd);
            yield return getToHaulTarget;
            yield return Toils_Haul.StartCarryThing(ingredientInd, putRemainderInQueue: true, subtractNumTakenFromJobCount, failIfStackCountLessThanJobCount, reserve: false);
            yield return JobDriver_DoBill.JumpToCollectNextIntoHandsForBill(getToHaulTarget, TargetIndex.B);
            yield return Toils_Goto.GotoThing(billGiverInd, PathEndMode.InteractionCell).FailOnDestroyedOrNull(ingredientInd);
            if (!placeInBillGiver)
            {
                Toil findPlaceTarget = Toils_JobTransforms.SetTargetToIngredientPlaceCell(billGiverInd, ingredientInd, ingredientPlaceCellInd);
                yield return findPlaceTarget;
                yield return Toils_Haul.PlaceHauledThingInCell(ingredientPlaceCellInd, findPlaceTarget, storageMode: false);
                Toil physReserveToil = ToilMaker.MakeToil("CollectIngredientsToils");
                physReserveToil.initAction = delegate
                {
                    physReserveToil.actor.Map.physicalInteractionReservationManager.Reserve(physReserveToil.actor, physReserveToil.actor.CurJob, physReserveToil.actor.CurJob.GetTarget(ingredientInd));
                };
                yield return physReserveToil;
            }
            else
            {
                yield return Toils_Haul.DepositHauledThingInContainer(billGiverInd, ingredientInd);
            }
            yield return jumpIfHaveTargetInQueue;
        }

        public static Toil DoRecipeWorkFixed()
        {
            Toil toil = ToilMaker.MakeToil("DoRecipeWork");
            toil.initAction = delegate
            {
                Pawn actor3 = toil.actor;
                Job curJob3 = actor3.jobs.curJob;
                JobDriver_DoBill jobDriver_DoBill2 = (JobDriver_DoBill)actor3.jobs.curDriver;
                UnfinishedThing unfinishedThing3 = curJob3.GetTarget(TargetIndex.B).Thing as UnfinishedThing;
                if (unfinishedThing3 != null && unfinishedThing3.Initialized)
                {
                    jobDriver_DoBill2.workLeft = unfinishedThing3.workLeft;
                }
                else
                {
                    jobDriver_DoBill2.workLeft = curJob3.bill.GetWorkAmount(unfinishedThing3);
                    if (unfinishedThing3 != null)
                    {
                        unfinishedThing3.workLeft = jobDriver_DoBill2.workLeft;
                    }
                }
                jobDriver_DoBill2.billStartTick = Find.TickManager.TicksGame;
                jobDriver_DoBill2.ticksSpentDoingRecipeWork = 0;
                curJob3.bill.Notify_BillWorkStarted(actor3);
            };
            toil.tickAction = delegate
            {
                Pawn actor2 = toil.actor;
                Job curJob2 = actor2.jobs.curJob;
                JobDriver_DoBill jobDriver_DoBill = (JobDriver_DoBill)actor2.jobs.curDriver;
                UnfinishedThing unfinishedThing2 = curJob2.GetTarget(TargetIndex.B).Thing as UnfinishedThing;
                if (unfinishedThing2 != null && unfinishedThing2.Destroyed)
                {
                    actor2.jobs.EndCurrentJob(JobCondition.Incompletable);
                }
                else
                {
                    jobDriver_DoBill.ticksSpentDoingRecipeWork++;
                    curJob2.bill.Notify_PawnDidWork(actor2);
                    (toil.actor.CurJob.GetTarget(TargetIndex.A).Thing as IBillGiverWithTickAction)?.UsedThisTick();
                    if (curJob2.RecipeDef.workSkill != null && curJob2.RecipeDef.UsesUnfinishedThing && actor2.skills != null)
                    {
                        actor2.skills.Learn(curJob2.RecipeDef.workSkill, 0.1f * curJob2.RecipeDef.workSkillLearnFactor);
                    }
                    float num2 = ((curJob2.RecipeDef.workSpeedStat == null) ? 1f : actor2.GetStatValue(curJob2.RecipeDef.workSpeedStat));
                    if (curJob2.RecipeDef.workTableSpeedStat != null)
                    {
                        Building_WorkTable building_WorkTable = jobDriver_DoBill.BillGiver as Building_WorkTable;
                        if (building_WorkTable != null)
                        {
                            num2 *= building_WorkTable.GetStatValue(curJob2.RecipeDef.workTableSpeedStat);
                        }
                    }
                    if (DebugSettings.fastCrafting)
                    {
                        num2 *= 30f;
                    }
                    jobDriver_DoBill.workLeft -= num2;
                    if (unfinishedThing2 != null)
                    {
                        unfinishedThing2.workLeft = jobDriver_DoBill.workLeft;
                    }
                    actor2.GainComfortFromCellIfPossible(chairsOnly: true);
                    if (jobDriver_DoBill.workLeft <= 0f)
                    {
                        curJob2.bill.Notify_BillWorkFinished(actor2);
                        jobDriver_DoBill.ReadyForNextToil();
                    }
                    else if (curJob2.bill.recipe.UsesUnfinishedThing)
                    {
                        int num3 = Find.TickManager.TicksGame - jobDriver_DoBill.billStartTick;
                        if (num3 >= 3000 && num3 % 1000 == 0)
                        {
                            actor2.jobs.CheckForJobOverride();
                        }
                    }
                }
            };
            toil.defaultCompleteMode = ToilCompleteMode.Never;
            toil.WithEffect(() => toil.actor.CurJob.bill.recipe.effectWorking, TargetIndex.A);
            toil.PlaySustainerOrSound(() => toil.actor.CurJob.bill.recipe.soundWorking);
            toil.WithProgressBar(TargetIndex.A, delegate
            {
                Pawn actor = toil.actor;
                Job curJob = actor.CurJob;
                UnfinishedThing unfinishedThing = curJob.GetTarget(TargetIndex.B).Thing as UnfinishedThing;
                float workLeft = ((JobDriver_DoBill)actor.jobs.curDriver).workLeft;
                Bill_Mech bill_Mech;
                float num = (((bill_Mech = curJob.bill as Bill_Mech) != null && bill_Mech.State == FormingState.Formed) ? 300f : curJob.bill.GetWorkAmount(unfinishedThing));
                return 1f - workLeft / num;
            });
            toil.FailOn((Func<bool>)delegate
            {
                RecipeDef recipeDef = toil.actor.CurJob.RecipeDef;
                if (recipeDef != null && recipeDef.interruptIfIngredientIsRotting)
                {
                    LocalTargetInfo target = toil.actor.CurJob.GetTarget(TargetIndex.B);
                    if (target.HasThing && (int)target.Thing.GetRotStage() > 0)
                    {
                        return true;
                    }
                }
                return toil.actor.CurJob.bill.suspended;
            });
            toil.activeSkill = () => toil.actor.CurJob.bill.recipe.workSkill;
            return toil;
        }

    }
}

