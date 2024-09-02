using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AlteredCarbon
{
    public class Recipe_RestoreStackFromNeuralPrint : Recipe_OperateOnNeuralPrint
    {
        public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
        {
            base.Notify_IterationCompleted(billDoer, ingredients);
            var neuralPrint = NeuralPrint(billDoer);
            var emptyStack = ingredients.OfType<NeuralStack>().FirstOrDefault(x => x.IsActiveStack is false);
            var stackRestoreTo = (NeuralStack)ThingMaker.MakeThing(AC_DefOf.AC_ActiveNeuralStack);
            stackRestoreTo.NeuralData.CopyDataFrom(neuralPrint.NeuralData, true);
            AlteredCarbonManager.Instance.RegisterStack(stackRestoreTo);
            Messages.Message("AC.SuccessfullyRestoredStackFromBackup".Translate(billDoer.Named("PAWN")), stackRestoreTo, MessageTypeDefOf.TaskCompletion);
            GenPlace.TryPlaceThing(stackRestoreTo, billDoer.Position, billDoer.Map, ThingPlaceMode.Near);
            neuralPrint.holdingOwner?.Remove(neuralPrint);
            neuralPrint.Destroy();
            emptyStack.Destroy();
        }
    }
}

