using RimWorld;
using Verse;

namespace AlteredCarbon
{
    [HotSwappable]
    public class Bill_EditStack : Bill_OperateOnStack
    {
        public Pawn curBillDoer;
        public Bill_EditStack()
        {

        }
        public Bill_EditStack(Thing thingWithStack, RecipeDef recipe, Precept_ThingStyle precept = null) 
            : base(thingWithStack, recipe, precept)
        {
        }

        public override void Notify_DoBillStarted(Pawn billDoer)
        {
            base.Notify_DoBillStarted(billDoer);
            this.curBillDoer = billDoer;
        }

        public override float GetWorkAmount(Thing thing = null)
        {
            var neuralStack = targetThing.GetNeuralData();
            var time = (float)neuralStack.editTime;
            if (this.curBillDoer != null)
            {
                var level = curBillDoer.skills.GetSkill(SkillDefOf.Intellectual).Level;
                time -= time * (level * 0.02f);
            }
            return time;
        }

        public override void Notify_BillWorkFinished(Pawn billDoer)
        {
            base.Notify_BillWorkFinished(billDoer);
            this.curBillDoer = null;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref curBillDoer, "curBillDoer");
        }
    }
}

