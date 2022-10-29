using System;
using System.Collections.Generic;
using HarmonyLib;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(LookTargets), MethodType.Constructor, new Type[] { typeof(Thing) })]
    public static class LookTargets_Patch
    {
        public static Dictionary<Pawn, List<LookTargets>> targets = new Dictionary<Pawn, List<LookTargets>>();
        public static void Postfix(LookTargets __instance, Thing t)
        {
            if (t is Pawn pawn)
            {
                if (targets.ContainsKey(pawn))
                {
                    targets[pawn].Add(__instance);
                }
                else
                {
                    targets[pawn] = new List<LookTargets> { __instance };
                }
            }
        }
    }
}

