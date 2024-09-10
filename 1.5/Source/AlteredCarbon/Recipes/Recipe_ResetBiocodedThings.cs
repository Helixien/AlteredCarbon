using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AlteredCarbon
{
    public class Recipe_ResetBiocodedThings : RecipeWorker
    {
        public override void ConsumeIngredient(Thing ingredient, RecipeDef recipe, Map map)
        {

        }
        public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
        {
            base.Notify_IterationCompleted(billDoer, ingredients);
            var biocodedThing = ingredients.FirstOrDefault(x => x.TryGetComp<CompBiocodable>() != null);
            var baseFailChance = 0.5f;
            var intelSkill = billDoer.skills.GetSkill(SkillDefOf.Intellectual)?.Level ?? 0;
            if (intelSkill >= 10)
            {
                var skillOffset = intelSkill - 10;
                baseFailChance -= 0.05f * skillOffset;
            }
            if (!Rand.Chance(baseFailChance))
            {
                var comp = biocodedThing.TryGetComp<CompBiocodable>();
                comp.UnCode();
                CompBiocodable_PostExposeData_Patch.wasBiocoded.Set(comp, true);
                Messages.Message("AC.ResettingBiocodedSuccess".Translate(biocodedThing.LabelShort), biocodedThing, MessageTypeDefOf.PositiveEvent);
            }
            else
            {
                biocodedThing.Destroy();
                Messages.Message("AC.ResettingBiocodedFailed".Translate(biocodedThing.LabelShort), billDoer, MessageTypeDefOf.NegativeEvent);
            }
        }
    }
}

