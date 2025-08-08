using _RepairMaxDurability.Injectors;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;

namespace _RepairMaxDurability.Helpers;

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

    public static ItemAssort CreateAssort(MongoId             itemId,       MongoId assortId, CurrencyType currencyType,
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