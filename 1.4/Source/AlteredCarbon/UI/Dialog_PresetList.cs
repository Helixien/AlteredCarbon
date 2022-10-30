using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Diagnostics;
using Verse;

namespace AlteredCarbon
{
    public class Dialog_PresetList_Load : Dialog_PresetList
    {
        public Dialog_PresetList_Load(Window_SleeveCustomization parent) : base(parent)
        {
            interactButLabel = "LoadGameButton".Translate();
            ACUtils.LoadPresets();
        }
        protected override void DoPresetInteraction(string name)
        {
            parent.LoadSleeve(AlteredCarbonMod.settings.presets[name]);
            Close();
        }
    }

    public class Dialog_PresetList_Save : Dialog_PresetList
    {
        public Dialog_PresetList_Save(Window_SleeveCustomization parent) : base(parent)
        {
            interactButLabel = "OverwriteButton".Translate();
        }
        protected override bool ShouldDoTypeInField => true;

        protected override void DoPresetInteraction(string name)
        {
            AlteredCarbonMod.settings.presets[name] = new SleevePreset { sleeve = parent.curSleeve };
            Messages.Message("SavedAs".Translate(name), MessageTypeDefOf.SilentInput, false);
            ACUtils.SavePresets();
            Close();
        }
    }

    public abstract class Dialog_PresetList : Window
    {
        protected const float EntryHeight = 40f;

        protected const float FileNameLeftMargin = 8f;

        protected const float FileNameRightMargin = 4f;

        protected const float FileInfoWidth = 94f;

        protected const float InteractButWidth = 100f;

        protected const float InteractButHeight = 36f;

        protected const float DeleteButSize = 36f;

        protected const float NameTextFieldWidth = 400f;

        protected const float NameTextFieldHeight = 35f;

        protected const float NameTextFieldButtonSpace = 20f;

        private static readonly Color DefaultFileTextColor = new Color(1f, 1f, 0.6f);

        protected float bottomAreaHeight;

        private bool focusedNameArea;
        protected string interactButLabel = "Error";

        protected Window_SleeveCustomization parent;

        protected Vector2 scrollPosition = Vector2.zero;

        protected string typingName = "";
        public Dialog_PresetList(Window_SleeveCustomization parent)
        {
            doCloseButton = true;
            doCloseX = true;
            forcePause = true;
            absorbInputAroundWindow = true;
            closeOnAccept = false;
            this.parent = parent;
        }

        public override Vector2 InitialSize => new Vector2(620f, 700f);
        protected virtual bool ShouldDoTypeInField => false;
        public override void DoWindowContents(Rect inRect)
        {
            var vector = new Vector2(inRect.width - 16f, 40f);
            var y = vector.y;
            var presets = AlteredCarbonMod.settings.presets;
            var height = presets.Count * y;
            var viewRect = new Rect(0f, 0f, inRect.width - 16f, height);
            var num = inRect.height - CloseButSize.y - bottomAreaHeight - 18f;
            if (ShouldDoTypeInField)
            {
                num -= 53f;
            }

            var outRect = inRect.TopPartPixels(num);
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
            var num2 = 0f;
            var num3 = 0;
            foreach (var preset in presets.Keys.ToList())
            {
                if (num2 + vector.y >= scrollPosition.y && num2 <= scrollPosition.y + outRect.height)
                {
                    var rect = new Rect(0f, num2, vector.x, vector.y);
                    if (num3 % 2 == 0)
                    {
                        Widgets.DrawAltRect(rect);
                    }

                    GUI.BeginGroup(rect);
                    var rect2 = new Rect(rect.width - 36f, (rect.height - 36f) / 2f, 36f, 36f);
                    if (Widgets.ButtonImage(rect2, TexButton.DeleteX, Color.white, GenUI.SubtleMouseoverColor))
                    {
                        Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmDelete".Translate(preset),
                            delegate { AlteredCarbonMod.settings.presets.Remove(preset); }, true));
                    }

                    Text.Font = GameFont.Small;
                    var rect3 = new Rect(rect2.x - 100f, (rect.height - 36f) / 2f, 100f, 36f);
                    if (Widgets.ButtonText(rect3, interactButLabel))
                    {
                        DoPresetInteraction(preset);
                    }

                    var rect4 = new Rect(rect3.x - 94f, 0f, 94f, rect.height);
                    GUI.color = Color.white;
                    Text.Anchor = TextAnchor.UpperLeft;
                    var rect5 = new Rect(8f, 0f, rect4.x - 8f - 4f, rect.height);
                    Text.Anchor = TextAnchor.MiddleLeft;
                    Text.Font = GameFont.Small;
                    Widgets.Label(rect5, preset.Truncate(rect5.width * 1.8f));
                    GUI.color = Color.white;
                    Text.Anchor = TextAnchor.UpperLeft;
                    GUI.EndGroup();
                }

                num2 += vector.y;
                num3++;
            }

            Widgets.EndScrollView();
            if (ShouldDoTypeInField)
            {
                DoTypeInField(inRect.TopPartPixels(inRect.height - CloseButSize.y - 18f));
            }
        }

        protected virtual void DoTypeInField(Rect rect)
        {
            GUI.BeginGroup(rect);
            var y = rect.height - 35f;
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleLeft;
            GUI.SetNextControlName("MapNameField");
            var str = Widgets.TextField(new Rect(5f, y, 400f, 35f), typingName);
            if (GenText.IsValidFilename(str))
            {
                typingName = str;
            }

            if (!focusedNameArea)
            {
                UI.FocusControl("MapNameField", this);
                focusedNameArea = true;
            }

            if (Widgets.ButtonText(new Rect(420f, y, rect.width - 400f - 20f, 35f), "SaveGameButton".Translate()) ||
                Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
            {
                if (typingName.NullOrEmpty())
                {
                    Messages.Message("NeedAName".Translate(), MessageTypeDefOf.RejectInput, false);
                }
                else
                {
                    DoPresetInteraction(typingName);
                }
            }

            Text.Anchor = TextAnchor.UpperLeft;
            GUI.EndGroup();
        }

        protected abstract void DoPresetInteraction(string name);

        protected virtual Color FileNameColor(SaveFileInfo sfi)
        {
            return DefaultFileTextColor;
        }
    }
}
