using Verse;

namespace AlteredCarbon
{
    public class Recipe_OperateOnPersonaStack : RecipeWorker
    {
        public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
        {
            return false;
        }
        public override void ConsumeIngredient(Thing ingredient, RecipeDef recipe, Map map)
        {

        }

        public PersonaStack PersonaStack(Pawn billDoer)
        {
            return (billDoer.jobs.curJob.bill as Bill_OperateOnStack).personaStack;
        }
    }
}

