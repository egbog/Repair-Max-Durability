using System;
using System.Collections.Generic;
using EFT;
using EFT.InventoryLogic;
using Newtonsoft.Json;

namespace _RepairMaxDurability.Utils;

public class ParseProfile {
    public record Profile {
        public record Item {
            public record Repairable {
                public int Durability    { get; set; }
                public int MaxDurability { get; set; }
            }

            public record RepairKit {
                public int Resource { get; set; }
            }

            public record Upd {
                public Repairable Repairable { get; set; }
                public RepairKit  RepairKit  { get; set; }
            }

            public MongoID _id { get; set; }
            public Upd     upd { get; set; }
        }

        [JsonProperty("data")]
        public List<Item> Items { get; set; }
    }

    public static void UpdateValues(Profile profile, RepairableComponent targetItemRc, Item repairKit) {
        if (profile.Items.Find(i => i._id == targetItemRc.Item.Id) is { } item) {
            targetItemRc.Durability    = item.upd.Repairable.Durability;
            targetItemRc.MaxDurability = item.upd.Repairable.MaxDurability;
            targetItemRc.Item.UpdateAttributes();
            //this.Log.LogInfo(item.LocalizedName() + " REPAIRED TO: " + repairableComponent.MaxDurability);
        }
        else // something went wrong with json sent from server
        {
            throw new Exception($"Target item {targetItemRc.Item.Id} not found in server.");
        }

        // update repair kit resource
        if (profile.Items.Find(i => i._id == repairKit.Id) is { } kit) {
            repairKit.TryGetItemComponent(out RepairKitComponent repairKitComponent);
            repairKitComponent.Resource = kit.upd.RepairKit.Resource;
            repairKit.RaiseRefreshEvent(true);
            //Plugin.Log.LogDebug("NEW REPAIR RESOURCE: " + rkc.Resource);

            // delete repair kit at 0 resource or below
            if (repairKitComponent.Resource <= 0) {
                var traderControllerClass = (TraderControllerClass)repairKit.Parent.GetOwner();
                traderControllerClass.ThrowItem(repairKit);
                //Plugin.Log.LogDebug("DESTROYED REPAIR KIT");
            }

            // all is well - minister fudge
        }
        else {
            throw new Exception($"Target item {repairKit.Id} not found in server.");
        }
    }
}