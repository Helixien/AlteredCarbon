using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace AlteredCarbon
{
    [HotSwappable]
    [StaticConstructorOnStartup]
    public class Building_XenogermDuplicator : Building_Processor
    {
        public static Texture2D InsertXenogerm = ContentFinder<Texture2D>.Get("UI/Icons/InsertXenogerm");

        public Xenogerm xenogermToDuplicate;
        public Xenogerm StoredXenogerm => this.innerContainer.OfType<Xenogerm>().FirstOrDefault();
        public override SoundDef SustainerDef => AC_DefOf.AC_XenoGermDuplicator_Ambience;

        public CompRefuelable compRefuelable;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            compRefuelable = this.GetComp<CompRefuelable>();
        }
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var g in base.GetGizmos())
            {
                yield return g;
            }
            if (Faction == Faction.OfPlayer)
            {
               var separateGene = new Command_Action
                {
                    defaultLabel = "AC.DuplicateXenogerm".Translate(),
                    defaultDesc = "AC.DuplicateXenogermDesc".Translate(),
                    icon = InsertXenogerm,
                    action = delegate
                    {
                        var allXenogerms = this.Map.listerThings.ThingsOfDef(ThingDefOf.Xenogerm)
                            .Cast<Xenogerm>().Where(x => x.GeneSet.GenesListForReading.Count > 0);
                        var floatList = new List<FloatMenuOption>();
                        foreach (var xenogerm in allXenogerms)
                        {
                            floatList.Add(new FloatMenuOption(xenogerm.LabelCap, delegate
                            {
                                var architeGenes = xenogerm.GeneSet.ArchitesTotal;
                                if (this.compRefuelable.Fuel < architeGenes)
                                {
                                    Find.WindowStack.Add(new Dialog_MessageBox("AC.NotEnoughArchiteCapsulesLoaded".Translate(), 
                                        "Confirm".Translate(), delegate
                                        {
                                            xenogermToDuplicate = xenogerm;
                                        }, "GoBack".Translate(), null));
                                }
                                else
                                {
                                    xenogermToDuplicate = xenogerm;
                                }
                            }));
                        }
                        if (floatList.Any() is false)
                        {
                            floatList.Add(new FloatMenuOption("NoneBrackets".Translate(), null));
                        }
                        Find.WindowStack.Add(new FloatMenu(floatList));
                    }
                };
                if (StoredXenogerm != null)
                {
                    separateGene.Disable("AC.DuplicatorWorking".Translate());
                }
                if (Powered is false)
                {
                    separateGene.Disable("NoPower".Translate());
                }
                yield return separateGene;

                if (StoredXenogerm != null || xenogermToDuplicate != null)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "AC.CancelXenogermDuplication".Translate(),
                        defaultDesc = "AC.CancelXenogermDuplicationDesc".Translate(),
                        activateSound = SoundDefOf.Tick_Tiny,
                        icon = CancelIcon,
                        action = delegate ()
                        {
                            JobCleanup();
                            if (StoredXenogerm != null)
                            {
                                this.EjectContents();
                            }
                        }
                    };
                }

                if (Prefs.DevMode && StoredXenogerm != null)
                {
                    Command_Action command_Action = new Command_Action
                    {
                        defaultLabel = "Debug: Finish duplication",
                        action = delegate
                        {
                            ticksDone = DuplicationDuration(StoredXenogerm);
                            FinishJob();
                        }
                    };
                    yield return command_Action;
                }
            }
        }

        public override bool Accepts(Thing thing)
        {
            return base.Accepts(thing) && xenogermToDuplicate == thing;
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref xenogermToDuplicate, "xenogermToDuplicate");
        }
        public override string GetInspectString()
        {
            var sb = new StringBuilder();
            if (StoredXenogerm != null)
            {
                var progress = ticksDone / (float)DuplicationDuration(StoredXenogerm);
                sb.AppendLine("AC.DuplicationProgress".Translate(progress.ToStringPercent()));
                sb.AppendLine("AC.ContainsXenogerm".Translate(StoredXenogerm.Label));
            }
            sb.Append(base.GetInspectString());
            return sb.ToString();
        }

        public override void Tick()
        {
            base.Tick();
            if (Powered && StoredXenogerm != null)
            {
                var durationTicks = DuplicationDuration(StoredXenogerm);
                DoWork(durationTicks);
            }
        }

        protected override void FinishJob()
        {
            var storedXenogerm = StoredXenogerm;
            var newXenogerm = (Xenogerm)ThingMaker.MakeThing(ThingDefOf.Xenogerm);
            var genesToCopy = new List<GeneDef>();
            foreach (var gene in storedXenogerm.GeneSet.genes.ToList())
            {
                if (gene.biostatArc <= 0)
                {
                    genesToCopy.Add(gene);
                }
                else if (compRefuelable.Fuel >= 1)
                {
                    compRefuelable.ConsumeFuel(1f);
                    genesToCopy.Add(gene);
                }
            }
            newXenogerm.xenotypeName = storedXenogerm.xenotypeName;
            newXenogerm.iconDef = storedXenogerm.iconDef;
            newXenogerm.geneSet = new GeneSet();
            foreach (var gene in genesToCopy)
            {
                newXenogerm.GeneSet.AddGene(gene);
            }
            newXenogerm.GeneSet.DirtyCache();
            GenPlace.TryPlaceThing(newXenogerm, Position, Map, ThingPlaceMode.Near, 
                nearPlaceValidator: (IntVec3 x) => x.GetFirstThing<Building_XenogermDuplicator>(this.Map) is null);
            GenPlace.TryPlaceThing(storedXenogerm, Position, Map, ThingPlaceMode.Near,
                nearPlaceValidator: (IntVec3 x) => x.GetFirstThing<Building_XenogermDuplicator>(this.Map) is null);
            Messages.Message("AC.FinishedXenogermDuplicationMessage".Translate(), new List<Thing>
                        {
                            storedXenogerm, newXenogerm
                        }, MessageTypeDefOf.CautionInput);
            JobCleanup();
        }
        protected override void JobCleanup()
        {
            base.JobCleanup();
            xenogermToDuplicate = null;
        }

        public int DuplicationDuration(Xenogerm xenogerm)
        {
            return xenogerm.GeneSet.GenesListForReading.Count() * 1875;
        }

        public override void StartJob()
        {
            xenogermToDuplicate = null;
        }
    }
}