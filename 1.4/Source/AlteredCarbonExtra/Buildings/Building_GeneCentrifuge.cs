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
    public class Building_GeneCentrifuge : Building, IThingHolder
    {
        public static Texture2D InsertGenePack = ContentFinder<Texture2D>.Get("UI/Icons/InsertGenePack");

        public Genepack genepackToStore;

        public GeneDef geneToSeparate;

        protected Effecter progressBarEffecter;

        private static readonly Texture2D CancelIcon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel");

        public int ticksDone;

        private Sustainer sustainerWorking;

        public Building_GeneCentrifuge()
        {
            this.innerContainer = new ThingOwner<Thing>(this, false, LookMode.Deep);
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            base.DeSpawn(mode);
            progressBarEffecter?.Cleanup();
        }

        public CompPowerTrader compPower;
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (Faction != null && Faction.IsPlayer)
            {
                this.contentsKnown = true;
            }
            compPower = this.TryGetComp<CompPowerTrader>();
        }


        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (this.innerContainer.Count > 0 && (mode == DestroyMode.Deconstruct || mode == DestroyMode.KillFinalize))
            {
                this.EjectContents();
            }
            this.innerContainer.ClearAndDestroyContents(DestroyMode.Vanish);
            base.Destroy(mode);
        }

        public bool Powered => this.compPower.PowerOn;
        public bool HasAnyContents
        {
            get
            {
                return this.innerContainer.Any();
            }
        }
        public Genepack StoredGenepack => this.innerContainer.OfType<Genepack>().FirstOrDefault();
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
                    defaultLabel = "AC.SeparateGene".Translate(),
                    defaultDesc = "AC.SeparateGeneDesc".Translate(),
                    icon = InsertGenePack,
                    action = delegate
                    {
                        var allGenePacks = this.Map.listerThings.ThingsOfDef(ThingDefOf.Genepack)
                            .Cast<Genepack>().Where(x => x.GeneSet.GenesListForReading.Count > 1);
                        var floatList = new List<FloatMenuOption>();
                        foreach (var genepack in allGenePacks)
                        {
                            floatList.Add(new FloatMenuOption(genepack.LabelCap, delegate
                            {
                                Find.WindowStack.Add(new Window_SeparateGene(this, genepack));
                            }));
                        }
                        if (floatList.Any() is false)
                        {
                            floatList.Add(new FloatMenuOption("NoneBrackets".Translate(), null));
                        }
                        Find.WindowStack.Add(new FloatMenu(floatList));
                    }
                };
                if (StoredGenepack != null)
                {
                    separateGene.Disable("AC.CentrigureWorking".Translate());
                }
                if (Powered is false)
                {
                    separateGene.Disable("NoPower".Translate());
                }
                yield return separateGene;

                if (StoredGenepack != null || genepackToStore != null)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "AC.CancelGeneSeparating".Translate(),
                        icon = CancelIcon,
                        action = delegate ()
                        {
                            JobCleanup();
                            if (StoredGenepack != null)
                            {
                                this.EjectContents();
                            }
                        }
                    };
                }
            }
        }

        public override string GetInspectString()
        {
            var sb = new StringBuilder();
            if (StoredGenepack != null)
            {
                var progress = ticksDone / (float)ExtractionDuration(StoredGenepack);
                sb.AppendLine("AC.SeparatingProgress".Translate(progress.ToStringPercent()));
                sb.AppendLine("AC.ContainsGenepack".Translate(StoredGenepack.Label));
            }
            sb.Append(base.GetInspectString());
            return sb.ToString();
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return this.innerContainer;
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
        }
        public override void Tick()
        {
            base.Tick();
            if (Powered && StoredGenepack != null)
            {
                ticksDone++;
                var durationTicks = ExtractionDuration(StoredGenepack);
                if (progressBarEffecter == null)
                {
                    progressBarEffecter = EffecterDefOf.ProgressBar.Spawn();
                }
                progressBarEffecter.EffectTick(this, TargetInfo.Invalid);
                MoteProgressBar mote = ((SubEffecter_ProgressBar)progressBarEffecter.children[0]).mote;
                mote.progress = ticksDone / (float)durationTicks;
                mote.yOffset += 1;
                if (sustainerWorking == null || sustainerWorking.Ended)
                {
                    sustainerWorking = AC_Extra_DefOf.AC_GeneCentrifuge_Ambience.TrySpawnSustainer(SoundInfo.InMap(this, MaintenanceType.PerTick));
                }
                else
                {
                    sustainerWorking.Maintain();
                }
                if (ticksDone >= durationTicks)
                {
                    FinishJob();
                }
            }
        }

        private void FinishJob()
        {
            var newGenepack = (Genepack)ThingMaker.MakeThing(ThingDefOf.Genepack);
            var storedGenepack = StoredGenepack;
            storedGenepack.GeneSet.genes.Remove(geneToSeparate);
            storedGenepack.GeneSet.DirtyCache();
            newGenepack.Initialize(new List<GeneDef>
                    {
                        geneToSeparate
                    });
            var removed = innerContainer.Remove(storedGenepack);
            if (Rand.Chance(0.1f))
            {
                if (Rand.Bool)
                {
                    GenPlace.TryPlaceThing(storedGenepack, Position, Map, ThingPlaceMode.Near);
                    Messages.Message("AC.FinishedSeparatingDestroyedMessage".Translate(newGenepack.LabelCap), new List<Thing>
                            {
                                storedGenepack
                            }, MessageTypeDefOf.NeutralEvent);
                    newGenepack.Destroy();
                }
                else
                {
                    GenPlace.TryPlaceThing(newGenepack, Position, Map, ThingPlaceMode.Near);
                    Messages.Message("AC.FinishedSeparatingDestroyedMessage".Translate(storedGenepack.LabelCap), new List<Thing>
                            {
                                newGenepack
                            }, MessageTypeDefOf.NeutralEvent);
                    storedGenepack.Destroy();
                }
            }
            else
            {
                GenPlace.TryPlaceThing(newGenepack, Position, Map, ThingPlaceMode.Near);
                GenPlace.TryPlaceThing(storedGenepack, Position, Map, ThingPlaceMode.Near);
                Messages.Message("AC.FinishedSeparatingMessage".Translate(), new List<Thing>
                        {
                            storedGenepack, newGenepack
                        }, MessageTypeDefOf.NeutralEvent);
            }
            JobCleanup();
        }

        private void JobCleanup()
        {
            if (progressBarEffecter != null)
            {
                progressBarEffecter.Cleanup();
                progressBarEffecter = null;
            }
            ticksDone = 0;
            geneToSeparate = null;
            genepackToStore = null;
        }

        public int ExtractionDuration(Genepack genepack)
        {
            if (genepack.GeneSet.ArchitesTotal > 0) 
            {
                return 360000;
            }
            return 120000;
        }

        public void StartJob()
        {
            genepackToStore = null;
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look<ThingOwner>(ref this.innerContainer, "innerContainer", new object[]
            {
                this
            });
            Scribe_Values.Look(ref this.contentsKnown, "contentsKnown", false);
            Scribe_References.Look(ref genepackToStore, "genepackToStore");
            Scribe_Defs.Look(ref geneToSeparate, "geneToSeparate");
            Scribe_Values.Look(ref ticksDone, "ticksDone");
        }

        public bool Accepts(Thing thing)
        {
            return this.innerContainer.CanAcceptAnyOf(thing, true) && genepackToStore == thing;
        }

        public bool TryAcceptThing(Thing thing, bool allowSpecialEffects = true)
        {
            if (!this.Accepts(thing))
            {
                return false;
            }
            if (thing.holdingOwner != null)
            {
                thing.holdingOwner.TryTransferToContainer(thing, this.innerContainer, thing.stackCount, true);
            }
            else if (this.innerContainer.TryAdd(thing, true))
            {
                if (thing.Faction != null && thing.Faction.IsPlayer)
                {
                    this.contentsKnown = true;
                }
                return true;
            }
            return false;
        }
        public void EjectContents()
        {
            this.innerContainer.TryDropAll(this.InteractionCell, Map, ThingPlaceMode.Direct, null, null);
            this.contentsKnown = true;
        }

        public ThingOwner innerContainer;

        public bool contentsKnown;
    }
}