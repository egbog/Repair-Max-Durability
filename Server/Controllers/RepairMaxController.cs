using _RepairMaxDurability.Injectors;
using _RepairMaxDurability.Services;
using _RepairMaxDurability.Static_Routers;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Utils;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

[Injectable]
public class RepairMaxController(
    ProfileHelper                   profileHelper,
    GetConfig                       config,
    ISptLogger<RepairMaxController> logger,
    RepairMaxService                repairMaxService) {
    public List<Item> RepairMaxWithKit(RepairInfo info, MongoId sessionId) {
        return repairMaxService.RepairMaxItemByKit(info, sessionId);
    }
}