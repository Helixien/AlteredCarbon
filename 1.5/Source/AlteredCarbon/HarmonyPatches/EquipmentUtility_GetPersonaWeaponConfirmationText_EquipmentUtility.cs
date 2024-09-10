using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(EquipmentUtility), nameof(EquipmentUtility.GetPersonaWeaponConfirmationText))]
    public static class EquipmentUtility_GetPersonaWeaponConfirmationText_Patch
    {
        public static void Postfix(Thing item, Pawn p, ref string __result)
        {
            var comp = item.TryGetComp<CompBiocodable>();
            if (comp != null && comp.CodedPawn != p &&
                CompBiocodable_PostExposeData_Patch.wasBiocoded.TryGet(comp, out var wasBiocoded) && wasBiocoded)
            {
                __result = "AC.BiocodableEquipWarning".Translate();
            }
        }
    }
}

