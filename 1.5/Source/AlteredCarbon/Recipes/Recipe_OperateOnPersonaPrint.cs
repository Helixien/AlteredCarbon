using Verse;

namespace AlteredCarbon
{
    public class Recipe_OperateOnPersonaPrint : RecipeWorker
    {
        public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
        {
            return false;
        }
        public override void ConsumeIngredient(Thing ingredient, RecipeDef recipe, Map map)
        {

        }

        public PersonaPrint PersonaPrint(Pawn billDoer)
        {
            return (billDoer.jobs.curJob.bill as Bill_OperateOnStack).thingWithPersonaData as PersonaPrint;
        }
    }
}

