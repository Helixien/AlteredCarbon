using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using Verse;
using Verse.AI;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(ReverseDesignatorDatabase), "InitDesignators")]
    public static class ReverseDesignatorDatabase_InitDesignators_Patch
    {
        public static void Postfix(ref List<Designator> ___desList)
        {
            ___desList.Add(new Designator_ExtractStack());
        }
    }
}

