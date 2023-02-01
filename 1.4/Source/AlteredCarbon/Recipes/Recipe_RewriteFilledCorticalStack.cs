using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{
    public class Recipe_RewriteFilledCorticalStack : Recipe_OperateOnCorticalStack
    {
        public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
        {
            base.Notify_IterationCompleted(billDoer, ingredients);
            var stack = ingredients.OfType<CorticalStack>().FirstOrDefault();
            stack.PersonaData = stack.personaDataRewritten;
            stack.PersonaData.stackDegradation += stack.personaDataRewritten.stackDegradationToAdd;
            stack.PersonaData.stackDegradation = Mathf.Clamp01(stack.PersonaData.stackDegradation);
            stack.personaDataRewritten = null;
            stack.Map.mapDrawer.MapMeshDirty(stack.Position, MapMeshFlag.Things);
        }
    }
}

