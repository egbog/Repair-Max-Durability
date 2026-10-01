using EFT.UI;
using SPT.Reflection.Patching;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EFT.InventoryLogic;
using EFT.Repairing;

namespace _RepairMaxDurability.Patches;

public class ShowRepairWindowPatch : ModulePatch {
    protected override MethodBase GetTargetMethod() {
        return typeof(RepairController).GetMethod("GetSuitableRepairersCollections", BindingFlags.Instance | BindingFlags.Public);
    }

    [PatchPostfix]
    public static void Postfix(ref IEnumerable<RepairKitsCollection> __result) {
        // this was way more complicated than it needed to be...
        __result = __result.Where(x => x.RepairerId!= Plugin.KitId).ToList();
    }
}

public class RepairerParametersPanelRefreshPatch : ModulePatch {
    protected override MethodBase GetTargetMethod() {
        return typeof(RepairerParametersPanel).GetMethod("method_0", BindingFlags.Instance | BindingFlags.Public);
    }

    [PatchPrefix]
    public static bool Prefix(RepairKit repairKit) {
        return repairKit.RepairKitTemplate._id != Plugin.KitId;
    }
}