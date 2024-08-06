using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{
    [HotSwappable]
    public class PersonaPrint : ThingWithPersonaData
    {
        public bool allowAutomaticRestoration = true;

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
                var timeOfDate = personaData.lastTimeBackedUp == null ? (string)"Unknown".Translate()
                    : GenDate.DateFullStringAt(personaData.lastTimeBackedUp.Value, Find.WorldGrid.LongLatOf(tile));
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
                    return GetPersonaPrintGraphic(ref slaveGraphic, ref slaveGraphicData,
                        "Things/Item/PersonaPrint/SlavePersonaPrint");
                }
                else if (personaData.faction == Faction.OfPlayer)
                {
                    return GetPersonaPrintGraphic(ref friendlyGraphic, ref friendlyGraphicData,
                        "Things/Item/PersonaPrint/FriendlyPersonaPrint");
                }
                else if (personaData.faction is null || !personaData.faction.HostileTo(Faction.OfPlayer))
                {
                    return GetPersonaPrintGraphic(ref strangerGraphic, ref strangerGraphicData,
                        "Things/Item/PersonaPrint/NeutralPersonaPrint");
                }
                else
                {
                    return GetPersonaPrintGraphic(ref hostileGraphic, ref hostileGraphicData,
                        "Things/Item/PersonaPrint/HostilePersonaPrint");
                }
            }
        }

        private Graphic GetPersonaPrintGraphic(ref Graphic graphic, ref GraphicData graphicData, string path)
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

        public bool CanAutoRestorePawn(PersonaData otherData)
        {
            if (allowAutomaticRestoration)
            {
                var personaData = PersonaData;
                if (otherData.IsPresetPawn(personaData) is false)
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        public override void PreTraded(TradeAction action, Pawn playerNegotiator, ITrader trader)
        {
            base.PreTraded(action, playerNegotiator, trader);
            if (AC_Utils.generalSettings.enableSoldPersonaPrintsCreatingPawnDuplicates
                && trader.Faction?.def.techLevel >= TechLevel.Spacer && Rand.Chance(0.15f))
            {
                var copy = new PersonaData();
                copy.CopyDataFrom(PersonaData);
                copy.faction = trader.Faction;
                Rand.PushState(copy.GetHashCode());
                GameComponent_AlteredCarbon.Instance.personaStacksToAppearAsWorldPawns[copy] =
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
                icon = ContentFinder<Texture2D>.Get("UI/Gizmos/AutoLoadPersonaPrint"),
                isActive = () => autoLoad,
                toggleAction = delegate
                {
                    autoLoad = !autoLoad;
                }
            };
            yield return new Command_Toggle
            {
                defaultLabel = "AC.AllowAutomaticRestoration".Translate(),
                defaultDesc = "AC.AllowAutomaticRestorationDesc".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Gizmos/EnableAutoRestore"),
                isActive = () => allowAutomaticRestoration,
                toggleAction = delegate
                {
                    allowAutomaticRestoration = !allowAutomaticRestoration;
                }
            };
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref allowAutomaticRestoration, "allowAutomaticRestoration", true);
        }
    }
}