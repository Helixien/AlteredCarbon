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

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            PersonaData.AppendInfo(stringBuilder);
            stringBuilder.Append(base.GetInspectString());
            return stringBuilder.ToString().TrimEndNewlines();
        }

        protected void GenerateInnerPersona()
        {
            PawnKindDef pawnKind = DefDatabase<PawnKindDef>.AllDefs.Where(x => x.RaceProps.Humanlike && x is not CreepJoinerFormKindDef).RandomElement();
            Faction faction = Find.FactionManager.AllFactions.Where(x => x.def.humanlikeFaction).RandomElement();
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

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref personaData, "personaData");
            Scribe_Values.Look(ref autoLoad, "autoLoad", true);
        }
    }
}