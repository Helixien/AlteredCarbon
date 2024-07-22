using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    [HotSwappable]
    public class GameComponent_DigitalStorage : GameComponent
    {
        public static GameComponent_DigitalStorage Instance;

        public GameComponent_DigitalStorage()
        {
            Init();
        }

        public GameComponent_DigitalStorage(Game game)
        {
            Init();
        }
        private void Init()
        {
            Instance = this;
            personaStacksToAppearAsWorldPawns ??= new();
        }

        public Dictionary<PersonaData, int> personaStacksToAppearAsWorldPawns;

        public PersonaData FirstPersonaStackToRestore(Map map)
        {
            foreach (var matrix in map.listerThings.AllThings.OfType<Building_PersonaMatrix>())
            {
                foreach (var frame in matrix.StoredMindFrames)
                {
                    var personaData = frame.PersonaData;
                    if (personaData.restoreToEmptyStack)
                    {
                        if (!AnyPersonaStackExist(personaData) && !AnyPawnExist(personaData))
                        {
                            return personaData;
                        }
                    }
                }
            }
            return null;
        }

        private static bool AnyPersonaStackExist(PersonaData personaData)
        {
            foreach (var map in Find.Maps)
            {
                if (map.listerThings.ThingsOfDef(AC_DefOf.AC_FilledPersonaStack).Cast<PersonaStack>()
                    .Any(x => x.PersonaData.IsPresetPawn(personaData) && x.Spawned && !x.Destroyed))
                {
                    return true;
                }
                if (map.listerThings.ThingsOfDef(AC_DefOf.AC_PersonaMatrix).Cast<Building_PersonaMatrix>()
                    .Any(x => x.StoredMindFrames.Any(y => y.PersonaData.IsPresetPawn(personaData))))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool AnyPawnExist(PersonaData personaData)
        {
            foreach (var pawn in PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead)
            {
                if (personaData.IsPresetPawn(pawn) && pawn.HasPersonaStack(out _))
                {
                    return true;
                }
            }
            return false;
        }
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

        public void PerformStackRestoration(Pawn doer, PersonaData personaDataToRestore)
        {
            var stackRestoreTo = (PersonaStack)ThingMaker.MakeThing(AC_DefOf.AC_FilledPersonaStack);
            stackRestoreTo.PersonaData.CopyDataFrom(personaDataToRestore, true);
            AlteredCarbonManager.Instance.RegisterStack(stackRestoreTo);
            Messages.Message("AC.SuccessfullyRestoredStackFromBackup".Translate(doer.Named("PAWN")), stackRestoreTo, MessageTypeDefOf.TaskCompletion);
            GenPlace.TryPlaceThing(stackRestoreTo, doer.Position, doer.Map, ThingPlaceMode.Near);
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
                copy.lastTimeUpdated = Find.TickManager.TicksAbs;
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
                    Log.Message(data.Key.GetDummyPawn + " should appear now");
                }
            }
        }

        private List<int> intKeys;
        private List<PersonaData> personaDataValues;
    }
}