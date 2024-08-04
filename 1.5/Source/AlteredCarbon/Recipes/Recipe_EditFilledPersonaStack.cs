using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{
    public class Recipe_EditFilledPersonaStack : Recipe_OperateOnPersonaStack
    {
        public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
        {
            base.Notify_IterationCompleted(billDoer, ingredients);
            var stack = PersonaStack(billDoer);
            var faction = stack.PersonaData.faction;
            if (faction != null && faction != Faction.OfPlayer) 
            {
                Faction.OfPlayer.TryAffectGoodwillWith(faction, faction.GoodwillToMakeHostile(Faction.OfPlayer), canSendMessage: true, !faction.temporary, AC_DefOf.AC_EditedStack);
            }
            stack.PersonaData = stack.personaDataRewritten;
            stack.PersonaData.stackDegradation += stack.personaDataRewritten.stackDegradationToAdd;
            stack.personaDataRewritten.stackDegradationToAdd = 0;
            stack.PersonaData.stackDegradation = Mathf.Clamp01(stack.PersonaData.stackDegradation);
            stack.personaDataRewritten = null;
            if (stack.Spawned)
            {
                stack.Map.mapDrawer.MapMeshDirty(stack.Position, MapMeshFlagDefOf.Things);
            }
        }
    }
}

