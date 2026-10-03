/* LICENSE:
 * MIT
 *
 * AUTHOR:
 * egbog
 * */

using System.Collections;
using System.Runtime.CompilerServices;
using _RepairMaxDurability.Patches;
using BepInEx;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.Repairing;
using UnityEngine;

namespace _RepairMaxDurability;

[BepInPlugin("com.egbog.repairmaxdurability", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInProcess("EscapeFromTarkov.exe")]
public class Plugin : BaseUnityPlugin {
    public static readonly ManualLogSource Log   = BepInEx.Logging.Logger.CreateLogSource("RepairMaxDurability");
    public static readonly MongoID         KitId = "86afd148ac929e6eddc5e370";

    private void Awake() {
        // Plugin startup logic
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

        new RepairMaxDurabilityPatch().Enable();
        new RepairerParametersPanelRefreshPatch().Enable();

        StartCoroutine(CheckMenuIsLoadedRoutine());
    }

    private IEnumerator CheckMenuIsLoadedRoutine() {
        WaitForSecondsRealtime wait = new(4f);
        while (true) {
            GlobalConfiguration cfg = Singleton<GlobalConfiguration>.Instance;
            if (cfg is { SkillsSettings: not null, RepairSettings: not null }) {
                // run cctor NOW, singleton is ready
                RuntimeHelpers.RunClassConstructor(typeof(RepairController).TypeHandle);
                new ShowRepairWindowPatch().Enable();
                yield break;
            }

            yield return wait;
        }
    }
}