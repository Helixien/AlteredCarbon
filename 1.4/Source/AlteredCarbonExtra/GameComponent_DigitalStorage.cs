using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    public class GameComponent_DigitalStorage : GameComponent
    {
        public static GameComponent_DigitalStorage Instance;
        public GameComponent_DigitalStorage(Game game)
        {
            Instance = this;
            backedUpStacks ??= new Dictionary<int, PersonaData>();
        }

        public Dictionary<int, PersonaData> backedUpStacks;
        public IEnumerable<PersonaData> StoredBackedUpStacks => this.backedUpStacks.Values;
        public PersonaData FirstPersonaStackToRestore
        {
            get
            {
                var pawns = AlteredCarbonManager.Instance.PawnsWithStacks.Concat(AlteredCarbonManager.Instance.deadPawns ?? Enumerable.Empty<Pawn>()).ToList();
                foreach (var personaData in StoredBackedUpStacks)
                {
                    if (personaData.restoreToEmptyStack)
                    {
                        foreach (var pawn in pawns)
                        {
                            if (pawn != null && personaData.IsMatchingPawn(pawn))
                            {
                                if (pawn.Destroyed && pawn.Corpse is null || pawn.Corpse != null
                                    && pawn.Corpse.Destroyed || pawn.ParentHolder is null ||
                                    pawn.health.hediffSet.GetFirstHediffOfDef(AC_DefOf.VFEU_CorticalStack) is null)
                                {
                                    if (!CorticalStack.corticalStacks.Any(x => x.PersonaData.pawnID == personaData.pawnID
                                     && x.Spawned && !x.Destroyed))
                                    {
                                        return personaData;
                                    }
                                }
                            }
                        }
                    }
                }
                return null;
            }
        }

        public override void ExposeData()
        {
            Instance = this;
            base.ExposeData();
            Scribe_Collections.Look(ref this.backedUpStacks, "backedUpStacks", LookMode.Value, LookMode.Deep, ref intKeys, ref personaDataValues);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (this.backedUpStacks is null)
                {
                    this.backedUpStacks = new Dictionary<int, PersonaData>();
                }
            }
        }

        public void PerformStackRestoration(Pawn doer)
        {
            var stackRestoreTo = (CorticalStack)ThingMaker.MakeThing(AC_DefOf.VFEU_FilledCorticalStack);
            var personaDataToRestore = FirstPersonaStackToRestore;
            stackRestoreTo.PersonaData.CopyDataFrom(personaDataToRestore, true);
            AlteredCarbonManager.Instance.RegisterStack(stackRestoreTo);
            backedUpStacks.Remove(personaDataToRestore.pawnID);
            Messages.Message("AC.SuccessfullyRestoredStackFromBackup".Translate(doer.Named("PAWN")), stackRestoreTo, MessageTypeDefOf.TaskCompletion);
            GenPlace.TryPlaceThing(stackRestoreTo, doer.Position, doer.Map, ThingPlaceMode.Near);
        }

        public bool CanBackup(Pawn pawn)
        {
            return pawn.IsColonist && pawn.HasCorticalStack(out _);
        }

        public void Backup(Pawn pawn)
        {
            var copy = new PersonaData();
            copy.CopyPawn(pawn, copyRaceGenderInfo: true);
            copy.isCopied = true;
            copy.lastTimeUpdated = Find.TickManager.TicksAbs;
            copy.RefreshDummyPawn();
            this.backedUpStacks[copy.pawnID] = copy;
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
            foreach (var storage in Building_StackStorage.building_StackStorages)
            {
                if (storage.backupIsEnabled && storage.compPower.PowerOn)
                {
                    if (Find.TickManager.TicksGame % GenDate.TicksPerDay == 0)
                    {
                        BackupAllColonistsWithStacks();
                    }
                }
            }
        }

        private List<int> intKeys;
        private List<PersonaData> personaDataValues;
    }
}