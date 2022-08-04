using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.IoC;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System.IO;
using System.Reflection;
using XivCommon;


namespace EasySort
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "Easy Sort";

        private const string commandName = "/boinky";

        [PluginService]
        internal static GameGui GameGui { get; private set; } = null!;
        internal static XivCommonBase common { get; private set; } = null!;
        private DalamudPluginInterface PluginInterface { get; init; }
        private CommandManager CommandManager { get; init; }
        private Configuration Configuration { get; init; }
        private PluginUI PluginUi { get; init; }

        internal ImGuiScene.TextureWrap image { get; set; }
        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] CommandManager commandManager)
        {
            this.PluginInterface = pluginInterface;
            this.CommandManager = commandManager;

            this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(this.PluginInterface);
            common = new(); // just need the chat feature to send commands
            // you might normally want to embed resources and load them from the manifest stream
            var imagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "goat.png");
            var goatImage = this.PluginInterface.UiBuilder.LoadImage(imagePath);
            this.PluginUi = new PluginUI(this.Configuration, goatImage);
            this.image = goatImage;
            this.CommandManager.AddHandler(commandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "A useful message to display in /xlhelp"
            });

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
            common.Functions.Chat.SendMessage("/echo boingo!  ");

        }

        private void DrawUI()
        {
            this.PluginUi.Draw();
            drawInven();
        }

        private void DrawConfigUI()
        {
            this.PluginUi.SettingsVisible = true;
        }


        private unsafe void drawInven()
        {
            if (GameGui == null || image == null) return;
            
            var addon = (AtkUnitBase*)GameGui.GetAddonByName("InventoryLarge", 1);
            if (addon != null && addon->IsVisible)
            {
                PluginUI.DrawHelper(addon, "glamaholic-editor-helper", true, common, image);
                return;
            }
             addon = (AtkUnitBase*)GameGui.GetAddonByName("Inventory", 1);
            if (addon != null && addon->IsVisible)
            {
                PluginUI.DrawHelper(addon, "glamaholic-editor-helper", true, common, image);
                return;
            }
             addon = (AtkUnitBase*)GameGui.GetAddonByName("InventoryExpansion", 1);
            if (addon != null && addon->IsVisible)
            {
                PluginUI.DrawHelper(addon, "glamaholic-editor-helper", true, common, image);
                return;
            }
            return;
        }
    }
}
