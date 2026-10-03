using _RepairMaxDurability.ItemEventRouters;
using _RepairMaxDurability.Services;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Routers;
using SPTarkov.Server.Core.Services.Commerce;

namespace _RepairMaxDurability.Controllers;

[Injectable]
public class RepairMaxController(EventOutputHolder eventOutputHolder, RepairMaxService repairMaxService, RepairService repairService) {
    public ItemEventRouterResponse RepairMaxWithKit(RepairDataRequest dataRequest, MongoId sessionId, PmcData pmcData) {
        ItemEventRouterResponse output = eventOutputHolder.GetOutput(sessionId);

        (RepairDetails repairDetails, Item repairKit) = repairMaxService.RepairMaxItemByKit(dataRequest, sessionId, pmcData);

        repairService.AddBuffToItem(repairDetails, pmcData);

        // Add skill points for repairing items
        repairService.AddRepairSkillPoints(sessionId, repairDetails, pmcData);
        
        output.ProfileChanges[sessionId].Items.ChangedItems.Add(repairDetails.RepairedItem);
        output.ProfileChanges[sessionId].Items.ChangedItems.Add(repairKit);

        return output;
    }
}