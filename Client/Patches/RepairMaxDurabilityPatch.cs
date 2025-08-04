#nullable enable
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

    public static T? Post<T>(string url, string data) {
        return JsonConvert.DeserializeObject<T>(RequestHandler.PostJson(url, data));
    }

    public static bool CheckOwner(Item item) {
        return item.Owner.OwnerType == EOwnerType.Profile;
    }

    protected override MethodBase GetTargetMethod() {
        return typeof(ItemView).GetMethod("method_8", BindingFlags.Instance | BindingFlags.Public);
    }

    [PatchPrefix]
    public static bool Prefix(ref ItemContextClass dragItemContext, ref PointerEventData eventData) {
        // make sure item is dragged onto another item, prevent null pointers
        if (!eventData.pointerEnter) return true; // return and run original method

        ItemView?                 componentInParent = eventData.pointerEnter.GetComponentInParent<ItemView>();
        ItemContextAbstractClass? targetItemContextAbstractClass = componentInParent?.ItemContext;
        Item?                     targetItem = targetItemContextAbstractClass?.Item;

        // check target item ownership
        if (targetItem == null || !CheckOwner(targetItem)) return true;

        // make sure the item being dragged is the repair kit
        // only repair Weapon types
        if (dragItemContext.Item.TemplateId                   != "86afd148ac929e6eddc5e370" ||
            ItemViewFactory.GetItemType(targetItem.GetType()) != EItemType.Weapon)
            return true;

        // must contain a RepairableComponent
        if (!targetItem.TryGetItemComponent(out RepairableComponent repairableComponent)) return true;

        // check if the durability is below 100
        // set isRepairable to true if it is below 100
        if (Mathf.Approximately(repairableComponent.MaxDurability, 100f)) // item already at 100 max durability
        {
            Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.ErrorMessage);
            NotificationManagerClass.DisplayMessageNotification("Weapon already at maximum durability",
                                                                ENotificationDurationType.Default,
                                                                ENotificationIconType.Alert);
            dragItemContext.DragCancelled();
            //Plugin.Log.LogInfo("NO REPAIR NECESSARY");
            return false;
        }

        // current durability is not at the maximum it can be at the moment
        if (!Mathf.Approximately(repairableComponent.Durability, repairableComponent.MaxDurability)) {
            Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.ErrorMessage);
            NotificationManagerClass.DisplayMessageNotification("Weapon not clean enough to install new parts",
                                                                ENotificationDurationType.Default,
                                                                ENotificationIconType.Alert);
            dragItemContext.DragCancelled();
            //Plugin.Log.LogInfo("WEAPON NOT REPAIRED ENOUGH");
            return false;
        }

        // if code runs to here, then we satisfied all conditions to start the repair process

        // setup json to send to server
        var info = new RepairInfo { ItemId = targetItem.Id, KitId = dragItemContext.Item.Id };

        // get data back from server
        ParseProfile.Profile? result =
            Post<ParseProfile.Profile>("/maxdura/checkdragged", JsonConvert.SerializeObject(info));

        // set durability and repair kit resource
        try {
            ParseProfile.UpdateValues(result, repairableComponent, dragItemContext.Item);
            // sound and notification
            Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.RepairComplete);
            NotificationManagerClass.DisplayMessageNotification($"{"Weapon successfully repaired to"
                .Localized()} {repairableComponent.MaxDurability:F1}");
            //Plugin.Log.LogInfo("REPAIR SUCCESSFUL");
        }
        catch (Exception ex) {
            Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.ErrorMessage);
            NotificationManagerClass.DisplayMessageNotification("Repair failed: Server error",
                                                                ENotificationDurationType.Default,
                                                                ENotificationIconType.Alert);
            Plugin.Log.LogError(ex.Message);
        }

        // whether repair fails or completes
        // stop original code from executing
        // in this case prevent repair window from opening
        return false;
    }
}