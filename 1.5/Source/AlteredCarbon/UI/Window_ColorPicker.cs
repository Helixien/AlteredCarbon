using RimWorld;
using System;
using UnityEngine;
using Verse;
namespace AlteredCarbon
{
    [HotSwappable]
    public class Window_ColorPicker : Window
    {
        public Action<Color> selectAction;

        private Color color;

        private Color oldColor;

        private bool hsvColorWheelDragging;

        private bool colorTemperatureDragging;

        private string[] textfieldBuffers = new string[6];

        private Color textfieldColorBuffer;

        private string previousFocusedControlName;

        public static Widgets.ColorComponents visibleColorTextfields = Widgets.ColorComponents.Hue | Widgets.ColorComponents.Sat;

        public static Widgets.ColorComponents editableColorTextfields = Widgets.ColorComponents.Hue | Widgets.ColorComponents.Sat;
        public override Vector2 InitialSize => new Vector2(600f, 450f);

        public Window_ColorPicker(Color currentColor, Action<Color> selectAction)
        {
            this.doCloseX = true;
            this.selectAction = selectAction;
            color = currentColor;
            oldColor = color;
            forcePause = true;
            absorbInputAroundWindow = true;
            closeOnClickedOutside = true;
            closeOnAccept = false;
        }

        private static void HeaderRow(ref RectDivider layout)
        {
            using (new TextBlock(GameFont.Medium))
            {
                TaggedString taggedString = "ChooseAColor".Translate().CapitalizeFirst();
                RectDivider rectDivider = layout.NewRow(Text.CalcHeight(taggedString, layout.Rect.width));
                GUI.SetNextControlName(Dialog_GlowerColorPicker.focusableControlNames[0]);
                Widgets.Label(rectDivider, taggedString);
            }
        }

        private void BottomButtons(ref RectDivider layout)
        {
            RectDivider rectDivider = layout.NewRow(Dialog_GlowerColorPicker.ButSize.y, VerticalJustification.Bottom);
            if (Widgets.ButtonText(rectDivider.NewCol(Dialog_GlowerColorPicker.ButSize.x), "Cancel".Translate()))
            {
                Close();
            }
            if (Widgets.ButtonText(rectDivider.NewCol(Dialog_GlowerColorPicker.ButSize.x, HorizontalJustification.Right), "Accept".Translate()))
            {
                selectAction(color);
                Close();
            }
        }

        private void ColorTextfields(ref RectDivider layout, out Vector2 size)
        {
            RectAggregator aggregator = new RectAggregator(new Rect(layout.Rect.position, new Vector2(125f, 0f)), 195906069);
            bool num = Widgets.ColorTextfields(ref aggregator, ref color, ref textfieldBuffers, ref textfieldColorBuffer, previousFocusedControlName, "colorTextfields", editableColorTextfields, visibleColorTextfields);
            size = aggregator.Rect.size;
            if (num)
            {
                Color.RGBToHSV(color, out var H, out var S, out var _);
                color = Color.HSVToRGB(H, S, 1f);
            }
        }

        private static void ColorReadback(Rect rect, Color color, Color oldColor)
        {
            rect.SplitVertically((rect.width - 26f) / 2f, out var left, out var right);
            RectDivider rectDivider = new RectDivider(left, 195906069);
            TaggedString label = "CurrentColor".Translate().CapitalizeFirst();
            TaggedString label2 = "OldColor".Translate().CapitalizeFirst();
            float width = Mathf.Max(100f, label.GetWidthCached(), label2.GetWidthCached());
            RectDivider rectDivider2 = rectDivider.NewRow(Text.LineHeight);
            Widgets.Label(rectDivider2.NewCol(width), label);
            Widgets.DrawBoxSolid(rectDivider2, color);
            RectDivider rectDivider3 = rectDivider.NewRow(Text.LineHeight);
            Widgets.Label(rectDivider3.NewCol(width), label2);
            Widgets.DrawBoxSolid(rectDivider3, oldColor);
            RectDivider rectDivider4 = new RectDivider(right, 195906069);
            rectDivider4.NewCol(26f);
            if (DarklightUtility.IsDarklight(color))
            {
                Widgets.Label(rectDivider4, "Darklight".Translate().CapitalizeFirst());
            }
            else
            {
                Widgets.Label(rectDivider4, "NotDarklight".Translate().CapitalizeFirst());
            }
        }

        private static void TabControl()
        {
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Tab)
            {
                bool num = !Event.current.shift;
                Event.current.Use();
                string text = GUI.GetNameOfFocusedControl();
                if (text.NullOrEmpty())
                {
                    text = Dialog_GlowerColorPicker.focusableControlNames[0];
                }
                int num2 = Dialog_GlowerColorPicker.focusableControlNames.IndexOf(text);
                if (num2 < 0)
                {
                    num2 = Dialog_GlowerColorPicker.focusableControlNames.Count;
                }
                num2 = ((!num) ? (num2 - 1) : (num2 + 1));
                if (num2 >= Dialog_GlowerColorPicker.focusableControlNames.Count)
                {
                    num2 = 0;
                }
                else if (num2 < 0)
                {
                    num2 = Dialog_GlowerColorPicker.focusableControlNames.Count - 1;
                }
                GUI.FocusControl(Dialog_GlowerColorPicker.focusableControlNames[num2]);
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            using (TextBlock.Default())
            {
                RectDivider layout = new RectDivider(inRect, 195906069);
                HeaderRow(ref layout);
                layout.NewRow(0f);
                BottomButtons(ref layout);
                layout.NewRow(0f, VerticalJustification.Bottom);
                Color.RGBToHSV(color, out var H, out var S, out var _);
                Color defaultColor = Color.HSVToRGB(H, S, 1f);
                defaultColor.a = 1f;
                Dialog_GlowerColorPicker.ColorPalette(ref layout, ref color, defaultColor, false, out var paletteHeight);
                ColorTextfields(ref layout, out var size);
                float height = Mathf.Max(paletteHeight, 128f, size.y);
                RectDivider rectDivider = layout.NewRow(height);
                rectDivider.NewCol(size.x);
                rectDivider.NewCol(250f, HorizontalJustification.Right);
                Widgets.HSVColorWheel(rectDivider.Rect.ContractedBy((rectDivider.Rect.width - 128f) / 2f, (rectDivider.Rect.height - 128f) / 2f), ref color, ref hsvColorWheelDragging, 1f);
                Widgets.ColorTemperatureBar(layout.NewRow(34f), ref color, ref colorTemperatureDragging, 1f);
                layout.NewRow(26f);
                ColorReadback(layout, color, oldColor);
                TabControl();
                if (Event.current.type == EventType.Layout)
                {
                    previousFocusedControlName = GUI.GetNameOfFocusedControl();
                }
            }
        }
    }
}
