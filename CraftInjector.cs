using SPTarkov.Common.Extensions;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Hideout;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Hideout;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace _RepairMaxDurability;

[Injectable(TypePriority = OnLoadOrder.TraderCallbacks + 1)]
public class CraftInjector(DatabaseService db, GetTraderConfig config, ISptLogger<CraftInjector> logger) {
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
        var count = 0;

        List<Requirement> reqs           = config.Requirements;
        HideoutProduction productionItem = CreateCraft(itemId, craftId, reqs, config.CraftTime);
        Hideout hideout = db.GetHideout();

        if (hideout.Production.Recipes == null)
            throw new Exception("Unable to find hideout recipes. Profile may be corrupt.");

        hideout.Production.Recipes.Add(productionItem);

        count++;
        if (logger.IsLogEnabled(LogLevel.Debug)) {
            LogResult(productionItem, reqs);
            logger.Debug($"Successfully injected {count} craft(s).");
        }
    }

    private void LogResult(HideoutProduction productionItem, List<Requirement> reqs) {
        var result = "";

        foreach (KeyValuePair<string, object?> item in productionItem.GetAllPropsAsDict()) {
            if (item.Key == "Requirements") {
                foreach (Requirement req in reqs) {
                    result += "[Requirement, {";
                    foreach (KeyValuePair<string, object?> a in req.GetAllPropsAsDict())
                        if (a.Value != null)
                            result += a + ", ";

                    result += "}], ";
                }

                continue;
            }

            result += item + ", ";
        }

        logger.Debug($"Added craft: {result}");
    }
}