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
    public void InjectAssort(MongoId itemId, MongoId assortId) {
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

public static class AssortHelperExtensions {
    public record ItemAssort {
        public required Item                     AssortItem   { get; init; }
        public required List<List<BarterScheme>> BarterScheme { get; init; }
        public required int                      LoyaltyLevel { get; init; }
    }

    public static CurrencyType GetTraderCurrencyType(Trader trader) {
        if (trader.Base.Currency == null)
            throw new
                Exception($"Trader '{trader.Base.Nickname}' has no assigned currency. Are you using a modded trader?");

        return (CurrencyType)trader.Base.Currency;
    }

    public static TraderAssort GetTraderAssortRef(DatabaseService db, MongoId traderId) {
        Dictionary<MongoId, Trader> tradersDict = db.GetTraders();
        if (tradersDict == null)
            throw new
                Exception("Traders not loaded properly. Check for any corrupt modded traders and restart server.");

        if (!tradersDict.TryGetValue(traderId, out Trader? trader))
            throw new Exception($"Trader {traderId} not found.");

        return trader.Assort;
    }

    public static void AddItemAssort(ItemAssort itemAssort, TraderAssort traderAssort) {
        traderAssort.Items.Add(itemAssort.AssortItem);
        traderAssort.BarterScheme.Add(itemAssort.AssortItem.Id, itemAssort.BarterScheme);
        traderAssort.LoyalLevelItems.Add(itemAssort.AssortItem.Id, itemAssort.LoyaltyLevel);
    }

    public static ItemAssort CreateAssort(string              itemId,       string assortId, CurrencyType currencyType,
                                          Config.TraderStruct assortConfig, TemplateItem templateitem) {
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
                    RepairKit             = new UpdRepairKit { Resource = templateitem.Properties?.MaxRepairResource }
                }
            },
            BarterScheme = [
                [new BarterScheme { Count = assortConfig.Price, Template = currencyType.GetCurrencyTpl() }]
            ],
            LoyaltyLevel = assortConfig.LoyaltyLevel
        };
    }
}