using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace AlteredCarbon
{
    public abstract class ThingWithStack : ThingWithComps
    {
        public bool autoLoad = true;
        private PersonaData personaData;
        public PersonaData PersonaData
        {
            get
            {
                if (personaData is null)
                {
                    personaData = new PersonaData();
                }
                return personaData;
            }
            set
            {
                personaData = value;
            }
        }

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

        public void GeneratePersona()
        {
            Faction faction = Find.FactionManager.AllFactions.Where(x => x.def.humanlikeFaction).RandomElement();
            GeneratePersona(faction);
        }

        public void GeneratePersona(Faction faction)
        {
            PawnKindDef pawnKind = GetPawnKind(faction);
            Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(pawnKind, faction));
            PersonaData.CopyFromPawn(pawn, this.def, copyRaceGenderInfo: true);
            PersonaData.OverwritePawn(pawn, this.def.GetModExtension<StackSavingOptionsModExtension>());
            PersonaData.dummyPawn = pawn;
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
            Scribe_Deep.Look(ref personaData, "personaData");
            Scribe_Values.Look(ref autoLoad, "autoLoad", true);
        }
    }
}