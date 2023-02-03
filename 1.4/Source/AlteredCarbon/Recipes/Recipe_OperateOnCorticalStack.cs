using Verse;

namespace AlteredCarbon
{
    public class Recipe_OperateOnCorticalStack : RecipeWorker
    {
        public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
        {
            return false;
        }
        public override void ConsumeIngredient(Thing ingredient, RecipeDef recipe, Map map)
        {

        }
    }
}

