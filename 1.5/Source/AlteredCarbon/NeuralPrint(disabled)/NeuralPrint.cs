using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{
    [HotSwappable]
    public class NeuralPrint : ThingWithNeuralData
    {
        public bool allowAutomaticRestoration = true;

        public override string LabelNoCount
        {
            get
            {
                var label = base.LabelNoCount;
                label += " (" + this.NeuralData.PawnNameColored.ToStringSafe() + ")";
                return label;
            }
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            var neuralData = NeuralData;
            if (neuralData.ContainsNeural)
            {
                if (neuralData.faction != null)
                {
                    stringBuilder.AppendLineTagged("AC.Faction".Translate() + ": " + neuralData.faction.NameColored);
                }
                stringBuilder.Append("AC.AgeChronologicalTicks".Translate() + ": " + (int)(neuralData.ageChronologicalTicks / 3600000) + "\n");
                var tile = this.Spawned ? this.Tile : Find.AnyPlayerHomeMap.Tile;
                var timeOfDate = neuralData.lastTimeBackedUp == null ? (string)"Unknown".Translate()
                    : GenDate.DateFullStringAt(neuralData.lastTimeBackedUp.Value, Find.WorldGrid.LongLatOf(tile));
                stringBuilder.Append("AC.TimeOfBackup".Translate(timeOfDate));
            }
            stringBuilder.Append(base.GetInspectString());
            return stringBuilder.ToString().TrimEndNewlines();
        }

        public override Graphic Graphic
        {
            get
            {
                var neuralData = NeuralData;
                if (neuralData.guestStatusInt == GuestStatus.Slave)
                {
                    return GetNeuralPrintGraphic(ref slaveGraphic, ref slaveGraphicData,
                        "Things/Item/NeuralPrint/SlaveNeuralPrint");
                }
                else if (neuralData.faction == Faction.OfPlayer)
                {
                    return GetNeuralPrintGraphic(ref friendlyGraphic, ref friendlyGraphicData,
                        "Things/Item/NeuralPrint/FriendlyNeuralPrint");
                }
                else if (neuralData.faction is null || !neuralData.faction.HostileTo(Faction.OfPlayer))
                {
                    return GetNeuralPrintGraphic(ref strangerGraphic, ref strangerGraphicData,
                        "Things/Item/NeuralPrint/NeutralNeuralPrint");
                }
                else
                {
                    return GetNeuralPrintGraphic(ref hostileGraphic, ref hostileGraphicData,
                        "Things/Item/NeuralPrint/HostileNeuralPrint");
                }
            }
        }

        private Graphic GetNeuralPrintGraphic(ref Graphic graphic, ref GraphicData graphicData, string path)
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

        public bool CanAutoRestorePawn(NeuralData otherData)
        {
            if (allowAutomaticRestoration)
            {
                var neuralData = NeuralData;
                if (otherData.IsPresetPawn(neuralData) is false)
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
            if (AC_Utils.generalSettings.enableSoldNeuralPrintsCreatingPawnDuplicates
                && trader.Faction?.def.techLevel >= TechLevel.Spacer && Rand.Chance(0.15f))
            {
                var copy = new NeuralData();
                copy.CopyDataFrom(NeuralData);
                copy.faction = trader.Faction;
                Rand.PushState(copy.GetHashCode());
                GameComponent_AlteredCarbon.Instance.neuralStacksToAppearAsWorldPawns[copy] =
                    (int)(Find.TickManager.TicksGame + (new FloatRange(5f, 30f).RandomInRange * GenDate.TicksPerDay));
                Rand.PopState();
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad && NeuralData.ContainsNeural is false)
            {
                GenerateNeural();
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
                icon = ContentFinder<Texture2D>.Get("UI/Gizmos/AutoLoadNeuralPrint"),
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