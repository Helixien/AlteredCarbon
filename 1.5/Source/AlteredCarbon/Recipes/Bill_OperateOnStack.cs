using System.Collections.Generic;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    public class Bill_OperateOnStack : Bill_Production
    {
        public ThingWithNeuralData thingWithNeuralData;
        public Bill_OperateOnStack()
        {

        }
        public Bill_OperateOnStack(ThingWithNeuralData thingWithNeuralData, RecipeDef recipe, Precept_ThingStyle precept = null)
            : base(recipe, precept)
        {
            this.thingWithNeuralData = thingWithNeuralData;
        }
        public override string Label => base.Label + " (" + (thingWithNeuralData?.NeuralData?.PawnNameColored ?? "Destroyed".Translate()) + ")";
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref thingWithNeuralData, "neuralStack");
        }

        public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
        {
            base.Notify_IterationCompleted(billDoer, ingredients);
            this.billStack.Bills.Remove(this);
        }
        public override Bill Clone()
        {
            var obj = base.Clone() as Bill_OperateOnStack;
            obj.thingWithNeuralData = thingWithNeuralData;
            return obj;
        }
    }
}

