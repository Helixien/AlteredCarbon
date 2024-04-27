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

    public class Recipe_InstallImplantAddon : Recipe_InstallImplant
    {
        public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
        {
            return MedicalRecipesUtility.GetFixedPartsToApplyOn(recipe, pawn, delegate (BodyPartRecord record)
            {
                return (!pawn.health.hediffSet.hediffs.Any((Hediff x) => x.Part == record && (x.def == recipe.addsHediff || !recipe.CompatibleWithHediff(x.def)))) ? true : false;
            });
        }

        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            if (billDoer != null)
            {
                if (CheckSurgeryFail(billDoer, pawn, ingredients, part, bill))
                {
                    return;
                }
                TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);
            }
			pawn.health.RestorePart(part);
            pawn.health.AddHediff(recipe.addsHediff, part);
        }
    }

    public class Bill_InstallStack : Bill_Medical
	{
		public PersonaStack stackToInstall;
		public Bill_InstallStack()
		{

		}

		public Bill_InstallStack(RecipeDef recipe, PersonaStack personaStack) : base(recipe, null)
		{
			stackToInstall = personaStack;
		}
		public override string Label => this.stackToInstall.IsFilledStack 
			? base.Label + " (" + (stackToInstall?.PersonaData?.PawnNameColored ?? "Destroyed".Translate()) + ")" : base.Label;
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref stackToInstall, "stackToInstall");
		}

		public override Bill Clone()
		{
			Bill_InstallStack obj = (Bill_InstallStack)base.Clone();
			obj.Part = Part;
			obj.stackToInstall = stackToInstall;
			obj.consumedInitialMedicineDef = consumedInitialMedicineDef;
			return obj;
		}
	}
}

