using EFT;

namespace _RepairMaxDurability.ServerJsonStructures;

public class RepairDataRequest {
    public MongoID ItemId { get; set; }
    public MongoID KitId  { get; set; }
}