using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.IoC;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System.IO;
using System.Reflection;
using XivCommon;
using System.Collections.Generic;
using ImGuiNET;


namespace EasySort
{
    public sealed class Plugin : IDalamudPlugin
    {
        internal const string PluginName = "Easy Sort";

        public string Name => PluginName;

        private const string commandName = "/boinky";

        [PluginService]
        internal static GameGui GameGui { get; private set; } = null!;
        internal static XivCommonBase common { get; private set; } = null!;
        private DalamudPluginInterface PluginInterface { get; init; }
        private CommandManager CommandManager { get; init; }
        private Configuration Configuration { get; init; }

        private PluginUI PluginUi { get; init; }
        private bool isOpen { get; set; } = false;

        internal ImGuiScene.TextureWrap SortImage { get; set; }
        internal ImGuiScene.TextureWrap SettingImage { get; set; }

        /// <summary>
        /// Gets an included FontAwesome icon font.
        /// </summary>
        public static ImFontPtr IconFont { get; private set; }

        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] CommandManager commandManager)
        {
            this.PluginInterface = pluginInterface;
            this.CommandManager = commandManager;
         //   IconFont = Util.SetupFont(pluginInterface);
            this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(this.PluginInterface);
            common = new(); // just need the chat feature to send commands
            // you might normally want to embed resources and load them from the manifest stream
            var imagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "sort.png");
            var sortImage = this.PluginInterface.UiBuilder.LoadImage(imagePath);
            var imagePath2 = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "setting.png");
            var SettingImage = this.PluginInterface.UiBuilder.LoadImage(imagePath2);
            this.PluginUi = new PluginUI(this.Configuration, this,this.PluginInterface);
            this.SortImage = sortImage;
            this.SettingImage = SettingImage;

            this.CommandManager.AddHandler(commandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "A useful message to display in /xlhelp"
            });
//            interfaceManager.OverrideGameCursor = false;
           this.PluginInterface.UiBuilder.Draw += DrawUI;
            this.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
        }

        public void Dispose()
        {
            this.PluginUi.Dispose();
            this.CommandManager.RemoveHandler(commandName);
            common.Dispose();

        }

        private void OnCommand(string command, string args)
        {
            // in response to the slash command, just display our main ui
            this.PluginUi.Visible = true;

        }

        public void runSort(string inven = "inventory", string direction="asc")
        {

            common.Functions.Chat.SendMessage("/isort clear inventory");
            if (Configuration.Conditions is not null && Configuration.Conditions.Count > 0)
            {
                foreach (var condition in Configuration.Conditions)
                {
                    if (condition.Condition == "tab") direction = "";

                    common.Functions.Chat.SendMessage($"/isort condition {condition.InventoryType} {condition.Condition} {condition.Direction}");
                    if (Configuration.ShowChat)
                        common.Functions.Chat.SendMessage($"/echo [EasySort] ran condition: {condition.InventoryType} {condition.Condition} {condition.Direction}");

                }

            }
            common.Functions.Chat.SendMessage("/isort execute inventory");
            if (Configuration.ShowChat)
            common.Functions.Chat.SendMessage("/echo [EasySort] sorted inventory!");


        }

        private void DrawUI()
        {
            this.PluginUi.Draw();
        
                drawInven();
          //  PluginInterface.UiBuilder.OverrideGameCursor = false;


        }

        private void DrawConfigUI()
        {
            this.PluginUi.SettingsVisible = true;
        }

        private unsafe void onDrawInven(AtkUnitBase* addon)
        {
            PluginUi.DrawHelper(addon, "easy-inventory-sort", true, common);
            PluginUi.DrawSettingsWindow(addon);
            if (!isOpen && Configuration.AutoSort) runSort();

            this.isOpen = true;
        }
        private unsafe void drawInven()
        {
            if (GameGui == null || SortImage == null || SettingImage == null) return;
            
            var addon = (AtkUnitBase*)GameGui.GetAddonByName("InventoryLarge", 1);
            if (addon != null && addon->IsVisible )
            {

                onDrawInven(addon);
                return;
            }
             addon = (AtkUnitBase*)GameGui.GetAddonByName("Inventory", 1);
            if (addon != null && addon->IsVisible)
            {
                onDrawInven(addon);

                return;
            }
             addon = (AtkUnitBase*)GameGui.GetAddonByName("InventoryExpansion", 1);
            if (addon != null && addon->IsVisible)
            {
                onDrawInven(addon);

                return;
            }
            this.isOpen = false;

            return;
        }
    }
}
