using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{
    [StaticConstructorOnStartup]
    public class Building_NeuralEditor : Building_WorkTable
    {
        public bool autoRestoreIsEnabled = true;
        public PersonaStack stackToDuplicate;
        public MindFrame mindFrameToRestore;
        public Building_PersonaMatrix ConnectedMatrix => CompAffectedByFacilities.LinkedFacilitiesListForReading.OfType<Building_PersonaMatrix>().FirstOrDefault();
        public bool Powered => (PowerComp as CompPowerTrader).PowerOn;
        private CompAffectedByFacilities _compAffectedByFacilities;
        public CompAffectedByFacilities CompAffectedByFacilities => 
            _compAffectedByFacilities ??= GetComp<CompAffectedByFacilities>();

        public bool HasMindFrameToRestore => GetMindFrameToRestore != null;

        public MindFrame GetMindFrameToRestore
        {
            get
            {
                if (mindFrameToRestore != null)
                {
                    return mindFrameToRestore;
                }
                if (autoRestoreIsEnabled)
                {
                    MindFrame mindFrame = null;
                    var matrix = ConnectedMatrix;
                    if (matrix != null)
                    {
                        foreach (var frame in matrix.StoredMindFrames)
                        {
                            if (frame.CanAutoRestorePawn)
                            {
                                mindFrame = frame;
                                break;
                            }
                        }
                    }

                    if (mindFrame != null)
                    {
                        return mindFrame;
                    }
                    foreach (var frame in Map.listerThings.AllThings.OfType<MindFrame>())
                    {
                        if (frame.CanAutoRestorePawn)
                        {
                            return frame;
                        }
                    }
                }
                return null;
            }
        }

        public bool CanDuplicateStack
        {
            get
            {
                if (this.stackToDuplicate is null)
                {
                    return false;
                }
                if (Powered is false)
                {
                    return false;
                }
                return true;
            }
        }
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos())
            {
                yield return g;
            }
            var wipeStacks = new Command_ActionOnStack(this, ForFilledStack(includeArchotechStack: false), InstallWipeStackBill)
            {
                defaultLabel = "AC.WipeStack".Translate(),
                defaultDesc = "AC.WipeStackDesc".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Gizmos/WipeStack"),
                activateSound = SoundDefOf.Tick_Tiny,
                action = delegate ()
                {
                    BeginTargetingForWipingStack();
                },
            };
            if (powerComp.PowerOn is false)
            {
                wipeStacks.Disable("NoPower".Translate().CapitalizeFirst());
            }
            yield return wipeStacks;
            var wipeStacksBills = this.billStack.Bills.Where(x => x.recipe == AC_DefOf.AC_WipeFilledPersonaStack).ToList();
            if (wipeStacksBills.Any())
            {
                yield return new Command_Action
                {
                    defaultLabel = "AC.CancelStackReset".Translate(),
                    defaultDesc = "AC.CancelStackResetDesc".Translate(),
                    activateSound = SoundDefOf.Tick_Tiny,
                    icon = UIHelper.CancelIcon,
                    action = delegate ()
                    {
                        foreach (var bill in wipeStacksBills)
                        {
                            this.billStack.Delete(bill);
                        }
                    }
                };
            }
            var rewriteStack = new Command_ActionOnStack(this, ForFilledStack(includeArchotechStack: AC_Utils.rewriteStacksSettings.enableArchostackRewriting), InstallRewriteBill)
            {
                defaultLabel = "AC.RewriteStack".Translate(),
                defaultDesc = "AC.RewriteStackDesc".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Gizmos/EditStack"),
                activateSound = SoundDefOf.Tick_Tiny,
                action = delegate ()
                {
                    Find.Targeter.BeginTargeting(ForFilledStack(includeArchotechStack: AC_Utils.rewriteStacksSettings.enableArchostackRewriting), delegate (LocalTargetInfo x)
                    {
                        InstallRewriteBill(x);
                    });
                }
            };
            rewriteStack.LockBehindReseach(AC_DefOf.AC_RewriteFilledPersonaStack.researchPrerequisites);
            if (powerComp.PowerOn is false)
            {
                rewriteStack.Disable("NoPower".Translate().CapitalizeFirst());
            }
            yield return rewriteStack;

            var rewriteStacksBills = this.billStack.Bills.Where(x => x.recipe == AC_DefOf.AC_RewriteFilledPersonaStack).ToList();
            if (rewriteStacksBills.Any())
            {
                yield return new Command_Action
                {
                    defaultLabel = "AC.CancelStackRewrite".Translate(),
                    defaultDesc = "AC.CancelStackRewriteDesc".Translate(),
                    activateSound = SoundDefOf.Tick_Tiny,
                    icon = UIHelper.CancelIcon,
                    action = delegate ()
                    {
                        foreach (var bill in rewriteStacksBills)
                        {
                            this.billStack.Delete(bill);
                        }
                    }
                };
            }


            var mindFrames = Map.listerThings.AllThings.OfType<MindFrame>().Where(x => x.PersonaData.ContainsPersona).ToList();
            if (ConnectedMatrix != null)
            {
                mindFrames.AddRange(ConnectedMatrix.StoredMindFrames);
            }
            var restoreFromMindFrame = new Command_Action
            {
                defaultLabel = "AC.RestoreFromMindFrame".Translate(),
                defaultDesc = "AC.RestoreFromMindFrameDesc".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Gizmos/RestoreFromMindFrame"),
                activateSound = SoundDefOf.Tick_Tiny,
                action = delegate ()
                {
                    var floatList = new List<FloatMenuOption>();
                    foreach (var mindFrame in mindFrames.Where(x => x.PersonaData.ContainsPersona))
                    {
                        var option = new FloatMenuOption(mindFrame.PersonaData.PawnNameColored, delegate ()
                        {
                            this.mindFrameToRestore = mindFrame;
                        }, mindFrame, mindFrame.DrawColor);
                        floatList.Add(option);
                    }
                    Find.WindowStack.Add(new FloatMenu(floatList));
                }
            };

            if (this.mindFrameToRestore != null)
            {
                restoreFromMindFrame.Disable("AC.AlreadySetToRestore".Translate());
            }
            if (!mindFrames.Any())
            {
                restoreFromMindFrame.Disable("AC.NoMindFrameToRestore".Translate());
            }
            if (this.Powered is false)
            {
                restoreFromMindFrame.Disable("NoPower".Translate());
            }
            yield return restoreFromMindFrame;
            if (this.mindFrameToRestore != null)
            {
                yield return new Command_Action
                {
                    defaultLabel = "AC.CancelMindFrameRestoration".Translate(),
                    defaultDesc = "AC.CancelMindFrameRestorationDesc".Translate(),
                    activateSound = SoundDefOf.Tick_Tiny,
                    icon = UIHelper.CancelIcon,
                    action = delegate ()
                    {
                        this.mindFrameToRestore = null;
                    }
                };
            }

            yield return new Command_Toggle()
            {
                defaultLabel = "AC.EnableAutoRestore".Translate(),
                defaultDesc = "AC.EnableAutoRestoreDesc".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Gizmos/EnableAutoRestore"),
                activateSound = SoundDefOf.Tick_Tiny,
                toggleAction = delegate ()
                {
                    autoRestoreIsEnabled = !autoRestoreIsEnabled;
                },
                isActive = () => autoRestoreIsEnabled
            };

            var stacks = Map.listerThings.AllThings.OfType<PersonaStack>().Where(x => x.PersonaData.ContainsPersona).ToList();
            var duplicateStacks = new Command_Action
            {
                defaultLabel = "AC.DuplicateStack".Translate(),
                defaultDesc = "AC.DuplicateStackDesc".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Gizmos/DuplicateStack"),
                activateSound = SoundDefOf.Tick_Tiny,
                action = delegate ()
                {
                    var floatList = new List<FloatMenuOption>();
                    foreach (var stack in stacks.Where(x => x.PersonaData.ContainsPersona))
                    {
                        if (stack.IsArchotechStack is false)
                        {
                            var option = new FloatMenuOption(stack.PersonaData.PawnNameColored, delegate ()
                            {
                                this.stackToDuplicate = stack;
                            }, stack, stack.DrawColor);
                            floatList.Add(option);
                        }
                    }
                    Find.WindowStack.Add(new FloatMenu(floatList));
                }
            };
            if (this.stackToDuplicate != null)
            {
                duplicateStacks.Disable("AC.AlreadySetToDuplicate".Translate());
            }
            if (!stacks.Any())
            {
                duplicateStacks.Disable("AC.NoStackToDuplicate".Translate());
            }
            if (this.Powered is false)
            {
                duplicateStacks.Disable("NoPower".Translate());
            }
            yield return duplicateStacks;
            duplicateStacks.LockBehindReseach(new List<ResearchProjectDef> { AC_DefOf.AC_RewritePersonaStack });
            if (this.stackToDuplicate != null)
            {
                yield return new Command_Action
                {
                    defaultLabel = "AC.CancelStackDuplication".Translate(),
                    defaultDesc = "AC.CancelStackDuplicationDesc".Translate(),
                    activateSound = SoundDefOf.Tick_Tiny,
                    icon = UIHelper.CancelIcon,
                    action = delegate ()
                    {
                        this.stackToDuplicate = null;
                    }
                };
            }
        }

        public void PerformStackDuplication(Pawn doer)
        {
            float successChance = 1f - Mathf.Abs((doer.skills.GetSkill(SkillDefOf.Intellectual).Level / 2f) - 11f) / 10f;
            if (Rand.Chance(successChance))
            {
                var stackCopyTo = (PersonaStack)ThingMaker.MakeThing(AC_DefOf.AC_FilledPersonaStack);
                stackCopyTo.PersonaData.CopyDataFrom(stackToDuplicate.PersonaData, true);
                AlteredCarbonManager.Instance.RegisterStack(stackCopyTo);
                stackToDuplicate = null;
                GenPlace.TryPlaceThing(stackCopyTo, doer.Position, Map, ThingPlaceMode.Near);
                Messages.Message("AC.SuccessfullyDuplicatedStack".Translate(doer.Named("PAWN")), this, MessageTypeDefOf.TaskCompletion);
            }
            else
            {
                Messages.Message("AC.FailedToDuplicatedStack".Translate(doer.Named("PAWN")), this, MessageTypeDefOf.NeutralEvent);
            }
        }

        private void BeginTargetingForWipingStack()
        {
            Find.Targeter.BeginTargeting(ForFilledStack(includeArchotechStack: false), delegate (LocalTargetInfo x)
            {
                InstallWipeStackBill(x);
                if (Event.current.shift)
                {
                    BeginTargetingForWipingStack();
                }
            });
        }

        public bool CanAddOperationOn(PersonaStack personaStack)
        {
            var bill = this.billStack.Bills.OfType<Bill_OperateOnStack>().Where(x => x.personaStack == personaStack).FirstOrDefault();
            if (bill != null)
            {
                if (bill.recipe == AC_DefOf.AC_WipeFilledPersonaStack)
                {
                    Messages.Message("AC.AlreadyOrderedToWipeStack".Translate(), MessageTypeDefOf.CautionInput);
                }
                else if (bill.recipe == AC_DefOf.AC_RewriteFilledPersonaStack)
                {
                    Messages.Message("AC.AlreadyOrderedToRewriteStack".Translate(), MessageTypeDefOf.CautionInput);
                }
                return false;
            }
            return true;
        }

        private TargetingParameters ForFilledStack(bool includeArchotechStack)
        {
            TargetingParameters targetingParameters = new TargetingParameters
            {
                canTargetItems = true,
                mapObjectTargetsMustBeAutoAttackable = false,
                validator = (TargetInfo x) => x.Thing is PersonaStack stack && stack.PersonaData.ContainsPersona && (includeArchotechStack ||
                stack.IsArchotechStack is false)
            };
            return targetingParameters;
        }

        public void InstallWipeStackBill(LocalTargetInfo x)
        {
            if (x.Thing is PersonaStack personaStack && CanAddOperationOn(personaStack))
            {
                billStack.AddBill(new Bill_OperateOnStack(personaStack, AC_DefOf.AC_WipeFilledPersonaStack, null));
            }
        }

        private void InstallRewriteBill(LocalTargetInfo x)
        {
            if (x.Thing is PersonaStack personaStack && CanAddOperationOn(personaStack))
            {
                Find.WindowStack.Add(new Window_StackEditor(this, personaStack));
            }
        }

        public void PerformStackRestoration(Pawn doer, MindFrame mindFrame, Building_PersonaMatrix matrix)
        {
            var stackRestoreTo = (PersonaStack)ThingMaker.MakeThing(AC_DefOf.AC_FilledPersonaStack);
            stackRestoreTo.PersonaData.CopyDataFrom(mindFrame.PersonaData, true);
            AlteredCarbonManager.Instance.RegisterStack(stackRestoreTo);
            Messages.Message("AC.SuccessfullyRestoredStackFromBackup".Translate(doer.Named("PAWN")), stackRestoreTo, MessageTypeDefOf.TaskCompletion);
            GenPlace.TryPlaceThing(stackRestoreTo, doer.Position, doer.Map, ThingPlaceMode.Near);
            matrix?.innerContainer.Remove(mindFrame);
            mindFrame.Destroy();
            mindFrameToRestore = null;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref this.autoRestoreIsEnabled, "autoRestoreIsEnabled", true);
            Scribe_References.Look(ref this.stackToDuplicate, "stackToDuplicate");
            Scribe_References.Look(ref this.mindFrameToRestore, "mindFrameToRestore");
        }
    }
}