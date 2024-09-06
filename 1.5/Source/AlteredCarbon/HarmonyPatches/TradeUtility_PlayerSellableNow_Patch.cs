using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(TradeUtility), "PlayerSellableNow")]
    public static class TradeUtility_PlayerSellableNow_Patch
    {
        public static void Postfix(ref bool __result, Thing t, ITrader trader)
        {
            if (t is NeuralStack neuralStack && neuralStack.NeuralData.ContainsData)
            {
                if (TradeSession.giftMode is false)
                {
                    __result = false;
                }
                else if (neuralStack.NeuralData.faction != trader.Faction)
                {
                    __result = false;
                }
            }
        }
    }
}
