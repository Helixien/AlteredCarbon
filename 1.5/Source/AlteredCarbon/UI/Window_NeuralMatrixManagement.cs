using Verse;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using RimWorld;

namespace AlteredCarbon
{
    [StaticConstructorOnStartup]
    [HotSwappable]
    public class Window_NeuralMatrixManagement : Window
    {
        public Building_NeuralMatrix matrix;
        private Vector2 scrollPosition;
        private IStackHolder selectedStack;
        private FilterManager<IStackHolder> filterManager;
        private List<IStackHolder> allItems, currentItems;
        private List<TabRecord> tabsList = new List<TabRecord>();

        public enum StackTab : byte
        {
            Log,
            Social,
            Character,
            Records,
        }

        private StackTab tab;

        public Window_NeuralMatrixManagement(Building_NeuralMatrix matrix)
        {
            this.matrix = matrix;
            this.doCloseX = true;
            this.doWindowBackground = true;
            this.forcePause = true;

            var items = matrix.AllNeuralStacks.Cast<IStackHolder>();
            var hediffs = matrix.Map.mapPawns.AllHumanlike.Select(x => x.GetNeuralStack()).Where(x => x != null).Cast<IStackHolder>();
            allItems = currentItems = [.. items, .. hediffs];

            var filters = new List<Func<IStackHolder, (string, bool)>>();

            (string, bool) Friendly(IStackHolder stackHolder)
            {
                return ("AC.Friendly".Translate(), stackHolder.NeuralData.Friendly);
            }

            filters.Add(Friendly);

            this.filterManager = new FilterManager<IStackHolder>(filters, () => selectedStack,
                    newItems => currentItems = newItems,
                    GetItems);
        }

        private void CreateTabs()
        {
            tabsList.Clear();
            tabsList.Add(new TabRecord("TabLog".Translate(), delegate
            {
                tab = StackTab.Log;
            }, tab == StackTab.Log));
            tabsList.Add(new TabRecord("TabSocial".Translate(), delegate
            {
                tab = StackTab.Social;
            }, tab == StackTab.Social));
            tabsList.Add(new TabRecord("TabCharacter".Translate(), delegate
            {
                tab = StackTab.Character;
            }, tab == StackTab.Character));
            tabsList.Add(new TabRecord("TabRecords".Translate(), delegate
            {
                tab = StackTab.Records;
            }, tab == StackTab.Records));
        }

        public List<IStackHolder> GetItems()
        {
            if (filterManager.currentFilter != null)
            {
                return allItems.Where(x => filterManager.currentFilter(x).Item2).ToList();
            }
            return allItems;
        }

        public override Vector2 InitialSize => new Vector2(1000f, 750f);

        public override void DoWindowContents(Rect inRect)
        {
            Rect leftRect = new Rect(inRect.x, inRect.y, 350, inRect.height);
            Rect rightRect = new Rect(leftRect.xMax + 15, inRect.y, inRect.width - leftRect.width - 15, inRect.height);
            DrawLeftPanel(leftRect);
            if (selectedStack != null)
            {
                DrawRightPanel(rightRect);
            }
        }

        private void DrawLeftPanel(Rect rect)
        {
            Rect leftRect = rect;
            leftRect.height = 30f;
            Text.Font = GameFont.Medium;
            Widgets.Label(leftRect, "AC.NeuralMatrixManagement".Translate());
            Text.Font = GameFont.Tiny;
            leftRect.y += 50f;
            GUI.color = Color.grey;
            Widgets.Label(leftRect, "AC.StorageFilters".Translate());
            GUI.color = Color.white;
            Text.Font = GameFont.Small;

            leftRect.y += 15f;
            Widgets.CheckboxLabeled(leftRect, "AC.AllowColonistNeuralStacks".Translate(), ref matrix.compCache.allowColonistNeuralStacks);
            leftRect.y += 30f;
            Widgets.CheckboxLabeled(leftRect, "AC.AllowStrangerNeuralStacks".Translate(), ref matrix.compCache.allowStrangerNeuralStacks);
            leftRect.y += 30f;
            Widgets.CheckboxLabeled(leftRect, "AC.AllowHostileNeuralStacks".Translate(), ref matrix.compCache.allowHostileNeuralStacks);
            leftRect.y += 40f;

            Text.Font = GameFont.Tiny;
            GUI.color = Color.grey;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(leftRect, "AC.StacksStored".Translate(matrix.AllNeuralStacks.Count(), matrix.AllNeuralCaches.Sum(x => x.Props.stackLimit)));
            leftRect.y += 15f;
            Widgets.Label(leftRect, "AC.StacksRegistered".Translate());
            GUI.color = Color.white;
            Text.Font = GameFont.Small;

            filterManager.DoFilters(leftRect);
            leftRect.y += 25f;
            GUI.color = Color.grey;
            leftRect.y += 5f;
            GUI.DrawTexture(new Rect(leftRect.x, leftRect.y, rect.width, 2f), BaseContent.WhiteTex);
            GUI.color = Color.white;
            leftRect.y += 5f;

            Rect outRect = new Rect(rect.x, leftRect.y, rect.width, rect.height - 40f - leftRect.y);
            Rect viewRect = new Rect(0f, 0f, rect.width - 16f, currentItems.Count() * 52f);
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);

            float curY = 0f;
            for (int i = 0; i < currentItems.Count(); i++)
            {
                var stack = currentItems[i];
                var stackEntryRect = new Rect(0f, curY, viewRect.width, 50f);
                if (stack == selectedStack)
                {
                    Widgets.DrawHighlightSelected(stackEntryRect);
                }
                else if (i % 2 == 1)
                {
                    Widgets.DrawHighlight(stackEntryRect);
                }
                Widgets.DrawHighlightIfMouseover(stackEntryRect);
                DrawStackEntry(stackEntryRect, stack);
                curY += 52;
            }

            Widgets.EndScrollView();
        }

        private Dictionary<IStackHolder, (Thing thing, TaggedString pawnName, string factionName)> cachedValues = new();

        private static readonly Texture2D needlecastIcon = ContentFinder<Texture2D>.Get("UI/Icons/Needlecast", true);
        private static readonly Texture2D installStackIcon = ContentFinder<Texture2D>.Get("UI/Icons/InstallStack", true);
        private static readonly Texture2D downArrowIcon = ContentFinder<Texture2D>.Get("UI/Icons/Drop", true);
        private static readonly Texture2D trashIcon = ContentFinder<Texture2D>.Get("UI/Icons/Erase", true);

        private void DrawStackEntry(Rect rect, IStackHolder stack)
        {
            if (cachedValues.TryGetValue(stack, out var cache) is false)
            {
                var data = stack.NeuralData;
                cachedValues[stack] = cache = new(stack.ThingHolder, data.PawnNameColored, data.faction?.Name);
            }

            Rect iconRect = new Rect(rect.x, rect.y, rect.height, rect.height);
            Widgets.ThingIcon(iconRect, cache.thing);

            float iconSize = 25f;
            float totalIconsWidth = (iconSize + 5f) * 4;

            Rect nameRect = new Rect(iconRect.xMax, rect.y, rect.width - iconRect.width - totalIconsWidth, 24);
            Widgets.Label(nameRect, cache.pawnName);
            Text.Font = GameFont.Tiny;
            GUI.color = Color.grey;
            Rect factionRect = new Rect(nameRect.x, nameRect.yMax - 5, nameRect.width, 15);
            if (stack.NeuralData.faction != null)
            {
                Widgets.Label(factionRect, "AC.Faction".Translate() + ": " + cache.factionName);
            }
            Rect statusRect = new Rect(nameRect.x, factionRect.yMax, nameRect.width, factionRect.height);
            Widgets.Label(statusRect, "AC.Status".Translate() + ": todo");
            Text.Font = GameFont.Small;
            GUI.color = Color.white;

            float iconSpacing = iconSize + 5f;
            float iconXPos = rect.xMax - totalIconsWidth;
            float iconYPos = rect.y + (rect.height - iconSize) / 2f;

            Rect iconRectWithOffset = new Rect(iconXPos, iconYPos, iconSize, iconSize);

            if (Widgets.ButtonImage(iconRectWithOffset, needlecastIcon))
            {
            }

            iconRectWithOffset.x += iconSpacing;
            if (Widgets.ButtonImage(iconRectWithOffset, installStackIcon))
            {
            }

            iconRectWithOffset.x += iconSpacing;
            if (Widgets.ButtonImage(iconRectWithOffset, downArrowIcon))
            {
            }

            iconRectWithOffset.x += iconSpacing;
            if (Widgets.ButtonImage(iconRectWithOffset, trashIcon))
            {
            }

            if (Widgets.ButtonInvisible(rect))
            {
                selectedStack = stack;
            }
        }

        private void DrawRightPanel(Rect rect)
        {
            var panelRect = rect;
            var thing = selectedStack.ThingHolder;
            var pawn = selectedStack.Pawn;
            CreateTabs();
            TabDrawer.DrawTabs(new Rect(rect.x, rect.y + 32, rect.width, rect.height), tabsList);
            rect.y += 32;
            rect.height -= 32;
            rect.height -= 200;
            Widgets.DrawMenuSection(rect);
            switch (tab)
            {
                case StackTab.Character:
                    {
                        Vector2 vector = CharacterCardUtility.PawnCardSize(pawn);
                        CharacterCardUtility.DrawCharacterCard(new Rect(rect.x + 17f, rect.y + 10f, vector.x, vector.y), pawn);
                    }
                    break;
                case StackTab.Social:
                    {
                        SocialCardUtility.DrawSocialCard(rect, pawn);
                    }
                    break;
                case StackTab.Log:
                    {
                        ITab_Pawn_Log_Static.FillTab(rect, pawn);
                    }
                    break;
                case StackTab.Records:
                    {
                        RecordsCardUtility.DrawRecordsCard(rect, pawn);
                    }
                    break;
            }

            DrawBottom(rect, panelRect);
        }

        private void DrawBottom(Rect rect, Rect panelRect)
        {
            // Drawing the bottom Stack Information and Stack Configuration
            float bottomSectionHeight = 140f;
            Rect bottomRect = new Rect(rect.x, rect.yMax + 15, rect.width, bottomSectionHeight);
            Widgets.DrawMenuSection(bottomRect);

            Rect stackInfoRect = new Rect(bottomRect.x + 10f, bottomRect.y + 10f, bottomRect.width / 2 - 20f,
                bottomRect.height - 20f);
            Rect stackConfigRect = new Rect(bottomRect.x + bottomRect.width / 2 + 10f, bottomRect.y + 10f, bottomRect.width / 2 - 20f, bottomRect.height - 20f);

            // Draw Stack Information
            Text.Anchor = TextAnchor.UpperLeft;
            // Set font to Medium for the header
            Text.Font = GameFont.Medium;
            Widgets.Label(stackInfoRect, "AC.StackInformation".Translate());

            // Reset font to Small for content
            Text.Font = GameFont.Small;
            var neuralData = selectedStack.NeuralData;
            float infoContentY = stackInfoRect.y + Text.LineHeight + 5f; // Adjust Y position after header
            Rect infoContentRect = new Rect(stackInfoRect.x, infoContentY, stackInfoRect.width, stackInfoRect.height - Text.LineHeight - 5f);
            Widgets.Label(infoContentRect,
                "AC.CurrentlyStatus".Translate() + ": todo" + "\n" +
                "AC.OriginalGender".Translate() + ": " + neuralData.OriginalGender.ToString() + "\n" +
                "AC.NeedlecastingRange".Translate() + ": todo" + "\n" +
                "AC.StackDegradation".Translate() + ": " + neuralData.stackDegradation.ToStringPercent().Colorize(Color.red));

            // Draw Stack Configuration
            // Set font to Medium for the header

            if (selectedStack.ThingHolder is NeuralStack neuralStack)
            {
                Text.Font = GameFont.Medium;
                Widgets.Label(stackConfigRect, "AC.StackConfiguration".Translate());
                // Reset font to Small for content
                Text.Font = GameFont.Small;
                float configContentY = stackConfigRect.y + Text.LineHeight + 5f; // Adjust Y position after header
                float checkboxHeight = 24f;
                Rect autobankRect = new Rect(stackConfigRect.x, configContentY, stackConfigRect.width, checkboxHeight);
                Widgets.CheckboxLabeled(autobankRect, "AC.AutobankStack".Translate(), ref neuralStack.autoLoad);
            }

            // Reset text anchor after drawing
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Tiny;
            GUI.color = Color.grey;
            Rect descriptionRect = new Rect(rect.x, bottomRect.yMax, rect.width - 150f, 45f);
            string descriptionText = "AC.NeuralMatrixManagementText".Translate();
            Widgets.Label(descriptionRect, descriptionText);
            Text.Font = GameFont.Small;
            GUI.color = Color.white;

            // Drawing the Close button
            Rect closeButtonRect = new Rect(rect.xMax - 150f, panelRect.height - 30f, 150, 30f);
            if (Widgets.ButtonText(closeButtonRect, "Close".Translate()))
            {
                Close();
            }
        }
    }
}
