using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{
    public class Recipe_InstallActiveNeuralStack : Recipe_InstallNeuralStack
    {
        public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
        {
            return false;
        }
    }

    [HotSwappable]
    public class Recipe_InstallNeuralStack : Recipe_Surgery
    {
        public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
        {
            var pawn = thing as Pawn;
            if (AC_Utils.CanImplantStackTo(this.recipe.addsHediff, pawn))
            {
                return base.AvailableOnNow(thing, part);
            }
            return false;
        }

        public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
        {
            return MedicalRecipesUtility.GetFixedPartsToApplyOn(recipe, pawn, delegate (BodyPartRecord record)
            {
                if (!pawn.health.hediffSet.GetNotMissingParts().Contains(record))
                {
                    return false;
                }
                if (pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(record))
                {
                    return false;
                }
                return (!pawn.health.hediffSet.hediffs.Any((Hediff x) => x.Part == record && (x.def == recipe.addsHediff || !recipe.CompatibleWithHediff(x.def)))) ? true : false;
            });
        }
        public override void ConsumeIngredient(Thing ingredient, RecipeDef recipe, Map map)
        {
            Thing.allowDestroyNonDestroyable = true;
            if (ingredient is NeuralStack c)
            {
                c.dontKillThePawn = true;
            }
            base.ConsumeIngredient(ingredient, recipe, map);
            Thing.allowDestroyNonDestroyable = false;
        }

        public static Pawn pawnToInstallStack;
        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            if (billDoer != null)
            {
                if (CheckSurgeryFail(billDoer, pawn, ingredients, part, bill))
                {
                    foreach (var i in ingredients)
                    {
                        if (i is NeuralStack c)
                        {
                            c.stackCount = 1;
                            c.mapIndexOrState = (sbyte)-1;
                            GenPlace.TryPlaceThing(c, billDoer.Position, billDoer.Map, ThingPlaceMode.Near);
                        }
                    }
                    return;
                }
                TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);
                if (pawn.HasNeuralStack(out var stackHediff))
                {
                    var emptyStack = AC_Utils.stacksPairs[stackHediff.SourceStack];
                    var stack = ThingMaker.MakeThing(emptyStack);
                    GenPlace.TryPlaceThing(stack, billDoer.Position, billDoer.Map, ThingPlaceMode.Near);
                    stackHediff.preventKill = true;
                    pawn.health.RemoveHediff(stackHediff);
                }
                ApplyNeuralStack(recipe, pawn, part, ingredients.OfType<NeuralStack>().FirstOrDefault());
            }
        }

        public static void ApplyNeuralStack(RecipeDef recipe, Pawn pawn, BodyPartRecord part, NeuralStack neuralStack)
        {
            pawnToInstallStack = pawn;
            var hediff = HediffMaker.MakeHediff(recipe.addsHediff, pawn) as Hediff_NeuralStack;
            if (neuralStack.NeuralData.ContainsData)
            {
                var data = hediff.NeuralData = neuralStack.NeuralData;
                if (pawn.IsEmptySleeve() is false)
                {
                    var copy = new NeuralData();
                    copy.CopyFromPawn(pawn, data.sourceStack);
                    var dummyPawn = copy.DummyPawn;
                    GenSpawn.Spawn(dummyPawn, pawn.Position, pawn.Map);
                    Pawn_HealthTracker_NotifyPlayerOfKilled_Patch.pawnToSkip = dummyPawn;
                    dummyPawn.Kill(null, hediff);
                    dummyPawn.Corpse.DeSpawn();
                }

                AlteredCarbonManager.Instance.StacksIndex.Remove(data.PawnID);
                AlteredCarbonManager.Instance.ReplaceStackWithPawn(neuralStack, pawn);
                if (AlteredCarbonManager.Instance.emptySleeves.Contains(pawn))
                {
                    AlteredCarbonManager.Instance.emptySleeves.Remove(pawn);
                }
                data.OverwritePawn(pawn, neuralStack.def.GetModExtension<StackSavingOptionsModExtension>(), copyFromOrigPawn: false);
                pawn.health.AddHediff(hediff, part);
                ApplyMindEffects(pawn, hediff);
            }
            else
            {
                pawn.health.AddHediff(hediff, part);
            }

            if (ModsConfig.IdeologyActive)
            {
                Find.HistoryEventsManager.RecordEvent(new HistoryEvent(AC_DefOf.AC_InstalledNeuralStack, pawn.Named(HistoryEventArgsNames.Doer)));
            }

            if (hediff.NeuralData.ideo?.HasPrecept(AC_DefOf.AC_CrossSleeving_DontCare) ?? false)
            {
                hediff.NeuralData.thoughts?.RemoveAll(x => x.def == AC_DefOf.AC_WrongGender);
                hediff.NeuralData.thoughts?.RemoveAll(x => x.def == AC_DefOf.AC_WrongGenderPregnant);
            }
        }

        public static void ApplyMindEffects(Pawn pawn, Hediff_NeuralStack hediff)
        {
            if (AC_Utils.editStacksSettings.enableStackDegradation && hediff.NeuralData.stackDegradation > 0)
            {
                var stackDegradationHediff = pawn.health.hediffSet.GetFirstHediffOfDef(AC_DefOf.AC_StackDegradation) as Hediff_StackDegradation;
                if (stackDegradationHediff is null)
                {
                    BodyPartRecord neckRecord = pawn.GetNeck();
                    stackDegradationHediff = HediffMaker.MakeHediff(AC_DefOf.AC_StackDegradation, pawn, neckRecord) as Hediff_StackDegradation;
                    pawn.health.AddHediff(stackDegradationHediff, neckRecord);
                }
                stackDegradationHediff.stackDegradation = hediff.NeuralData.stackDegradation;

                var brainTraumaChance = (hediff.NeuralData.stackDegradation - 0.8f) * 5f;
                if (brainTraumaChance > 0 && Rand.Chance(brainTraumaChance))
                {
                    pawn.health.AddHediff(AC_DefOf.AC_BrainTrauma, pawn.health.hediffSet.GetBrain());
                }
                pawn.needs.mood.thoughts.memories.TryGainMemory(AC_DefOf.AC_StackDegradationThought);
            }

            bool isAndroid = pawn.IsAndroid();

            if (pawn.gender != hediff.NeuralData.OriginalGender)
            {
                pawn.needs.mood.thoughts.memories.TryGainMemory(isAndroid ? AC_DefOf.AC_WrongShellGender : AC_DefOf.AC_WrongGender);
            }

            if (ModCompatibility.AlienRacesIsActive && hediff.NeuralData.OriginalRace != null && pawn.kindDef.race != hediff.NeuralData.OriginalRace)
            {
                pawn.needs.mood.thoughts.memories.TryGainMemory(AC_DefOf.AC_WrongRace);
            }
            if (pawn.SleeveMatchesOriginalXenotype(hediff.NeuralData))
            {
                pawn.needs.mood.thoughts.memories.TryGainMemory(AC_DefOf.AC_WrongXenotype);
            }

            pawn.needs.mood.thoughts.memories.TryGainMemory(isAndroid ? AC_DefOf.AC_NewShell : AC_DefOf.AC_NewSleeve);

            if (ModCompatibility.VanillaRacesExpandedAndroidIsActive)
            {
                if (pawn.story.traits.HasTrait(AC_DefOf.AC_Shellwalker) && isAndroid is false)
                {
                    pawn.needs.mood.thoughts.memories.TryGainMemory(AC_DefOf.AC_WantsShell);
                }
            }

            if (hediff.NeuralData.diedFromCombat.HasValue && hediff.NeuralData.diedFromCombat.Value)
            {
                hediff.NeuralData.diedFromCombat = null;
            }

            if (pawn.story.traits.HasTrait(AC_DefOf.AC_Sleever) is false)
            {
                var sleeveShock = HediffMaker.MakeHediff(AC_DefOf.AC_SleeveShock, pawn);
                sleeveShock.Severity = Rand.Range(0.2f, 1f);
                pawn.health.AddHediff(sleeveShock);
            }
            pawn.needs.AddOrRemoveNeedsAsAppropriate();
        }
    }
}