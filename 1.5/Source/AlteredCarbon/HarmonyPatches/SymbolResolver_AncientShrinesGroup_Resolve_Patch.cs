﻿using HarmonyLib;
using RimWorld;
using RimWorld.BaseGen;
using System.Collections.Generic;
using System.Reflection.Emit;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(SymbolResolver_AncientShrinesGroup), "Resolve")]
    public static class SymbolResolver_AncientShrinesGroup_Resolve_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            foreach (var instruction in codeInstructions)
            {
                yield return instruction;
                if (instruction.opcode == OpCodes.Stloc_S && instruction.operand is LocalBuilder lb 
                    && lb.LocalIndex == 8)
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 4);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Call,
                        AccessTools.Method(typeof(SymbolResolver_AncientShrinesGroup_Resolve_Patch), "TryAddMindFrame"));
                }
            }
        }
        public static void TryAddMindFrame(PodContentsType? podContentsType, ResolveParams rp)
        {
            if (AC_Utils.generalSettings.enableMindFramesInAncientDangers && Rand.Chance(0.25f))
            {
                ResolveParams resolveParams = rp;
                var mindFrame = ThingMaker.MakeThing(AC_DefOf.AC_MindFrame) as MindFrame;
                var faction = podContentsType is null || podContentsType.Value != PodContentsType.AncientHostile ? Faction.OfAncients : Faction.OfAncientsHostile;
                mindFrame.GeneratePersona(faction);
                mindFrame.PersonaData.lastTimeUpdated = null;
                resolveParams.singleThingToSpawn = mindFrame;
                BaseGen.symbolStack.Push("thing", resolveParams);
            }
        }
    }
}