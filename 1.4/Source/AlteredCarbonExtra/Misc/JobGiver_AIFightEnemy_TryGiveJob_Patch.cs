using HarmonyLib;
using RimWorld;
using System;
using Verse;
using Verse.AI;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(JobGiver_AIFightEnemy), "TryGiveJob")]
    public static class JobGiver_AIFightEnemy_TryGiveJob_Patch
    {
        public static void Postfix(ref Job __result, Pawn pawn)
        {
            if (__result is null && pawn.Faction != Faction.OfPlayer)
            {
                JobGiver_TakeStackWhenClose jbg = new();
                ThinkResult result = jbg.TryIssueJobPackage(pawn, default);
                if (result.Job != null)
                {
                    __result = result.Job;
                }
            }
        }
    }
}

