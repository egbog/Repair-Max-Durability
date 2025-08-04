using System.Diagnostics.CodeAnalysis;
using _RepairMaxDurability.Injectors;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace _RepairMaxDurability.Static_Routers;

[Injectable]
public class RepairMaxRouter : StaticRouter {
    public RepairMaxRouter(JsonUtil jsonUtil, RepairMaxCallback repairMaxCallback) : base(jsonUtil,
    [
        new RouteAction("/maxdura/checkdragged",
                        async (url, info, sessionId, _) =>
                            await repairMaxCallback.CheckRepair(url, info as RepairInfo, sessionId),
                        typeof(RepairInfo))
    ]) { }
}

[Injectable]
public class RepairMaxCallback(HttpResponseUtil httpResponseUtil, RepairMaxController repairMaxController) {
    public ValueTask<string> CheckRepair(string url, RepairInfo info, MongoId sessionId) {
        return new ValueTask<string>(httpResponseUtil.GetBody(repairMaxController.CheckRepair(info, sessionId)));
    }
}

[Injectable]
public class RepairMaxController(
    ProfileHelper                   profileHelper,
    GetConfig                       config,
    ISptLogger<RepairMaxController> logger) {
    public List<Item> CheckRepair(RepairInfo info, MongoId sessionId) {
        // grab profile inventory
        PmcData?          pmcData   = profileHelper.GetPmcProfile(sessionId);
        BotBaseInventory? inventory = pmcData?.Inventory;
        if (inventory?.Items == null)
            throw new Exception($"Unable to find pmc inventory data for id: {sessionId}. Profile may be corrupt.");

        Item itemToRepair = inventory.Items.Find((x) => info.ItemId == x.Id) ??
                            throw new Exception($"Item {info.ItemId} not found in inventory.");
        Item repairKit = inventory.Items.Find((x) => info.KitId == x.Id) ??
                         throw new Exception($"Repair kit {info.KitId} not found in inventory.");

        // lookup
        //Item itemToRepair = inventory.Items.Find((x) => x.Id == itemId) ??
        //                    throw new Exception($"Item {itemId} not found in inventory.");
        //Item repairKit = inventory.Items.Find((x) => x.Id == kitId) ??
        //                 throw new Exception($"Repair kit {kitId} not found in inventory.");

        if (itemToRepair.Upd?.Repairable == null) throw new Exception($"Item {itemToRepair.Id} is not repairable.");

        double? amountToRepair = 100 - itemToRepair.Upd.Repairable.MaxDurability;
        // update our item
        itemToRepair.Upd.Repairable.Durability = itemToRepair.Upd.Repairable.MaxDurability = 100;

        if (logger.IsLogEnabled(LogLevel.Debug))
            logger.Debug($"Repaired {itemToRepair.Id} by {amountToRepair:F5} points.");

        // check if repair kit was crafted
        // for some reason crafted kits don't contain a "RepairKit" component in upd
        // so just workaround add it ourselves
        if (repairKit.Upd?.RepairKit?.Resource == null) {
            logger.Warning($"Repair kit {repairKit.Id} is corrupted. Fixing...");
            repairKit.Upd = new Upd { RepairKit = new UpdRepairKit { Resource = config.MaxRepairResource } };
        }

        repairKit.Upd.RepairKit.Resource--;

        // it appears that calling TraderControllerClass.DestroyItem() clientside will trigger /client/game/profile/items/moving
        // event: Remove
        // and delete our item from the profile for us
        // TraderControllerClass.DestroyItem() was changed to ThrowItem() and TryThrowItem()

        // organize our items into a parent "Items" so we can use JToken.First and JToken.Next client-side
        return [itemToRepair, repairKit];
    }
}

public record RepairInfo : IRequestData {
    public required MongoId ItemId { get; init; }
    public required MongoId KitId  { get; init; }
}