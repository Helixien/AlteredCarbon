using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    [HotSwappable]
    public class StackGroupData : IExposable
    {
        public Pawn originalPawn;
        public NeuralStack originalStack;

        public HashSet<Pawn> copiedPawns = new HashSet<Pawn>();
        public HashSet<NeuralStack> copiedStacks = new HashSet<NeuralStack>();
        public HashSet<Pawn> deadPawns = new HashSet<Pawn>();
        public void AssignRelationships(Pawn pawn)
        {
            if (this.originalPawn != null)
            {
                if (pawn != this.originalPawn)
                {
                    AssignOriginalCopyRelationships(this.originalPawn, pawn);
                }
                else
                {
                    foreach (Pawn copiedPawn in this.copiedPawns)
                    {
                        if (copiedPawn != null && pawn != copiedPawn)
                        {
                            AssignOriginalCopyRelationships(pawn, copiedPawn);
                        }
                    }
                }
            }


            foreach (Pawn copiedPawn in this.copiedPawns)
            {
                if (this.copiedPawns.Contains(pawn) && copiedPawn != null && pawn != copiedPawn && pawn != this.originalPawn)
                {
                    AssignRelation(pawn, AC_DefOf.AC_Copy, copiedPawn);
                    AssignRelation(copiedPawn, AC_DefOf.AC_Copy, pawn);
                }
            }
        }

        private void AssignOriginalCopyRelationships(Pawn original, Pawn copy)
        {
            AssignRelation(copy, AC_DefOf.AC_Original, original);
            AssignRelation(original, AC_DefOf.AC_Copy, copy);

            foreach (var relationship in original.relations.DirectRelations)
            {
                if (relationshipBlackListForCopies.Contains(relationship.def) is false && relationship.def.implied is false)
                {
                    copy.relations.AddDirectRelation(relationship.def, relationship.otherPawn);
                }
            }

            foreach (var related in original.relations.PotentiallyRelatedPawns)
            {
                if (related.CanThink())
                {
                    foreach (var thought in related.needs.mood.thoughts.memories.Memories.ToList())
                    {
                        if (thought.otherPawn == original)
                        {
                            var thoughtCopy = (Thought_Memory)ThoughtMaker.MakeThought(thought.def);
                            thoughtCopy.moodPowerFactor = thought.moodPowerFactor;
                            thoughtCopy.moodOffset = thought.moodOffset;
                            if (thought is Thought_MemorySocial social && thoughtCopy is Thought_MemorySocial socialCopy)
                            {
                                socialCopy.opinionOffset = social.opinionOffset;
                            }
                            related.needs.mood.thoughts.memories.TryGainMemory(thoughtCopy, copy);
                        }
                    }
                }
            }
        }

        public static HashSet<PawnRelationDef> relationshipBlackListForCopies = new HashSet<PawnRelationDef>
        {
            PawnRelationDefOf.Lover, PawnRelationDefOf.Spouse, PawnRelationDefOf.Bond,
            PawnRelationDefOf.Overseer, PawnRelationDefOf.Fiance, PawnRelationDefOf.ExSpouse, PawnRelationDefOf.ExLover,
            AC_DefOf.AC_Copy, AC_DefOf.AC_Original
        };

        public void AssignRelation(Pawn pawn, PawnRelationDef def, Pawn otherPawn)
        {
            pawn.relations.hidePawnRelations = false;
            pawn.relations.everSeenByPlayer = true;

            otherPawn.relations.hidePawnRelations = false;
            otherPawn.relations.everSeenByPlayer = true;

            if (!pawn.relations.DirectRelationExists(def, otherPawn))
            {
                pawn.relations.AddDirectRelation(def, otherPawn);
            }
        }
        public void ExposeData()
        {
            Scribe_References.Look(ref originalPawn, "originalPawn", true);
            Scribe_References.Look(ref originalStack, "originalStack", true);
            Scribe_Collections.Look(ref copiedPawns, saveDestroyedThings: true, "copiedPawns", LookMode.Reference);
            Scribe_Collections.Look(ref copiedStacks, "copiedStacks", LookMode.Reference);
            Scribe_Collections.Look(ref deadPawns, saveDestroyedThings: true, "deadPawns", LookMode.Reference);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                copiedPawns ??= new HashSet<Pawn>();
                copiedStacks ??= new HashSet<NeuralStack>();
                deadPawns ??= new HashSet<Pawn>();
            }
        }
    }
}

