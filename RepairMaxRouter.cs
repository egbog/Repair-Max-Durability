using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;

namespace _RepairMaxDurability;

[Injectable]
public class RepairMaxRouter : StaticRouter {
    public RepairMaxRouter(JsonUtil jsonUtil, RepairMaxCallback repairMaxCallback) : base(jsonUtil,
        // Add an array of routes we want to add
        [
            new RouteAction("/maxdura/checkdragged",
                            async (url, info, sessionId, _) =>
                                await repairMaxCallback.CheckRepair(url, (info as RepairInfo)!, sessionId),
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
    SaveServer                      saveServer,
    GetTraderConfig                 config,
    ISptLogger<RepairMaxController> logger) {
    public List<Item> CheckRepair(RepairInfo info, MongoId sessionId) {
        // get values from our client
        MongoId id    = info.ItemId;
        MongoId kitId = info.KitId;

        // grab profile inventory
        SptProfile? profile = saveServer.GetProfile(sessionId);
        if (profile?.CharacterData == null)
            throw new Exception($"Unable to find character data for id: {sessionId}. Profile may be corrupt.");

        BotBaseInventory? inventory = profile.CharacterData?.PmcData?.Inventory;
        if (inventory?.Items == null)
            throw new Exception($"Unable to find pmc inventory data for id: {sessionId}. Profile may be corrupt.");

        // lookup
        Item itemToRepair = inventory.Items.Find((x) => x.Id == id) ??
                             throw new Exception("Item to repair not found in server.");
        Item repairKit = inventory.Items.Find((x) => x.Id == kitId) ??
                         throw new Exception("Repair kit not found in server.");

        if (itemToRepair.Upd?.Repairable == null) throw new Exception($"Item {itemToRepair.Id} is not repairable.");

        const double itemMaxDurability        = 100;
        double?      itemCurrentMaxDurability = itemToRepair.Upd.Repairable.MaxDurability;

        // set new values
        double? amountToRepair          = itemMaxDurability        - itemCurrentMaxDurability;
        double? newCurrentMaxDurability = itemCurrentMaxDurability + amountToRepair;

        // update our item
        itemToRepair.Upd.Repairable.Durability    = newCurrentMaxDurability;
        itemToRepair.Upd.Repairable.MaxDurability = newCurrentMaxDurability;

        // check if repair kit was crafted
        // for some reason crafted kits don't contain a "RepairKit" component in upd
        // so just workaround add it ourselves
        if (repairKit.Upd?.RepairKit?.Resource == null) {
            logger.Warning($"Repair kit {repairKit.Id} is corrupted. Fixing...");
            repairKit = FixKit(repairKit);
        }

        repairKit.Upd!.RepairKit!.Resource--;

        // it appears that calling TraderControllerClass.DestroyItem() clientside will trigger /client/game/profile/items/moving
        // event: Remove
        // and delete our item from the profile for us
        // TraderControllerClass.DestroyItem() was changed to ThrowItem() and TryThrowItem()

        // organize our items into a parent "Items" so we can use JToken.First and JToken.Next client-side
        return [itemToRepair, repairKit];
    }

    private Item FixKit(Item kit) {
        kit.Upd = new Upd { RepairKit = new UpdRepairKit { Resource = config.MaxRepairResource } };
        return kit;
    }
}

public record RepairInfo : IRequestData {
    public MongoId ItemId { get; set; }
    public MongoId KitId  { get; set; }
}