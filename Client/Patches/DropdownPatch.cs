using EFT.UI;
using SPT.Reflection.Patching;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace _RepairMaxDurability.Patches;

public class ShowRepairWindowPatch : ModulePatch {
    protected override MethodBase GetTargetMethod() {
        return typeof(RepairControllerClass).GetMethod("method_1", BindingFlags.Instance | BindingFlags.Public);
    }

    [PatchPostfix]
    public static void Postfix(ref IEnumerable<GClass904> __result) {
        // this was way more complicated than it needed to be...
        __result = __result.Where(x => x.RepairKitsTemplateClass._id != "86afd148ac929e6eddc5e370").ToList();
    }
}

public class RepairerParametersPanelRefreshPatch : ModulePatch {
    protected override MethodBase GetTargetMethod() {
        return typeof(RepairerParametersPanel).GetMethod("method_0", BindingFlags.Instance | BindingFlags.Public);
    }

    [PatchPrefix]
    public static bool Prefix(RepairKitsItemClass repairKit) {
        return repairKit.RepairKitsTemplateClass._id != "86afd148ac929e6eddc5e370";
    }
}