/* LICENSE:
 * MIT
 * 
 * AUTHOR:
 * egbog
 * */

using _RepairMaxDurability.Patches;
using BepInEx;

namespace _RepairMaxDurability {
    [BepInPlugin("com.egbog.maxdura", "MaxDurability", "2.0.0")]
    [BepInProcess("EscapeFromTarkov.exe")]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            new RepairMaxDurabilityPatch().Enable();
            new RepairWindowPatch().Enable();
        }
    }
}