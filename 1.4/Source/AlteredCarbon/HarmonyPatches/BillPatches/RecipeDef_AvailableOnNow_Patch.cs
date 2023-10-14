using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(RecipeDef), "AvailableOnNow")]
	public static class RecipeDef_AvailableOnNow_Patch
    {
		private static bool Prefix(RecipeDef __instance, Thing thing, ref bool __result)
		{
			if (ACUtils.installEmptyStacksRecipes.Contains(__instance) && thing is Pawn pawn)
			{
				if (ACUtils.CanImplantStackTo(__instance.addsHediff, pawn) is false || pawn.IsEmptySleeve())
				{
					__result = false;
					return false;
				}
			}

			else if (ACUtils.installFilledStacksRecipes.Contains(__instance) && thing is Pawn pawn2)
            {
				if (ACUtils.CanImplantStackTo(__instance.addsHediff, pawn2) is false)
				{
					__result = false;
					return false;
				}
			}
			return true;
		}
	}
}

