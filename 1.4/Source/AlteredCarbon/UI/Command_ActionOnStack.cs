using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AlteredCarbon
{
    [HotSwappable]
    public class Command_ActionOnStack : Command_Action
    {
        public Building_DecryptionBench decryptionBench;

        public TargetingParameters targetParameters;
        public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions
        {
            get
            {
                Find.Targeter.StopTargeting();
                var stacks = decryptionBench.Map.listerThings
                    .ThingsOfDef(AC_DefOf.VFEU_FilledCorticalStack).OfType<CorticalStack>().Where(x => x.PersonaData.ContainsInnerPersona).ToList();
                foreach (CorticalStack corticalStack in stacks)
                {
                    if (targetParameters is null || targetParameters.CanTarget(corticalStack))
                    {
                        yield return new FloatMenuOption(corticalStack.PersonaData.PawnNameColored, delegate ()
                        {
                            if (decryptionBench.CanAddOperationOn(corticalStack))
                            {
                                decryptionBench.InstallWipeStackRecipe(corticalStack);
                            }
                        }, iconThing: corticalStack, iconColor: corticalStack.DrawColor);
                    }
                }
            }
        }
    }
}