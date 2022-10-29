using System;
using UnityEngine;
using Verse;
using static AlteredCarbon.UIHelpers;
namespace AlteredCarbon
{
    [HotSwappable]
    public class Window_ColorPicker : Window
    {
        public override Vector2 InitialSize => new Vector2(400, 150);
        public Texture2D texColor;
        public Action<Color> selectAction;
        public Color pick;
        public Window_ColorPicker(Color currentColor, Action<Color> selectAction)
        {
            this.doCloseX = true;
            this.selectAction = selectAction;
            this.pick = currentColor;
            InitColorPicker();
        }
        private void InitColorPicker()
        {
            texColor = new Texture2D(Convert.ToInt32(InitialSize.x - 45), Convert.ToInt32(InitialSize.y - 70));
            for (int x = 0; x < texColor.width; x++)
            {
                for (int y = 0; y < texColor.height; y++)
                {
                    texColor.SetPixel(x, y, Color.HSVToRGB(((float)x) / texColor.width, 1.0f, 1.0f));
                }
            }
            texColor.Apply(false);
        }

        public override void DoWindowContents(Rect inRect)
        {
            inRect.y += 15;
            inRect.height -= 15;
            var colorPickerRect = new Rect(inRect.x + 5, inRect.y + 5, texColor.width, texColor.height);
            GUI.DrawTexture(colorPickerRect, texColor);
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && Mouse.IsOver(colorPickerRect))
            {
                var pos = Event.current.mousePosition - new Vector2(colorPickerRect.xMin, colorPickerRect.yMin);
                pick = texColor.GetPixel(Convert.ToInt32(pos.x), Convert.ToInt32(pos.y));
                Event.current.Use();
                selectAction(pick);
            }
        }
    }
}
