using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(RestUtility), "FindBedFor", 
        new Type[] { typeof(Pawn), typeof(Pawn), typeof(bool), typeof(bool), typeof(GuestStatus?)},
        new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal})]
    public static class RestUtility_FindBedFor_Patch
    {
        public static void Prefix(Pawn sleeper, Pawn traveler)
        {
            if (sleeper.IsEmptySleeve())
            {
                RestUtility.bedDefsBestToWorst_RestEffectiveness.Insert(0, AC_DefOf.AC_SleeveCasket);
                RestUtility.bedDefsBestToWorst_Medical.Insert(0, AC_DefOf.AC_SleeveCasket);
            }
        }
        public static void Postfix()
        {
            RestUtility.bedDefsBestToWorst_RestEffectiveness.RemoveAll(x => x == AC_DefOf.AC_SleeveCasket);
            RestUtility.bedDefsBestToWorst_Medical.RemoveAll(x => x == AC_DefOf.AC_SleeveCasket);
        }
    }

}

