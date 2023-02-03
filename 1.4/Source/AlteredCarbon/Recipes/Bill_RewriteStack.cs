using RimWorld;
using Verse;
using Verse.AI;

namespace AlteredCarbon
{
    public class Bill_RewriteStack : Bill_OperateOnStack
    {
        public Pawn curBillDoer;
        public Bill_RewriteStack()
        {

        }
        public Bill_RewriteStack(CorticalStack corticalStack, RecipeDef recipe, Precept_ThingStyle precept = null) 
            : base(corticalStack, recipe, precept)
        {
        }

        public override void Notify_DoBillStarted(Pawn billDoer)
        {
            base.Notify_DoBillStarted(billDoer);
            this.curBillDoer = billDoer;
            Log.Message("this.curBillDoer: " + this.curBillDoer + " - " + this.recipe.effectWorking);
        }

        public override float GetWorkAmount(UnfinishedThing uft = null)
        {
            var time = (float)this.corticalStack.personaDataRewritten.editTime;
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

