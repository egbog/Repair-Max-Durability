using System.Text.Json.Nodes;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Utils;

namespace _RepairMaxDurability.Logger;

[Injectable]
public class DebugLoggerUtil(JsonUtil jsonUtil) {
    public string? LogResult<T>(T t) {
        return jsonUtil.Deserialize<JsonNode>(jsonUtil.Serialize(t))?.ToString();
    }

}