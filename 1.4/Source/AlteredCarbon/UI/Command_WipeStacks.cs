using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AlteredCarbon
{
    public class Command_WipeStacks : Command_Action
    {
        public Building_DecryptionBench decryptionBench;
        public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions
        {
            get
            {
                foreach (CorticalStack corticalStack in decryptionBench.Map.listerThings
                    .ThingsOfDef(AC_DefOf.VFEU_FilledCorticalStack).OfType<CorticalStack>())
                {
                    if (corticalStack.PersonaData.ContainsInnerPersona && !decryptionBench.billStack.Bills
                        .Any(x => x is Bill_OperateOnStack hackStack
                            && hackStack.corticalStack == corticalStack && hackStack.recipe == AC_DefOf.VFEU_WipeFilledCorticalStack)
                            && corticalStack.MapHeld == Find.CurrentMap)
                    {
                        yield return new FloatMenuOption(corticalStack.PersonaData.PawnNameColored, delegate ()
                        {
                            decryptionBench.InstallWipeStackRecipe(corticalStack);
                        });
                    }
                }
            }
        }
    }
}