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
                if (stackGroup.Value.deadPawns != null && stackGroup.Value.deadPawns.Contains(___pawn))
                {
                    stackGroup.Value.deadPawns.Remove(___pawn);
                    stackGroup.Value.copiedPawns.Add(___pawn);
                    if (AlteredCarbonManager.Instance.emptySleeves != null && AlteredCarbonManager.Instance.emptySleeves.Contains(___pawn))
                    {
                        AlteredCarbonManager.Instance.emptySleeves.Remove(___pawn);
                    }
                    AlteredCarbonManager.Instance.TryAddRelationships(___pawn);
                }
            }
        }
    }
}