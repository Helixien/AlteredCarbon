using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(ContainingSelectionUtility), "CanSelect")]
	public static class ContainingSelectionUtility_CanSelect_Patch
	{
		public static void Postfix(ref bool __result, Thing carriedThing, Thing container)
		{
			if (!__result && container is Building_SleeveGestator && carriedThing is Corpse)
				__result = true;
		}
	}
}