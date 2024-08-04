using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{
    public class Recipe_DuplicatePersonaStack : Recipe_OperateOnPersonaStack
    {
        public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
        {
            base.Notify_IterationCompleted(billDoer, ingredients);
            var filledStack = PersonaStack(billDoer);
            var emptyStack = ingredients.OfType<PersonaStack>().FirstOrDefault(x => x.IsFilledStack is false);
            PerformStackDuplication(billDoer, filledStack);
            emptyStack.Destroy();
        }

        public void PerformStackDuplication(Pawn doer, PersonaStack stackToDuplicate)
        {
            float successChance = 1f - Mathf.Abs((doer.skills.GetSkill(SkillDefOf.Intellectual).Level / 2f) - 11f) / 10f;
            if (Rand.Chance(successChance))
            {
                var stackCopyTo = (PersonaStack)ThingMaker.MakeThing(AC_DefOf.AC_FilledPersonaStack);
                stackCopyTo.PersonaData.CopyDataFrom(stackToDuplicate.PersonaData, true);
                AlteredCarbonManager.Instance.RegisterStack(stackCopyTo);
                GenPlace.TryPlaceThing(stackCopyTo, doer.Position, doer.Map, ThingPlaceMode.Near);
                Messages.Message("AC.SuccessfullyDuplicatedStack".Translate(doer.Named("PAWN")), doer, MessageTypeDefOf.TaskCompletion);
                if (stackCopyTo.PersonaData.faction != null && doer.Faction != null 
                    && doer.Faction != stackCopyTo.PersonaData.faction)
                {
                    stackCopyTo.PersonaData.faction.TryAffectGoodwillWith(doer.Faction, stackCopyTo.PersonaData.faction.GoodwillToMakeHostile(doer.Faction), canSendMessage: true, reason: AC_DefOf.AC_DuplicatedStackEvent);
                }
            }
            else
            {
                Messages.Message("AC.FailedToDuplicatedStack".Translate(doer.Named("PAWN")), doer, MessageTypeDefOf.NeutralEvent);
            }
        }
    }
}

