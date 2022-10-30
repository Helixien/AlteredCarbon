using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace AlteredCarbon
{
    internal class IncidentWorker_StackRequest : IncidentWorker
    {
        public override bool CanFireNowSub(IncidentParms parms)
        {
            if (!base.CanFireNowSub(parms))
            {
                return false;
            }
            Map map = (Map)parms.target;
            List<CorticalStack> stacks = GetStacks(map, out _);
            return stacks?.Any() ?? false;
        }

        public override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            List<CorticalStack> stacks = GetStacks(map, out Faction faction);
            parms.faction = faction;
            if (stacks?.Any() ?? false)
            {
                int goodwillChange = stacks.Count() * 5;
                DiaNode diaNode = new("AC.FactionDemandsStacks".Translate(string.Join(", ", stacks.Select(x => x.PersonaData.name)), faction.Named("FACTION")));
                DiaOption accept = new(text: "AC.AppeptFactionDemand".Translate(goodwillChange))
                {
                    action = () =>
                    {
                        SpawnPawns(parms, stacks);
                        faction.TryAffectGoodwillWith(Faction.OfPlayer, goodwillChange);
                    },
                    resolveTree = true
                };
                DiaOption reject = new(text: "AC.RejectFactionDemand".Translate(-goodwillChange))
                {
                    action = () =>
                    {
                        faction.TryAffectGoodwillWith(Faction.OfPlayer, -goodwillChange);
                    },
                    resolveTree = true
                };
                diaNode.options = new List<DiaOption> { accept, reject };
                Find.WindowStack.Add(new Dialog_NodeTree(diaNode, title: def.letterLabel));
                return true;
            }
            return false;
        }

        protected void SpawnPawns(IncidentParms parms, List<CorticalStack> stacks)
        {
            Map map = (Map)parms.target;

            PawnGroupKindDef pawnGroupKindDef = parms.faction.def.pawnGroupMakers.Any(x => x.kindDef == PawnGroupKindDefOf.Peaceful) ? PawnGroupKindDefOf.Peaceful :
                parms.faction.def.pawnGroupMakers.Any(x => x.kindDef == PawnGroupKindDefOf.Settlement) ? PawnGroupKindDefOf.Settlement :
                parms.faction.def.pawnGroupMakers.Any(x => x.kindDef == PawnGroupKindDefOf.Trader) ? PawnGroupKindDefOf.Trader :
                parms.faction.def.pawnGroupMakers.Any(x => x.kindDef == PawnGroupKindDefOf.Combat) ? PawnGroupKindDefOf.Combat :
                parms.faction.def.pawnGroupMakers.Select(x => x.kindDef).FirstOrDefault();
            int minimumPawnCount = (int)(stacks.Count * 1.7f);
            List<Pawn> list = new();
            while (list.Count < minimumPawnCount)
            {
                List<Pawn> temp = PawnGroupMakerUtility.GeneratePawns(IncidentParmsUtility.GetDefaultPawnGroupMakerParms(pawnGroupKindDef, parms, ensureCanGenerateAtLeastOnePawn: true), warnOnZeroResults: false).ToList();
                foreach (Pawn item in temp)
                {
                    if (!parms.spawnCenter.IsValid && !RCellFinder.TryFindRandomPawnEntryCell(out parms.spawnCenter, map, CellFinder.EdgeRoadChance_Friendly))
                    {
                        return;
                    }
                    parms.spawnRotation = Rot4.FromAngleFlat((map.Center - parms.spawnCenter).AngleFlat);
                    IntVec3 loc = CellFinder.RandomClosewalkCellNear(parms.spawnCenter, map, 8);
                    GenSpawn.Spawn(item, loc, map, parms.spawnRotation);

                    item.mindState.duty = new PawnDuty(AC_Extra_DefOf.VFEU_TakeStacks, stacks.RandomElement().Position);
                }
                list.AddRange(temp);
            }
            LordMaker.MakeNewLord(parms.faction, new LordJob_TakeStacks(), map, list);
        }
        public List<CorticalStack> GetStacks(Map map, out Faction faction)
        {
            IEnumerable<CorticalStack> corticalStacks = map.listerThings.ThingsOfDef(AC_DefOf.VFEU_FilledCorticalStack).Cast<CorticalStack>()
                .Where(x => x.PersonaData.faction != null && x.PersonaData.faction.def.humanlikeFaction && !x.PersonaData.faction.IsPlayer
                && x.PersonaData.faction.RelationKindWith(Faction.OfPlayer) != FactionRelationKind.Hostile);
            Dictionary<Faction, List<CorticalStack>> factionCandidates = new();
            foreach (CorticalStack stack in corticalStacks)
            {
                if (stack.PersonaData.faction != null && stack.PersonaData.faction.def?.pawnGroupMakers != null && !map.mapPawns.PawnsInFaction(stack.PersonaData.faction).Any())
                {
                    if (factionCandidates.ContainsKey(stack.PersonaData.faction))
                    {
                        factionCandidates[stack.PersonaData.faction].Add(stack);
                    }
                    else
                    {
                        factionCandidates[stack.PersonaData.faction] = new List<CorticalStack> { stack };
                    }
                }
            }

            if (factionCandidates.TryRandomElementByWeight(x => x.Value.Count, out KeyValuePair<Faction, List<CorticalStack>> value))
            {
                faction = value.Key;
                return value.Value;
            }
            faction = null;
            return null;
        }
    }
}

