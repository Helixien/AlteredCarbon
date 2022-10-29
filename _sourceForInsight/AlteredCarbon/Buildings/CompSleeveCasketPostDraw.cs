using UnityEngine;
using Verse;

namespace AlteredCarbon
{
	[StaticConstructorOnStartup]
	public class CompSleeveCasketPostDraw : ThingComp
	{
		public Graphic glass;
		public Graphic Glass
		{
			get
			{
				if (glass == null)
				{
					glass = GraphicDatabase.Get<Graphic_Multi>("Things/Building/Furniture/Bed/SleeveCasketTop",
						ShaderDatabase.CutoutComplex, parent.def.graphicData.drawSize, Color.white);
				}
				return glass;
			}
		}
		public override void PostDraw()
		{
			base.PostDraw();
			Vector3 vector = parent.DrawPos + Altitudes.AltIncVect;
			vector.y += 3;
			Glass.Draw(vector, parent.Rotation, parent);
		}
	}
}
