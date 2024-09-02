using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{
    public class Building_NeuralMatrix : Building, IThingHolder
    {
        public CompPowerTrader compPower;
        public const int MaxActiveStackCapacity = 25;
        public bool allowColonistNeuralStacks = true;
        public bool allowStrangerNeuralStacks = true;
        public bool allowHostileNeuralStacks = true;

        public Building_NeuralMatrix()
        {
            this.innerContainer = new ThingOwner<Thing>(this, false, LookMode.Deep);
        }

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

        public bool HasAnyContents => StoredNeuralStacks.Any();

        public IEnumerable<NeuralStack> StoredNeuralStacks => this.innerContainer.OfType<NeuralStack>();

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var g in base.GetGizmos())
            {
                yield return g;
            }
            if (Faction == Faction.OfPlayer)
            {
                var stacks = StoredNeuralStacks.ToList();
                if (stacks.Any())
                {
                    var ejectAll = new Command_Action();
                    ejectAll.defaultLabel = "AC.EjectAll".Translate();
                    ejectAll.defaultDesc = "AC.EjectAllNeuralStacksDesc".Translate();
                    ejectAll.icon = ContentFinder<Texture2D>.Get("UI/Gizmos/EjectAllNeuralStacks");
                    ejectAll.action = delegate
                    {
                        EjectContents();
                    };
                    yield return ejectAll;
                }
            }
        }

        public override string GetInspectString()
        {
            var sb = new StringBuilder();
            sb.Append(base.GetInspectString() + "\n");
            sb.AppendLine("AC.NeuralStacksStored".Translate(StoredNeuralStacks.Count(), MaxActiveStackCapacity));
            if (StoredNeuralStacks.Any())
            {
                var lastTimeUpdated = StoredNeuralStacks.Select(x => x.NeuralData.lastTimeBackedUp).Max();
                if (lastTimeUpdated.HasValue)
                {
                    Vector2 vector = Find.WorldGrid.LongLatOf(this.Map.Tile);
                    sb.AppendLine("AC.LastBackup".Translate(GenDate.DateReadoutStringAt(lastTimeUpdated.Value, vector)));
                }
            }
            return sb.ToString().TrimEndNewlines();
        }


        //public void PerformStackBackup(Hediff_NeuralStack hediff_NeuralStack)
        //{
        //    var stackCopyTo = (NeuralStack)ThingMaker.MakeThing(AC_DefOf.AC_ActiveNeuralStack);
        //    this.innerContainer.TryAdd(stackCopyTo);
        //    stackCopyTo.NeuralData.CopyDataFrom(hediff_NeuralStack.NeuralData);
        //    AlteredCarbonManager.Instance.RegisterStack(stackCopyTo);
        //}

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

        public bool HasFreeSpace => this.innerContainer.Count < MaxActiveStackCapacity;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look<ThingOwner>(ref this.innerContainer, "innerContainer", new object[]
            {
                this
            });
            Scribe_Values.Look(ref this.contentsKnown, "contentsKnown", false);
            Scribe_Values.Look(ref this.allowColonistNeuralStacks, "allowColonistNeuralStacks", true);
            Scribe_Values.Look(ref this.allowHostileNeuralStacks, "allowHostileNeuralStacks", true);
            Scribe_Values.Look(ref this.allowStrangerNeuralStacks, "allowStrangerNeuralStacks", true);
        }

        public bool Accepts(Thing thing)
        {
            Predicate<Thing> validator = delegate (Thing x)
            {
                var neuralStack = thing as NeuralStack;
                if (neuralStack is null)
                {
                    return false;
                }
                if (!neuralStack.NeuralData.ContainsNeural)
                {
                    return false;
                }

                if (this.allowColonistNeuralStacks && neuralStack.NeuralData.faction != null && neuralStack.NeuralData.faction == Faction.OfPlayer)
                {
                    return true;
                }
                if (this.allowHostileNeuralStacks && neuralStack.NeuralData.faction.HostileTo(Faction.OfPlayer))
                {
                    return true;
                }
                if (this.allowStrangerNeuralStacks && (neuralStack.NeuralData.faction is null || neuralStack.NeuralData.faction != Faction.OfPlayer && !neuralStack.NeuralData.faction.HostileTo(Faction.OfPlayer)))
                {
                    return true;
                }
                return false;
            };
            return validator(thing) && this.innerContainer.CanAcceptAnyOf(thing, true) && HasFreeSpace;
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