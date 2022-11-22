using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(RecipeDef), "AvailableOnNow")]
	public static class RecipeDef_AvailableOnNow_Patch
    {
		public static HashSet<ThingDef> unstackableRaces = InitCache();
		static HashSet<ThingDef> InitCache()
		{
			if (ModCompatibility.AlienRacesIsActive)
			{
				HashSet<ThingDef> excludedRaces = new HashSet<ThingDef>();
				foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs.Where(def => def.category == ThingCategory.Pawn))
				{
					if (def.GetModExtension<ExcludeRacesModExtension>() is ExcludeRacesModExtension props)
					{
						if (!props.acceptsStacks)
						{
							excludedRaces.Add(def);
						}
					}
				}
				return excludedRaces;
			}
			else
            {
				return new HashSet<ThingDef>();
            }
		}
		private static bool Prefix(RecipeDef __instance, Thing thing, ref bool __result)
		{
			if (ACUtils.installEmptyStacksRecipes.Contains(__instance) && thing is Pawn pawn)
			{
				if (unstackableRaces.Contains(pawn.def) || pawn.IsEmptySleeve())
				{
					__result = false;
					return false;
				}
			}

			else if (ACUtils.installFilledStacksRecipes.Contains(__instance) && thing is Pawn pawn2)
            {
				if (unstackableRaces.Contains(pawn2.def))
				{
					__result = false;
					return false;
				}
			}
			return true;
		}
	}
}

