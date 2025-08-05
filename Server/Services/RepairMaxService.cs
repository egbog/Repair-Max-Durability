using _RepairMaxDurability.Static_Routers;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace _RepairMaxDurability.Services;

[Injectable]
public class RepairMaxService(DatabaseService db, RepairHelper repairHelper, ISptLogger<RepairMaxService> logger) {
    public (RepairDetails repairDetails, Item repairKit) RepairMaxItemByKit(
        RepairDataRequest dataRequest, MongoId sessionId, PmcData pmcData) {
        Item itemToRepair = pmcData.Inventory?.Items?.FirstOrDefault(x => x.Id == dataRequest.ItemId) ??
                            throw new Exception($"Item {dataRequest.ItemId} not found in inventory.");

        Item repairKit = pmcData.Inventory?.Items?.FirstOrDefault(x => x.Id == dataRequest.KitId) ??
                         throw new Exception($"Repair kit {dataRequest.KitId} not found in inventory.");

        Dictionary<MongoId, TemplateItem> itemsDict             = db.GetItems();
        TemplateItem                      itemToRepairTemplate  = itemsDict[itemToRepair.Template];
        TemplateItem                      repairKitTemplateItem = itemsDict[repairKit.Template];

        double amountToRepair = 100 - itemToRepair.Upd?.Repairable?.Durability ?? 0;
        itemToRepair.Upd.Repairable.MaxDurability += amountToRepair;

        repairHelper.UpdateItemDurability(itemToRepair, itemToRepairTemplate, false, amountToRepair, true, 1, false);

        // check if repair kit was crafted
        // for some reason crafted kits don't contain a "RepairKit" component in upd
        // so just workaround add it ourselves
        AddMaxResourceToKitIfMissing(repairKitTemplateItem, repairKit);

        repairKit.Upd.RepairKit.Resource--;

        // it appears that calling TraderControllerClass.DestroyItem() clientside will trigger /client/game/profile/items/moving
        // event: Remove
        // and delete our item from the profile for us
        // TraderControllerClass.DestroyItem() was changed to ThrowItem() and TryThrowItem()
        return (
            new RepairDetails {
                RepairPoints        = amountToRepair,
                RepairedItem        = itemToRepair,
                RepairedItemIsArmor = false,
                RepairAmount        = amountToRepair,
                RepairedByKit       = true
            }, repairKit);
    }

    /// <summary>
    ///     Update repair kits Resource object if it doesn't exist
    /// </summary>
    /// <param name="repairKitDetails">Repair kit details from db</param>
    /// <param name="repairKitInInventory">Repair kit to update</param>
    protected void AddMaxResourceToKitIfMissing(TemplateItem repairKitDetails, Item repairKitInInventory) {
        int? maxRepairAmount = repairKitDetails.Properties.MaxRepairResource;
        if (repairKitInInventory.Upd is null) {
            if (logger.IsLogEnabled(LogLevel.Debug))
                logger.Debug($"Repair kit: {repairKitInInventory.Id} in inventory lacks upd object, adding");

            repairKitInInventory.Upd = new Upd { RepairKit = new UpdRepairKit { Resource = maxRepairAmount } };
        }

        if (repairKitInInventory.Upd.RepairKit?.Resource is null)
            repairKitInInventory.Upd.RepairKit = new UpdRepairKit { Resource = maxRepairAmount };
    }
}