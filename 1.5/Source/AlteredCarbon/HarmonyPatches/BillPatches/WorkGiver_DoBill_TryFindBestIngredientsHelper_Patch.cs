using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(WorkGiver_DoBill), "TryFindBestIngredientsHelper")]
    public static class WorkGiver_DoBill_TryFindBestIngredientsHelper_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var method = AccessTools.Method(typeof(RegionTraverser), "BreadthFirstTraverse", new Type[]
            {typeof(Region), typeof(RegionEntryPredicate), typeof(RegionProcessor), typeof(int), typeof(RegionType)});
            var foundAll = typeof(WorkGiver_DoBill).GetNestedTypes(AccessTools.all).SelectMany(t => t.GetFields(AccessTools.all))
                .First(x => x.Name == "foundAll");
            foreach (var instruction in codeInstructions)
            {
                yield return instruction;
                if (instruction.Calls(method))
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Ldflda, foundAll);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Ldarg_3);
                    yield return new CodeInstruction(OpCodes.Call,
                        AccessTools.Method(typeof(WorkGiver_DoBill_TryFindBestIngredientsHelper_Patch), "TryModifyResult"));
                }
            }
        }

        public static void TryModifyResult(ref bool __result, Predicate<List<Thing>> foundAllIngredientsAndChoose, Pawn pawn)
        {
            if (__result is false && WorkGiver_DoBill_TryFindBestBillIngredients_Patch.curBill is Bill bill)
            {
                if (bill.recipe.Worker is Recipe_OperateOnPersonaStack || 
                    AC_Utils.installFilledStacksRecipes.Contains(bill.recipe))
                {
                    var personaCaches = pawn.Map.listerThings.ThingsOfDef(AC_DefOf.AC_PersonaCache);
                    foreach (var personaCache in personaCaches)
                    {
                        var comp = personaCache.TryGetComp<CompPersonaCache>();
                        var stacks = comp.innerContainer.ToList();
                        WorkGiver_DoBill.relevantThings.AddRange(stacks);
                    }
                    if (foundAllIngredientsAndChoose(WorkGiver_DoBill.relevantThings))
                    {
                        __result = true;
                    }
                }
                else if (bill.recipe.Worker is Recipe_OperateOnPersonaPrint)
                {
                    var personaMatrices = pawn.Map.listerThings.ThingsOfDef(AC_DefOf.AC_PersonaMatrix).OfType<Building_PersonaMatrix>();
                    foreach (var personaMatrix in personaMatrices)
                    {
                        WorkGiver_DoBill.relevantThings.AddRange(personaMatrix.StoredPersonaPrints);
                    }
                    if (foundAllIngredientsAndChoose(WorkGiver_DoBill.relevantThings))
                    {
                        __result = true;
                    }
                }
            }
        }
    }
}

