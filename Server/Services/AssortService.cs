using _RepairMaxDurability.Helpers;
using _RepairMaxDurability.Injectors;
using _RepairMaxDurability.Logger;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Tables;

namespace _RepairMaxDurability.Services;

[Injectable(TypePriority = OnLoadOrder.Preload)]
public class AssortService(
    TemplateTable             templateTable,
    TradersTable tradersTable,
    GetConfig                 config,
    ISptLogger<AssortService> logger,
    DebugLoggerUtil           debugLoggerUtil) {
    public void AddAssort(MongoId itemId, MongoId assortId) {
        var count        = 0;
        var injectResult = "";

        // cache in case we have to accomodate a large number of traders
        Dictionary<MongoId, TemplateItem> itemsDict = templateTable.Items;

        foreach (TraderStruct assortConfig in config.Traders.Where(assortConfig => assortConfig.Enabled)) {
            // fetch trader
            (MongoId traderId, Trader trader) = tradersTable.FirstOrDefault(x => x.Value.Base.Nickname == assortConfig.Name);
            if (trader == null) {
                throw new
                    Exception($"Trader '{assortConfig.Name}' not found. Ensure trader's name is correct in config file.");
            }

            CurrencyType currency = AssortHelperExtensions.GetTraderCurrencyType(trader);

            AssortHelperExtensions.ItemAssort itemAssort =
                AssortHelperExtensions.CreateAssort(itemId, assortId, currency, assortConfig, itemsDict[itemId]);

            TraderAssort traderAssort = GetTraderAssortRef(traderId);

            AddItemAssort(itemAssort, traderAssort);

            if (!RepairMaxDurability.Debug) {
                continue;
            }

            count++;
            injectResult += debugLoggerUtil.LogResult(assortConfig);
        }

        if (!RepairMaxDurability.Debug) {
            return;
        }

        logger.Debug($"[{RepairMaxDurability.Mod.Name}] Successfully injected {count} assort(s).\n{injectResult}");
    }

    protected TraderAssort GetTraderAssortRef(MongoId traderId) {
        if (tradersTable == null) {
            throw new
                Exception("Traders not loaded properly. Check for any corrupt modded traders and restart server.");
        }

        if (!tradersTable.TryGetValue(traderId, out Trader? trader)) {
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