using _RepairMaxDurability.Injectors;
using _RepairMaxDurability.Services;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Services.Modding.Custom;

namespace _RepairMaxDurability;

public record ModMetadata : IModMetadata {
    public string ModGuid { get; init; } = "com.egbog.repairmaxdurability";
    public string Name { get; init; } = "RepairMaxDurability";
    public string Author { get; init; } = "egbog";
    public List<string>? Contributors { get; init; }
    public SemanticVersioning.Version Version { get; init; } = new("2.2.0");
    public SemanticVersioning.Range SptVersion { get; init; } = new("~4.1.0");
    public List<string>? Incompatibilities { get; init; }
    public Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; }
    public string? Url { get; init; } = "https://github.com/egbog/Repair-Max-Durability";
    public string License { get; init; } = "MIT";
    public bool HasPrepatcher { get; init; } = false;
}

[Injectable(TypePriority = OnLoadOrder.Preload)]
public class RepairMaxDurability(
    ISptLogger<RepairMaxDurability> logger,
	CustomItemService               customItem,
	Config                          config,
    AssortService                   assortService,
    CraftService                    craftService) : IOnLoad {
    public static          bool        Debug;
    public static readonly ModMetadata Mod = new();

    public Task OnLoadAsync(CancellationToken cancellationToken) {
        Debug = config.Debug || logger.IsLogEnabled(LogLevel.Debug);

        MongoId itemId   = "86afd148ac929e6eddc5e370"; // repair kit id
        MongoId assortId = "db6e9955c9672e4fdd7e38ad"; // pregenerated mongoid
        MongoId craftId  = "6747a15d68e0b74658000001"; // pregenerated mongoid
        

        var maxRepairKit = new NewItemFromCloneDetails {
            ItemTplToClone       = ItemTpl.REPAIRKITS_WEAPON_REPAIR_KIT, //5910968f86f77425cf569c32 weaprepairkit
            ParentId             = "616eb7aea207f41933308f46",
            NewId                = itemId,
            NewItemName			 = "Max Durability Repair Kit",
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
                Weight = 1.4, MaxRepairResource = config.MaxRepairResource, Width = 2
            }
        };

        customItem.CreateItemFromClone(maxRepairKit);

        try {
            craftService.AddCraft(itemId, craftId);
            assortService.AddAssort(itemId, assortId);
            logger.Success($"[{Mod.Name}] Loaded successfully");
        }
        catch (Exception ex) {
            logger.Error($"[{Mod.Name}] Failed to inject crafts or assorts: [{ex.Message}]");
        }

        return Task.CompletedTask;
    }
}