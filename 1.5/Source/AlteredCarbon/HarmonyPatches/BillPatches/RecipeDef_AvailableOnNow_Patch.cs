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
			if (AC_Utils.installEmptyStacksRecipes.Contains(__instance) && thing is Pawn pawn)
			{
				if (AC_Utils.CanImplantStackTo(__instance.addsHediff, pawn) is false || pawn.IsEmptySleeve())
				{
					__result = false;
					return false;
				}
			}

			else if (AC_Utils.installActiveStacksRecipes.Contains(__instance) && thing is Pawn pawn2)
            {
				if (AC_Utils.CanImplantStackTo(__instance.addsHediff, pawn2) is false)
				{
					__result = false;
					return false;
				}
			}
			return true;
		}
	}
}

