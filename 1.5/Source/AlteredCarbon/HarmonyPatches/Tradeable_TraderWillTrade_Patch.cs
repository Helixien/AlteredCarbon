using HarmonyLib;
using RimWorld;
using System.Linq;
using Verse;

namespace AlteredCarbon
{
    //[HarmonyPatch(typeof(Tradeable), "TraderWillTrade", MethodType.Getter)]
    //public static class Tradeable_TraderWillTrade_Patch
    //{
    //    public static void Postfix(ref bool __result, Tradeable __instance)
    //    {
    //        if (__result is false && __instance.AnyThing is Thing thing)
    //        {
    //            var comp = thing.TryGetComp<CompBiocodable>();
    //            if (comp != null && comp.Biocoded is false)
    //            {
    //                if (TradeSession.trader.TraderKind.stockGenerators.Any(x => x is StockGenerator_Category category
    //                && category.categoryDef == AC_DefOf.WeaponsMelee || x is StockGenerator_MarketValue marketValue 
    //                && marketValue.tradeTag == "WeaponMelee"))
    //                {
    //                    __result = true;
    //                }
    //            }
    //        }
    //    }
    //}
}