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
            foreach (IngredientCount li in AC_Extra_DefOf.AC_HackBiocodedThings.ingredients)
            {
                li.filter = new ThingFilterBiocodable();
                List<ThingDef> list = Traverse.Create(li.filter).Field("thingDefs").GetValue<List<ThingDef>>();
                if (list is null)
                {
                    list = new List<ThingDef>();
                    Traverse.Create(li.filter).Field("thingDefs").SetValue(list);
                }
                foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefs.Where(x => x.comps != null && x.HasAssignableCompFrom(typeof(CompBiocodable))))
                {
                    li.filter.SetAllow(thingDef, true);
                    list.Add(thingDef);
                }
            }
            AC_Extra_DefOf.AC_HackBiocodedThings.fixedIngredientFilter = new ThingFilterBiocodable();
            List<ThingDef> list2 = Traverse.Create(AC_Extra_DefOf.AC_HackBiocodedThings.fixedIngredientFilter).Field("thingDefs").GetValue<List<ThingDef>>();
            if (list2 is null)
            {
                list2 = new List<ThingDef>();
                Traverse.Create(AC_Extra_DefOf.AC_HackBiocodedThings.fixedIngredientFilter).Field("thingDefs").SetValue(list2);
            }

            foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefs.Where(x => x.comps != null && x.HasAssignableCompFrom(typeof(CompBiocodable))))
            {
                list2.Add(thingDef);
                AC_Extra_DefOf.AC_HackBiocodedThings.fixedIngredientFilter.SetAllow(thingDef, true);
            }


            AC_Extra_DefOf.AC_HackBiocodedThings.defaultIngredientFilter = new ThingFilterBiocodable();
            List<ThingDef> list3 = Traverse.Create(AC_Extra_DefOf.AC_HackBiocodedThings.defaultIngredientFilter).Field("thingDefs").GetValue<List<ThingDef>>();
            if (list3 is null)
            {
                list3 = new List<ThingDef>();
                Traverse.Create(AC_Extra_DefOf.AC_HackBiocodedThings.defaultIngredientFilter).Field("thingDefs").SetValue(list2);
            }

            foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefs.Where(x => x.comps != null && x.HasAssignableCompFrom(typeof(CompBiocodable))))
            {
                list3.Add(thingDef);
                AC_Extra_DefOf.AC_HackBiocodedThings.defaultIngredientFilter.SetAllow(thingDef, true);
            }
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

