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
            backedUpStacks ??= new Dictionary<int, PersonaData>();
        }
        public void ClearBackedUpStacksIfNoStackStorages()
        {
            foreach (var map in Find.Maps)
            {
                if (map.listerThings.ThingsOfDef(AC_Extra_DefOf.AC_StackArray).Any(x => x.Faction == Faction.OfPlayer))
                {
                    return;
                }
            }
            backedUpStacks.Clear();
        }
        public Dictionary<int, PersonaData> backedUpStacks;
        public IEnumerable<PersonaData> StoredBackedUpStacks => this.backedUpStacks.Values;
        public PersonaData FirstPersonaStackToRestore
        {
            get
            {
                foreach (var personaData in StoredBackedUpStacks)
                {
                    if (personaData.restoreToEmptyStack)
                    {
                        if (!AnyCorticalStackExist(personaData) && !AnyPawnExist(personaData))
                        {
                            return personaData;
                        }
                    }
                }
                return null;
            }
        }

        private static bool AnyCorticalStackExist(PersonaData personaData)
        {
            foreach (var map in Find.Maps)
            {
                if (map.listerThings.ThingsOfDef(AC_DefOf.VFEU_FilledCorticalStack).Cast<CorticalStack>()
                    .Any(x => x.PersonaData.IsPresetPawn(personaData) && x.Spawned && !x.Destroyed))
                {
                    return true;
                }
                if (map.listerThings.ThingsOfDef(AC_Extra_DefOf.AC_StackArray).Cast<Building_StackStorage>()
                    .Any(x => x.StoredStacks.Any(y => y.PersonaData.IsPresetPawn(personaData))))
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
                if (personaData.IsPresetPawn(pawn) && pawn.HasCorticalStack(out _))
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
            Scribe_Collections.Look(ref this.backedUpStacks, "backedUpStacks", LookMode.Value, LookMode.Deep, ref intKeys, ref personaDataValues);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                this.backedUpStacks ??= new Dictionary<int, PersonaData>();
            }
        }

        public void PerformStackRestoration(Pawn doer)
        {
            var stackRestoreTo = (CorticalStack)ThingMaker.MakeThing(AC_DefOf.VFEU_FilledCorticalStack);
            var personaDataToRestore = FirstPersonaStackToRestore;
            stackRestoreTo.PersonaData.CopyDataFrom(personaDataToRestore, true);
            AlteredCarbonManager.Instance.RegisterStack(stackRestoreTo);
            backedUpStacks.Remove(personaDataToRestore.PawnID);
            Messages.Message("AC.SuccessfullyRestoredStackFromBackup".Translate(doer.Named("PAWN")), stackRestoreTo, MessageTypeDefOf.TaskCompletion);
            GenPlace.TryPlaceThing(stackRestoreTo, doer.Position, doer.Map, ThingPlaceMode.Near);
        }

        public bool CanBackup(Pawn pawn)
        {
            return pawn.Dead is false && pawn.IsColonist && pawn.HasCorticalStack(out var hediff_CorticalStack)
                && hediff_CorticalStack.def != AC_DefOf.AC_ArchoStack;
        }

        public void Backup(Pawn pawn)
        {
            if (pawn.HasCorticalStack(out var stackHediff))
            {
                var copy = new PersonaData();
                copy.CopyFromPawn(pawn, stackHediff.SourceStack, copyRaceGenderInfo: true);
                if (this.backedUpStacks.TryGetValue(copy.PawnID, out var oldBackup))
                {
                    copy.restoreToEmptyStack = oldBackup.restoreToEmptyStack;
                }
                copy.isCopied = true;
                copy.lastTimeUpdated = Find.TickManager.TicksAbs;
                copy.RefreshDummyPawn();
                this.backedUpStacks[copy.PawnID] = copy;
            }
        }

        public void BackupAllColonistsWithStacks()
        {
            int num = 0;
            foreach (var pawn in AlteredCarbonManager.Instance.PawnsWithStacks)
            {
                if (CanBackup(pawn))
                {
                    num++;
                    Backup(pawn);
                }
            }
            Messages.Message("AC.BackupsCompleted".Translate(num), MessageTypeDefOf.NeutralEvent);
        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();
            if (Find.TickManager.TicksGame % GenDate.TicksPerDay == 0)
            {
                foreach (var map in Find.Maps)
                {
                    foreach (var storage in map.listerThings.ThingsOfDef(AC_Extra_DefOf.AC_StackArray)
                        .Where(x => x.Faction == Faction.OfPlayer).OfType<Building_StackStorage>())
                    {
                        if (storage.backupIsEnabled && storage.compPower.PowerOn)
                        {
                            BackupAllColonistsWithStacks();
                            break;
                        }
                    }
                }
            }
        }

        private List<int> intKeys;
        private List<PersonaData> personaDataValues;
    }
}