using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(HealthCardUtility), "GenerateSurgeryOption")]
	public static class HealthCardUtility_GenerateSurgeryOption_Patch
	{
		public static void Prefix(Pawn pawn, Thing thingForMedBills, RecipeDef recipe, IEnumerable<ThingDef> missingIngredients, 
			ref AcceptanceReport report, int index, BodyPartRecord part = null)
		{
			if (recipe.Worker is Recipe_InstallImplantAddon implantAddon)
			{
                if (part != null && pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(part) is false)
                {
                    report = "AC.RequiresArtificalParentPart".Translate();
                }
            }
		}
	}
}

