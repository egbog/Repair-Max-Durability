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
public class RepairMaxService(
    DatabaseService              db,
    ProfileHelper                profileHelper,
    RepairHelper                 repairHelper,
    ISptLogger<RepairMaxService> logger) {
    public List<Item> RepairMaxItemByKit(RepairDataRequest dataRequest, MongoId sessionId) {
        // grab profile inventory
        PmcData?          pmcData   = profileHelper.GetPmcProfile(sessionId);
        BotBaseInventory? inventory = pmcData?.Inventory;
        if (inventory?.Items == null)
            throw new Exception($"Unable to find pmc inventory data for id: {sessionId}. Profile may be corrupt.");

        Item itemToRepair = inventory.Items.Find((x) => dataRequest.ItemId == x.Id) ??
                            throw new Exception($"Item {dataRequest.ItemId} not found in inventory.");
        Item repairKit = inventory.Items.Find((x) => dataRequest.KitId == x.Id) ??
                         throw new Exception($"Repair kit {dataRequest.KitId} not found in inventory.");

        if (itemToRepair.Upd?.Repairable == null) throw new Exception($"Item {itemToRepair.Id} is not repairable.");

        double       amountToRepair       = 100 - itemToRepair.Upd.Repairable?.MaxDurability ?? 0;
        TemplateItem itemToRepairTemplate = db.GetItems()[itemToRepair.Template];
        repairHelper.UpdateItemDurability(itemToRepair, itemToRepairTemplate, false, amountToRepair, true, 0, false);

        // update our item
        itemToRepair.Upd.Repairable.Durability = itemToRepair.Upd.Repairable.MaxDurability = 100;

        if (logger.IsLogEnabled(LogLevel.Debug))
            logger.Debug($"Repaired max {itemToRepairTemplate.Name} by {amountToRepair:F5} points.");

        // check if repair kit was crafted
        // for some reason crafted kits don't contain a "RepairKit" component in upd
        // so just workaround add it ourselves
        TemplateItem repairKitTemplateItem = db.GetItems()[repairKit.Template];

        AddMaxResourceToKitIfMissing(repairKitTemplateItem, repairKit);

        //if (repairKit.Upd?.RepairKit?.Resource == null) {
        //    logger.Warning($"Repair kit {repairKit.Id} is corrupted. Fixing...");
        //    repairKit.Upd = new Upd { RepairKit = new UpdRepairKit { Resource = config.MaxRepairResource } };
        //}

        repairKit.Upd.RepairKit.Resource--;

        // it appears that calling TraderControllerClass.DestroyItem() clientside will trigger /client/game/profile/items/moving
        // event: Remove
        // and delete our item from the profile for us
        // TraderControllerClass.DestroyItem() was changed to ThrowItem() and TryThrowItem()

        // organize our items into a parent "Items" so we can use JToken.First and JToken.Next client-side
        return [itemToRepair, repairKit];
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