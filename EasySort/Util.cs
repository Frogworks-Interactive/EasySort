using Dalamud.Logging;
using Dalamud.Plugin;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EasySort
{
    public class Util
    {
        internal const string Inventory = "inventory";
        internal const string Descending = "des";
        internal const string Ascending = "asc";

        internal static ImmutableDictionary<string, string> Direction = new Dictionary<string, string>
        {
            {"des", "Descending" },
            {"asc", "Ascending" }
        }.ToImmutableDictionary();

        /// <summary>
        /// sort conditions and descriptive names
        /// </summary>
        internal static ImmutableDictionary<string, string> conditions = new Dictionary<string, string>
        {
            {"id", "item id" },
            {"spiritbond", "spiritbound" },
            {"category", "category" },
            {"lv", "level" },
            {"ilv", "item level" },
            {"stack", "stack" },
            {"hq", "high quality" },
            {"materia", "materia" },
            {"pdamage", "physical damage" },
            {"mdamage", "magic damage" },
            {"delay", "delay" },
            {"autoattack", "autoattack" },
            {"defense", "physical defence" },
            {"mdefense", "magic defence" },
            {"dex", "dexterity" },
            {"vit", "vitality" },

            {"int", "intelligence" },
            {"mnd", "mind" },
            {"craftsmanship", "craftmanship" },
            {"control", "control" },

            {"gathering", "gathering" },

            {"perception", "perception" },
            {"tab", "tab" },


        }.ToImmutableDictionary();

        public record SortConditonItem(string InventoryType = Inventory, string Condition = "category", string Direction = Ascending);


        

        public unsafe static ImFontPtr SetupFont(DalamudPluginInterface pluginInterface)
        {
            var fontPathIcon = Path.Combine(pluginInterface.AssemblyLocation.Directory?.FullName!, "FontAwesome6Free-Solid-900.otf");
            if (!File.Exists(fontPathIcon))
                PluginLog.Fatal(fontPathIcon, "font failed to load");
            var iconRangeHandle = GCHandle.Alloc(
           new ushort[]
           {
                    0xE000,
                    0xF8FF,
                    0,
           },
           GCHandleType.Pinned);
            var iconFont = ImGui.GetIO().Fonts.AddFontFromFileTTF(fontPathIcon, 17.0f, null, iconRangeHandle.AddrOfPinnedObject());
            iconRangeHandle.Free();

            return iconFont;


        }

        public static bool ImGuiToggleButton(string text, ref bool flag)
        {
            var state = false;
            var colored = false;
            if (flag)
            {
                colored = true;
              
            }
            if (ImGui.Button(text))
            {
                flag = !flag;
                state = true;
            }
            if (colored)
            {
            }
            return state;
        }


    }
}
