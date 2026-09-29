using System.Collections.Generic;
using EFT;
using Newtonsoft.Json;

namespace _RepairMaxDurability.ServerJsonStructures;

public class ItemEventRequest {
    [JsonProperty("data")]
    public List<RepairAction> Data { get; set; }
    [JsonProperty("tm")]
    public long Tm { get; set; }
    [JsonProperty("reload")]
    public int Reload { get; set; }
}

public class RepairAction {
    [JsonProperty("Action")]
    public string Action { get; set; } = "MaxDuraRepair";
    [JsonProperty("item")]
    public MongoID Item { get; set; } // weapon id
    [JsonProperty("kit")]
    public MongoID Kit { get; set; } // kit id
}