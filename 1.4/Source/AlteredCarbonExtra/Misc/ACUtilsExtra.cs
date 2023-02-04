using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace AlteredCarbon
{
    [StaticConstructorOnStartup]
    public static class ACUtilsExtra
    {
        static ACUtilsExtra()
        {
            Harmony harmony = new("AlteredCarbonExtra");
            harmony.PatchAll();
        }

        public static bool IsUltraTech(this Thing thing)
        {
            return thing.def == AC_DefOf.VFEU_SleeveIncubator
                || thing.def == AC_DefOf.VFEU_SleeveCasket || thing.def == AC_DefOf.VFEU_SleeveCasket
                || thing.def == AC_Extra_DefOf.AC_StackArray
                || thing.def == AC_DefOf.VFEU_DecryptionBench;
        }
    }
}

