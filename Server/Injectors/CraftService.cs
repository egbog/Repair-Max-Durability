using _RepairMaxDurability.Logger;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Hideout;
using SPTarkov.Server.Core.Models.Enums.Hideout;
using SPTarkov.Server.Core.Models.Spt.Hideout;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;

namespace _RepairMaxDurability.Injectors;

[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1)]
public class CraftService(
    DatabaseService          db,
    GetConfig                config,
    ISptLogger<CraftService> logger,
    DebugLoggerUtil          debugLoggerUtil) {
    private static HideoutProduction CreateCraft(string itemId,         string craftId, List<Requirement> requirements,
                                                 int    productionTime, int    count) {
        return new HideoutProduction {
            Id                           = craftId,
            EndProduct                   = itemId,
            Requirements                 = requirements,
            AreaType                     = HideoutAreas.Workbench,
            Continuous                   = false,
            Count                        = count,
            ProductionTime               = productionTime,
            ProductionLimitCount         = 1,
            IsEncoded                    = false,
            Locked                       = false,
            NeedFuelForAllProductionTime = false,
            IsCodeProduction             = false
        };
    }

    public void AddCraft(MongoId itemId, MongoId craftId) {
        var count        = 0;
        var injectResult = "";

        Hideout hideout = db.GetHideout();
        if (hideout.Production.Recipes == null) {
            throw new Exception("Unable to find hideout recipes. Profile may be corrupt.");
        }

        foreach (CraftStruct craft in config.Crafts.Where(x => x.Enabled)) {
            HideoutProduction productionItem = CreateCraft(itemId, craftId, craft.Requirements, craft.CraftTime,
                                                           craft.AmountCrafted);

            hideout.Production.Recipes.Add(productionItem);

            if (!RepairMaxDurability.Debug) {
                continue;
            }

            count++;
            injectResult += debugLoggerUtil.LogResult(productionItem);
        }

        if (!RepairMaxDurability.Debug) {
            return;
        }

        logger.Debug($"{RepairMaxDurability.Mod.Name} v{RepairMaxDurability.Mod.Version}: Successfully injected {count} craft(s).\n{injectResult}");
    }
}