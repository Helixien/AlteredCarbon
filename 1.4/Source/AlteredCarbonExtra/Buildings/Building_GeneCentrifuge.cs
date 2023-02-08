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
        public Building_GeneCentrifuge()
        {
            this.innerContainer = new ThingOwner<Thing>(this, false, LookMode.Deep);
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
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var g in base.GetGizmos())
            {
                yield return g;
            }
            if (Faction == Faction.OfPlayer)
            {
                yield return new Command_Action
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
            }
        }

        public override string GetInspectString()
        {
            var sb = new StringBuilder();

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
            this.innerContainer.ThingOwnerTick(true);
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look<ThingOwner>(ref this.innerContainer, "innerContainer", new object[]
            {
                this
            });
            Scribe_Values.Look(ref this.contentsKnown, "contentsKnown", false);
        }

        public bool Accepts(Thing thing)
        {
            return this.innerContainer.CanAcceptAnyOf(thing, true);
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