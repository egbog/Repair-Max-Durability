using _RepairMaxDurability.Helpers;
using _RepairMaxDurability.Logger;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace _RepairMaxDurability.Injectors;

[Injectable(TypePriority = OnLoadOrder.TraderCallbacks + 1)]
public class AssortInjector(
    DatabaseService            db,
    GetConfig                  config,
    ISptLogger<AssortInjector> logger,
    DebugLoggerUtil            debugLoggerUtil) {
    public void AddAssort(MongoId itemId, MongoId assortId) {
        var metaData = new ModMetadata();

        var count        = 0;
        var injectResult = "";

        Dictionary<MongoId, TemplateItem> itemsDict = db.GetItems();
        Dictionary<MongoId, Trader>       traders   = db.GetTraders();

        foreach (Config.TraderStruct assortConfig in config.Traders.Where(assortConfig => assortConfig.Enabled)) {
            // fetch trader
            (MongoId traderId, Trader trader) = traders.FirstOrDefault(x => x.Value.Base.Nickname == assortConfig.Name);
            if (trader == null)
                throw new
                    Exception($"Trader '{assortConfig.Name}' not found. Ensure trader's name is correct in config file.");

            CurrencyType currency = AssortHelperExtensions.GetTraderCurrencyType(trader);

            AssortHelperExtensions.ItemAssort itemAssort =
                AssortHelperExtensions.CreateAssort(itemId, assortId, currency, assortConfig, itemsDict[itemId]);

            TraderAssort traderAssort = AssortHelperExtensions.GetTraderAssortRef(db, traderId);

            AssortHelperExtensions.AddItemAssort(itemAssort, traderAssort);

            if (!logger.IsLogEnabled(LogLevel.Debug)) continue;
            count++;
            injectResult += debugLoggerUtil.LogResult(assortConfig);
        }

        if (!logger.IsLogEnabled(LogLevel.Debug)) return;
        logger.Debug($"{metaData.Name} v{metaData.Version}: Successfully injected {count} assort(s).");
        logger.Debug($"{injectResult}");
    }
}