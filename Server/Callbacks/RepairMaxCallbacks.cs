using _RepairMaxDurability.Static_Routers;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Utils;

namespace _RepairMaxDurability.Callbacks;

[Injectable]
public class RepairMaxCallback(HttpResponseUtil httpResponseUtil, RepairMaxController repairMaxController) {
    public ValueTask<string> RepairMax(string url, RepairInfo info, MongoId sessionId) {
        return new
            ValueTask<string>(httpResponseUtil.GetBody(repairMaxController.RepairMaxWithKit(info, sessionId)));
    }
}