using System;
using _RepairMaxDurability.ServerJsonStructures;
using Newtonsoft.Json;
using SPT.Common.Http;

namespace _RepairMaxDurability.Utils;

public class RequestHandler {
    public static T SendRequest<T>(string url, object data) where T : IRepairDataResponse {
        string serializedData = JsonConvert.SerializeObject(data);
        string response       = SPT.Common.Http.RequestHandler.PostJson(url, serializedData);
        T deserializedResponse = JsonConvert.DeserializeObject<T>(response) ??
                                 throw new Exception($"Null response from server");

        return deserializedResponse;
    }
}