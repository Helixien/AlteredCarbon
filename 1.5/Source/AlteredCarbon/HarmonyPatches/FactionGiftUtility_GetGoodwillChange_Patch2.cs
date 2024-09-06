using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(FactionGiftUtility), "GetGoodwillChange", new Type[] { typeof(List<Tradeable>), typeof(Faction) })]
    public static class FactionGiftUtility_GetGoodwillChange_Patch2
    {
        public static void Postfix(List<Tradeable> tradeables, Faction theirFaction, ref int __result)
        {
            for (int i = 0; i < tradeables.Count; i++)
            {
                if (tradeables[i].ActionToDo == TradeAction.PlayerSells)
                {
                    if (tradeables[i].AnyThing is NeuralStack neuralStack && neuralStack.NeuralData.ContainsData
                        && neuralStack.NeuralData.faction == theirFaction)
                    {
                        __result += 8;
                    }
                }
            }
        }
    }
}
