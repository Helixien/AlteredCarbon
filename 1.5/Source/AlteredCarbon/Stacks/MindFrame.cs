using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{
    [HotSwappable]
    public class MindFrame : ThingWithStack
    {
        public int backupCreationDataTicks;

        public override string LabelNoCount
        {
            get
            {
                var label = base.LabelNoCount;
                label += " (" + this.PersonaData.PawnNameColored.ToStringSafe() + ")";
                return label;
            }
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            var personaData = PersonaData;
            if (personaData.ContainsPersona)
            {
                if (personaData.faction != null)
                {
                    stringBuilder.AppendLineTagged("AC.Faction".Translate() + ": " + personaData.faction.NameColored);
                }
                stringBuilder.Append("AC.AgeChronologicalTicks".Translate() + ": " + (int)(personaData.ageChronologicalTicks / 3600000) + "\n");
                var tile = this.Spawned ? this.Tile : Find.AnyPlayerHomeMap.Tile;
                var timeOfDate = personaData.lastTimeUpdated == null ? (string)"Unknown".Translate()
                    : GenDate.DateFullStringAt(personaData.lastTimeUpdated.Value, Find.WorldGrid.LongLatOf(tile));
                stringBuilder.Append("AC.TimeOfBackup".Translate(timeOfDate));
            }
            stringBuilder.Append(base.GetInspectString());
            return stringBuilder.ToString().TrimEndNewlines();
        }

        public override Graphic Graphic
        {
            get
            {
                var personaData = PersonaData;
                if (personaData.guestStatusInt == GuestStatus.Slave)
                {
                    return GetMindFrameGraphic(ref slaveGraphic, ref slaveGraphicData,
                        "Things/Item/MindFrame/SlaveMindFrame");
                }
                else if (personaData.faction == Faction.OfPlayer)
                {
                    return GetMindFrameGraphic(ref friendlyGraphic, ref friendlyGraphicData,
                        "Things/Item/MindFrame/FriendlyMindFrame");
                }
                else if (personaData.faction is null || !personaData.faction.HostileTo(Faction.OfPlayer))
                {
                    return GetMindFrameGraphic(ref strangerGraphic, ref strangerGraphicData,
                        "Things/Item/MindFrame/NeutralMindFrame");
                }
                else
                {
                    return GetMindFrameGraphic(ref hostileGraphic, ref hostileGraphicData,
                        "Things/Item/MindFrame/HostileMindFrame");
                }
            }
        }

        private Graphic GetMindFrameGraphic(ref Graphic graphic, ref GraphicData graphicData, string path)
        {
            if (graphic is null)
            {
                if (graphicData is null)
                {
                    graphicData = GetGraphicDataWithOtherPath(path);
                }
                graphic = graphicData.GraphicColoredFor(this);
            }
            return graphic;
        }

        public bool CanAutoRestorePawn
        {
            get
            {
                var personaData = PersonaData;
                if (personaData.restoreToEmptyStack)
                {
                    if (!AnyPersonaStackExist(personaData) && !AnyPawnExist(personaData))
                    {
                        return true;
                    }
                }
                return false;
            }
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

        public override void PreTraded(TradeAction action, Pawn playerNegotiator, ITrader trader)
        {
            base.PreTraded(action, playerNegotiator, trader);
            if (AC_Utils.generalSettings.enableSoldMindFramesCreatingPawnDuplicates
                && trader.Faction?.def.techLevel >= TechLevel.Spacer && Rand.Chance(0.15f))
            {
                var copy = new PersonaData();
                copy.CopyDataFrom(PersonaData);
                copy.faction = trader.Faction;
                Rand.PushState(copy.GetHashCode());
                GameComponent_DigitalStorage.Instance.personaStacksToAppearAsWorldPawns[copy] =
                    (int)(Find.TickManager.TicksGame + (new FloatRange(5f, 30f).RandomInRange * GenDate.TicksPerDay));
                Rand.PopState();
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad && PersonaData.ContainsPersona is false)
            {
                GeneratePersona();
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var g in base.GetGizmos())
            {
                yield return g;
            }
            yield return new Command_Toggle
            {
                defaultLabel = "AC.AutoLoad".Translate(),
                defaultDesc = "AC.AutoLoadDesc".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Gizmos/AutoLoadMindFrame"),
                isActive = () => autoLoad,
                toggleAction = delegate
                {
                    autoLoad = !autoLoad;
                }
            };
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref backupCreationDataTicks, "backupCreationDataTicks");
        }
    }
}