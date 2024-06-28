using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace AlteredCarbon
{

    public class Recipe_RemovePersonaStack : Recipe_Surgery
	{
		public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
		{
			List<Hediff> allHediffs = pawn.health.hediffSet.hediffs;
			for (int i = 0; i < allHediffs.Count; i++)
			{
				if (allHediffs[i].Part != null && allHediffs[i].def == recipe.removesHediff && allHediffs[i].Visible)
				{
					yield return allHediffs[i].Part;
				}
			}
		}

		public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
		{
			MedicalRecipesUtility.IsClean(pawn, part);
			bool flag = IsViolationOnPawn(pawn, part, Faction.OfPlayer);
			if (billDoer != null)
			{
				TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);
				if (!pawn.health.hediffSet.GetNotMissingParts().Contains(part))
				{
					return;
				}

				if (pawn.HasPersonaStack(out var hediff))
				{
					var personaStack = ThingMaker.MakeThing(hediff.def.spawnThingOnRemoved) as PersonaStack;
					personaStack.PersonaData.CopyFromPawn(hediff.pawn, hediff.SourceStack);
					personaStack.PersonaData.CopyOriginalData(hediff.PersonaData);
                    personaStack.PersonaData.RefreshDummyPawn();
                    GenPlace.TryPlaceThing(personaStack, billDoer.Position, billDoer.Map, ThingPlaceMode.Near);
					hediff.preventSpawningStack = true;
                    pawn.health.RemoveHediff(hediff);
                    hediff.preventSpawningStack = false;
                    var head = pawn.health.hediffSet.GetNotMissingParts().FirstOrDefault((BodyPartRecord x) => x.def == BodyPartDefOf.Head);
                    if (head != null)
                    {
                        pawn.TakeDamage(new DamageInfo(DamageDefOf.SurgicalCut, 99999f, 999f, -1f, null, head));
                    }
					AlteredCarbonManager.Instance.ReplacePawnWithStack(pawn, personaStack);
					AlteredCarbonManager.Instance.RegisterSleeve(pawn, personaStack);
					AlteredCarbonManager.Instance.deadPawns.Add(pawn);
					personaStack.PersonaData.hostPawn = null;
                    if (LookTargets_Patch.targets.TryGetValue(pawn, out var targets))
					{
						foreach (var target in targets)
						{
							target.targets.Remove(pawn);
							target.targets.Add(personaStack);
						}
					}
				}

			}
			if (flag)
			{
				ReportViolation(pawn, billDoer, pawn.Faction, -70);
			}
		}
	}
}

