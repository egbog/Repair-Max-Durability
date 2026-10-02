using _RepairMaxDurability.ItemEventRouters;
using _RepairMaxDurability.Services;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Services.Commerce;

namespace _RepairMaxDurability.Controllers;

[Injectable]
public class RepairMaxController(RepairMaxService repairMaxService, RepairService repairService) {
    public async Task<List<Item?>> RepairMaxWithKit(RepairDataRequest dataRequest, MongoId sessionId, PmcData pmcData) {
        if (pmcData is null) {
            throw new Exception($"pmcData not found for id: {sessionId}. Aborting repair.");
        }

        (RepairDetails repairDetails, Item repairKit) = repairMaxService.RepairMaxItemByKit(dataRequest, sessionId, pmcData);

        repairService.AddBuffToItem(repairDetails, pmcData);

        // Add skill points for repairing items
        repairService.AddRepairSkillPoints(sessionId, repairDetails, pmcData);

        return await Task.FromResult(new List<Item?> { repairDetails.RepairedItem, repairKit });
    }
}