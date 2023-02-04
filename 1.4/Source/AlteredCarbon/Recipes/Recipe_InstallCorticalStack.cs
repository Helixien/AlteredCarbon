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
    public class Recipe_InstallFilledCorticalStack : Recipe_InstallCorticalStack
    {
        public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
        {
            return false;
        }
    }

    [HotSwappable]
    public class Recipe_InstallCorticalStack : Recipe_Surgery
    {
        public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
        {
            var pawn = thing as Pawn;
            if (ACUtils.CanImplantStackTo(this.recipe.addsHediff, pawn))
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
            if (ingredient is CorticalStack c)
            {
                c.dontKillThePawn = true;
            }
            base.ConsumeIngredient(ingredient, recipe, map);
            Thing.allowDestroyNonDestroyable = false;
        }
        private void CopyAllPhysicalDataFrom(Pawn source, Pawn to)
        {
            to.health.hediffSet.hediffs = new List<Hediff>();
            foreach (var hediff in source.health.hediffSet.hediffs)
            {
                to.health.hediffSet.hediffs.Add(hediff);
            }
            to.health.hediffSet.DirtyCache();
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
                        if (i is CorticalStack c)
                        {
                            c.stackCount = 1;
                            c.mapIndexOrState = (sbyte)-1;
                            GenPlace.TryPlaceThing(c, billDoer.Position, billDoer.Map, ThingPlaceMode.Near);
                        }
                    }
                    return;
                }
                TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);
            }

            var corticalStack = ingredients.OfType<CorticalStack>().FirstOrDefault();
            pawnToInstallStack = pawn;
            if (pawn.HasCorticalStack(out var stackHediff))
            {
                var emptyStack = ACUtils.stacksPairs[stackHediff.SourceStack];
                var stack = ThingMaker.MakeThing(emptyStack);
                GenPlace.TryPlaceThing(stack, billDoer.Position, billDoer.Map, ThingPlaceMode.Near);
                stackHediff.preventKill = true;
                pawn.health.RemoveHediff(stackHediff);
            }
            
            var hediff = HediffMaker.MakeHediff(recipe.addsHediff, pawn) as Hediff_CorticalStack;
            if (corticalStack.PersonaData.ContainsInnerPersona)
            {
                hediff.PersonaData = corticalStack.PersonaData;
                if (pawn.IsEmptySleeve() is false)
                {
                    var gender = pawn.gender;
                    var kindDef = pawn.kindDef;
                    var faction = pawn.Faction;
                    var dummyPawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kindDef, faction, fixedGender: gender,
                        fixedBiologicalAge: pawn.ageTracker.AgeBiologicalYearsFloat, fixedChronologicalAge: pawn.ageTracker.AgeChronologicalYearsFloat));
                    var copy = new PersonaData();
                    copy.OverwritePawn(pawn: dummyPawn, null, original: pawn, overwriteOriginalPawn: false);
                    CopyAllPhysicalDataFrom(pawn, dummyPawn);
                    GenSpawn.Spawn(dummyPawn, pawn.Position, pawn.Map);
                    Pawn_HealthTracker_NotifyPlayerOfKilled_Patch.pawnToSkip = dummyPawn;
                    dummyPawn.Kill(null, hediff);
                    dummyPawn.Corpse.DeSpawn();
                }

                AlteredCarbonManager.Instance.StacksIndex.Remove(hediff.PersonaData.pawnID);
                AlteredCarbonManager.Instance.ReplaceStackWithPawn(corticalStack, pawn);
                if (AlteredCarbonManager.Instance.emptySleeves != null && AlteredCarbonManager.Instance.emptySleeves.Contains(pawn))
                {
                    AlteredCarbonManager.Instance.emptySleeves.Remove(pawn);
                }

                hediff.PersonaData.OverwritePawn(pawn, corticalStack.def.GetModExtension<StackSavingOptionsModExtension>(), null);
                pawn.health.AddHediff(hediff, part);
                pawn.needs.AddOrRemoveNeedsAsAppropriate();

                if (ModCompatibility.HelixienAlteredCarbonIsActive && hediff.PersonaData.stackDegradation > 0)
                {
                    var stackDegradationHediff = pawn.health.hediffSet.GetFirstHediffOfDef(AC_DefOf.AC_StackDegradation) as Hediff_StackDegradation;
                    if (stackDegradationHediff is null)
                    {
                        stackDegradationHediff = HediffMaker.MakeHediff(AC_DefOf.AC_StackDegradation, pawn) as Hediff_StackDegradation;
                        pawn.health.AddHediff(stackDegradationHediff);
                    }
                    stackDegradationHediff.stackDegradation = hediff.PersonaData.stackDegradation;

                    var brainTraumaChance = (hediff.PersonaData.stackDegradation - 0.8f) * 5f;
                    if (brainTraumaChance > 0 && Rand.Chance(brainTraumaChance))
                    {
                        pawn.health.AddHediff(AC_DefOf.AC_BrainTrauma, pawn.health.hediffSet.GetBrain());
                    }
                    pawn.needs.mood.thoughts.memories.TryGainMemory(AC_DefOf.AC_StackDegradationThought);
                }
                
                if (pawn.gender != hediff.PersonaData.originalGender && (hediff.PersonaData.ideo is null || !hediff.PersonaData.ideo.HasPrecept(AC_DefOf.AC_CrossSleeving_DontCare)))
                {
                    if (pawn.story.traits.HasTrait(TraitDefOf.BodyPurist))
                    {
                        pawn.needs.mood.thoughts.memories.TryGainMemory(AC_DefOf.VFEU_WrongGenderDouble);
                        Log.Message(pawn + " - Should gain wrong double gender");
                    }
                    else
                    {
                        pawn.needs.mood.thoughts.memories.TryGainMemory(AC_DefOf.VFEU_WrongGender);
                        Log.Message(pawn + " - Should gain wrong gender");
                    }
                }

                if (hediff.PersonaData.originalRace != null && pawn.kindDef.race != hediff.PersonaData.originalRace)
                {
                    pawn.needs.mood.thoughts.memories.TryGainMemory(AC_DefOf.VFEU_WrongRace);
                }

                var naturalMood = pawn.story.traits.GetTrait(TraitDefOf.NaturalMood);
                var nerves = pawn.story.traits.GetTrait(TraitDefOf.Nerves);
                if ((naturalMood != null && naturalMood.Degree == -2)
                        || pawn.story.traits.HasTrait(TraitDefOf.BodyPurist)
                        || (nerves != null && nerves.Degree == -2))
                {
                    pawn.needs.mood.thoughts.memories.TryGainMemory(AC_DefOf.VFEU_NewSleeveDouble);
                }
                else
                {
                    pawn.needs.mood.thoughts.memories.TryGainMemory(AC_DefOf.VFEU_NewSleeve);
                }
            
                if (hediff.PersonaData.diedFromCombat.HasValue && hediff.PersonaData.diedFromCombat.Value)
                {
                    pawn.health.AddHediff(HediffMaker.MakeHediff(AC_DefOf.VFEU_SleeveShock, pawn));
                    hediff.PersonaData.diedFromCombat = null;
                }
            }
            else
            {
                pawn.health.AddHediff(hediff, part);
            }
            
            if (ModsConfig.IdeologyActive)
            {
                Find.HistoryEventsManager.RecordEvent(new HistoryEvent(AC_DefOf.VFEU_InstalledCorticalStack, pawn.Named(HistoryEventArgsNames.Doer)));
            }
        }
    }
}