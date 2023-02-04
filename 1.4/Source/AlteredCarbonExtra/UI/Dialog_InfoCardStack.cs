using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{
    public class Dialog_InfoCardStack : Dialog_InfoCard
    {
        public Dialog_InfoCardStack(Thing thing, Precept_ThingStyle precept = null) : base(thing, precept)
        {

        }

        public override void DoWindowContents(Rect inRect)
        {
            Rect rect = new Rect(inRect);
            rect = rect.ContractedBy(18f);
            rect.height = 34f;
            rect.x += 34f;
            Text.Font = GameFont.Medium;
            Widgets.Label(rect, GetTitle());
            Rect rect2 = new Rect(inRect.x + 9f, rect.y, 34f, 34f);
            if (thing != null)
            {
                Widgets.ThingIcon(rect2, thing);
            }
            else
            {
                Widgets.DefIcon(rect2, def, stuff, 1f, null, drawPlaceholder: true);
            }
            Rect rect3 = new Rect(inRect);
            rect3.yMin = rect.yMax;
            rect3.yMax -= 38f;
            Rect rect4 = rect3;
            List<TabRecord> list = new List<TabRecord>();
            TabRecord item = new TabRecord("TabStats".Translate(), delegate
            {
                tab = InfoCardTab.Stats;
            }, tab == InfoCardTab.Stats);
            list.Add(item);
            if (ThingPawn != null)
            {
                if (ThingPawn.RaceProps.Humanlike)
                {
                    TabRecord item2 = new TabRecord("TabCharacter".Translate(), delegate
                    {
                        tab = InfoCardTab.Character;
                    }, tab == InfoCardTab.Character);
                    list.Add(item2);
                }
                TabRecord item3 = new TabRecord("TabHealth".Translate(), delegate
                {
                    tab = InfoCardTab.Health;
                }, tab == InfoCardTab.Health);
                list.Add(item3);
                if (ModsConfig.RoyaltyActive && ThingPawn.RaceProps.Humanlike && ThingPawn.Faction == Faction.OfPlayer && !ThingPawn.IsQuestLodger() && ThingPawn.royalty != null && PermitsCardUtility.selectedFaction != null)
                {
                    TabRecord item4 = new TabRecord("TabPermits".Translate(), delegate
                    {
                        tab = InfoCardTab.Permits;
                    }, tab == InfoCardTab.Permits);
                    list.Add(item4);
                }
                TabRecord item5 = new TabRecord("TabRecords".Translate(), delegate
                {
                    tab = InfoCardTab.Records;
                }, tab == InfoCardTab.Records);
                list.Add(item5);
            }
            if (list.Count > 1)
            {
                rect4.yMin += 45f;
                TabDrawer.DrawTabs(rect4, list);
            }
            FillCard(rect4.ContractedBy(18f));
            if (def != null && def is BuildableDef)
            {
                IEnumerable<ThingDef> enumerable = GenStuff.AllowedStuffsFor((BuildableDef)def);
                if (enumerable.Count() > 0 && ShowMaterialsButton(inRect, history.Count > 0))
                {
                    List<FloatMenuOption> list2 = new List<FloatMenuOption>();
                    foreach (ThingDef item6 in enumerable)
                    {
                        ThingDef localStuff = item6;
                        list2.Add(new FloatMenuOption(item6.LabelCap, delegate
                        {
                            stuff = localStuff;
                            Setup();
                        }, item6));
                    }
                    Find.WindowStack.Add(new FloatMenu(list2));
                }
            }
            if (history.Count > 0 && Widgets.BackButtonFor(inRect))
            {
                Hyperlink hyperlink = history[history.Count - 1];
                history.RemoveAt(history.Count - 1);
                Find.WindowStack.TryRemove(typeof(Dialog_InfoCard), doCloseSound: false);
                hyperlink.ActivateHyperlink();
            }
        }
    }
}
