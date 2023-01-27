using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    public class StacksData : IExposable
    {
        public Pawn originalPawn;
        public CorticalStack originalStack;

        public List<Pawn> copiedPawns;
        public List<CorticalStack> copiedStacks;

        public List<Pawn> deadPawns;
        public void ExposeData()
        {
            Scribe_References.Look(ref originalPawn, "originalPawn", true);
            Scribe_References.Look(ref originalStack, "originalStack", true);
            Scribe_Collections.Look(ref copiedPawns, saveDestroyedThings: true, "copiedPawns", LookMode.Reference);
            Scribe_Collections.Look(ref copiedStacks, "copiedStacks", LookMode.Reference);
            Scribe_Collections.Look(ref deadPawns, saveDestroyedThings: true, "deadPawns", LookMode.Reference);

        }
    }
}

