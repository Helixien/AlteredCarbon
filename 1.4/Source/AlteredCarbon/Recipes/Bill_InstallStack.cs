using RimWorld;
using Verse;

namespace AlteredCarbon
{
	public class Bill_InstallStack : Bill_Medical
	{
		public CorticalStack stackToInstall;
		public Bill_InstallStack()
		{

		}

		public Bill_InstallStack(RecipeDef recipe, CorticalStack corticalStack) : base(recipe, null)
		{
			stackToInstall = corticalStack;
		}
		public override string Label => this.stackToInstall.IsFilledStack 
			? base.Label + " (" + (stackToInstall?.PersonaData?.PawnNameColored ?? "Destroyed".Translate()) + ")" : base.Label;
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref stackToInstall, "stackToInstall");
		}

		public override Bill Clone()
		{
			Bill_InstallStack obj = (Bill_InstallStack)base.Clone();
			obj.Part = Part;
			obj.stackToInstall = stackToInstall;
			obj.consumedInitialMedicineDef = consumedInitialMedicineDef;
			return obj;
		}
	}
}

