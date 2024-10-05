using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace AlteredCarbon
{

    [HarmonyPatch(typeof(Pawn_HealthTracker), "Notify_Resurrected")]
    public static class Pawn_HealthTracker_Notify_Resurrected_Patch
    {
        public static void Postfix(Pawn ___pawn)
        {
            foreach (var stackGroup in AlteredCarbonManager.Instance.stacksRelationships)
            {
                if (stackGroup.Value.deadPawns.Contains(___pawn) && !stackGroup.Value.copiedPawns.Contains(___pawn))
                {
                    stackGroup.Value.deadPawns.Remove(___pawn);
                    stackGroup.Value.copiedPawns.Add(___pawn);
                    if (___pawn.IsEmptySleeve())
                    {
                        ___pawn.UndoEmptySleeve();
                    }
                    AlteredCarbonManager.Instance.TryAddRelationships(___pawn, stackGroup.Value);
                    ___pawn.health.AddHediff(AC_DefOf.AC_BrainTrauma, ___pawn.health.hediffSet.GetBrain());
                }
            }
        }
    }
}