using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AlteredCarbon
{
    [HotSwappable]
    public class AlteredCarbonManager : GameComponent
    {
        public static AlteredCarbonManager Instance;
        public AlteredCarbonManager()
        {
            Instance = this;
        }
        public AlteredCarbonManager(Game game)
        {
            Instance = this;
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
            stacksIndex ??= new Dictionary<int, CorticalStack>();
            pawnsWithStacks ??= new HashSet<Pawn>();
            emptySleeves ??= new HashSet<Pawn>();
            deadPawns ??= new HashSet<Pawn>();
            ResetStackLimitIfNeeded(AC_DefOf.VFEU_FilledCorticalStack);
            if (AC_DefOf.AC_FilledArchoStack != null)
            {
                ResetStackLimitIfNeeded(AC_DefOf.AC_FilledArchoStack);
            }
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
        public void TryAddRelationships(Pawn pawn, StackGroupData stackData)
        {
            if (ModCompatibility.HelixienAlteredCarbonIsActive)
            {
                stackData.AssignRelationships(pawn);
                if (pawn.IsCopy() && pawn.CanThink())
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
            var stackData = stack.PersonaData.StackGroupData;
            if (stackData.originalPawn == pawn)
            {
                stackData.originalPawn = null;
                stackData.originalStack = stack;
            }
            else if (stackData.copiedPawns.Contains(pawn))
            {
                stackData.copiedPawns.Remove(pawn);
                stackData.copiedStacks.Add(stack);
            }
        }

        public void ReplaceStackWithPawn(CorticalStack stack, Pawn pawn)
        {
            var stackData = stack.PersonaData.StackGroupData;
            if (stackData.originalStack == stack)
            {
                stackData.originalStack = null;
                stackData.originalPawn = pawn;
            }
            else if (stackData.copiedStacks.Contains(stack))
            {
                stackData.copiedStacks.Remove(stack);
                stackData.copiedPawns.Add(pawn);
            }
        }

        public void RegisterStack(CorticalStack stack)
        {
            var stackData = stack.PersonaData.StackGroupData;
            if (stack.PersonaData.isCopied)
            {
                stackData.copiedStacks.Add(stack);
            }
            else
            {
                stackData.originalStack = stack;
            }
            stack.PersonaData.hostPawn = null;
        }

        public void RegisterPawn(Pawn pawn)
        {
            if (pawn.HasCorticalStack(out Hediff_CorticalStack hediff))
            {
                var stackData = hediff.PersonaData.StackGroupData;
                if (hediff.PersonaData.isCopied)
                {
                    stackData.copiedPawns.Add(pawn);
                }
                else
                {
                    stackData.originalPawn = pawn;
                }
                emptySleeves.Remove(pawn);
                pawnsWithStacks.Add(pawn);
            }
        }

        public void RegisterSleeve(Pawn pawn, CorticalStack stack)
        {
            pawnsWithStacks.Remove(pawn);
            emptySleeves.Add(pawn);
            StacksIndex[pawn.thingIDNumber] = stack;
            var stackData = stack.PersonaData.StackGroupData;
            stackData.deadPawns.Add(pawn);
        }

        public int GetStackGroupID(CorticalStack corticalStack)
        {
            if (corticalStack.PersonaData.stackGroupID != 0)
            {
                return corticalStack.PersonaData.stackGroupID;
            }
            return stacksRelationships.Count + 1;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            pawnsWithStacks.RemoveWhere(x => x is null || x.Destroyed);
            Scribe_Collections.Look(ref stacksIndex, "stacksIndex", LookMode.Value, LookMode.Reference, ref pawnKeys, ref stacksValues);
            Scribe_Collections.Look(ref pawnsWithStacks, "pawnsWithStacks", LookMode.Reference);
            Scribe_Collections.Look(ref emptySleeves, "emptySleeves", LookMode.Reference);
            Scribe_Collections.Look(ref deadPawns, saveDestroyedThings: true, "deadPawns", LookMode.Reference);
            Scribe_Collections.Look(ref stacksRelationships, "stacksRelationships", LookMode.Value, LookMode.Deep, ref stacksRelationshipsKeys, ref stacksRelationshipsValues);
            pawnsWithStacks.RemoveWhere(x => x is null || x.Destroyed);
            Instance = this;
        }

        public Dictionary<int, StackGroupData> stacksRelationships = new Dictionary<int, StackGroupData>();
        private List<int> stacksRelationshipsKeys = new List<int>();
        private List<StackGroupData> stacksRelationshipsValues = new List<StackGroupData>();

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