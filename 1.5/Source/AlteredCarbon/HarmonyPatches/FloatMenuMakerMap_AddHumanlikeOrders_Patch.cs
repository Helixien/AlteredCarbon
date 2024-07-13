using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(FloatMenuMakerMap), "AddHumanlikeOrders")]
    public static class FloatMenuMakerMap_AddHumanlikeOrders_Patch
    {
        public static void Postfix(Vector3 clickPos, Pawn pawn, ref List<FloatMenuOption> opts)
        {
            for (var i = opts.Count- 1; i >= 0;--i)
            {
                var option = opts[i];
                if (option.revalidateClickTarget is Pawn pawnTarget && pawnTarget.IsEmptySleeve())
                {
                    var captureLabel = "Capture".Translate(pawnTarget.LabelCap, pawnTarget);
                    var rescueLabel = "Rescue".Translate(pawnTarget.LabelCap, pawnTarget);
                    if (captureLabel == option.Label)
                    {
                        opts.RemoveAt(i);
                    }
                    else if (rescueLabel == option.Label)
                    {
                        option.Label = "AC.TakeToSleeveCasketOrMedicalBed".Translate();
                        option.action = delegate
                        {
                            AC_Utils.AddTakeEmptySleeveJob(pawn, pawnTarget, true);
                        };
                    }
                }
            }

            if (pawn.Wears(AC_DefOf.AC_CuirassierBelt, out var apparel))
            {
                foreach (LocalTargetInfo localTargetInfo in GenUI.TargetsAt(clickPos, ForPowerBuilding(), true))
                {
                    if (JobDriver_ChargeCuirassierBelt.CanDoWork(pawn, apparel, localTargetInfo.Thing as Building, JobDriver_ChargeCuirassierBelt.MakePowerComp(apparel)))
                    {
                        JobDef jobDef = AC_DefOf.AC_ChargeCuirassierBelt;
                        Action action = delegate ()
                        {
                            Job job = JobMaker.MakeJob(jobDef, localTargetInfo, apparel);
                            pawn.jobs.TryTakeOrderedJob(job, 0);
                        };
                        string text = TranslatorFormattedStringExtensions.Translate("AC.ChargeCuirassierBelt",
                            localTargetInfo.Thing.LabelCap, localTargetInfo);
                        FloatMenuOption opt = new FloatMenuOption
                            (text, action, MenuOptionPriority.RescueOrCapture, null, localTargetInfo.Thing, 0f, null, null);
                        if (opts.Where(x => x.Label == text).Count() == 0)
                        {
                            opts.Add(opt);
                        }
                    }
                }
            }

            foreach (LocalTargetInfo localTargetInfo in GenUI.TargetsAt(clickPos, UninstallStack(pawn), true))
            {
                JobDef jobDef = AC_DefOf.AC_ExtractStack;
                Action action = delegate ()
                {
                    Job job = JobMaker.MakeJob(jobDef, localTargetInfo);
                    pawn.jobs.TryTakeOrderedJob(job, 0);
                };
                string text = TranslatorFormattedStringExtensions.Translate("AC.ExtractStack",
                    localTargetInfo.Thing.LabelCap, localTargetInfo);
                FloatMenuOption opt = new FloatMenuOption
                    (text, action, MenuOptionPriority.RescueOrCapture, null, localTargetInfo.Thing, 0f, null, null);
                if (opts.Where(x => x.Label == text).Count() == 0)
                {
                    opts.Add(opt);
                }
            }
        }

        public static TargetingParameters UninstallStack(Pawn p)
        {
            return new TargetingParameters
            {
                canTargetPawns = true,
                canTargetItems = true,
                mapObjectTargetsMustBeAutoAttackable = false,
                validator = (TargetInfo targ) => targ.HasThing &&
                targ.Thing is Corpse corpse && corpse.InnerPawn.HasPersonaStack(out _)
            };
        }

        public static TargetingParameters ForPowerBuilding()
        {
            return new TargetingParameters
            {
                canTargetPawns = false,
                canTargetItems = false,
                canTargetBuildings = true,
                validator = delegate (TargetInfo targ)
                {
                    if (!targ.HasThing)
                    {
                        return false;
                    }
                    return targ.Thing.TryGetComp<CompPower>() is CompPower compPower;
                }
            };
        }
    }
}