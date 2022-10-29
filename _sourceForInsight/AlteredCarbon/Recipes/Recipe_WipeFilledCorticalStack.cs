using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{
    public class Recipe_WipeFilledCorticalStack : RecipeWorker
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
            var stack = ingredients.OfType<CorticalStack>().FirstOrDefault();
            if (stack.PersonaData.faction != null && billDoer != null && billDoer.Faction != null && billDoer.Faction != stack.Faction)
            {
                stack.EmptyStack(billDoer, true);
            }
            else
            {
                stack.EmptyStack(billDoer);
            }
        }
    }
}

