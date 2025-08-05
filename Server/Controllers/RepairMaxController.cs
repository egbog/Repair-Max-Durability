using _RepairMaxDurability.Injectors;
using _RepairMaxDurability.Services;
using _RepairMaxDurability.Static_Routers;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

[Injectable]
public class RepairMaxController(
    ProfileHelper                   profileHelper,
    GetConfig                       config,
    ISptLogger<RepairMaxController> logger,
    RepairMaxService                repairMaxService,
    RepairService                   repairService) {
    public List<Item> RepairMaxWithKit(RepairDataRequest dataRequest, MongoId sessionId) {
        PmcData? pmcData = profileHelper.GetPmcProfile(sessionId);

        (RepairDetails repairDetails, Item repairKit) =
            repairMaxService.RepairMaxItemByKit(dataRequest, sessionId, pmcData);

        repairService.AddBuffToItem(repairDetails, pmcData);

        // Add skill points for repairing items
        repairService.AddRepairSkillPoints(sessionId, repairDetails, pmcData);

        return [repairDetails.RepairedItem, repairKit];
    }
}