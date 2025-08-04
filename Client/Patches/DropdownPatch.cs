using System.Reflection;
using EFT.UI;
using SPT.Reflection.Patching;
using RepairStrategy = GInterface37;

namespace _RepairMaxDurability.Patches;

public class ShowRepairWindowPatch : ModulePatch {
    protected override MethodBase GetTargetMethod() {
        return typeof(RepairWindow).GetMethod("method_2", BindingFlags.Instance | BindingFlags.Public);
    }

    [PatchPostfix]
    public static void Postfix(ref RepairStrategy __result) {
        // this was way more complicated than it needed to be...
        __result.RepairKitsCollections.RemoveAll(x => x.RepairKitsTemplateClass._id == "86afd148ac929e6eddc5e370");
    }
}

public class RepairerParametersPanelRefreshPatch : ModulePatch {
    protected override MethodBase GetTargetMethod() {
        return typeof(RepairerParametersPanel).GetMethod("method_0", BindingFlags.Instance | BindingFlags.Public);
    }

    [PatchPrefix]
    public static bool Prefix(ref RepairKitsItemClass repairKit) {
        return repairKit.RepairKitsTemplateClass._id != "86afd148ac929e6eddc5e370";
    }
}