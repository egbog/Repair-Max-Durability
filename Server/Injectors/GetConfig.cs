using System.Reflection;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Hideout;

namespace _RepairMaxDurability.Injectors;

[Injectable]
public class GetConfig(ModHelper modHelper) {
    // Optionally expose the full config if needed
    public Config Config { get; } =
        modHelper.GetJsonDataFromFile<Config>(modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly()) + "/config",
                                              "config.json");
    // Forwarded properties
    public int                       FleaPrice         { get => Config.FleaPrice; }
    public int                       MaxRepairResource { get => Config.MaxRepairResource; }
    public List<Config.TraderStruct> Traders           { get => Config.Traders; }
    public List<Config.CraftStruct>  Crafts            { get => Config.Crafts; }
}

public record Config {
    public record TraderStruct {
        public required string Name         { get; init; }
        public required bool   Enabled      { get; init; }
        public required int    Price        { get; init; }
        public required int    LoyaltyLevel { get; init; }
        public required int    BuyLimit     { get; init; }
        public required int    Stock        { get; init; }
    }

    public record CraftStruct {
        public required bool              Enabled       { get; init; }
        public required int               CraftTime     { get; init; }
        public required int               AmountCrafted { get; init; }
        public required List<Requirement> Requirements  { get; init; }
    }

    public required int                FleaPrice         { get; init; }
    public required int                MaxRepairResource { get; init; }
    public required List<TraderStruct> Traders           { get; init; }
    public required List<CraftStruct>  Crafts            { get; init; }
}