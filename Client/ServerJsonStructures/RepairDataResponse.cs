using System.Collections.Generic;
using EFT;
using Newtonsoft.Json;

namespace _RepairMaxDurability.ServerJsonStructures;

public record RepairDataResponse {
    [JsonProperty("data")]
    public List<Items> Items { get; set; }
}

public record Items {
    [JsonProperty("_id")]
    public MongoID Id { get; set; }
    [JsonProperty("upd")]
    public Upd Upd { get; set; }
}

public record Upd {
    public Repairable Repairable { get; set; }
    public RepairKit  RepairKit  { get; set; }
}

public record Repairable {
    public float Durability    { get; set; }
    public float MaxDurability { get; set; }
}

public record RepairKit {
    public int Resource { get; set; }
}