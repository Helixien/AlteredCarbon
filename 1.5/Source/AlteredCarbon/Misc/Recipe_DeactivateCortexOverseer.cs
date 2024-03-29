using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AlteredCarbon
{
    public class Recipe_DeactivateCortexOverseer : Recipe_Surgery
    {
        public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
        {
            List<Hediff> allHediffs = pawn.health.hediffSet.hediffs;
            for (int i = 0; i < allHediffs.Count; i++)
            {
                if (allHediffs[i].Part != null && allHediffs[i] is Hediff_CortexOverseer && allHediffs[i].Visible)
                {
                    yield return allHediffs[i].Part;
                }
            }
        }

        public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
        {
            if (thing is not Pawn pawn || pawn.health.hediffSet.hediffs
                .OfType<Hediff_CortexOverseer>().Any(x => x.activated) is false)
            {
                return false;
            }
            return base.AvailableOnNow(thing, part);
        }

        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            MedicalRecipesUtility.IsClean(pawn, part);
            bool flag = IsViolationOnPawn(pawn, part, Faction.OfPlayer);
            if (billDoer != null)
            {
                if (CheckSurgeryFail(billDoer, pawn, ingredients, part, bill))
                {
                    return;
                }
                TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);
                var hediff = pawn.health.hediffSet.hediffs.OfType<Hediff_CortexOverseer>().First();
                hediff.activated = false;
                pawn.needs.mood.thoughts.memories.TryGainMemory(AC_DefOf.AC_CortexOverseerFreed);
            }
            if (flag)
            {
                ReportViolation(pawn, billDoer, pawn.HomeFaction, -70);
            }
        }
    }
}

