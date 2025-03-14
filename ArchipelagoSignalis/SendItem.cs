using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Archipelago.MultiClient.Net.Enums;
using HarmonyLib;
using MelonLoader;

namespace ArchipelagoSignalis
{
    class SendItem : MelonMod
    {
        public static void SendCheckToArchipelago(string itemName)
        {
            if (null != ArchipelagoHelper.Session)
            {
                MelonLogger.Msg($"Sending item {itemName} to Archipelago");
                long locationId = ArchipelagoHelper.Session.Locations.GetLocationIdFromName(ArchipelagoHelper.GameName, itemName);
                ArchipelagoHelper.Session.Locations.CompleteLocationChecks(locationId);
            }
            else
            {
                MelonLogger.Msg("Archipelago session is null, not sending item");
            }
        }
        
        [HarmonyPatch(typeof(ItemPickup), "release")]
        public static class DetectItemPickup
        {
            private static int numItemsInInventory = 0;

            private static void Prefix(ItemPickup __instance)
            {
                numItemsInInventory = InventoryManager.elsterItems.Count;
                var item = __instance._item;
                var playerState = PlayerState.currentRoom;

                // Infinite pickups in Mynah Arena to avoid softlock
                if (playerState.roomName == "Surgery Mynah")
                {
                    __instance.dontDestroyOnPickup = true;
                }

                MelonLogger.Msg($"Item picked up: {item._item} in room: {playerState.roomName}");
            }

            private static void Postfix(ItemPickup __instance)
            {
                var currentNumItemsInInventory = InventoryManager.elsterItems.Count;
                var item = __instance._item;
                if (currentNumItemsInInventory != numItemsInInventory)
                {
                    if (string.Equals(item._item.ToString(), "FalkeSpear", StringComparison.OrdinalIgnoreCase))
                    {
                        return;
                    }
                    if (PlayerState.currentRoom.roomName == "Surgery Mynah")
                    {
                        return;
                    }

                    MelonLogger.Msg($"Removed item {item._item} from inventory");
                    InventoryManager.RemoveItem(item);

                    var fullItemName = item._item.ToString();
                    var playerState = PlayerState.currentRoom;
                    if (fullItemName.Contains("Ammo") || fullItemName.Contains("Health") || fullItemName.Contains("Injector"))
                    {
                        fullItemName += "_" + playerState.roomName;
                    }

                    SaveManagement.UpdateItemsCollected(fullItemName);

                    //TODO: Call Archipelago API for check
                    SendCheckToArchipelago(fullItemName);
                    ArchipelagoHelper.Session.DataStorage[Scope.Slot, "LastItemSent"] = fullItemName;
                }

            }
        }
    }

    [HarmonyPatch(typeof(RadioModuleAcquirer), "getRadioModule")]
    public static class DetectRadioPickup
    {
        private static void Prefix()
        {
            MelonLogger.Msg("Picked up Radio");
        }
    }
}
