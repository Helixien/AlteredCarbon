using Verse;

namespace AlteredCarbon
{
    public class Recipe_OperateOnNeuralStack : RecipeWorker
    {
        public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
        {
            return false;
        }
        public override void ConsumeIngredient(Thing ingredient, RecipeDef recipe, Map map)
        {

        }

        public NeuralData NeuralData(Pawn billDoer)
        {
            return Thing(billDoer).GetNeuralData();
        }

        public Thing Thing(Pawn billDoer)
        {
            return (billDoer.jobs.curJob.bill as Bill_OperateOnStack).targetThing;
        }
    }
}

