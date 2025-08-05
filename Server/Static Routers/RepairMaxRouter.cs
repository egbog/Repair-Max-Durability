using _RepairMaxDurability.Callbacks;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;

namespace _RepairMaxDurability.Static_Routers;

[Injectable]
public class RepairMaxRouter : StaticRouter {
    public RepairMaxRouter(JsonUtil jsonUtil, RepairMaxCallback repairMaxCallback) : base(jsonUtil,
    [
        new RouteAction("/maxdura/checkdragged",
                        async (url, info, sessionId, _) =>
                            await repairMaxCallback.RepairMax(url, info as RepairInfo, sessionId),
                        typeof(RepairInfo))
    ]) { }
}

public record RepairInfo : IRequestData {
    public required MongoId ItemId { get; init; }
    public required MongoId KitId  { get; init; }
}