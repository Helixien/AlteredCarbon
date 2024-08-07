using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace AlteredCarbon;

[HarmonyPatch(typeof(MentalBreakWorker), "TryStart")]
public class MentalBreakWorker_TryStart_Patch
{
    public static bool Prefix(Pawn pawn)
    {
        if (pawn.IsEmptySleeve())
        {
            return false;
        }
        
        if (pawn.health.hediffSet.HasHediff(AC_DefOf.AC_MentalFuse))
        {
            //Send Letter
            var letter = LetterMaker.MakeLetter(
                "AC.TriggeredMentalFuseLabel".Translate(pawn.Named("PAWN")).AdjustedFor(pawn),
                "AC.TriggeredMentalFuseText".Translate(pawn.Named("PAWN")).AdjustedFor(pawn),
                LetterDefOf.NeutralEvent, new LookTargets(pawn));
            Find.LetterStack.ReceiveLetter(letter);

            //Stop current job
            pawn.jobs.StopAll();
            if (pawn.IsCarrying())
            {
                pawn.carryTracker.TryDropCarriedThing(pawn.PositionHeld, ThingPlaceMode.Near, out _);
            }

            //Give Catharsis
            pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.Catharsis);

            //Roll 5% chance
            if (!pawn.health.hediffSet.HasHediff(AC_DefOf.TraumaSavant) && Rand.Chance(0.05f))
            {
                //Give TraumaSavant
                pawn.health.AddHediff(AC_DefOf.TraumaSavant);

                //Send Trauma LetterX
                Find.LetterStack.ReceiveLetter(
                    "AC.MentalFuseTraumaLabel".Translate(),
                    "AC.MentalFuseTraumaText".Translate(pawn.Named("PAWN")).AdjustedFor(pawn),
                    LetterDefOf.NegativeEvent, pawn);
            }

            return false;
        }

        return true;
    }
}