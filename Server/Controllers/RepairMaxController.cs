using _RepairMaxDurability.Services;
using _RepairMaxDurability.Static_Routers;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Services;

[Injectable]
public class RepairMaxController(
    ProfileHelper    profileHelper,
    RepairMaxService repairMaxService,
    RepairService    repairService) {
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