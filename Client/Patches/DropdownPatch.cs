using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EFT.InventoryLogic;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using RepairDropdownInterface = GInterface37;
using RepairDropdown = GClass903;
using RepairKits = GClass902;

namespace _RepairMaxDurability.Patches {
    public class RepairWindowPatch : ModulePatch {
        public static PropertyInfo RepairKitsCollections;
        public static FieldInfo    List_1;

        protected override MethodBase GetTargetMethod() {
            // The type argument needs to be the class that declares the property, not the return type
            RepairKitsCollections = AccessTools.Property(typeof(RepairDropdownInterface), "RepairKitsCollections");
            List_1                = AccessTools.Field(typeof(RepairDropdown), "List_1");

            return typeof(RepairWindow).GetMethod("method_2", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        public static void Postfix(ref RepairDropdownInterface __result, Item item,
                                   RepairControllerClass       repairController) {
            // check that we actually have Spare firearm parts in our inventory
            // get List<RepairKits> instance __result
            var repairKitList = (List<RepairKits>)RepairKitsCollections.GetValue(__result);

            // we good to go
            if (repairKitList.Exists(x => x.LocalizedName.Contains("Spare firearm parts"))) {
                // we have to generate a new RepairDropdown in order to edit the RepairKitsCollections->list_1
                // once it is returned and cast to a RepairDropdownInterface, the values have no setter
                var repairDropdown = new RepairDropdown(item, repairController);

                // get list_1 from RepairKits
                var newList_1 = (List<RepairKits>)RepairKitsCollections.GetValue(repairDropdown);

                // if list contains our item then do work
                newList_1.Remove(newList_1.First(x => x.LocalizedName.Contains("Spare firearm parts")));
                // replace with our list_1 with Spare firearm parts removed
                List_1.SetValue(repairDropdown, newList_1);

                // change return value
                __result = repairDropdown;
            }
        }
    }
}