#nullable enable
using _RepairMaxDurability.ServerJsonStructures;
using _RepairMaxDurability.Utils;
using Comfort.Common;
using EFT.Communications;
using EFT.InventoryLogic;
using EFT.UI;
using EFT.UI.DragAndDrop;
using RuntimeInspector;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EFT;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _RepairMaxDurability.Patches;

public class RepairMaxDurabilityPatch : ModulePatch {
    protected override MethodBase GetTargetMethod() {
        return typeof(ItemView).GetMethod("method_8", BindingFlags.Instance | BindingFlags.Public)!;
    }

    [PatchPrefix]
    public static bool Prefix(DragItemContext dragItemContext, PointerEventData eventData) {
        // make sure item is dragged onto another item, prevent null pointers
        if (!eventData.pointerEnter) {
            return true; // return and run original method
        }

        ItemView?    componentInParent = eventData.pointerEnter.GetComponentInParent<ItemView>();
        ItemContext? targetItemContext = componentInParent?.ItemContext;
        Item?        targetItem        = targetItemContext?.Item;

        // check target item ownership
        if (targetItem == null || targetItem.Owner.OwnerType != EOwnerType.Profile) {
            return true;
        }

        // make sure the item being dragged is the repair kit
        // only repair Weapon types
        if (dragItemContext.Item.TemplateId                   != Plugin.KitId ||
            ItemViewFactory.GetItemType(targetItem.GetType()) != EItemType.Weapon) {
            return true;
        }

        // must contain a RepairableComponent
        if (!targetItem.TryGetItemComponent(out RepairableComponent repairableComponent)) {
            return true;
        }

        // check if the durability is below 100

        if (Mathf.Approximately(repairableComponent.MaxDurability, 100f)) // item already at 100 max durability
        {
            Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.ErrorMessage);
            NotificationManager.DisplayMessageNotification("Weapon already at maximum durability",
                                                           ENotificationDurationType.Default,
                                                           ENotificationIconType.Alert);
            dragItemContext.DragCancelled();
            //Plugin.Log.LogInfo("NO REPAIR NECESSARY");
            return false;
        }

        // need more precision
        //if (!Mathf.Approximately(repairableComponent.Durability, repairableComponent.MaxDurability)) {

        // current durability is not at the maximum it can be at the moment
        if (Mathf.Abs(1.0f - repairableComponent.RelativeValue) >= 0.01f) {
            Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.ErrorMessage);
            NotificationManager.DisplayMessageNotification("Weapon not clean enough to install new parts",
                                                           ENotificationDurationType.Default,
                                                           ENotificationIconType.Alert);
            dragItemContext.DragCancelled();
            //Plugin.Log.LogInfo("WEAPON NOT REPAIRED ENOUGH");
            return false;
        }

        // if code runs to here, then we satisfied all conditions to start the repair process

        // setup json to send to server
        var request = new ItemEventRequest {
            Data = [new RepairAction { Item = targetItem.Id, Kit = dragItemContext.Item.Id }], Tm = 0, Reload = 0
        };

        try {
            // get data back from server
            JsonResponse<ItemEventData>? response =
                RequestHandler.SendRequest<JsonResponse<ItemEventData>?>("/client/game/profile/items/moving", request);

            // aggregate any warnings and throw exception
            if (response?.data.Warnings?.Count > 0) {
                throw new Exception(string.Join(Environment.NewLine,
                                                response.data.Warnings.Select(w => w.ErrorMessage)));
            }

            // there's exactly one profileChanges entry (your session)
            List<Items>? changed = response?.data.ProfileChanges.Values.First().Items.Change;

            // set durability and repair kit resource
            ResponseHandler.UpdateValues(changed, repairableComponent, dragItemContext.Item);

            // sound and notification
            Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.RepairComplete);
            NotificationManager.DisplayMessageNotification($"{"Weapon successfully repaired to"
                .Localized()} {repairableComponent.MaxDurability:F1}");
            //Plugin.Log.LogInfo("REPAIR SUCCESSFUL");
        }
        catch (Exception ex) {
            Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.ErrorMessage);
            NotificationManager.DisplayMessageNotification("Repair failed: Server error",
                                                           ENotificationDurationType.Default,
                                                           ENotificationIconType.Alert);
            Plugin.Log.LogError(ex);
        }

        // whether repair fails or completes
        // stop original code from executing
        // in this case prevent repair window from opening
        return false;
    }
}