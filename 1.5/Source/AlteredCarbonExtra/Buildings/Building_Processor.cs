using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace AlteredCarbon
{
    [HotSwappable]
    [StaticConstructorOnStartup]
    public abstract class Building_Processor : Building, IThingHolder
    {
        protected Effecter progressBarEffecter;
        public static readonly Texture2D CancelIcon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel");
        public int ticksDone;
        private Sustainer sustainerWorking;
        public abstract SoundDef SustainerDef { get; }
        public Building_Processor()
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

        public ThingOwner GetDirectlyHeldThings()
        {
            return this.innerContainer;
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look<ThingOwner>(ref this.innerContainer, "innerContainer", new object[]
            {
                this
            });
            Scribe_Values.Look(ref this.contentsKnown, "contentsKnown", false);
            Scribe_Values.Look(ref ticksDone, "ticksDone");
        }

        public virtual bool Accepts(Thing thing)
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

        protected void DoWork(int durationTicks)
        {
            ticksDone++;
            DoProgressBar(ticksDone / (float)durationTicks);
            DoSustainer();
            if (ticksDone >= durationTicks)
            {
                FinishJob();
            }
        }

        private void DoSustainer()
        {
            if (sustainerWorking == null || sustainerWorking.Ended)
            {
                sustainerWorking = SustainerDef.TrySpawnSustainer(SoundInfo.InMap(this, MaintenanceType.PerTick));
            }
            else
            {
                sustainerWorking.Maintain();
            }
        }

        private void DoProgressBar(float progress)
        {
            if (progressBarEffecter == null)
            {
                progressBarEffecter = EffecterDefOf.ProgressBar.Spawn();
            }
            MoteProgressBar mote = ((SubEffecter_ProgressBar)progressBarEffecter.children[0]).mote;
            if (mote.DestroyedOrNull())
            {
                progressBarEffecter = EffecterDefOf.ProgressBar.Spawn();
                progressBarEffecter.EffectTick(this, TargetInfo.Invalid);
                mote = ((SubEffecter_ProgressBar)progressBarEffecter.children[0]).mote;
            }
            else
            {
                progressBarEffecter.EffectTick(this, TargetInfo.Invalid);
            }
            mote.yOffset += 1;
            mote.progress = progress;
        }

        public abstract void StartJob();
        protected abstract void FinishJob();
        protected virtual void JobCleanup()
        {
            if (progressBarEffecter != null)
            {
                progressBarEffecter.Cleanup();
                progressBarEffecter = null;
            }
            ticksDone = 0;
        }
    }
}