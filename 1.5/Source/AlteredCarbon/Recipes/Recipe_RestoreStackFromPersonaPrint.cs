using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AlteredCarbon
{
    public class Recipe_RestoreStackFromPersonaPrint : Recipe_OperateOnPersonaPrint
    {
        public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
        {
            base.Notify_IterationCompleted(billDoer, ingredients);
            var personaPrint = PersonaPrint(billDoer);
            var emptyStack = ingredients.OfType<PersonaStack>().FirstOrDefault(x => x.IsFilledStack is false);
            var stackRestoreTo = (PersonaStack)ThingMaker.MakeThing(AC_DefOf.AC_FilledPersonaStack);
            stackRestoreTo.PersonaData.CopyDataFrom(personaPrint.PersonaData, true);
            AlteredCarbonManager.Instance.RegisterStack(stackRestoreTo);
            Messages.Message("AC.SuccessfullyRestoredStackFromBackup".Translate(billDoer.Named("PAWN")), stackRestoreTo, MessageTypeDefOf.TaskCompletion);
            GenPlace.TryPlaceThing(stackRestoreTo, billDoer.Position, billDoer.Map, ThingPlaceMode.Near);
            personaPrint.holdingOwner?.Remove(personaPrint);
            personaPrint.Destroy();
            emptyStack.Destroy();
        }
    }
}

