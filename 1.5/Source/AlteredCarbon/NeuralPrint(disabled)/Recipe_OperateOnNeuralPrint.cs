using Verse;

namespace AlteredCarbon
{
    public class Recipe_OperateOnNeuralPrint : RecipeWorker
    {
        public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
        {
            return false;
        }
        public override void ConsumeIngredient(Thing ingredient, RecipeDef recipe, Map map)
        {

        }

        public NeuralPrint NeuralPrint(Pawn billDoer)
        {
            return (billDoer.jobs.curJob.bill as Bill_OperateOnStack).thingWithNeuralData as NeuralPrint;
        }
    }
}

