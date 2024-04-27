using HarmonyLib;
using Verse;

namespace AlteredCarbon;

[HarmonyPatch(typeof(Recipe_InstallPersonaStack), "ApplyPersonaStack")]
internal static class Recipe_InstallPersonaStack_ApplyPersonaStack_Patch
{
    private static void Postfix(Pawn pawn)
    {
        if (pawn.HasPersonaStack(out var hediff) && (hediff.PersonaData.ideo?.HasPrecept(AC_DefOf.AC_CrossSleeving_DontCare) ?? false))
        {
            hediff.PersonaData.thoughts?.RemoveAll(x => x.def == AC_DefOf.AC_WrongGender);
            hediff.PersonaData.thoughts?.RemoveAll(x => x.def == AC_DefOf.AC_WrongGenderDouble);
            hediff.PersonaData.thoughts?.RemoveAll(x => x.def == AC_DefOf.AC_WrongGenderPregnant);
        }
    }
    
}
