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
			if (__instance is Bill_InstallStack installStack && thing is PersonaStack)
            {
				__result = thing == installStack.stackToInstall;
				return false;
			}
			else if (__instance is Bill_OperateOnStack operateOnStack && thing is PersonaStack stack && stack.IsFilledStack)
            {
				__result = stack == operateOnStack.thingWithPersonaData;
				return false;
			}
            return true;
		}
	}
}

