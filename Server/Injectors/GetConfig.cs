using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Hideout;
using System.Reflection;
using System.Text.Json;

namespace _RepairMaxDurability.Injectors;

public class ConfigRegistration : IOnDIConstruct {
    public static async Task OnDIConstructAsync(IServiceCollection serviceCollection, CancellationToken cancellationToken) {
        Config config = await LoadConfigFromDiskAsync(cancellationToken);
        serviceCollection.AddSingleton(config);
    }

    // stolen from mod examples github
    private static async Task<Config> LoadConfigFromDiskAsync(CancellationToken ct) {
        string configPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ??
                                         throw new InvalidOperationException(),
                                         "config.json");

        if (!File.Exists(configPath)) {
            var defaultConfig = new Config();
            // TODO: write save config method
            //await SaveConfigToDiskAsync(defaultConfig, configPath, ct);
            return defaultConfig;
        }

        await using FileStream stream = File.OpenRead(configPath);

        // TODO: fix this formatting in r#
        Config? config = await JsonSerializer.DeserializeAsync<Config>(stream,
                                                                       new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
                                                                       ct);

        return config ?? new Config();
    }
}

public record Config {
    public bool Debug     { get; init; } = false;
    public int  FleaPrice { get; init; } = 100000;

    public int MaxRepairResource { get;      init; } = 5;
    public List<TraderStruct> Traders { get; init; } = [
        new() {
            Name         = "Mechanic",
            Enabled      = true,
            Price        = 100000,
            LoyaltyLevel = 2,
            BuyLimit     = 5,
            Stock        = 5
        },
        new() {
            Name         = "Prapor",
            Enabled      = false,
            Price        = 100000,
            LoyaltyLevel = 1,
            BuyLimit     = 50,
            Stock        = 2000
        }
    ];
    public List<CraftStruct> Crafts { get; init; } = [
        new() {
            Enabled       = true,
            CraftTime     = 3600,
            AmountCrafted = 1,
            Requirements = [
                new Requirement { Type = "Tool", TemplateId = "590c2e1186f77425357b6124" },
                new Requirement { Type = "Item", TemplateId = "5bc9b355d4351e6d1509862a", IsFunctional = false, Count = 1 },
                new Requirement { Type = "Item", TemplateId = "5d1c819a86f774771b0acd6c", IsFunctional = false, Count = 1 },
                new Requirement { Type = "Area", AreaType   = 10, RequiredLevel                        = 1 }
            ]
        }
    ];
}

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