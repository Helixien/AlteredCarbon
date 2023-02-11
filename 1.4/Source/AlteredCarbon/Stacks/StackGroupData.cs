using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    public class StackGroupData : IExposable
    {
        public Pawn originalPawn;
        public CorticalStack originalStack;

        public HashSet<Pawn> copiedPawns = new HashSet<Pawn>();
        public HashSet<CorticalStack> copiedStacks = new HashSet<CorticalStack>();
        public HashSet<Pawn> deadPawns = new HashSet<Pawn>();
        public void AssignRelationships(Pawn pawn)
        {
            if (this.originalPawn != null)
            {
                if (pawn != this.originalPawn)
                {
                    AssignRelation(pawn, AC_DefOf.AC_Original, originalPawn, 1);
                    AssignRelation(originalPawn, AC_DefOf.AC_Copy, pawn, 2);
                }
                else
                {
                    foreach (Pawn copiedPawn in this.copiedPawns)
                    {
                        if (copiedPawn != null && pawn != copiedPawn)
                        {
                            AssignRelation(copiedPawn, AC_DefOf.AC_Original, pawn, 3);
                            AssignRelation(pawn, AC_DefOf.AC_Copy, copiedPawn, 4);
                        }
                    }
                }
            }

            foreach (Pawn copiedPawn in this.copiedPawns)
            {
                if (this.copiedPawns.Contains(pawn) && copiedPawn != null && pawn != copiedPawn && pawn != this.originalPawn)
                {
                    AssignRelation(pawn, AC_DefOf.AC_Copy, copiedPawn, 5);
                    AssignRelation(copiedPawn, AC_DefOf.AC_Copy, pawn, 6);
                }
            }
        }

        public void AssignRelation(Pawn pawn, PawnRelationDef def, Pawn otherPawn, int lineCount)
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
                copiedStacks ??= new HashSet<CorticalStack>();
                deadPawns ??= new HashSet<Pawn>();
            }
        }
    }
}

