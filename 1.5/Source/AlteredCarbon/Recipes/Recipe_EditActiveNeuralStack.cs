using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{
    [HotSwappable]
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

            int intellectualSkill = billDoer.skills.GetSkill(SkillDefOf.Intellectual).Level;
            float degradationOffset = GetDegradationOffset(intellectualSkill);
            rewrittenData.stackDegradationToAdd *= (1 - degradationOffset);
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
                rewrittenData.OverwritePawn(pawn, hediff.SourceStack.GetModExtension<StackSavingOptionsModExtension>(), copyFromOrigPawn: false);
                hediff.NeuralData = rewrittenData;
            }
        }

        private float GetDegradationOffset(int intellectualSkill)
        {
            switch (intellectualSkill)
            {
                case 13: return 0.05f;
                case 14: return 0.10f;
                case 15: return 0.15f;
                case 16: return 0.20f;
                case 17: return 0.25f;
                case 18: return 0.30f;
                case 19: return 0.35f;
                case 20: return 0.40f;
                default: return intellectualSkill >= 12 ? 0f : 0f;
            }
        }
    }
}

