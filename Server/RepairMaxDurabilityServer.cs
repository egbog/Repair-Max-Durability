using _RepairMaxDurability.Injectors;
using _RepairMaxDurability.Services;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services.Mod;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace _RepairMaxDurability;

public record ModMetadata : AbstractModMetadata {
    public override string                                          ModGuid           { get; init; } = "com.egbog.repairmaxdurability";
    public override string                                          Name              { get; init; } = "RepairMaxDurability";
    public override string                                          Author            { get; init; } = "egbog";
    public override List<string>?                                   Contributors      { get; init; }
    public override SemanticVersioning.Version                      Version           { get; init; } = new("0.0.1"); 
    public override SemanticVersioning.Version                      SptVersion        { get; init; } = new("4.0.0");
    public override List<string>?                                   Incompatibilities { get; init; }
    public override Dictionary<string, SemanticVersioning.Version>? ModDependencies   { get; init; } = new();
    public override string?                                         Url               { get; init; }  = "https://github.com/egbog/Repair-Max-Durability";
    public override bool?                                           IsBundleMod       { get; init; }  = false;
    public override string                                          License           { get; init; } = "MIT";
}

[Injectable(TypePriority = OnLoadOrder.TraderCallbacks + 1)]
public class RepairMaxDurability(
    ISptLogger<RepairMaxDurability> logger,
    CustomItemService               customItem,
    GetConfig                       config,
    AssortService                   assortService,
    CraftService                    craftService) : IOnLoad {
    public static bool        Debug;
    public static ModMetadata Mod = new();
    public Task OnLoad() {
        Debug = config.Debug || logger.IsLogEnabled(LogLevel.Debug);

        MongoId itemId   = "86afd148ac929e6eddc5e370"; // repair kit id
        MongoId assortId = "db6e9955c9672e4fdd7e38ad"; // pregenerated mongoid
        MongoId craftId  = "6747a15d68e0b74658000001"; // pregenerated mongoid

        var maxRepairKit = new NewItemFromCloneDetails {
            ItemTplToClone       = ItemTpl.REPAIRKITS_WEAPON_REPAIR_KIT, //5910968f86f77425cf569c32 weaprepairkit
            ParentId             = "616eb7aea207f41933308f46",
            NewId                = itemId,
            FleaPriceRoubles     = config.FleaPrice,
            HandbookPriceRoubles = config.FleaPrice,
            HandbookParentId     = "5b47574386f77428ca22b345",
            Locales = new Dictionary<string, LocaleDetails> {
                {
                    "en",
                    new LocaleDetails {
                        Name      = "Spare firearm parts",
                        ShortName = "Spare firearm parts",
                        Description =
                            "A collection of spare parts such as bolt carrier groups, firing pins, springs, and other common wear items."
                    }
                }
            },
            OverrideProperties = new TemplateItemProperties {
                Weight            = 1.4,
                MaxRepairResource = config.MaxRepairResource,
                Width             = 2
            }
        };

        customItem.CreateItemFromClone(maxRepairKit);

        try {
            craftService.AddCraft(itemId, craftId);
            assortService.AddAssort(itemId, assortId);
            logger.Success($"{Mod.Name} v{Mod.Version}: Loaded successfully");
        }
        catch (Exception ex) {
            logger.Error($"{Mod.Name} v{Mod.Version}: Failed to inject crafts or assorts: [{ex.Message}]");
        }

        return Task.CompletedTask;
    }
}