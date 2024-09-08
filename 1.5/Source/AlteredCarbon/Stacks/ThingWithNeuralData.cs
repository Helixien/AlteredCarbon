using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace AlteredCarbon
{
    public interface IStackHolder
    {
        public NeuralData NeuralData { get; set; }
        public Thing ThingHolder { get; }
        public Pawn Pawn { get; }
    }
    public abstract class ThingWithNeuralData : ThingWithComps, IStackHolder
    {
        public bool autoLoad = true;
        private NeuralData neuralData;
        public NeuralData NeuralData
        {
            get
            {
                if (neuralData is null)
                {
                    neuralData = new NeuralData();
                }
                return neuralData;
            }
            set
            {
                neuralData = value;
            }
        }

        public Thing ThingHolder => this;
        public Pawn Pawn => NeuralData.DummyPawn;
        protected GraphicData hostileGraphicData;
        protected GraphicData friendlyGraphicData;
        protected GraphicData strangerGraphicData;
        protected GraphicData slaveGraphicData;

        protected Graphic hostileGraphic;
        protected Graphic friendlyGraphic;
        protected Graphic strangerGraphic;
        protected Graphic slaveGraphic;

        protected GraphicData GetGraphicDataWithOtherPath(string texPath)
        {
            var copy = new GraphicData();
            copy.CopyFrom(def.graphicData);
            copy.texPath = texPath;
            return copy;
        }

        public void GenerateNeural()
        {
            Faction faction = Find.FactionManager.AllFactions.Where(x => x.def.humanlikeFaction).RandomElement();
            GenerateNeural(faction);
        }

        public void GenerateNeural(Faction faction)
        {
            PawnKindDef pawnKind = GetPawnKind(faction);
            Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(pawnKind, faction));
            NeuralData.CopyFromPawn(pawn, this.def, copyRaceGenderInfo: true);
            NeuralData.hostPawn = null;
            if (LookTargets_Patch.targets.TryGetValue(pawn, out List<LookTargets> targets))
            {
                foreach (LookTargets target in targets)
                {
                    target.targets.Remove(pawn);
                    target.targets.Add(this);
                }
            }
        }

        private PawnKindDef GetPawnKind(Faction faction)
        {
            if (faction != null)
            {
                if (faction == Faction.OfAncients)
                {
                    return PawnKindDefOf.AncientSoldier;
                }
                if (DefDatabase<PawnKindDef>.AllDefs.Where(x => x.defaultFactionType == faction.def
                    && x.RaceProps.Humanlike && x is not CreepJoinerFormKindDef).TryRandomElement(out var pawnKind))
                {
                    return pawnKind;
                }
            }
            return DefDatabase<PawnKindDef>.AllDefs.Where(x => x.RaceProps.Humanlike
                && x is not CreepJoinerFormKindDef).RandomElement();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref neuralData, "neuralData");
            Scribe_Values.Look(ref autoLoad, "autoLoad", true);
        }
    }
}