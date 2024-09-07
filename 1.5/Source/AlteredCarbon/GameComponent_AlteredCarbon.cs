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