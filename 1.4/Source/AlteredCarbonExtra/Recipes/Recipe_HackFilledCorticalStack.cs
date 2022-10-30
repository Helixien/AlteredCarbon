using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    public class Recipe_HackFilledCorticalStack : RecipeWorker
    {
        public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
        {
            return false;
        }
        public override void ConsumeIngredient(Thing ingredient, RecipeDef recipe, Map map)
        {

        }
        public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
        {
            base.Notify_IterationCompleted(billDoer, ingredients);
            var corticalStack = ingredients.OfType<CorticalStack>().First();
            var intelSkill = billDoer.skills?.GetSkill(SkillDefOf.Intellectual)?.levelInt ?? 0;
            var baseFailChance = 0.3f;
            var diff = intelSkill - 10;
            for (var i = 0; i < diff; i++)
            {
                baseFailChance -= 0.03f;
            }
            if (!Rand.Chance(baseFailChance))
            {
                corticalStack.PersonaData.faction = Faction.OfPlayer;
                corticalStack.PersonaData.hackedWhileOnStack = true;
                corticalStack.PersonaData.RefreshDummyPawn();
            }
            else
            {
                corticalStack.Destroy();
            }
            corticalStack.MapHeld?.mapDrawer.MapMeshDirty(corticalStack.PositionHeld, MapMeshFlag.Things);
        }
    }
}
