using Dalamud.Interface;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using System;
using System.Numerics;
using XivCommon;

namespace SamplePlugin
{
    // It is good to have this be disposable in general, in case you ever need it
    // to do any cleanup
    class PluginUI : IDisposable
    {

        //internal Plugin plugin { get; }
        internal const ImGuiWindowFlags HelperWindowFlags = ImGuiWindowFlags.NoBackground
                                                    | ImGuiWindowFlags.NoDecoration
                                                    | ImGuiWindowFlags.NoCollapse
                                                    | ImGuiWindowFlags.NoTitleBar
                                                    | ImGuiWindowFlags.NoNav
                                                    | ImGuiWindowFlags.NoNavFocus
                                                    | ImGuiWindowFlags.NoNavInputs
                                                    | ImGuiWindowFlags.NoResize
                                                    | ImGuiWindowFlags.NoScrollbar
                                                    | ImGuiWindowFlags.NoSavedSettings
                                                    | ImGuiWindowFlags.NoFocusOnAppearing
                                                    | ImGuiWindowFlags.AlwaysAutoResize
                                                    | ImGuiWindowFlags.NoDocking;
        private Configuration configuration;

        private ImGuiScene.TextureWrap goatImage;

        // this extra bool exists for ImGui, since you can't ref a property
        private bool visible = false;
        public bool Visible
        {
            get { return this.visible; }
            set { this.visible = value; }
        }

        private bool settingsVisible = false;
        public bool SettingsVisible
        {
            get { return this.settingsVisible; }
            set { this.settingsVisible = value; }
        }
        internal const string btnText  = "Sort";

        // passing in the image here just for simplicity
        public PluginUI(Configuration configuration, ImGuiScene.TextureWrap goatImage)
        {
            this.configuration = configuration;
            this.goatImage = goatImage;
        }

        public void Dispose()
        {
            this.goatImage.Dispose();
        }

        public void Draw()
        {
            // This is our only draw handler attached to UIBuilder, so it needs to be
            // able to draw any windows we might have open.
            // Each method checks its own visibility/state to ensure it only draws when
            // it actually makes sense.
            // There are other ways to do this, but it is generally best to keep the number of
            // draw delegates as low as possible.

            DrawMainWindow();
            DrawSettingsWindow();
        }

        public void DrawMainWindow()
        {
            if (!Visible)
            {
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(375, 330), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSizeConstraints(new Vector2(375, 330), new Vector2(float.MaxValue, float.MaxValue));
            if (ImGui.Begin("My Amazing Window", ref this.visible, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                ImGui.Text($"The random config bool is {this.configuration.SomePropertyToBeSavedAndWithADefault}");

                if (ImGui.Button("Show Settings"))
                {
                    SettingsVisible = true;
                }

                ImGui.Spacing();

                ImGui.Text("Have a goat:");
                ImGui.Indent(55);
                ImGui.Image(this.goatImage.ImGuiHandle, new Vector2(100, 100));
                ImGui.Unindent(55);
            }
            ImGui.End();
        }

        public void DrawSettingsWindow()
        {
            if (!SettingsVisible)
            {
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(232, 75), ImGuiCond.Always);
            if (ImGui.Begin("A Wonderful Configuration Window", ref this.settingsVisible,
                ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                // can't ref a property, so use a local copy
                var configValue = this.configuration.SomePropertyToBeSavedAndWithADefault;
                if (ImGui.Checkbox("Random Config Bool", ref configValue))
                {
                    this.configuration.SomePropertyToBeSavedAndWithADefault = configValue;
                    // can save immediately on change, if you don't want to provide a "Save and Close" button
                    this.configuration.Save();
                }
            }
            ImGui.End();
        }

        internal static unsafe Vector2? DrawPosForAddon(AtkUnitBase* addon, bool right = false)
        {
            if (addon == null)
            {
                return null;
            }

            var root = addon->RootNode;
            if (root == null)
            {
                return null;
            }

            var xModifier = right
                ? root->Height * addon->Scale
                : 0;

            return ImGuiHelpers.MainViewport.Pos
                   + new Vector2(addon->X, addon->Y)
                   + Vector2.UnitY * xModifier
                   
                   + Vector2.UnitX * 21 * addon->Scale
                   - Vector2.UnitY * 40 * addon->Scale

                   - Vector2.UnitY * ImGui.CalcTextSize("A") 
                   - Vector2.UnitY * (ImGui.GetStyle().FramePadding.Y + ImGui.GetStyle().FrameBorderSize);
        }
        internal class HelperStyles : IDisposable
        {
            internal HelperStyles()
            {
                ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
                ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0);
                ImGui.PushStyleVar(ImGuiStyleVar.WindowMinSize, Vector2.Zero);
            }

            public void Dispose()
            {
                ImGui.PopStyleVar(3);
            }
        }
        internal static unsafe void DrawHelper(AtkUnitBase* addon, string id, bool right, XivCommonBase common, ImGuiScene.TextureWrap goatImage)
        {
            var drawPos = DrawPosForAddon(addon, right);
            if (drawPos == null)
            {
                return;
            }

            using (new HelperStyles())
            {
                // get first frame
                ImGui.SetNextWindowPos(drawPos.Value, ImGuiCond.Appearing);
                if (!ImGui.Begin($"##{id}", HelperWindowFlags))
                {
                    ImGui.End();
                    return;
                }
            }
           
            if (ImGui.ImageButton(goatImage.ImGuiHandle, new Vector2(30, 30)))

            {
                common.Functions.Chat.SendMessage("/isort condition inventory category asc");
                common.Functions.Chat.SendMessage("/isort execute inventory");
                common.Functions.Chat.SendMessage("/echo sorted inventory! <se.6>");


            };
            //ImGui.SetNextItemWidth(DropdownWidth());
            //if (ImGui.BeginCombo($"##{id}-combo", Plugin.PluginName))
            //{
            //    try
            //    {
            //        dropdown();
            //    }
            //    catch (Exception ex)
            //    {
            //        PluginLog.LogError(ex, "Error drawing helper combo");
            //    }

            //    ImGui.EndCombo();
            //}

            ImGui.SetWindowPos(drawPos.Value);

            ImGui.End();
        }
    }
}
