using System.Reflection;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Hideout;

namespace _RepairMaxDurability;

[Injectable]
public class GetTraderConfig(ModHelper modHelper) {
    // Optionally expose the full config if needed
    public TraderConfig Config { get; } = modHelper
        .GetJsonDataFromFile<
            TraderConfig>(modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly()) + "/config",
                          "config.json");
    // Forwarded properties
    public int                             FleaPrice         { get => Config.FleaPrice; }
    public int                             MaxRepairResource { get => Config.MaxRepairResource; }
    public List<TraderConfig.TraderStruct> Traders           { get => Config.Traders; }
    public int                             CraftTime         { get => Config.CraftTime; }
    public List<Requirement>               Requirements      { get => Config.Requirements; }

}
public class TraderConfig {
    public class TraderStruct {
        public required string Name         { get; set; }
        public required bool   Enabled      { get; set; }
        public required int    Price        { get; set; }
        public required int    LoyaltyLevel { get; set; }
        public required int    BuyLimit     { get; set; }
        public required int    Stock        { get; set; }
    }

    public required int                FleaPrice         { get; set; }
    public required int                MaxRepairResource { get; set; }
    public required List<TraderStruct> Traders           { get; set; }
    public required int                CraftTime         { get; set; }
    public required List<Requirement>  Requirements      { get; set; }
}