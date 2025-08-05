using System;
using Newtonsoft.Json;
using SPT.Common.Http;

namespace _RepairMaxDurability.Utils;

public class Requester {
    public static T1 SendRequest<T1, T2>(string url, T2 data) {
        string serializedData = JsonConvert.SerializeObject(data);
        string response       = RequestHandler.PostJson(url, serializedData);
        T1 deserializedResponse = JsonConvert.DeserializeObject<T1>(response) ??
                                  throw new Exception($"Null response from server");

        return deserializedResponse;
    }
}