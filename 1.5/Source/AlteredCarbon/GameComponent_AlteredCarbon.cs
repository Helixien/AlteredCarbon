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
            personaStacksToAppearAsWorldPawns ??= new();
        }

        public Dictionary<PersonaData, int> personaStacksToAppearAsWorldPawns;

        public override void ExposeData()
        {
            Instance = this;
            base.ExposeData();
            Scribe_Collections.Look(ref this.personaStacksToAppearAsWorldPawns, "personaStacksToAppearAsWorldPawns", 
                LookMode.Deep, LookMode.Value, ref personaDataValues, ref intKeys);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                this.personaStacksToAppearAsWorldPawns ??= new Dictionary<PersonaData, int>();
            }
        }

        public bool CanBackup(Pawn pawn)
        {
            return pawn.Dead is false && pawn.IsColonist && pawn.HasPersonaStack(out var hediff_PersonaStack)
                && hediff_PersonaStack.def != AC_DefOf.AC_ArchotechStack;
        }

        public void Backup(Pawn pawn)
        {
            if (pawn.HasPersonaStack(out var stackHediff))
            {
                var copy = new PersonaData();
                copy.CopyFromPawn(pawn, stackHediff.SourceStack, copyRaceGenderInfo: true);
                copy.isCopied = true;
                copy.lastTimeBackedUp = Find.TickManager.TicksAbs;
                copy.RefreshDummyPawn();
            }
        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();
            foreach (var data in personaStacksToAppearAsWorldPawns.ToList())
            {
                if (Find.TickManager.TicksGame >= data.Value)
                {
                    var pawn = data.Key.GetDummyPawn;
                    Find.WorldPawns.AddPawn(pawn);
                    personaStacksToAppearAsWorldPawns.Remove(data.Key);
                }
            }
        }

        private List<int> intKeys;
        private List<PersonaData> personaDataValues;
    }
}