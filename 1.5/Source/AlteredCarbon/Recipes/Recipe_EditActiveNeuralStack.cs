using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{
    public class Recipe_EditActiveNeuralStack : Recipe_OperateOnNeuralStack
    {
        public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
        {
            base.Notify_IterationCompleted(billDoer, ingredients);
            var stack = NeuralStack(billDoer);
            var faction = stack.NeuralData.faction;
            if (faction != null && faction != Faction.OfPlayer) 
            {
                Faction.OfPlayer.TryAffectGoodwillWith(faction, faction.GoodwillToMakeHostile(Faction.OfPlayer), canSendMessage: true, !faction.temporary, AC_DefOf.AC_EditedStack);
            }
            stack.NeuralData = stack.neuralDataRewritten;
            stack.NeuralData.stackDegradation += stack.neuralDataRewritten.stackDegradationToAdd;
            stack.neuralDataRewritten.stackDegradationToAdd = 0;
            stack.NeuralData.stackDegradation = Mathf.Clamp01(stack.NeuralData.stackDegradation);
            stack.neuralDataRewritten = null;
            if (stack.Spawned)
            {
                stack.Map.mapDrawer.MapMeshDirty(stack.Position, MapMeshFlagDefOf.Things);
            }
        }
    }
}

