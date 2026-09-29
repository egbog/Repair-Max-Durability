using System.Text.Json.Serialization;
using _RepairMaxDurability.Controllers;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Request;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;

namespace _RepairMaxDurability.ItemEventRouters;

[Injectable]
public class RepairMaxRouter(RepairMaxController repairMaxController) : ItemEventRouterDefinition {
    protected override List<HandledRoute> GetHandledRoutes() => [new("MaxDuraRepair", false)];

    protected override ValueTask<ItemEventRouterResponse> HandleItemEventInternal(
        string                  url, PmcData pmcData, BaseInteractionRequestData body, MongoId sessionID,
        ItemEventRouterResponse output) {
        var req = (RepairDataRequest)body;

        List<Item?> items = repairMaxController.RepairMaxWithKit(req, sessionID, pmcData);
        
        foreach (Item item in items.OfType<Item>())
        {
            output.ProfileChanges[sessionID].Items.ChangedItems.Add(item);
        }
        
        return new ValueTask<ItemEventRouterResponse>(output);
    }
}

public record RepairDataRequest : BaseInteractionRequestData {
    [JsonPropertyName("item")]
    public MongoId ItemId { get; init; }
    [JsonPropertyName("kit")]
    public MongoId KitId { get; init; }
}