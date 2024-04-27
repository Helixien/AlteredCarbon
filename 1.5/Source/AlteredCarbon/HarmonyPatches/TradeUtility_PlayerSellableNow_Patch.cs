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
            if (t is PersonaStack personaStack && personaStack.PersonaData.ContainsInnerPersona)
            {
                if (TradeSession.giftMode is false)
                {
                    __result = false;
                }
                else if (personaStack.PersonaData.faction != trader.Faction)
                {
                    __result = false;
                }
            }
        }
    }
}
