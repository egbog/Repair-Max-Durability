using System.Collections.Generic;
using EFT;
using Newtonsoft.Json;

namespace _RepairMaxDurability.ServerJsonStructures;

public class ItemEventData {
    [JsonProperty("warnings")]
    public List<Warning> Warnings { get; set; }
    [JsonProperty("profileChanges")]
    public Dictionary<string, ProfileChange> ProfileChanges { get; set; }
}

public class ProfileChange {
    [JsonProperty("items")]
    public ItemChanges Items { get; set; }
}

public class ItemChanges {
    [JsonProperty("change")]
    public List<Items> Change { get; set; } // ← reuse your existing item POCO
}

public class Warning {
    [JsonProperty("errmsg")]
    public string ErrorMessage { get; set; }
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