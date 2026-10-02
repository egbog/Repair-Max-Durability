using System.Text.Json.Serialization;
using _RepairMaxDurability.Controllers;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.DI.Routing;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Request;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace _RepairMaxDurability.ItemEventRouters;

[Injectable(TypePriority = OnLoadOrder.Routers)]
public sealed class RepairMaxRouter(RepairMaxController repairMaxController) : ItemEventRouter([
    new ItemRouteAction<RepairDataRequest>("MaxDuraRepair",
                                           async (url, pmcData, body, sessionID, output, cancellationToken) => {
                                               List<Item?> items =
                                                   await repairMaxController.RepairMaxWithKit(body, sessionID, pmcData);
                                               foreach (Item item in items.OfType<Item>()) {
                                                   output.ProfileChanges[sessionID].Items?.ChangedItems?.Add(item);
                                               }

                                               return output; // adjust type to match expected return
                                           })
]);

public record RepairDataRequest : BaseInteractionRequestData {
    [JsonPropertyName("item")]
    public MongoId ItemId { get; init; }
    [JsonPropertyName("kit")]
    public MongoId KitId { get; init; }
}