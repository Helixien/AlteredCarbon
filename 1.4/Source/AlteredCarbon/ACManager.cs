using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AlteredCarbon
{
    public class AlteredCarbonManager : GameComponent
    {
        public static AlteredCarbonManager Instance;
        public AlteredCarbonManager()
        {
            Instance = this;
            ResetStaticData();
        }

        public AlteredCarbonManager(Game game)
        {
            Instance = this;
            ResetStaticData();
        }

        public void ResetStaticData()
        {
            CorticalStack.corticalStacks?.Clear();
        }

        public void ResetStackLimitIfNeeded(ThingDef def)
        {
            if (def.stackLimit != 1)
            {
                def.stackLimit = 1;
                def.drawGUIOverlay = false;
            }
        }

        public void PreInit()
        {
            if (stacksIndex == null)
            {
                stacksIndex = new Dictionary<int, CorticalStack>();
            }

            if (pawnsWithStacks == null)
            {
                pawnsWithStacks = new HashSet<Pawn>();
            }

            if (emptySleeves == null)
            {
                emptySleeves = new HashSet<Pawn>();
            }

            if (deadPawns == null)
            {
                deadPawns = new HashSet<Pawn>();
            }

            ResetStackLimitIfNeeded(AC_DefOf.VFEU_FilledCorticalStack);
        }
        public override void StartedNewGame()
        {
            base.StartedNewGame();
            PreInit();
        }

        public override void LoadedGame()
        {
            base.LoadedGame();
            PreInit();
        }
        public void TryAddRelationships(Pawn pawn)
        {
            if (ModCompatibility.HelixienAlteredCarbonIsActive)
            {
                if (pawn.HasCorticalStack(out Hediff_CorticalStack hediff) && stacksRelationships.TryGetValue(hediff.PersonaData.stackGroupID, out StacksData stackData))
                {
                    if (stackData.originalPawn != null)
                    {
                        if (pawn != stackData.originalPawn)
                        {
                            pawn.relations.AddDirectRelation(AC_DefOf.AC_Original, stackData.originalPawn);
                            stackData.originalPawn.relations.AddDirectRelation(AC_DefOf.AC_Copy, pawn);
                        }
                        else if (stackData.copiedPawns != null)
                        {
                            foreach (Pawn copiedPawn in stackData.copiedPawns)
                            {
                                if (pawn != copiedPawn)
                                {
                                    pawn.relations.AddDirectRelation(AC_DefOf.AC_Original, copiedPawn);
                                    copiedPawn.relations.AddDirectRelation(AC_DefOf.AC_Copy, pawn);
                                }
                            }
                        }
                    }
                    else if (stackData.copiedPawns != null)
                    {
                        foreach (Pawn copiedPawn in stackData.copiedPawns)
                        {
                            if (pawn != copiedPawn)
                            {
                                pawn.relations.AddDirectRelation(AC_DefOf.AC_Copy, copiedPawn);
                                copiedPawn.relations.AddDirectRelation(AC_DefOf.AC_Copy, pawn);
                            }
                        }
                    }
                }
                else
                {
                    foreach (KeyValuePair<int, StacksData> stackGroup in stacksRelationships)
                    {
                        if (stackGroup.Value.copiedPawns != null)
                        {
                            if (pawn == stackGroup.Value.originalPawn && stackGroup.Value.copiedPawns != null)
                            {
                                foreach (Pawn copiedPawn in stackGroup.Value.copiedPawns)
                                {
                                    if (pawn != copiedPawn)
                                    {
                                        pawn.relations.AddDirectRelation(AC_DefOf.AC_Original, copiedPawn);
                                        copiedPawn.relations.AddDirectRelation(AC_DefOf.AC_Copy, pawn);
                                    }
                                }
                            }
                            else if (stackGroup.Value.copiedPawns != null)
                            {
                                foreach (Pawn copiedPawn in stackGroup.Value.copiedPawns)
                                {
                                    if (pawn == copiedPawn && stackGroup.Value.originalPawn != null)
                                    {
                                        pawn.relations.AddDirectRelation(AC_DefOf.AC_Original, stackGroup.Value.originalPawn);
                                        stackGroup.Value.originalPawn.relations.AddDirectRelation(AC_DefOf.AC_Copy, pawn);
                                    }
                                }
                            }
                        }
                    }
                }

                if (pawn.IsCopy())
                {
                    pawn.needs.mood.thoughts.memories.TryGainMemory(AC_DefOf.AC_JustCopy);
                    Pawn otherPawn = pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Spouse);
                    if (pawn.relations.TryRemoveDirectRelation(PawnRelationDefOf.Spouse, otherPawn))
                    {
                        pawn.needs.mood.thoughts.memories.TryGainMemory(AC_DefOf.AC_LostMySpouse, otherPawn);
                    }

                    otherPawn = pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Fiance);
                    if (pawn.relations.TryRemoveDirectRelation(PawnRelationDefOf.Fiance, otherPawn))
                    {
                        pawn.needs.mood.thoughts.memories.TryGainMemory(AC_DefOf.AC_LostMyFiance, otherPawn);
                    }

                    otherPawn = pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Lover);
                    if (pawn.relations.TryRemoveDirectRelation(PawnRelationDefOf.Lover, otherPawn))
                    {
                        pawn.needs.mood.thoughts.memories.TryGainMemory(AC_DefOf.AC_LostMyLover, otherPawn);
                    }
                }
            }
        }

        public void ReplacePawnWithStack(Pawn pawn, CorticalStack stack)
        {
            if (pawn.HasCorticalStack(out Hediff_CorticalStack hediff))
            {
                stack.PersonaData.stackGroupID = hediff.PersonaData.stackGroupID;
                if (stacksRelationships.TryGetValue(hediff.PersonaData.stackGroupID, out StacksData stackData))
                {
                    if (stackData.originalPawn == pawn)
                    {
                        stackData.originalPawn = null;
                        stackData.originalStack = stack;
                    }
                    else if (stackData.copiedPawns.Contains(pawn))
                    {
                        stackData.copiedPawns.Remove(pawn);
                        if (stackData.copiedStacks == null)
                        {
                            stackData.copiedStacks = new List<CorticalStack>();
                        }

                        stackData.copiedStacks.Add(stack);
                    }
                }
            }
        }

        public void ReplaceStackWithPawn(CorticalStack stack, Pawn pawn)
        {
            if (pawn.HasCorticalStack(out Hediff_CorticalStack hediff))
            {
                hediff.PersonaData.stackGroupID = stack.PersonaData.stackGroupID;
                if (stacksRelationships.TryGetValue(hediff.PersonaData.stackGroupID, out StacksData stackData))
                {
                    if (stackData.originalStack == stack)
                    {
                        stackData.originalStack = null;
                        stackData.originalPawn = pawn;
                    }
                    else if (stackData.copiedStacks?.Contains(stack) ?? false)
                    {
                        stackData.copiedStacks.Remove(stack);
                        if (stackData.copiedPawns == null)
                        {
                            stackData.copiedPawns = new List<Pawn>();
                        }

                        stackData.copiedPawns.Add(pawn);
                    }
                }
                pawnsWithStacks.Add(pawn);
            }
        }

        public void RegisterStack(CorticalStack stack)
        {
            if (!stacksRelationships.ContainsKey(stack.PersonaData.stackGroupID))
            {
                stacksRelationships[stack.PersonaData.stackGroupID] = new StacksData();
            }
            if (stack.PersonaData.isCopied)
            {
                if (stacksRelationships[stack.PersonaData.stackGroupID].copiedStacks == null)
                {
                    stacksRelationships[stack.PersonaData.stackGroupID].copiedStacks = new List<CorticalStack>();
                }

                stacksRelationships[stack.PersonaData.stackGroupID].copiedStacks.Add(stack);
            }
            else
            {
                stacksRelationships[stack.PersonaData.stackGroupID].originalStack = stack;
            }
        }

        public void RegisterPawn(Pawn pawn)
        {
            if (pawn.HasCorticalStack(out Hediff_CorticalStack hediff))
            {
                if (!stacksRelationships.ContainsKey(hediff.PersonaData.stackGroupID))
                {
                    stacksRelationships[hediff.PersonaData.stackGroupID] = new StacksData();
                }
                if (hediff.PersonaData.isCopied)
                {
                    if (stacksRelationships[hediff.PersonaData.stackGroupID].copiedPawns == null)
                    {
                        stacksRelationships[hediff.PersonaData.stackGroupID].copiedPawns = new List<Pawn>();
                    }

                    stacksRelationships[hediff.PersonaData.stackGroupID].copiedPawns.Add(pawn);
                }
                else
                {
                    stacksRelationships[hediff.PersonaData.stackGroupID].originalPawn = pawn;
                }
                emptySleeves.Remove(pawn);
                pawnsWithStacks.Add(pawn);
            }
        }

        public void RegisterSleeve(Pawn pawn, int stackGroupID = -1)
        {
            pawnsWithStacks.Remove(pawn);
            emptySleeves.Add(pawn);
            if (stackGroupID != -1 && stacksRelationships.ContainsKey(stackGroupID))
            {
                if (stacksRelationships[stackGroupID].deadPawns == null)
                {
                    stacksRelationships[stackGroupID].deadPawns = new List<Pawn>();
                }

                stacksRelationships[stackGroupID].deadPawns.Add(pawn);
            }
        }

        public int GetStackGroupID(CorticalStack corticalStack)
        {
            if (corticalStack.PersonaData.stackGroupID != 0)
            {
                return corticalStack.PersonaData.stackGroupID;
            }

            return stacksRelationships != null ? stacksRelationships.Count + 1 : 0;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            pawnsWithStacks.RemoveWhere(x => x is null || x.Destroyed);
            Scribe_Collections.Look<int, CorticalStack>(ref stacksIndex, "stacksIndex", LookMode.Value, LookMode.Reference, ref pawnKeys, ref stacksValues);
            Scribe_Collections.Look<Pawn>(ref pawnsWithStacks, "pawnsWithStacks", LookMode.Reference);
            Scribe_Collections.Look<Pawn>(ref emptySleeves, "emptySleeves", LookMode.Reference);
            Scribe_Collections.Look<Pawn>(ref deadPawns, saveDestroyedThings: true, "deadPawns", LookMode.Reference);
            Scribe_Collections.Look<int, StacksData>(ref stacksRelationships, "stacksRelationships", LookMode.Value, LookMode.Deep, ref stacksRelationshipsKeys, ref stacksRelationshipsValues);
            pawnsWithStacks.RemoveWhere(x => x is null || x.Destroyed);
            Instance = this;
        }

        public Dictionary<int, StacksData> stacksRelationships = new Dictionary<int, StacksData>();
        private List<int> stacksRelationshipsKeys = new List<int>();
        private List<StacksData> stacksRelationshipsValues = new List<StacksData>();

        private HashSet<Pawn> pawnsWithStacks = new HashSet<Pawn>();
        public HashSet<Pawn> PawnsWithStacks
        {
            get
            {
                if (pawnsWithStacks is null)
                {
                    pawnsWithStacks = new HashSet<Pawn>();
                }
                return pawnsWithStacks.Where(x => x != null).ToHashSet();
            }
        }

        public HashSet<Pawn> emptySleeves = new HashSet<Pawn>();
        public HashSet<Pawn> deadPawns = new HashSet<Pawn>();

        private Dictionary<int, CorticalStack> stacksIndex;
        public Dictionary<int, CorticalStack> StacksIndex
        {
            get
            {
                if (stacksIndex is null)
                {
                    stacksIndex = new Dictionary<int, CorticalStack>();
                }
                return stacksIndex;
            }
        }
        private List<int> pawnKeys = new List<int>();
        private List<CorticalStack> stacksValues = new List<CorticalStack>();
    }
}