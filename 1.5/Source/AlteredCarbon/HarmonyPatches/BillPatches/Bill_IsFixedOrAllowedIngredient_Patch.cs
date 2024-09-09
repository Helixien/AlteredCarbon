using System;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace AlteredCarbon
{
	[HarmonyPatch(typeof(Bill), "IsFixedOrAllowedIngredient", new Type[] { typeof(Thing) })]
	public static class Bill_IsFixedOrAllowedIngredient_Patch
    {
		private static bool Prefix(ref bool __result, Bill __instance, Thing thing)
		{
			if (__instance is Bill_InstallStack installStack && thing is NeuralStack)
            {
				__result = thing == installStack.stackToInstall;
                return false;
			}
			else if (__instance is Bill_OperateOnStack operateOnStack)
            {
				if (thing is ThingWithNeuralData data  && data.NeuralData.ContainsData)
				{
                    __result = thing == operateOnStack.targetThing;
                    return false;
                }
			}
			else if (__instance is Bill_OperateOnThing operateOnThing)
			{
                __result = thing == operateOnThing.targetThing;
                return false;
            }
            return true;
		}
	}
}

