using System.Collections.Generic;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    public class Bill_OperateOnStack : Bill_Production
    {
        public Thing thingWithStack;
        public Bill_OperateOnStack()
        {

        }
        public Bill_OperateOnStack(Thing thing, RecipeDef recipe, Precept_ThingStyle precept = null)
            : base(recipe, precept)
        {
            this.thingWithStack = thing;
        }

        public override bool ShouldDoNow()
        {
            if (thingWithStack is Pawn patient && 
                (patient.ParentHolder is not Building_NeuralConnector connector || connector.PowerOn is false))
            {
                return false;
            }
            return base.ShouldDoNow();
        }
        public override string Label => base.Label + " (" + (thingWithStack.GetNeuralData()?.PawnNameColored ?? "Destroyed".Translate()) + ")";
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref thingWithStack, "thingWithStack");
        }

        public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
        {
            base.Notify_IterationCompleted(billDoer, ingredients);
            this.billStack.Bills.Remove(this);
        }
        public override Bill Clone()
        {
            var obj = base.Clone() as Bill_OperateOnStack;
            obj.thingWithStack = thingWithStack;
            return obj;
        }
    }
}

