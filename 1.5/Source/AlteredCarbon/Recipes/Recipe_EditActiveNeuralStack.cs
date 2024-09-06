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
            var origData = NeuralData(billDoer);
            var rewrittenData = origData.neuralDataRewritten;
            var faction = origData.faction;
            if (faction != null && faction != Faction.OfPlayer) 
            {
                Faction.OfPlayer.TryAffectGoodwillWith(faction, faction.GoodwillToMakeHostile(Faction.OfPlayer), canSendMessage: true, !faction.temporary, AC_DefOf.AC_EditedStack);
            }
            rewrittenData.stackDegradation += rewrittenData.stackDegradationToAdd;
            rewrittenData.stackDegradationToAdd = 0;
            rewrittenData.stackDegradation = Mathf.Clamp01(rewrittenData.stackDegradation);
            rewrittenData.neuralDataRewritten = null;
            var thing = Thing(billDoer);
            if (thing is NeuralStack stack)
            {
                stack.NeuralData = rewrittenData;
                if (stack.Spawned)
                {
                    stack.Map.mapDrawer.MapMeshDirty(stack.Position, MapMeshFlagDefOf.Things);
                }
            }
            else if (thing is Pawn pawn && pawn.HasNeuralStack(out var hediff))
            {
                hediff.NeuralData = rewrittenData;
                hediff.NeuralData.OverwritePawn(pawn, hediff.SourceStack.GetModExtension<StackSavingOptionsModExtension>(), copyFromOrigPawn: false);
            }
        }
    }
}

