using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{
    public class Recipe_DuplicateNeuralStack : Recipe_OperateOnNeuralStack
    {
        public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
        {
            base.Notify_IterationCompleted(billDoer, ingredients);
            var emptyStack = ingredients.OfType<NeuralStack>().FirstOrDefault(x => x.IsActiveStack is false);
            PerformStackDuplication(billDoer, NeuralData(billDoer));
            emptyStack.Destroy();
        }

        public void PerformStackDuplication(Pawn doer, NeuralData neuralDataToDuplicate)
        {
            float intellectualSkill = doer.skills.GetSkill(SkillDefOf.Intellectual).Level;
            float successChance = 1f - Mathf.Abs((intellectualSkill / 2f) - 11f) / 10f;
            if (Rand.Chance(successChance))
            {
                var stackCopyTo = (NeuralStack)ThingMaker.MakeThing(AC_DefOf.AC_ActiveNeuralStack);
                stackCopyTo.NeuralData.CopyDataFrom(neuralDataToDuplicate, true);

                AlteredCarbonManager.Instance.RegisterStack(stackCopyTo);
                GenPlace.TryPlaceThing(stackCopyTo, doer.Position, doer.Map, ThingPlaceMode.Near);
                Messages.Message("AC.SuccessfullyDuplicatedStack".Translate(doer.Named("PAWN")), doer, MessageTypeDefOf.TaskCompletion);
                if (stackCopyTo.NeuralData.faction != null && doer.Faction != null 
                    && doer.Faction != stackCopyTo.NeuralData.faction)
                {
                    stackCopyTo.NeuralData.faction.TryAffectGoodwillWith(doer.Faction, stackCopyTo.NeuralData.faction.GoodwillToMakeHostile(doer.Faction), canSendMessage: true, reason: AC_DefOf.AC_DuplicatedStackEvent);
                }
                float degradationChance = DegradationChanceCurve.Evaluate(intellectualSkill);
                if (Rand.Chance(degradationChance))
                {
                    float degradationAmount = EvaluateDegradationAmount(intellectualSkill);
                    ApplyStackDegradation(stackCopyTo, degradationAmount);
                }
            }
            else
            {
                Messages.Message("AC.FailedToDuplicatedStack".Translate(doer.Named("PAWN")), doer, MessageTypeDefOf.NeutralEvent);
            }
        }

        private static readonly SimpleCurve DegradationChanceCurve = new SimpleCurve
        {
            new CurvePoint(10f, 0.50f),
            new CurvePoint(11f, 0.45f),
            new CurvePoint(12f, 0.40f),
            new CurvePoint(13f, 0.35f),
            new CurvePoint(14f, 0.30f),
            new CurvePoint(15f, 0.25f),
            new CurvePoint(16f, 0.20f),
            new CurvePoint(17f, 0.15f),
            new CurvePoint(18f, 0.10f),
            new CurvePoint(19f, 0.05f),
            new CurvePoint(20f, 0.00f)
        };

        private float EvaluateDegradationAmount(float intellectualSkill)
        {
            // Dynamically generate random values within a range based on intellectual skill
            if (intellectualSkill >= 20) return 0f;
            if (intellectualSkill >= 19) return Rand.Range(0.00f, 0.15f);
            if (intellectualSkill >= 18) return Rand.Range(0.00f, 0.20f);
            if (intellectualSkill >= 17) return Rand.Range(0.00f, 0.25f);
            if (intellectualSkill >= 16) return Rand.Range(0.00f, 0.30f);
            if (intellectualSkill >= 15) return Rand.Range(0.05f, 0.35f);
            if (intellectualSkill >= 14) return Rand.Range(0.10f, 0.40f);
            if (intellectualSkill >= 13) return Rand.Range(0.15f, 0.45f);
            if (intellectualSkill >= 12) return Rand.Range(0.20f, 0.50f);
            if (intellectualSkill >= 11) return Rand.Range(0.25f, 0.55f);
            return Rand.Range(0.30f, 0.60f); // Skill level 10 and below
        }

        private void ApplyStackDegradation(NeuralStack stack, float degradationAmount)
        {
            stack.NeuralData.stackDegradation = Mathf.Clamp01(stack.NeuralData.stackDegradation + degradationAmount);
        }
    }
}

