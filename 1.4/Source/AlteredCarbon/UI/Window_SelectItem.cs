using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace AlteredCarbon
{
    [HotSwappable]
    public class Window_SelectItem<T> : Window
    {
        private Vector2 scrollPosition;
        public override Vector2 InitialSize => new Vector2(620f, 500f);
        public List<T> allItems;
        public T chosen;
        public Action<T> actionOnSelect;
        public Func<T, int> ordering;
        public Func<T, string> labelGetter;
        public Func<T, string> tooltipGetter;
        public bool includeInfoCard;
        public List<Func<T, (string, bool)>> filters;
        public Func<T, (string, bool)> currentFilter;
        public Window_SelectItem(T currentItem, List<T> items, Action<T> actionOnSelect, Func<T, int> ordering = null, 
            List<Func<T, (string, bool)>> filters = null, Func<T, string> labelGetter = null, Func<T, string> tooltipGetter = null, bool includeInfoCard = true)
        {
            doCloseButton = true;
            doCloseX = true;
            closeOnClickedOutside = true;
            absorbInputAroundWindow = false;
            this.allItems = items;
            this.actionOnSelect = actionOnSelect;
            this.ordering = ordering;
            this.labelGetter = labelGetter;
            this.tooltipGetter = tooltipGetter;
            this.includeInfoCard = includeInfoCard;
            this.filters = filters;
            chosen = currentItem;
            currentItems = GetItems();
        }
        string searchKey;
        public string GetLabel(T item) => labelGetter != null ? labelGetter(item) : item is Def def ? def.label : "";

        List<T> currentItems;
        public List<T> GetItems()
        {
            var items = searchKey.NullOrEmpty() ? allItems : allItems.Where(x => GetLabel(x).ToLower().Contains(searchKey.ToLower())).ToList();
            if (currentFilter != null)
            {
                items = items.Where(x => currentFilter(x).Item2).ToList();
            }
            if (ordering != null)
            {
                items = items.OrderBy(x => ordering(x)).ThenBy(x => GetLabel(x)).ToList();
            }
            return items;
        }
        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleLeft;
            var searchLabel = new Rect(inRect.x, inRect.y, 60, 24);
            Widgets.Label(searchLabel, "AC.Search".Translate());
            var searchRect = new Rect(searchLabel.xMax + 5, searchLabel.y, 200, 24f);
            var oldSearchKey = searchKey;
            searchKey = Widgets.TextField(searchRect, searchKey);
            if (oldSearchKey != searchKey)
            {
                currentItems = GetItems();
            }
            Text.Anchor = TextAnchor.UpperLeft;
            if (filters != null)
            {
                var addFilterRect = new Rect(inRect.xMax - 120, inRect.y, 100, 24);
                Widgets.DrawAtlas(addFilterRect, UIHelper.FilterAtlas);
                Text.Font = GameFont.Tiny;
                if (addFilterRect.Contains(Event.current.mousePosition))
                {
                    GUI.color = UIHelper.ColorButtonHighlight;
                }
                else
                {
                    GUI.color = UIHelper.ColorText;
                }
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(new Rect(addFilterRect.x + 10, addFilterRect.y, addFilterRect.width + 20, addFilterRect.height), "AC.AddFilter".Translate());
                GUI.DrawTexture(new Rect(addFilterRect.xMax - 20, addFilterRect.y + 6, 11, 8), UIHelper.DropdownIndicator);
                if (Widgets.ButtonInvisible(addFilterRect))
                {
                    FloatMenuUtility.MakeMenu(filters, x => x(chosen).Item1, x => delegate 
                    {
                        currentFilter = x;
                        currentItems = GetItems();
                    });
                }
                Text.Font = GameFont.Small;
                GUI.color = Color.white;
                Text.Anchor = TextAnchor.UpperLeft;
            }


            Rect outRect = new Rect(inRect);
            outRect.y = searchRect.yMax + 5;
            outRect.yMax -= 70f;
            Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, (float)currentItems.Count * 35f);
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
            float yPos = 0f;
            foreach (T item in currentItems)
            {
                if (yPos + 35 >= scrollPosition.y && yPos <= scrollPosition.y + outRect.height)
                {
                    Rect iconRect = new Rect(0f, yPos, 0, 32);
                    if (item is Def def && includeInfoCard)
                    {
                        iconRect.width = 24;
                        Widgets.InfoCardButton(iconRect, def);
                    }
                    if (item is ThingDef thingDef2)
                    {
                        iconRect.x += 24;
                        Widgets.ThingIcon(iconRect, thingDef2);
                    }
                    Rect rect = new Rect(iconRect.xMax + 5, yPos, viewRect.width * 0.7f, 32f);
                    Text.Anchor = TextAnchor.MiddleLeft;
                    Widgets.Label(rect, GetLabel(item));
                    if (tooltipGetter != null)
                    {
                        TooltipHandler.TipRegion(rect, tooltipGetter(item));
                    }
                    Text.Anchor = TextAnchor.UpperLeft;
                    var radioButtonPos = new Vector2(inRect.xMax - 50, rect.y);

                    if (Widgets.RadioButton(radioButtonPos, chosen.Equals(item)))
                    {
                        chosen = item;
                    }
                }
                yPos += 35f;
            }
            Widgets.EndScrollView();
        }
    }
}
