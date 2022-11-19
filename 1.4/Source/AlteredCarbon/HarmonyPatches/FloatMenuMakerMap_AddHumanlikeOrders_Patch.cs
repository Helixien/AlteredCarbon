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
            foreach (LocalTargetInfo localTargetInfo in GenUI.TargetsAt(clickPos, UninstallStack(pawn), true))
            {
                JobDef jobDef = AC_DefOf.VFEU_ExtractStack;
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
                targ.Thing is Corpse corpse && corpse.InnerPawn.HasCorticalStack(out _)
            };
        }
    }
}