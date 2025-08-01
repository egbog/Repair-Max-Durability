using System.Text.Json;
using System.Text.Json.Nodes;
using SPTarkov.Common.Extensions;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Models;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace _RepairMaxDurability;

[Injectable(TypePriority = OnLoadOrder.TraderCallbacks + 1)]
public class AssortInjector(
    DatabaseService            db,
    TraderStore                store,
    GetTraderConfig            config,
    ISptLogger<AssortInjector> logger,
    HttpResponseUtil           httpResponseUtil) {
    public void InjectAssort(MongoId itemId, MongoId assortId) {
        var count = 0;

        Dictionary<MongoId, Trader> tradersDict = db.GetTraders();
        if (tradersDict == null)
            throw new
                Exception("Traders not loaded properly. Check for any corrupt modded traders and restart server.");

        foreach (TraderConfig.TraderStruct t in config.Traders) {
            if (!t.Enabled) continue;

            // find matching trader
            ITrader? currentITrader = store.GetAllTraders().FirstOrDefault((x) => x.Name == t.Name) ?? null;
            if (currentITrader == null)
                throw new Exception($"Trader '{t.Name}' not found. Check spelling in config file.");

            // get assort using ITrader mongoId
            Trader       currentTrader = tradersDict[currentITrader.Id];
            TraderAssort assort        = currentTrader.Assort;

            assort.Items.Add(new Item {
                                 Id       = assortId,
                                 Template = itemId,
                                 ParentId = "hideout",
                                 SlotId   = "hideout",
                                 Upd = new Upd {
                                     BuyRestrictionMax     = t.BuyLimit,
                                     BuyRestrictionCurrent = 0,
                                     StackObjectsCount     = t.Stock,
                                     RepairKit             = new UpdRepairKit { Resource = config.MaxRepairResource }
                                 }
                             });

            if (!currentTrader.Base.Currency.HasValue)
                throw new
                    Exception($"Trader '{currentITrader.Name}' has no assigned currency. Are you using a modded trader?");

            assort.BarterScheme[assortId] = new List<List<BarterScheme>>([
                [new BarterScheme { Count = t.Price, Template = currentTrader.Base.Currency.Value.GetCurrencyTpl() }]
            ]);

            assort.LoyalLevelItems[assortId] = t.LoyaltyLevel;

            count++;
            if (logger.IsLogEnabled(LogLevel.Debug)) LogResult(t);
        }

        if (logger.IsLogEnabled(LogLevel.Debug)) logger.Debug($"Successfully injected {count} assort(s).");
    }

    private void LogResult(TraderConfig.TraderStruct t) {
        var result = JsonSerializer.Deserialize<JsonNode>(httpResponseUtil.GetBody(t))!["data"]!
                                   .ToString();
        logger.Debug($"{result}");
    }
}