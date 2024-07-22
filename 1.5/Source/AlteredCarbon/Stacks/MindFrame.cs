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

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad && PersonaData.ContainsInnerPersona is false)
            {
                GenerateInnerPersona();
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