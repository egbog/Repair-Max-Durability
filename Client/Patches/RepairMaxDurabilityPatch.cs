using System;
using System.Reflection;
using _RepairMaxDurability.Utils;
using Comfort.Common;
using EFT;
using EFT.Communications;
using EFT.InventoryLogic;
using EFT.UI;
using EFT.UI.DragAndDrop;
using Newtonsoft.Json;
using SPT.Common.Http;
using SPT.Reflection.Patching;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _RepairMaxDurability.Patches;

public class RepairMaxDurabilityPatch : ModulePatch {
    public class RepairInfo {
        public MongoID ItemId { get; set; }
        public MongoID KitId  { get; set; }
    }

    public static T Post<T>(string url, string data) {
        return JsonConvert.DeserializeObject<T>(RequestHandler.PostJson(url, data));
    }

    public static bool CheckOwner(Item item) {
        return item.Owner.OwnerType == EOwnerType.Profile;
    }

    public static bool CheckName(Item item) {
        return item.LocalizedName().Contains("Spare firearm parts");
    }

    protected override MethodBase GetTargetMethod() {
        return typeof(ItemView).GetMethod("method_8", BindingFlags.Instance | BindingFlags.Public);
    }

    [PatchPrefix]
    public static bool Prefix(ref ItemContextClass dragItemContext, ref PointerEventData eventData) {
        // make sure item is dragged onto another item, prevent null pointers
        if (!eventData?.pointerEnter) return true; // return and skip original method

        ItemView                 componentInParent = eventData.pointerEnter.GetComponentInParent<ItemView>();
        ItemContextAbstractClass targetItemContextAbstractClass = componentInParent?.ItemContext;
        Item                     targetItem = targetItemContextAbstractClass?.Item;

        if (targetItem == null) return true; // return and skip original method

        // make sure it's an item that can actually be repaired ie. weapon
        // must contain a RepairableComponent
        targetItem.TryGetItemComponent(out RepairableComponent repairableComponent);
        // make sure we aren't repairing armor
        targetItem.TryGetItemComponent(out ArmorComponent armorComponent);

        if (!(CheckName(dragItemContext.Item) && CheckOwner(targetItem))) return false;

        // check target item ownership
        if (!(repairableComponent != null && armorComponent == null)) return false;

        // only do work when our item is dragged AND dragged onto another item
        // make sure the item being dragged is the repair kit
        // check if the durability is below 100
        // set isRepairable to true if it is below 100
        if (Mathf.Approximately(repairableComponent.MaxDurability, 100f)) // item already at 100 max durability
        {
            Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.ErrorMessage);
            NotificationManagerClass.DisplayMessageNotification("Weapon already at maximum durability",
                                                                ENotificationDurationType.Default,
                                                                ENotificationIconType.Alert);
            dragItemContext.DragCancelled();
            //log.LogInfo("NO REPAIR NECESSARY");
            return false;
        }

        // current durability is not at the maximum it can be at the moment
        if (!Mathf.Approximately(repairableComponent.Durability, repairableComponent.MaxDurability)) {
            Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.ErrorMessage);
            NotificationManagerClass.DisplayMessageNotification("Weapon not clean enough to install new parts",
                                                                ENotificationDurationType.Default,
                                                                ENotificationIconType.Alert);
            dragItemContext.DragCancelled();
            return false;
        }

        // if code runs to here, then we satisfied all conditions to start the repair process

        var info = new RepairInfo() // setup json to send to server
        {
            Item = targetItem, Kit = dragItemContext.Item
        };

        var prof = new ParseProfile(Post("/maxdura/checkdragged",
                                         JsonConvert.SerializeObject(info))); // instantiate our profile parsing class
        bool updated = prof.UpdateValues(repairableComponent,
                                         dragItemContext.Item); // set durability and repair kit resource
        string status = updated ? "REPAIR SUCCESSFUL" : "REPAIR FAILED: JSON ERROR";

        if (updated) // success
        {
            // sound and notification
            Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.RepairComplete);
            NotificationManagerClass.DisplayMessageNotification(string.Format("{0} {1:F1}",
                                                                              "Weapon successfully repaired to"
                                                                                  .Localized(),
                                                                              repairableComponent.MaxDurability));
            log.LogInfo(status);
        }
        else // failure for some reason
        {
            Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.ErrorMessage);
            NotificationManagerClass.DisplayMessageNotification("Repair failed: JSON error",
                                                                ENotificationDurationType.Default,
                                                                ENotificationIconType.Alert);
            log.LogError(status);
        }

        // whether repair fails or completes
        // stop original code from executing
        // in this case prevent repair window from opening
        return false;
    }
}