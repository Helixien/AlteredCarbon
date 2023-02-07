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
        public bool alphabetOrder;
        public Func<T, string> labelGetter;
        public Func<T, string> tooltipGetter;
        public bool includeInfoCard;
        public List<Func<T, (string, bool)>> filters;
        public Func<T, (string, bool)> currentFilter;
        public Func<T, Texture2D> icon;
        public Func<T, Color?> iconColor;
        public Func<T, string> labelGetterPostfix;
        public Window_SelectItem(T currentItem, List<T> items, Action<T> actionOnSelect, bool alphabetOrder = true, 
            List<Func<T, (string, bool)>> filters = null, Func<T, string> labelGetter = null, Func<T, string> tooltipGetter = null, 
            bool includeInfoCard = true, Func<T, Texture2D> icon = null, Func<T, Color?> iconColor = null, Func<T, string> labelGetterPostfix = null)
        {
            doCloseX = true;
            doCloseButton = false;
            absorbInputAroundWindow = true;
            this.allItems = items;
            this.actionOnSelect = actionOnSelect;
            this.alphabetOrder = alphabetOrder;
            this.labelGetter = labelGetter;
            this.tooltipGetter = tooltipGetter;
            this.includeInfoCard = includeInfoCard;
            this.filters = filters;
            this.icon = icon;
            this.iconColor = iconColor;
            this.labelGetterPostfix = labelGetterPostfix;
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
            if (alphabetOrder)
            {
                items = items.OrderBy(x => GetLabel(x)).ToList();
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
                var addFilterRect = new Rect(inRect.xMax - 300, inRect.y, 100, 24);
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
                GUI.color = Color.white;
                if (Widgets.ButtonInvisible(addFilterRect))
                {
                    FloatMenuUtility.MakeMenu(filters, x => x(chosen).Item1, x => delegate 
                    {
                        currentFilter = x;
                        currentItems = GetItems();
                    });
                }
                if (currentFilter != null)
                {
                    var currentFilterRect = new Rect(addFilterRect.xMax + 15, addFilterRect.y, 180, addFilterRect.height);
                    Widgets.DrawAtlas(currentFilterRect, UIHelper.FilterAtlas);
                    var filterLabel = currentFilter(chosen).Item1;
                    var filterLabelRect = new Rect(currentFilterRect.x + 10, currentFilterRect.y, currentFilterRect.width - 30,
                        currentFilterRect.height);
                    Widgets.Label(filterLabelRect, filterLabel);
                    var removeX = new Rect(filterLabelRect.xMax, filterLabelRect.y + 8, 9f, 9f);
                    GUI.DrawTexture(removeX, UIHelper.ButtonCloseSmall);
                    if (Widgets.ButtonInvisible(currentFilterRect))
                    {
                        currentFilter = null;
                    }
                }
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.UpperLeft;
            }


            Rect outRect = new Rect(inRect.x, searchRect.yMax + 15, inRect.width, inRect.height - (32 + 15 + 24 + 15));
            Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, (float)currentItems.Count * 35f);
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
            float yPos = 0f;
            foreach (T item in currentItems)
            {
                if (yPos + 35 >= scrollPosition.y && yPos <= scrollPosition.y + outRect.height)
                {
                    Rect iconRect = new Rect(0f, yPos, 0, 32);
                    //if (item is Def def && includeInfoCard)
                    //{
                    //    iconRect.width = 24;
                    //    Widgets.InfoCardButton(iconRect, def);
                    //}
                    //if (item is ThingDef thingDef2)
                    //{
                    //    iconRect.x += 24;
                    //    Widgets.ThingIcon(iconRect, thingDef2);
                    //}

                    var label = GetLabel(item);
                    var size = Text.CalcSize(label);
                    Rect rect = new Rect(5, yPos, size.x + 35, size.y);

                    Text.Anchor = TextAnchor.MiddleLeft;
                    if (icon != null)
                    {
                        UIHelper.LabelWithIcon(rect, label, icon(item), iconColor != null ? iconColor(item) : null, labelIconScale: 1f,
                            labelRectShift: 35);
                    }
                    else
                    {
                        Widgets.Label(rect, label);
                    }

                    if (labelGetterPostfix != null)
                    {
                        label = labelGetterPostfix(item);
                        size = Text.CalcSize(label);
                        var postfixRect = new Rect(rect.xMax, yPos, size.x, size.y);
                        Widgets.Label(postfixRect, label);
                    }
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


            var cancelButtonRect = new Rect(inRect.x + 35, inRect.yMax - 32, 200, 32);
            if (Widgets.ButtonText(cancelButtonRect, "Cancel".Translate()))
            {
                SoundDefOf.Click.PlayOneShotOnCamera();
                this.Close();
            }

            var acceptButtonRect = new Rect(inRect.xMax - 200 - 35, cancelButtonRect.y, 200, 32);
            if (Widgets.ButtonText(acceptButtonRect, "Accept".Translate()))
            {
                actionOnSelect(chosen);
                SoundDefOf.Click.PlayOneShotOnCamera();
                this.Close();
            }
        }
    }
}
