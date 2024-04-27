using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{
    public class Recipe_RewriteFilledPersonaStack : Recipe_OperateOnPersonaStack
    {
        public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
        {
            base.Notify_IterationCompleted(billDoer, ingredients);
            var stack = ingredients.OfType<PersonaStack>().FirstOrDefault();
            var faction = stack.PersonaData.faction;
            if (faction != null && faction != Faction.OfPlayer) 
            {
                Faction.OfPlayer.TryAffectGoodwillWith(faction, -15, canSendMessage: true, !faction.temporary, AC_DefOf.AC_RewroteStack);

            }
            stack.PersonaData = stack.personaDataRewritten;
            stack.PersonaData.stackDegradation += stack.personaDataRewritten.stackDegradationToAdd;
            stack.personaDataRewritten.stackDegradationToAdd = 0;
            stack.PersonaData.stackDegradation = Mathf.Clamp01(stack.PersonaData.stackDegradation);
            stack.personaDataRewritten = null;
            stack.Map.mapDrawer.MapMeshDirty(stack.Position, MapMeshFlagDefOf.Things);
        }
    }
}

