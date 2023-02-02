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
    public class Building_StackStorage : Building, IThingHolder
    {
        public const int MaxFilledStackCapacity = 25;
        public static HashSet<Building_StackStorage> building_StackStorages = new HashSet<Building_StackStorage>();
        public Building_StackStorage()
        {
            this.innerContainer = new ThingOwner<Thing>(this, false, LookMode.Deep);
        }

        public bool allowColonistCorticalStacks = true;
        public bool allowStrangerCorticalStacks = true;
        public bool allowHostileCorticalStacks = true;
        public bool allowArchoStacks = true;
        public CompPowerTrader compPower;
        public bool backupIsEnabled;
        public bool autoRestoreIsEnabled = true;
        public CorticalStack stackToDuplicate;
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            building_StackStorages.Add(this);
            base.SpawnSetup(map, respawningAfterLoad);
            if (Faction != null && Faction.IsPlayer)
            {
                this.contentsKnown = true;
            }
            compPower = this.TryGetComp<CompPowerTrader>();
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            base.DeSpawn(mode);
            building_StackStorages.Remove(this);
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            building_StackStorages.Remove(this);
            if (this.innerContainer.Count > 0 && (mode == DestroyMode.Deconstruct || mode == DestroyMode.KillFinalize))
            {
                this.EjectContents();
            }
            this.innerContainer.ClearAndDestroyContents(DestroyMode.Vanish);
            base.Destroy(mode);
            if (building_StackStorages.Any() is false)
            {
                GameComponent_DigitalStorage.Instance.backedUpStacks.Clear();
            }
        }
        public bool CanDuplicateStack
        {
            get
            {
                if (this.stackToDuplicate is null || !this.innerContainer.Contains(this.stackToDuplicate))
                {
                    return false;
                }
                if (!Powered)
                {
                    return false;
                }
                return true;
            }
        }

        public bool Powered => this.compPower.PowerOn;
        public bool HasAnyContents
        {
            get
            {
                return this.innerContainer.Any();
            }
        }
        public IEnumerable<CorticalStack> StoredStacks => this.innerContainer.OfType<CorticalStack>();
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var g in base.GetGizmos())
            {
                yield return g;
            }
            if (Faction == Faction.OfPlayer)
            {
                var stacks = StoredStacks.ToList();
                if (stacks.Any())
                {
                    var duplicateStacks = new Command_Action
                    {
                        defaultLabel = "AC.DuplicateStack".Translate(),
                        defaultDesc = "AC.DuplicateStackDesc".Translate(),
                        icon = ContentFinder<Texture2D>.Get("UI/Icons/DuplicateStack"),
                        activateSound = SoundDefOf.Tick_Tiny,
                        action = delegate ()
                        {
                            var floatList = new List<FloatMenuOption>();
                            foreach (var stack in StoredStacks.Where(x => x.PersonaData.ContainsInnerPersona))
                            {
                                if (stack.IsArchoStack is false)
                                {
                                    floatList.Add(new FloatMenuOption(stack.PersonaData.PawnNameColored, delegate ()
                                    {
                                        this.stackToDuplicate = stack;
                                    }));
                                }
                            }
                            Find.WindowStack.Add(new FloatMenu(floatList));
                        }
                    };

                    if (this.stackToDuplicate != null)
                    {
                        duplicateStacks.Disable("AC.AlreadySetToDuplicate".Translate());
                    }
                    else if (stacks.Count() >= MaxFilledStackCapacity)
                    {
                        duplicateStacks.Disable("AC.NoEnoughSpaceForNewStack".Translate());
                    }
                    if (this.Powered is false)
                    {
                        duplicateStacks.Disable("NoPower".Translate());
                    }
                    yield return duplicateStacks;

                    var ejectAll = new Command_Action();
                    ejectAll.defaultLabel = "AC.EjectAll".Translate();
                    ejectAll.defaultDesc = "AC.EjectAllDesc".Translate();
                    ejectAll.icon = ContentFinder<Texture2D>.Get("UI/Icons/EjectAllStacks");
                    ejectAll.action = delegate
                    {
                        EjectContents();
                    };
                    yield return ejectAll;

                }

                var enableBackup = new Command_Toggle()
                {
                    defaultLabel = "AC.EnableBackup".Translate(),
                    defaultDesc = "AC.EnableBackupDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Icons/EnableBackup"),
                    activateSound = SoundDefOf.Tick_Tiny,
                    toggleAction = delegate ()
                    {
                        backupIsEnabled = !backupIsEnabled;
                    },
                    isActive = () => backupIsEnabled
                };
                yield return enableBackup;
                if (backupIsEnabled)
                {
                    var backupAll = new Command_Action()
                    {
                        defaultLabel = "AC.BackupAllStacks".Translate(),
                        defaultDesc = "AC.BackupAllStacksDesc".Translate(),
                        icon = ContentFinder<Texture2D>.Get("UI/Icons/BackupAllStacks"),
                        activateSound = SoundDefOf.Tick_Tiny,
                        action = delegate
                        {
                            GameComponent_DigitalStorage.Instance.BackupAllColonistsWithStacks();
                        },
                    };
                    if (!this.compPower.PowerOn)
                    {
                        backupAll.Disable("NoPower".Translate());
                    }
                    yield return backupAll;
                }
                yield return new Command_Toggle()
                {
                    defaultLabel = "AC.EnableAutoRestore".Translate(),
                    defaultDesc = "AC.EnableAutoRestoreDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Icons/EnableAutoRestore"),
                    activateSound = SoundDefOf.Tick_Tiny,
                    toggleAction = delegate ()
                    {
                        autoRestoreIsEnabled = !autoRestoreIsEnabled;
                    },
                    isActive = () => autoRestoreIsEnabled
                };
            }
        }

        public override string GetInspectString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("AC.CorticalStacksStored".Translate(StoredStacks.Count()));
            if (GameComponent_DigitalStorage.Instance.backedUpStacks.Values.Any())
            {
                var lastTimeUpdated = GameComponent_DigitalStorage.Instance.backedUpStacks.Select(x => x.Value.lastTimeUpdated).Max();
                Vector2 vector = Find.WorldGrid.LongLatOf(this.Map.Tile);
                sb.AppendLine("AC.LastBackup".Translate(GenDate.DateReadoutStringAt(lastTimeUpdated, vector)));
            }
            sb.Append(base.GetInspectString());
            return sb.ToString();
        }

        public void PerformStackDuplication(Pawn doer)
        {
            float successChance = 1f - Mathf.Abs((doer.skills.GetSkill(SkillDefOf.Intellectual).levelInt / 2f) - 11f) / 10f;
            if (Rand.Chance(successChance))
            {
                var stackCopyTo = (CorticalStack)ThingMaker.MakeThing(AC_DefOf.VFEU_FilledCorticalStack);
                this.innerContainer.TryAdd(stackCopyTo);
                stackCopyTo.PersonaData.CopyDataFrom(stackToDuplicate.PersonaData, true);
                AlteredCarbonManager.Instance.RegisterStack(stackCopyTo);
                stackToDuplicate = null;
                Messages.Message("AC.SuccessfullyDuplicatedStack".Translate(doer.Named("PAWN")), this, MessageTypeDefOf.TaskCompletion);
            }
            else
            {
                Messages.Message("AC.FailedToDuplicatedStack".Translate(doer.Named("PAWN")), this, MessageTypeDefOf.NeutralEvent);
            }
        }
        public void PerformStackBackup(Hediff_CorticalStack hediff_CorticalStack)
        {
            var stackCopyTo = (CorticalStack)ThingMaker.MakeThing(AC_DefOf.VFEU_FilledCorticalStack);
            this.innerContainer.TryAdd(stackCopyTo);
            stackCopyTo.PersonaData.CopyDataFrom(hediff_CorticalStack.PersonaData);
            AlteredCarbonManager.Instance.RegisterStack(stackCopyTo);
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
        {
            foreach (FloatMenuOption opt in base.GetFloatMenuOptions(myPawn))
            {
                yield return opt;
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
            Scribe_Values.Look(ref this.allowColonistCorticalStacks, "allowColonistCorticalStacks", true);
            Scribe_Values.Look(ref this.allowHostileCorticalStacks, "allowHostileCorticalStacks", true);
            Scribe_Values.Look(ref this.allowStrangerCorticalStacks, "allowStrangerCorticalStacks", true);
            Scribe_Values.Look(ref this.allowArchoStacks, "allowArchoStacks", true);
            Scribe_Values.Look(ref this.backupIsEnabled, "backupIsEnabled");
            Scribe_Values.Look(ref this.autoRestoreIsEnabled, "autoRestoreIsEnabled", true);
            Scribe_References.Look(ref this.stackToDuplicate, "stackToDuplicate");
        }

        public bool Accepts(Thing thing)
        {
            Predicate<Thing> validator = delegate (Thing x)
            {
                var stack = thing as CorticalStack;
                if (stack is null)
                {
                    return false;
                }
                if (!stack.PersonaData.ContainsInnerPersona)
                {
                    return false;
                }

                if (this.allowColonistCorticalStacks && stack.PersonaData.faction != null && stack.PersonaData.faction == Faction.OfPlayer)
                {
                    return true;
                }

                if (this.allowHostileCorticalStacks && stack.PersonaData.faction.HostileTo(Faction.OfPlayer))
                {
                    return true;
                }
                if (this.allowArchoStacks && stack.IsArchoStack)
                {
                    return true;
                }
                if (this.allowStrangerCorticalStacks && (stack.PersonaData.faction is null || stack.PersonaData.faction != Faction.OfPlayer && !stack.PersonaData.faction.HostileTo(Faction.OfPlayer)))
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