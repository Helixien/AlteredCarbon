using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using VFECore;

namespace AlteredCarbon
{
    public class JobDriver_ChargeCuirassierBelt : JobDriver
    {
        public Building Building => TargetA.Thing as Building;
        public Apparel Apparel => TargetB.Thing as Apparel;
        public CompPowerTrader _apparelPowerComp;
        public CompPowerTrader ApparelPowerComp => _apparelPowerComp ??= MakePowerComp(Apparel);
        public int chargeDuration;
        public static CompPowerTrader MakePowerComp(Apparel apparel)
        {
            var comp = new CompPowerTrader();
            comp.Initialize(new CompProperties_Power
            {
                compClass = typeof(CompPowerTrader),
                basePowerConsumption = 10,
            });
            comp.parent = apparel;
            comp.powerOnInt = true;
            comp.SetUpPowerVars();
            return comp;
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(TargetA, job);
        }

        public static bool CanDoWork(Pawn pawn, Apparel apparel, Building building, CompPowerTrader compPowerTrader)
        {
            if (apparel.Wearer != pawn)
            {
                return false;
            }
            var comp = building?.PowerComp;
            if (comp is null)
            {
                return false;
            }
            if (building.PowerComp.PowerNet.CanPowerNow(compPowerTrader) is false)
            {
                return false;
            }
            return true;
        }

        public override void Notify_Starting()
        {
            base.Notify_Starting();
            var comp = Apparel.GetComp<CompShieldBubble>();
            chargeDuration = (int)((comp.EnergyMax - comp.Energy) * 10f);

        }

        public override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOn(() => CanDoWork(pawn, Apparel, Building, ApparelPowerComp) is false);
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            Toil doWork = Toils_General.Wait(chargeDuration, TargetIndex.A);
            doWork.initAction = () =>
            {
                AddPowerComp();
            };
            doWork.AddPreTickAction(() =>
            {
                pawn.rotationTracker.FaceCell(TargetThingA.Position);
                var comp = Apparel.GetComp<CompShieldBubble>();
                comp.Energy = Mathf.Min(comp.EnergyMax, comp.Energy + 0.1f);
                if (Building.PowerComp.PowerNet.powerComps.Any(x => x.parent == Apparel) is false)
                {
                    AddPowerComp();
                }
            });
            ToilEffects.WithProgressBarToilDelay(doWork, TargetIndex.A, false, -0.5f);
            ToilFailConditions.FailOnDespawnedNullOrForbidden<Toil>(doWork, TargetIndex.A);
            yield return doWork;
            yield return new Toil
            {
                initAction = delegate ()
                {
                    Building.PowerComp.PowerNet.powerComps.RemoveAll(x => x.parent == Apparel);
                }
            };
        }

        private void AddPowerComp()
        {
            Building.PowerComp.PowerNet.powerComps.Add(ApparelPowerComp);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref chargeDuration, "chargeDuration");
        }
    }
}

