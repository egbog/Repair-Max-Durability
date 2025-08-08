using _RepairMaxDurability.Logger;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Models;
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
    TraderStore                store,
    GetConfig                  config,
    ISptLogger<AssortInjector> logger,
    DebugLoggerUtil            debugLoggerUtil,
    AssortHelperExtensions     assortHelperExtensions) {
    public void InjectAssort(MongoId itemId, MongoId assortId) {
        var metaData = new ModMetadata();

        var count        = 0;
        var injectResult = "";

        Dictionary<MongoId, TemplateItem> itemsDict = db.GetItems();
        Dictionary<MongoId, Trader>       traders   = db.GetTraders();

        foreach (Config.TraderStruct t in config.Traders) {
            if (!t.Enabled) continue;

            (MongoId traderId, Trader trader) = traders.FirstOrDefault(x => x.Value.Base.Nickname == t.Name);
            if (trader == null) throw new Exception($"Trader '{t.Name}' not found. Check spelling in config file.");


            if (currentTrader.Base.Currency == null)
                throw new
                    Exception($"Trader '{currentTrader.Base.Nickname}' has no assigned currency. Are you using a modded trader?");

            AssortHelperExtensions.ItemAssort result = assortHelperExtensions.CreateAssort(itemId, assortId,
                (CurrencyType)currentTrader.Base.Currency, t, config.MaxRepairResource);

            assortHelperExtensions.AddItemAssort(result, currentTraderId);

            if (!logger.IsLogEnabled(LogLevel.Debug)) continue;
            count++;
            injectResult += debugLoggerUtil.LogResult(t);
        }

        if (!logger.IsLogEnabled(LogLevel.Debug)) return;
        logger.Debug($"{metaData.Name} v{metaData.Version}: Successfully injected {count} assort(s).");
        logger.Debug($"{injectResult}");
    }
}

[Injectable]
public class AssortHelperExtensions(DatabaseService db) {
    public record ItemAssort {
        public required Item                     AssortItem   { get; init; }
        public required List<List<BarterScheme>> BarterScheme { get; init; }
        public required int                      LoyaltyLevel { get; init; }
    }

    public void AddItemAssort(ItemAssort itemAssort, MongoId traderId) {
        Dictionary<MongoId, Trader> tradersDict = db.GetTraders();
        if (tradersDict == null)
            throw new
                Exception("Traders not loaded properly. Check for any corrupt modded traders and restart server.");

        if (!tradersDict.TryGetValue(traderId, out Trader? trader))
            throw new Exception($"Trader {traderId} not found.");

        trader.Assort.Items.Add(itemAssort.AssortItem);
        trader.Assort.BarterScheme.Add(itemAssort.AssortItem.Id, itemAssort.BarterScheme);
        trader.Assort.LoyalLevelItems.Add(itemAssort.AssortItem.Id, itemAssort.LoyaltyLevel);
    }

    public ItemAssort CreateAssort(string              itemId,       string assortId, CurrencyType currencyType,
                                   Config.TraderStruct assortConfig, int    maxRepairResource) {
        return new ItemAssort {
            AssortItem = new Item {
                Id       = assortId,
                Template = itemId,
                ParentId = "hideout",
                SlotId   = "hideout",
                Upd = new Upd {
                    BuyRestrictionMax     = assortConfig.BuyLimit,
                    BuyRestrictionCurrent = 0,
                    StackObjectsCount     = assortConfig.Stock,
                    RepairKit             = new UpdRepairKit { Resource = maxRepairResource }
                }
            },
            BarterScheme = [
                [new BarterScheme { Count = assortConfig.Price, Template = currencyType.GetCurrencyTpl() }]
            ],
            LoyaltyLevel = assortConfig.LoyaltyLevel
        };
    }
}