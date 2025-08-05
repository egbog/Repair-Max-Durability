using System;
using _RepairMaxDurability.ServerJsonStructures;
using EFT.InventoryLogic;
using JetBrains.Annotations;

namespace _RepairMaxDurability.Utils;

public class ParseProfile {
    public static void UpdateValues([CanBeNull] RepairDataResponse repairDataResponse, RepairableComponent targetItemRc,
                                    Item                           repairKit) {
        if (repairDataResponse == null) throw new ArgumentNullException(nameof(repairDataResponse));

        if (repairDataResponse.Items.Find(i => i.Id == targetItemRc.Item.Id) is { } item) {
            targetItemRc.Durability    = item.Upd.Repairable.Durability;
            targetItemRc.MaxDurability = item.Upd.Repairable.MaxDurability;
            targetItemRc.Item.UpdateAttributes();
            targetItemRc.Item.RaiseRefreshEvent(true);
            //this.Log.LogInfo(item.LocalizedName() + " REPAIRED TO: " + repairableComponent.MaxDurability);
        }
        else // something went wrong with json sent from server
        {
            throw new Exception($"Target item {targetItemRc.Item.Id} not found in server.");
        }

        // update repair kit resource
        if (repairDataResponse.Items.Find(i => i.Id == repairKit.Id) is { } kit) {
            repairKit.TryGetItemComponent(out RepairKitComponent repairKitComponent);
            repairKitComponent.Resource = kit.Upd.RepairKit.Resource;
            repairKit.UpdateAttributes();
            repairKit.RaiseRefreshEvent(true);
            //Plugin.Log.LogDebug("NEW REPAIR RESOURCE: " + rkc.Resource);

            // delete repair kit at 0 resource or below
            if (repairKitComponent.Resource <= 0) {
                var traderControllerClass = (TraderControllerClass)repairKit.Parent.GetOwner();
                traderControllerClass.ThrowItem(repairKit);
                //Plugin.Log.LogDebug("DESTROYED REPAIR KIT");
            }

            // all is well - minister fudge
        }
        else {
            throw new Exception($"Target item {repairKit.Id} not found in server.");
        }
    }
}