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

public class Config {
    public class TraderStruct {
        public required string Name         { get; set; }
        public required bool   Enabled      { get; set; }
        public required int    Price        { get; set; }
        public required int    LoyaltyLevel { get; set; }
        public required int    BuyLimit     { get; set; }
        public required int    Stock        { get; set; }
    }

    public class CraftStruct {
        public required bool              Enabled      { get; set; }
        public required int               CraftTime    { get; set; }
        public required List<Requirement> Requirements { get; set; }
    }

    public required int                FleaPrice         { get; set; }
    public required int                MaxRepairResource { get; set; }
    public required List<TraderStruct> Traders           { get; set; }
    public required List<CraftStruct>  Crafts            { get; set; }
}