using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    [HotSwappable]
    public class GameComponent_AlteredCarbon : GameComponent
    {
        public static GameComponent_AlteredCarbon Instance;

        public GameComponent_AlteredCarbon()
        {
            Init();
        }

        public GameComponent_AlteredCarbon(Game game)
        {
            Init();
        }
        private void Init()
        {
            Instance = this;
            neuralStacksToAppearAsWorldPawns ??= new();
        }

        public Dictionary<NeuralData, int> neuralStacksToAppearAsWorldPawns;

        public override void ExposeData()
        {
            Instance = this;
            base.ExposeData();
            Scribe_Collections.Look(ref this.neuralStacksToAppearAsWorldPawns, "neuralStacksToAppearAsWorldPawns", 
                LookMode.Deep, LookMode.Value, ref neuralDataValues, ref intKeys);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                this.neuralStacksToAppearAsWorldPawns ??= new Dictionary<NeuralData, int>();
            }
        }

        public bool CanBackup(Pawn pawn)
        {
            return pawn.Dead is false && pawn.IsColonist && pawn.HasNeuralStack(out var hediff_NeuralStack)
                && hediff_NeuralStack.def != AC_DefOf.AC_ArchotechStack;
        }

        public void Backup(Pawn pawn)
        {
            if (pawn.HasNeuralStack(out var stackHediff))
            {
                var copy = new NeuralData();
                copy.CopyFromPawn(pawn, stackHediff.SourceStack, copyRaceGenderInfo: true);
                copy.isCopied = true;
                copy.lastTimeBackedUp = Find.TickManager.TicksAbs;
                copy.RefreshDummyPawn();
            }
        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();
            foreach (var data in neuralStacksToAppearAsWorldPawns.ToList())
            {
                if (Find.TickManager.TicksGame >= data.Value)
                {
                    var pawn = data.Key.DummyPawn;
                    Find.WorldPawns.AddPawn(pawn);
                    neuralStacksToAppearAsWorldPawns.Remove(data.Key);
                }
            }
        }

        private List<int> intKeys;
        private List<NeuralData> neuralDataValues;
    }
}