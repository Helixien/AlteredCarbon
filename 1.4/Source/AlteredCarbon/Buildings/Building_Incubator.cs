using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.Sound;

namespace AlteredCarbon
{
	public enum IncubatorState
	{
		Inactive,
		ToBeCanceled,
		ToBeActivated,
		Growing,
	};
	[HotSwappable]
	public class Building_Incubator : Building, IThingHolder, IOpenable
	{
		public Building_Incubator()
		{
			innerContainer = new ThingOwner<Thing>(this, oneStackOnly: false);
		}

		public int totalTicksToGrow = 0;
		public int curTicksToGrow = 0;

		public float totalGrowthCost = 0;
		public IncubatorState incubatorState;
		protected CompPowerTrader powerTrader;
		protected CompBreakdownable breakdownable;
		protected CompRefuelable compRefuelable;
		protected ThingOwner innerContainer;
		protected bool contentsKnown;

		public int runningOutPowerInTicks;
		public bool isRunningOutPower;
		public int runningOutFuelInTicks;
		public bool isRunningOutFuel;
		public virtual bool HasAnyContents => innerContainer.FirstOrDefault() != null;
		public virtual bool CanOpen => HasAnyContents && incubatorState == IncubatorState.Inactive && !InnerThingIsDead;
		public ThingOwner GetDirectlyHeldThings()
		{
			return innerContainer;
		}

		public void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
		}
		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			if (base.Faction != null && base.Faction.IsPlayer)
			{
				contentsKnown = true;
			}
			powerTrader = base.GetComp<CompPowerTrader>();
			breakdownable = base.GetComp<CompBreakdownable>();
			compRefuelable = base.GetComp<CompRefuelable>();
		}

		public override void TickRare()
		{
			base.TickRare();
			innerContainer.ThingOwnerTickRare();
		}
		public virtual void Open()
		{
			if (HasAnyContents)
			{
				EjectContents();
			}
		}

		public override bool ClaimableBy(Faction by, StringBuilder reason = null)
		{
			if (innerContainer.Any)
			{
				for (int i = 0; i < innerContainer.Count; i++)
				{
					if (innerContainer[i].Faction == by)
					{
						return true;
					}
				}
				return false;
			}
			return base.ClaimableBy(by, reason);
		}

		public virtual bool TryAcceptThing(Thing thing, bool allowSpecialEffects = true)
		{
			if (!Accepts(thing))
			{
				return false;
			}
			bool flag;
			if (thing.holdingOwner != null)
			{
				thing.holdingOwner.TryTransferToContainer(thing, innerContainer, thing.stackCount);
				flag = true;
			}
			else
			{
				flag = innerContainer.TryAdd(thing);
			}
			if (flag)
			{
				if (thing.Faction != null && thing.Faction.IsPlayer)
				{
					contentsKnown = true;
				}
				return true;
			}
			return false;
		}

		public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
		{
			if (innerContainer.Any() && (mode == DestroyMode.Deconstruct || mode == DestroyMode.KillFinalize) && incubatorState == IncubatorState.Inactive
				&& curTicksToGrow == totalTicksToGrow && !InnerThingIsDead)
			{
				if (mode != DestroyMode.Deconstruct)
				{
					List<Pawn> list = new List<Pawn>();
					foreach (Thing item in innerContainer)
					{
						if (item is Pawn pawn)
						{
							list.Add(pawn);
						}
					}
					foreach (Pawn item2 in list)
					{
						HealthUtility.DamageUntilDowned(item2);
					}
				}
				EjectContents();
			}
			innerContainer.ClearAndDestroyContents();
			base.Destroy(mode);
		}

		protected virtual bool InnerThingIsDead { get; }
		public bool IsOperating
		{
			get
			{
				CompPowerTrader compPowerTrader = powerTrader;
				if (compPowerTrader == null || compPowerTrader.PowerOn)
				{
					CompBreakdownable compBreakdownable = breakdownable;
					return compBreakdownable == null || !compBreakdownable.BrokenDown;
				}
				return false;
			}
		}
		public virtual Thing InnerThing => innerContainer.FirstOrDefault();
		public float GrowthProgress => curTicksToGrow / (float)totalTicksToGrow;
		public virtual int OpenTicks => -1;
		public bool Accepts(Thing thing)
		{
			return InnerThing == null;
		}

		public override string GetInspectString()
		{
			string str = base.GetInspectString();
			if (InnerThing != null)
			{
				float growthProgress = GrowthProgress * 100f;
				return str + "\n" + "AC.GrowthProgress".Translate() + ((int)growthProgress).ToString() + "%";
			}
			return str;
		}
		public virtual void EjectContents()
		{
			if (!base.Destroyed)
			{
				SoundStarter.PlayOneShot(SoundDefOf.CryptosleepCasket_Eject, SoundInfo.InMap(new TargetInfo(base.Position, base.Map, false), 0));
			}
			contentsKnown = true;
		}
		public virtual void KillInnerThing()
		{

		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
			Scribe_Values.Look(ref totalTicksToGrow, "totalTicksToGrow", 0, true);
			Scribe_Values.Look(ref curTicksToGrow, "curTicksToGrow", 0, true);
			Scribe_Values.Look(ref totalGrowthCost, "totalGrowthCost", 0f, true);
			Scribe_Values.Look(ref contentsKnown, "contentsKnown", false, true);
			Scribe_Values.Look(ref incubatorState, "incubatorState", IncubatorState.Inactive);
			Scribe_Values.Look(ref isRunningOutPower, "isRunningOutPower", false, true);
			Scribe_Values.Look(ref runningOutPowerInTicks, "runningOutPowerInTicks", 0, true);
			Scribe_Values.Look(ref isRunningOutFuel, "isRunningOutFuel", false, true);
			Scribe_Values.Look(ref runningOutFuelInTicks, "runningOutFuelInTicks", 0, true);

		}
	}
}