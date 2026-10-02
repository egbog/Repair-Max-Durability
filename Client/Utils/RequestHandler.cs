using System;
using Newtonsoft.Json;

namespace _RepairMaxDurability.Utils;

public class RequestHandler {
    public static T SendRequest<T>(string url, object data) {
        string serializedData       = JsonConvert.SerializeObject(data);
        string response             = SPT.Common.Http.RequestHandler.PostJson(url, serializedData);
        T      deserializedResponse = JsonConvert.DeserializeObject<T>(response) ?? throw new Exception("Null response from server");

        return deserializedResponse;
    }
}