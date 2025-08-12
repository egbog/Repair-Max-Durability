/* LICENSE:
 * MIT
 * 
 * AUTHOR:
 * egbog
 * */

using _RepairMaxDurability.Patches;
using BepInEx;
using BepInEx.Logging;

namespace _RepairMaxDurability {

    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInProcess("EscapeFromTarkov.exe")]
    public class Plugin : BaseUnityPlugin
    {
        public static readonly ManualLogSource Log = BepInEx.Logging.Logger.CreateLogSource("RepairMaxDurability");

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            new RepairMaxDurabilityPatch().Enable();
            new ShowRepairWindowPatch().Enable();
            new RepairerParametersPanelRefreshPatch().Enable();
        }
    }
}