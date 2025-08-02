using System.Text.Json;
using System.Text.Json.Nodes;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Utils;

namespace _RepairMaxDurability.Logger;

[Injectable]
public class DebugLoggerUtil(HttpResponseUtil httpResponseUtil) {
    public string LogResult<T>(T t) {
        return JsonSerializer.Deserialize<JsonNode>(httpResponseUtil.GetBody(t))!["data"]!.ToString();
    }
}