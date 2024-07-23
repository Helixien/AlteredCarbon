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
        public bool allowColonistMindFrames = true;
        public bool allowStrangerMindFrames = true;
        public bool allowHostileMindFrames = true;

        public Building_PersonaMatrix()
        {
            this.innerContainer = new ThingOwner<Thing>(this, false, LookMode.Deep);
        }

        public MindFrame GetFirstMindFrameToRestore()
        {
            foreach (var frame in StoredMindFrames)
            {
                var personaData = frame.PersonaData;
                if (personaData.restoreToEmptyStack)
                {
                    if (!AnyPersonaStackExist(personaData) && !AnyPawnExist(personaData))
                    {
                        return frame;
                    }
                }
            }
            return null;
        }

        private static bool AnyPersonaStackExist(PersonaData personaData)
        {
            foreach (var map in Find.Maps)
            {
                if (map.listerThings.ThingsOfDef(AC_DefOf.AC_FilledPersonaStack).Cast<PersonaStack>()
                    .Any(x => x.PersonaData.IsPresetPawn(personaData) && x.Spawned && !x.Destroyed))
                {
                    return true;
                }
                if (map.listerThings.ThingsOfDef(AC_DefOf.AC_PersonaMatrix).Cast<Building_PersonaMatrix>()
                    .Any(x => x.StoredMindFrames.Any(y => y.PersonaData.IsPresetPawn(personaData))))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool AnyPawnExist(PersonaData personaData)
        {
            foreach (var pawn in PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead)
            {
                if (personaData.IsPresetPawn(pawn) && pawn.HasPersonaStack(out _))
                {
                    return true;
                }
            }
            return false;
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

        public bool HasAnyContents => StoredMindFrames.Any();

        public IEnumerable<MindFrame> StoredMindFrames => this.innerContainer.OfType<MindFrame>();

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var g in base.GetGizmos())
            {
                yield return g;
            }
            if (Faction == Faction.OfPlayer)
            {
                var frames = StoredMindFrames.ToList();
                if (frames.Any())
                {
                    var ejectAll = new Command_Action();
                    ejectAll.defaultLabel = "AC.EjectAll".Translate();
                    ejectAll.defaultDesc = "AC.EjectAllDesc".Translate();
                    ejectAll.icon = ContentFinder<Texture2D>.Get("UI/Gizmos/EjectAllStacks");
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
            sb.AppendLine("AC.MindFramesStored".Translate(StoredMindFrames.Count(), MaxFilledStackCapacity));
            if (StoredMindFrames.Any())
            {
                var lastTimeUpdated = StoredMindFrames.Select(x => x.backupCreationDataTicks).Max();
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
            Scribe_Values.Look(ref this.allowColonistMindFrames, "allowColonistMindFrames", true);
            Scribe_Values.Look(ref this.allowHostileMindFrames, "allowHostileMindFrames", true);
            Scribe_Values.Look(ref this.allowStrangerMindFrames, "allowStrangerMindFrames", true);
        }

        public bool Accepts(Thing thing)
        {
            Predicate<Thing> validator = delegate (Thing x)
            {
                var mindFrame = thing as MindFrame;
                if (mindFrame is null)
                {
                    return false;
                }
                if (!mindFrame.PersonaData.ContainsPersona)
                {
                    return false;
                }

                if (this.allowColonistMindFrames && mindFrame.PersonaData.faction != null && mindFrame.PersonaData.faction == Faction.OfPlayer)
                {
                    return true;
                }
                if (this.allowHostileMindFrames && mindFrame.PersonaData.faction.HostileTo(Faction.OfPlayer))
                {
                    return true;
                }
                if (this.allowStrangerMindFrames && (mindFrame.PersonaData.faction is null || mindFrame.PersonaData.faction != Faction.OfPlayer && !mindFrame.PersonaData.faction.HostileTo(Faction.OfPlayer)))
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