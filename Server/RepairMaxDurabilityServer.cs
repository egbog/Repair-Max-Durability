using _RepairMaxDurability.Injectors;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services.Mod;

namespace _RepairMaxDurability;

public record ModMetadata : AbstractModMetadata {
    public override string                      ModGuid           { get; init; } = "com.egbog.repairmaxdurability";
    public override string                      Name              { get; init; } = "RepairMaxDurability";
    public override string                      Author            { get; init; } = "egbog";
    public override List<string>?               Contributors      { get; set; }
    public override string                      Version           { get; init; } = "0.0.1";
    public override string                      SptVersion        { get; init; } = "4.0.0";
    public override List<string>?               LoadBefore        { get; set; }
    public override List<string>?               LoadAfter         { get; set; }
    public override List<string>?               Incompatibilities { get; set; }
    public override Dictionary<string, string>? ModDependencies   { get; set; }
    public override string?                     Url               { get; set; } = "https://github.com/egbog/Repair-Max-Durability-Server";
    public override bool?                       IsBundleMod       { get; set; }  = false;
    public override string                      License           { get; init; } = "MIT";
}

[Injectable(TypePriority = OnLoadOrder.TraderCallbacks + 1)]
public class RepairMaxDurability(
    ISptLogger<RepairMaxDurability> logger,
    CustomItemService               customItem,
    GetConfig                       config,
    AssortInjector                  assortInjector,
    CraftInjector                   craftInjector) : IOnLoad {
    public Task OnLoad() {
        var metaData = new ModMetadata();

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
                            "Spare parts such as bolt carrier groups, firing pins and other common wear items. Enough to make about 5 repairs."
                    }
                }
            },
            OverrideProperties = new Props {
                Name      = "Spare firearm parts",
                ShortName = "Spare firearm parts",
                Description =
                    "Spare parts such as bolt carrier groups, firing pins and other common wear items. Enough to make approximately 5 repairs.",
                Weight            = 1,
                MaxRepairResource = config.MaxRepairResource,
                Height            = 2,
                Width             = 2,
                TargetItemFilter  = ["5422acb9af1c889c16000029"]
            }
        };

        customItem.CreateItemFromClone(maxRepairKit);

        try {
            craftInjector.InjectCraft(itemId, craftId);
            assortInjector.InjectAssort(itemId, assortId);
            logger.Success($"{metaData.Name} v{metaData.Version}: Loaded successfully");
        }
        catch (Exception ex) {
            logger.Error($"{metaData.Name} v{metaData.Version}: Failed to inject crafts or assorts: [{ex.Message}]");
        }

        return Task.CompletedTask;
    }
}