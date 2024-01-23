using HarmonyLib;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(FactionGiftUtility), "GetGoodwillChange", new Type[] { typeof(IEnumerable<IThingHolder>), typeof(Settlement) })]
    public static class FactionGiftUtility_GetGoodwillChange_Patch1
    {
        public static void Postfix(IEnumerable<IThingHolder> pods, Settlement giveTo, ref int __result)
        {
            foreach (IThingHolder pod in pods)
            {
                ThingOwner directlyHeldThings = pod.GetDirectlyHeldThings();
                for (int i = 0; i < directlyHeldThings.Count; i++)
                {
                    if (directlyHeldThings[i] is CorticalStack corticalStack && corticalStack.PersonaData.ContainsInnerPersona 
                        && corticalStack.PersonaData.faction == giveTo.Faction)
                    {
                        __result += 8;
                    }
                }
            }
        }
    }
}
