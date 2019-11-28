// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace WinDynamicDesktop
{
    public enum PluginEvent
    {
        ON_UNINSTALL,
        ON_INIT,
        ON_EXIT,
        ON_SCHEDULER_2SEG,
        ON_SCHEDULER_4SEG
    }

    class PluginManager
    {
        public static Dictionary<string, object> loadedPlugins = new Dictionary<string, object>();

        public static void Initialize()
        {
            foreach (string pluginDir in Directory.EnumerateDirectories("plugins", "*"))
            {
                string pluginName = Path.GetFileName(pluginDir);

                if (!pluginName.StartsWith("."))
                {
                    LoadPlugin(pluginName);
                }
            }
        }

        public static void LoadPlugin(string name)
        {
            Assembly pluginDll = Assembly.LoadFile(Path.Combine(Environment.CurrentDirectory, "plugins", name, name + ".dll"));
            object plugin = Activator.CreateInstance(pluginDll.GetType("WinDynamicDesktop.Plugin"));
            loadedPlugins.Add(name, plugin);
        }

        public static void UnloadPlugin(string name)
        {
            // TODO Remove from list and free up memory
        }

        public static List<ToolStripItem> GetMenuItems()
        {
            List<ToolStripItem> items = new List<ToolStripItem>();

            foreach (KeyValuePair<string, object> pluginData in loadedPlugins)
            {
                object plugin = pluginData.Value;
                List<ToolStripItem> newItems = (List<ToolStripItem>)plugin.GetType().GetMethod("GetMenuItems").Invoke(plugin, new object[] { });
                if (newItems.Count > 0)
                {
                    newItems.Prepend(new ToolStripSeparator());
                }
                items.AddRange(newItems);
            }

            return items;
        }

        public static void ProcessEvent(PluginEvent eType, object[] eArgs = null)
        {
            foreach (KeyValuePair<string, object> pluginData in loadedPlugins)
            {
                object plugin = pluginData.Value;
                plugin.GetType().GetMethod("ProcessEvent").Invoke(plugin, new object[] { eType, eArgs });
            }
        }
    }
}
