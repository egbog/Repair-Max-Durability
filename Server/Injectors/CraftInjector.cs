using _RepairMaxDurability.Logger;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Hideout;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Hideout;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace _RepairMaxDurability.Injectors;

[Injectable(TypePriority = OnLoadOrder.TraderCallbacks + 1)]
public class CraftInjector(
    DatabaseService           db,
    GetConfig                 config,
    ISptLogger<CraftInjector> logger,
    DebugLoggerUtil           debugLoggerUtil) {
    private static HideoutProduction CreateCraft(string itemId, string craftId, List<Requirement> requirements,
                                                 int    productionTime) {
        return new HideoutProduction {
            Id                           = craftId,
            EndProduct                   = itemId,
            Requirements                 = requirements,
            AreaType                     = HideoutAreas.Workbench,
            Continuous                   = false,
            Count                        = 1,
            ProductionTime               = productionTime,
            ProductionLimitCount         = 1,
            IsEncoded                    = false,
            Locked                       = false,
            NeedFuelForAllProductionTime = false,
            IsCodeProduction             = false
        };
    }

    public void InjectCraft(MongoId itemId, MongoId craftId) {
        var metaData = new ModMetadata();

        var count        = 0;
        var injectResult = "";

        Hideout hideout = db.GetHideout();
        if (hideout.Production.Recipes == null) {
            throw new Exception("Unable to find hideout recipes. Profile may be corrupt.");
        }

        foreach (Config.CraftStruct craft in config.Crafts) {
            if (!craft.Enabled) {
                continue;
            }

            HideoutProduction productionItem = CreateCraft(itemId, craftId, craft.Requirements, craft.CraftTime);

            hideout.Production.Recipes.Add(productionItem);

            if (!logger.IsLogEnabled(LogLevel.Debug)) {
                continue;
            }

            count++;
            injectResult += debugLoggerUtil.LogResult(productionItem);
        }

        if (!logger.IsLogEnabled(LogLevel.Debug)) {
            return;
        }

        logger.Debug($"{metaData.Name} v{metaData.Version}: Successfully injected {count} craft(s).");
        logger.Debug($"{injectResult}");
    }
}