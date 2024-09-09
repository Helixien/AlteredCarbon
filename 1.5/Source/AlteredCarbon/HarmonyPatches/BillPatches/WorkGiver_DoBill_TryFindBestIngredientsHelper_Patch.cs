using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;

namespace AlteredCarbon
{
    [HotSwappable]
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
                var requiredStack = bill is Bill_OperateOnStack operate ? operate.targetThing as NeuralStack
                    : bill is Bill_InstallStack installStack ? installStack.stackToInstall : null;
                if (requiredStack != null)
                {
                    var neuralCaches = pawn.Map.GetAllStackCaches();
                    foreach (var neuralCache in neuralCaches)
                    {
                        var comp = neuralCache.TryGetComp<CompNeuralCache>();
                        var stacks = comp.innerContainer.ToList();
                        if (stacks.Contains(requiredStack))
                        {
                            WorkGiver_DoBill.relevantThings.Add(requiredStack);
                        }
                    }
                    if (foundAllIngredientsAndChoose(WorkGiver_DoBill.relevantThings))
                    {
                        WorkGiver_DoBill.relevantThings.Clear();
                        __result = true;
                    }
                }
                else
                {
                    if (bill.recipe.Worker is Recipe_OperateOnNeuralStack ||
                        AC_Utils.installActiveStacksRecipes.Contains(bill.recipe))
                    {
                        var neuralCaches = pawn.Map.GetAllStackCaches();
                        foreach (var neuralCache in neuralCaches)
                        {
                            var comp = neuralCache.TryGetComp<CompNeuralCache>();
                            var stacks = comp.innerContainer.ToList();
                            WorkGiver_DoBill.relevantThings.AddRange(stacks);
                        }
                        if (foundAllIngredientsAndChoose(WorkGiver_DoBill.relevantThings))
                        {
                            WorkGiver_DoBill.relevantThings.Clear();
                            __result = true;
                        }
                    }
                }
            }
        }
    }
}

