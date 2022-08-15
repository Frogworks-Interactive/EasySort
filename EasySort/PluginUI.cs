using Dalamud.Interface;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using System;
using System.Numerics;
using XivCommon;
using System.Collections.Generic;
using System.Linq;

namespace EasySort
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
        private Plugin plugin;

        internal const int settingWidth = 350;

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
        public PluginUI(Configuration configuration,  Plugin plugin)
        {
            this.configuration = configuration;
            this.plugin = plugin;
        }

        public void Dispose()
        {
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
                ImGui.Text($"The random config bool is {this.configuration.AutoSort}");

                if (ImGui.Button("Show Settings"))
                {
                    SettingsVisible = true;
                }

                ImGui.Spacing();

            }
            ImGui.End();
        }

        public void DropdownUi( string title, Util.SortConditonItem sortCondition, int index = 0)
        {
            if (configuration.Conditions.ElementAtOrDefault(index) == null) return;
            if (ImGui.BeginCombo($"##condition-combo-{index}", Util.conditions[sortCondition.Condition]))
            {
                try
                {
                    foreach (KeyValuePair<string, string> condition in Util.conditions.ToList())
                        if (ImGui.Selectable($"{condition.Value}"))
                        {

                            configuration.Conditions[index] = new Util.SortConditonItem(sortCondition.InventoryType, condition.Key, sortCondition.Direction);
                            this.configuration.Save();
                            plugin.runSort();
                        }

                }
                catch (Exception ex)
                {
                    PluginLog.LogError(ex, "Error drawing helper combo");
                }

                ImGui.EndCombo();
              
            }
        }
        public unsafe void DrawSettingsWindow(AtkUnitBase* addon = null)
        {
            if (!SettingsVisible)
            {
                return;
            }
            var drawPos = DrawPosForAddon(addon, false, true);
            if (drawPos != null)
            {
                ImGui.SetNextWindowPos(drawPos.Value, ImGuiCond.Appearing);

            }

            ImGui.SetNextWindowSize(new Vector2(settingWidth, 300), ImGuiCond.Always);
            if (ImGui.Begin("Inventory Sort Settings", ref this.settingsVisible,
                ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                // can't ref a property, so use a local copy
                var AutoSortValue = this.configuration.AutoSort;
                if (ImGui.Checkbox("Automatically sort on Inventory Open", ref AutoSortValue))
                {
                    this.configuration.AutoSort = AutoSortValue;
                    this.configuration.Save();
                }
                // can't ref a property, so use a local copy
                var ShowChatValue = this.configuration.ShowChat;
                if (ImGui.Checkbox("Show text in Chat", ref ShowChatValue))
                {
                    this.configuration.ShowChat = ShowChatValue;
                    this.configuration.Save();
                }
                if (configuration.Conditions is not null && configuration.Conditions.Count > 0)
                {
                    ImGui.Separator();

                    ImGui.Text("Sort By:");

                    for (var i = 0; i < configuration.Conditions.Count; i++ )
                    {

                        var value = configuration.Conditions[i];
                        // one of the random ids of all time
                        ImGui.PushID(3023030 + i); // Use field index as identifier.

                        DropdownUi("sort by:", value, i);
                        ImGui.SameLine();
                        var DirectionRef = value.Direction == Util.Ascending;

                        if (Util.ImGuiToggleButton(Util.Direction[value.Direction],ref DirectionRef))
                        {

                            PluginLog.Log(value.Direction);
                            if (value.Direction == Util.Ascending)
                            {
                                configuration.Conditions[i] = new Util.SortConditonItem(value.InventoryType, value.Condition, Util.Descending);
                            }
                            else 
                            {
                                configuration.Conditions[i] = new Util.SortConditonItem(value.InventoryType, value.Condition, Util.Ascending);

                            }
                            this.configuration.Save();

                            plugin.runSort();



                        }
                        ImGui.PopID(); // Use field index as identifier.

                    }

                    ImGui.Separator();

                    if (ImGui.Button("Add Condition"))
                    {
                        configuration.Conditions.Add(new Util.SortConditonItem());

                    }
                    ImGui.SameLine();
                    if (configuration.Conditions.Count > 1)
                    if (ImGui.Button("Remove Condition"))
                    {
                        configuration.Conditions.RemoveAt(configuration.Conditions.Count - 1);

                    }

                }


            }
            if (drawPos != null)
            {
                ImGui.SetWindowPos(drawPos.Value);
            }
            ImGui.End();
        }

        internal static unsafe Vector2? DrawPosForAddon(AtkUnitBase* addon, bool bottom = false, bool quickAction = false)
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

            var xModifier = bottom ?
                 root->Width * addon->Scale : 0;

            var quickActionPos = quickAction ? new Vector2(-settingWidth - 10, 0)  : new Vector2(0, 0) - Vector2.UnitX * 130 * addon->Scale
                   + Vector2.UnitY * 31 * addon->Scale;



            return ImGuiHelpers.MainViewport.Pos
                   + new Vector2(addon->X, addon->Y)
                   + Vector2.UnitX * xModifier
                   
                   + quickActionPos

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
        internal  unsafe void DrawHelper(AtkUnitBase* addon, string id, bool right, XivCommonBase common, ImGuiScene.TextureWrap goatImage)
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
           
            if (ImGui.ImageButton(goatImage.ImGuiHandle, new Vector2(16, 16)))

            {
                plugin.runSort();


            };
            ImGui.SameLine();
            if (ImGui.Button("settings"))

            {
                SettingsVisible = !SettingsVisible;

            }
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
