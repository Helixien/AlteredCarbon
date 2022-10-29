using UnityEngine;
using Verse;

namespace AlteredCarbon
{
	[StaticConstructorOnStartup]
	public class CompSleeveGrowerPostDraw : ThingComp
	{
		public CompProperties_SleeveGrowerPostDraw Props => base.props as CompProperties_SleeveGrowerPostDraw;
		public Graphic glass;
		public Graphic Glass
		{
			get
			{
				if (glass == null)
				{
					glass = GraphicDatabase.Get<Graphic_Single>(Props.overlayTexPath, ShaderDatabase.CutoutComplex, parent.def.graphicData.drawSize, Color.white);
				}
				return glass;
			}
		}

		public override void PostDraw()
		{
			base.PostDraw();
			Vector3 vector = parent.DrawPos + Altitudes.AltIncVect;
			vector.y += 6;
			Glass.Draw(vector, Rot4.North, parent);
		}
	}
}
