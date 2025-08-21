using _RepairMaxDurability.Helpers;
using _RepairMaxDurability.Injectors;
using _RepairMaxDurability.Logger;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace _RepairMaxDurability.Services;

[Injectable(TypePriority = OnLoadOrder.TraderCallbacks + 1)]
public class AssortService(
    DatabaseService           db,
    GetConfig                 config,
    ISptLogger<AssortService> logger,
    DebugLoggerUtil           debugLoggerUtil) {
    public void AddAssort(MongoId itemId, MongoId assortId) {
        var metaData = new ModMetadata();

        var count        = 0;
        var injectResult = "";

        // cache in case we have to accomodate a large number of traders
        Dictionary<MongoId, TemplateItem> itemsDict = db.GetItems();
        Dictionary<MongoId, Trader>       traders   = db.GetTraders();

        foreach (Config.TraderStruct assortConfig in config.Traders.Where(assortConfig => assortConfig.Enabled)) {
            // fetch trader
            (MongoId traderId, Trader trader) = traders.FirstOrDefault(x => x.Value.Base.Nickname == assortConfig.Name);
            if (trader == null) {
                throw new
                    Exception($"Trader '{assortConfig.Name}' not found. Ensure trader's name is correct in config file.");
            }

            CurrencyType currency = AssortHelperExtensions.GetTraderCurrencyType(trader);

            AssortHelperExtensions.ItemAssort itemAssort =
                AssortHelperExtensions.CreateAssort(itemId, assortId, currency, assortConfig, itemsDict[itemId]);

            TraderAssort traderAssort = GetTraderAssortRef(traderId);

            AddItemAssort(itemAssort, traderAssort);

            if (!logger.IsLogEnabled(LogLevel.Debug)) {
                continue;
            }

            count++;
            injectResult += debugLoggerUtil.LogResult(assortConfig);
        }

        if (!logger.IsLogEnabled(LogLevel.Debug)) {
            return;
        }

        logger.Debug($"{metaData.Name} v{metaData.Version}: Successfully injected {count} assort(s).");
        logger.Debug($"{injectResult}");
    }

    protected TraderAssort GetTraderAssortRef(MongoId traderId) {
        Dictionary<MongoId, Trader> tradersDict = db.GetTraders();
        if (tradersDict == null) {
            throw new
                Exception("Traders not loaded properly. Check for any corrupt modded traders and restart server.");
        }

        if (!tradersDict.TryGetValue(traderId, out Trader? trader)) {
            throw new Exception($"Trader {traderId} not found.");
        }

        return trader.Assort;
    }

    protected static void AddItemAssort(AssortHelperExtensions.ItemAssort itemAssort, TraderAssort traderAssort) {
        traderAssort.Items.Add(itemAssort.AssortItem);
        traderAssort.BarterScheme.Add(itemAssort.AssortItem.Id, itemAssort.BarterScheme);
        traderAssort.LoyalLevelItems.Add(itemAssort.AssortItem.Id, itemAssort.LoyaltyLevel);
    }
}