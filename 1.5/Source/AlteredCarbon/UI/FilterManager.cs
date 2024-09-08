using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{
    [HotSwappable]
    public class FilterManager<T>
    {
        private List<Func<T, (string, bool)>> filters;
        public Func<T, (string, bool)> currentFilter;
        private Func<T> chosenAction;
        private Action<List<T>> setCurrentItems;
        private Func<List<T>> getItems;

        public FilterManager(List<Func<T, (string, bool)>> filters, Func<T> chosenAction,
            Action<List<T>> setCurrentItems, Func<List<T>> getItems)
        {
            this.filters = filters;
            this.chosenAction = chosenAction;
            this.setCurrentItems = setCurrentItems;
            this.getItems = getItems;
        }

        public void DoFilters(Rect inRect)
        {
            Rect addFilterRect;
            if (currentFilter == null)
            {
                addFilterRect = new Rect(inRect.xMax - 100, inRect.y, 100, 24);
            }
            else
            {
                // Adjust the addFilterRect position to account for the currentFilterRect width
                var filterLabel = currentFilter(chosenAction()).Item1;
                Vector2 labelSize = Text.CalcSize(filterLabel);
                addFilterRect = new Rect(inRect.xMax - (labelSize.x + 135), inRect.y, 100, 24);
            }

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
                FloatMenuUtility.MakeMenu(filters, x => x(chosenAction()).Item1, x => delegate
                {
                    currentFilter = x;
                    setCurrentItems(getItems());
                });
            }

            if (currentFilter != null)
            {
                // Dynamically calculate the width of currentFilterRect based on the label size
                Vector2 labelSize = Text.CalcSize(currentFilter(chosenAction()).Item1);
                var currentFilterRect = new Rect(addFilterRect.xMax + 15, addFilterRect.y, 
                    labelSize.x + 40, addFilterRect.height);
                Widgets.DrawAtlas(currentFilterRect, UIHelper.FilterAtlas);
                var filterLabelRect = new Rect(currentFilterRect.x + 10, currentFilterRect.y, labelSize.x, currentFilterRect.height);
                Widgets.Label(filterLabelRect, currentFilter(chosenAction()).Item1);
                var removeX = new Rect(filterLabelRect.xMax + 10, filterLabelRect.y + 8, 9f, 9f);
                GUI.DrawTexture(removeX, UIHelper.ButtonCloseSmall);

                if (Widgets.ButtonInvisible(currentFilterRect))
                {
                    currentFilter = null;
                    setCurrentItems(getItems());
                }
            }

            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
        }
    }
}
