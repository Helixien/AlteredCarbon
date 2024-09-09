using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{
    public class Filter<T>
    {
        public string Name { get; }
        public Func<T, bool> Logic { get; }

        public Filter(string name, Func<T, bool> logic)
        {
            Name = name;
            Logic = logic;
        }
    }

    public class FilterManager<T>
    {
        private List<Filter<T>> filters;  // Update to use the Filter<T> class
        public Filter<T> currentFilter;   // Now points to a Filter<T> object, not a Func
        private Action<List<T>> setCurrentItems;
        private Func<List<T>> getItems;

        public FilterManager(List<Filter<T>> filters, Action<List<T>> setCurrentItems, Func<List<T>> getItems)
        {
            this.filters = filters;
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
                var filterLabel = currentFilter.Name;  // Get the filter name directly
                Vector2 labelSize = Text.CalcSize(filterLabel);
                addFilterRect = new Rect(inRect.xMax - (labelSize.x + 145), inRect.y, 100, 24);
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
                FloatMenuUtility.MakeMenu(filters, x => x.Name, x => delegate
                {
                    currentFilter = x;  // Set the current filter as the selected filter
                    setCurrentItems(getItems());
                });
            }

            if (currentFilter != null)
            {
                Vector2 labelSize = Text.CalcSize(currentFilter.Name);
                var currentFilterRect = new Rect(addFilterRect.xMax + 15, addFilterRect.y,
                    labelSize.x + 40, addFilterRect.height);
                Widgets.DrawAtlas(currentFilterRect, UIHelper.FilterAtlas);
                var filterLabelRect = new Rect(currentFilterRect.x + 10, currentFilterRect.y, labelSize.x, currentFilterRect.height);
                Widgets.Label(filterLabelRect, currentFilter.Name);
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
