/* LICENSE:
 * MIT
 *
 * AUTHOR:
 * egbog
 * */

using System.Collections;
using _RepairMaxDurability.Patches;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _RepairMaxDurability;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInProcess("EscapeFromTarkov.exe")]
public class Plugin : BaseUnityPlugin {
    public static readonly ManualLogSource Log = BepInEx.Logging.Logger.CreateLogSource("RepairMaxDurability");

    private void Awake() {
        // Plugin startup logic
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

        new RepairMaxDurabilityPatch().Enable();
        new RepairerParametersPanelRefreshPatch().Enable();

        StartCoroutine(CheckMenuIsLoadedRoutine());
    }

    private IEnumerator CheckMenuIsLoadedRoutine() {
        WaitForSecondsRealtime wait = new(5f);
        while (true) {
            // Scene is a value type so we have to get the scene every time...
            Scene s = SceneManager.GetActiveScene();
            if (s.IsValid() && s.name == "CommonUIScene") {
                // small delay to allow static constructors to run
                yield return new WaitForSecondsRealtime(20f);
                new ShowRepairWindowPatch().Enable();
                yield break;
            }

            yield return wait;
        }
    }
}