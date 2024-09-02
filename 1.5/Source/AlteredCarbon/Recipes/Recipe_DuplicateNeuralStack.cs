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
            var activeStack = NeuralStack(billDoer);
            var emptyStack = ingredients.OfType<NeuralStack>().FirstOrDefault(x => x.IsActiveStack is false);
            PerformStackDuplication(billDoer, activeStack);
            emptyStack.Destroy();
        }

        public void PerformStackDuplication(Pawn doer, NeuralStack stackToDuplicate)
        {
            float successChance = 1f - Mathf.Abs((doer.skills.GetSkill(SkillDefOf.Intellectual).Level / 2f) - 11f) / 10f;
            if (Rand.Chance(successChance))
            {
                var stackCopyTo = (NeuralStack)ThingMaker.MakeThing(AC_DefOf.AC_ActiveNeuralStack);
                stackCopyTo.NeuralData.CopyDataFrom(stackToDuplicate.NeuralData, true);
                AlteredCarbonManager.Instance.RegisterStack(stackCopyTo);
                GenPlace.TryPlaceThing(stackCopyTo, doer.Position, doer.Map, ThingPlaceMode.Near);
                Messages.Message("AC.SuccessfullyDuplicatedStack".Translate(doer.Named("PAWN")), doer, MessageTypeDefOf.TaskCompletion);
                if (stackCopyTo.NeuralData.faction != null && doer.Faction != null 
                    && doer.Faction != stackCopyTo.NeuralData.faction)
                {
                    stackCopyTo.NeuralData.faction.TryAffectGoodwillWith(doer.Faction, stackCopyTo.NeuralData.faction.GoodwillToMakeHostile(doer.Faction), canSendMessage: true, reason: AC_DefOf.AC_DuplicatedStackEvent);
                }
            }
            else
            {
                Messages.Message("AC.FailedToDuplicatedStack".Translate(doer.Named("PAWN")), doer, MessageTypeDefOf.NeutralEvent);
            }
        }
    }
}

