using RimWorld;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{
    [HotSwappable]
    public class MindFrame : ThingWithComps
    {
        public PersonaData personaData;
        public int backupCreationDataTicks;
        public bool autoLoad = true;

        public override string LabelNoCount
        {
            get
            {
                var label = base.LabelNoCount;
                label += " (" + this.personaData.PawnNameColored.ToStringSafe() + ")";
                return label;
            }
        }


        private GraphicData hostileGraphicData;
        private GraphicData friendlyGraphicData;
        private GraphicData strangerGraphicData;
        private GraphicData slaveGraphicData;

        private Graphic hostileGraphic;
        private Graphic friendlyGraphic;
        private Graphic strangerGraphic;
        private Graphic slaveGraphic;

        public override Graphic Graphic
        {
            get
            {
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

        private GraphicData GetGraphicDataWithOtherPath(string texPath)
        {
            var copy = new GraphicData();
            copy.CopyFrom(def.graphicData);
            copy.texPath = texPath;
            return copy;
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            personaData.AppendInfo(stringBuilder);
            stringBuilder.Append(base.GetInspectString());
            return stringBuilder.ToString().TrimEndNewlines();
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
            Scribe_Deep.Look(ref personaData, "personaData");
            Scribe_Values.Look(ref backupCreationDataTicks, "backupCreationDataTicks");
            Scribe_Values.Look(ref autoLoad, "autoLoad", true);
        }
    }
}