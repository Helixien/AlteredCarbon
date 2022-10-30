using RimWorld;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using Verse;

namespace AlteredCarbon
{
	public class RoyalTitlePermitWorker_CallNeuroformers : RoyalTitlePermitWorker
	{
        public override IEnumerable<FloatMenuOption> GetRoyalAidOptions(Map map, Pawn pawn, Faction faction)
        {
            if (AidDisabled(map, pawn, faction, out string reason))
            {
                yield return new FloatMenuOption(def.LabelCap + ": " + reason, null);
                yield break;
            }
            Action action = null;
            string description = def.LabelCap + " (" + "AC.CommandCallsNeuroformers".Translate() + "): ";
            if (FillAidOption(pawn, faction, ref description, out bool free))
            {
                action = delegate
                {
                    CallNeuroformers(pawn, map, faction, free);
                };
            }
            yield return new FloatMenuOption(description, action, faction.def.FactionIcon, faction.Color);
        }

        private void CallNeuroformers(Pawn pawn, Map map, Faction faction, bool free)
        {
            if (!faction.HostileTo(Faction.OfPlayer))
            {
                List<Thing> things = new List<Thing>();
                int InRange = pawn.GetMaxPsylinkLevelByTitle();
                for (int i = 0; i < InRange; i++)
                {
                    things.Add(ThingMaker.MakeThing(ThingDefOf.PsychicAmplifier));
                }
                DropPodUtility.DropThingsNear(pawn.Position, pawn.Map, things);
                pawn.royalty.GetPermit(def, faction).Notify_Used();
                if (!free)
                {
                    pawn.royalty.TryRemoveFavor(faction, def.royalAid.favorCost);
                }
            }
        }
	}
}
