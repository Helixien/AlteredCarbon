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
    public class ThingFilterBiocodable : ThingFilter
    {
        public override bool Allows(Thing t)
        {
            var comp = t.TryGetComp<CompBiocodable>();
            if (comp != null && comp.Biocoded)
            {
                return true;
            }
            return false;
        }
    }
    public class Recipe_HackBiocodedThings : RecipeWorker
    {
        public override void ConsumeIngredient(Thing ingredient, RecipeDef recipe, Map map)
        {

        }
        public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
        {
            base.Notify_IterationCompleted(billDoer, ingredients);
            var biocodedThing = ingredients.FirstOrDefault(x => x.TryGetComp<CompBiocodable>() != null);
            var baseFailChance = 0.3f;
            var intelSkill = billDoer.skills.GetSkill(SkillDefOf.Intellectual)?.levelInt ?? 0;
            var diff = intelSkill - 10;
            for (var i = 0; i < diff; i++)
            {
                baseFailChance -= 0.03f;
            }
            var name = biocodedThing.LabelShort;
            if (!Rand.Chance(baseFailChance))
            {
                var comp = biocodedThing.TryGetComp<CompBiocodable>();
                comp.UnCode();
                Messages.Message("AC.HackingBiocodedSuccess".Translate(name), biocodedThing, MessageTypeDefOf.PositiveEvent);
            }
            else
            {
                biocodedThing.Destroy();
                Messages.Message("AC.HackingBiocodedFailed".Translate(name), billDoer, MessageTypeDefOf.NegativeEvent);
            }
        }
    }
}

