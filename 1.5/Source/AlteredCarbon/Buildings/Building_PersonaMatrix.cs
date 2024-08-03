using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{
    public class Building_PersonaMatrix : Building, IThingHolder
    {
        public CompPowerTrader compPower;
        public const int MaxFilledStackCapacity = 25;
        public bool allowColonistPersonaPrints = true;
        public bool allowStrangerPersonaPrints = true;
        public bool allowHostilePersonaPrints = true;

        public Building_PersonaMatrix()
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

        public bool HasAnyContents => StoredPersonaPrints.Any();

        public IEnumerable<PersonaPrint> StoredPersonaPrints => this.innerContainer.OfType<PersonaPrint>();

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var g in base.GetGizmos())
            {
                yield return g;
            }
            if (Faction == Faction.OfPlayer)
            {
                var frames = StoredPersonaPrints.ToList();
                if (frames.Any())
                {
                    var ejectAll = new Command_Action();
                    ejectAll.defaultLabel = "AC.EjectAll".Translate();
                    ejectAll.defaultDesc = "AC.EjectAllPersonaPrintsDesc".Translate();
                    ejectAll.icon = ContentFinder<Texture2D>.Get("UI/Gizmos/EjectAllPersonaPrints");
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
            sb.AppendLine("AC.PersonaPrintsStored".Translate(StoredPersonaPrints.Count(), MaxFilledStackCapacity));
            if (StoredPersonaPrints.Any())
            {
                var lastTimeUpdated = StoredPersonaPrints.Select(x => x.backupCreationDataTicks).Max();
                Vector2 vector = Find.WorldGrid.LongLatOf(this.Map.Tile);
                sb.AppendLine("AC.LastBackup".Translate(GenDate.DateReadoutStringAt(lastTimeUpdated, vector)));
            }
            sb.Append(base.GetInspectString());
            return sb.ToString();
        }


        public void PerformStackBackup(Hediff_PersonaStack hediff_PersonaStack)
        {
            var stackCopyTo = (PersonaStack)ThingMaker.MakeThing(AC_DefOf.AC_FilledPersonaStack);
            this.innerContainer.TryAdd(stackCopyTo);
            stackCopyTo.PersonaData.CopyDataFrom(hediff_PersonaStack.PersonaData);
            AlteredCarbonManager.Instance.RegisterStack(stackCopyTo);
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

        public bool HasFreeSpace => this.innerContainer.Count < MaxFilledStackCapacity;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look<ThingOwner>(ref this.innerContainer, "innerContainer", new object[]
            {
                this
            });
            Scribe_Values.Look(ref this.contentsKnown, "contentsKnown", false);
            Scribe_Values.Look(ref this.allowColonistPersonaPrints, "allowColonistPersonaPrints", true);
            Scribe_Values.Look(ref this.allowHostilePersonaPrints, "allowHostilePersonaPrints", true);
            Scribe_Values.Look(ref this.allowStrangerPersonaPrints, "allowStrangerPersonaPrints", true);
        }

        public bool Accepts(Thing thing)
        {
            Predicate<Thing> validator = delegate (Thing x)
            {
                var personaPrint = thing as PersonaPrint;
                if (personaPrint is null)
                {
                    return false;
                }
                if (!personaPrint.PersonaData.ContainsPersona)
                {
                    return false;
                }

                if (this.allowColonistPersonaPrints && personaPrint.PersonaData.faction != null && personaPrint.PersonaData.faction == Faction.OfPlayer)
                {
                    return true;
                }
                if (this.allowHostilePersonaPrints && personaPrint.PersonaData.faction.HostileTo(Faction.OfPlayer))
                {
                    return true;
                }
                if (this.allowStrangerPersonaPrints && (personaPrint.PersonaData.faction is null || personaPrint.PersonaData.faction != Faction.OfPlayer && !personaPrint.PersonaData.faction.HostileTo(Faction.OfPlayer)))
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