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
    class SendLocation : MelonMod
    {
        public static void SendCheckToArchipelago(string itemName)
        {
            var translatedArchipelagoItemName = ArchipelagoStart.GetArchipelagoItemNameFromLocation(itemName, LevelSelect.currentScene, PlayerState.currentRoom.roomName);
            SaveManagement.UpdateLocationsChecked(translatedArchipelagoItemName);

            if (null != ArchipelagoHelper.Session && ArchipelagoHelper.Session.Socket.Connected)
            {
                MelonLogger.Msg($"Sending item {itemName} to Archipelago");
                long locationId = ArchipelagoHelper.Session.Locations.GetLocationIdFromName(ArchipelagoHelper.GameName, translatedArchipelagoItemName);
                MelonLogger.Msg($"Location ID: {locationId}");
                ArchipelagoHelper.Session.Locations.CompleteLocationChecks(locationId);
            }
            else
            {
                MelonLogger.Msg("Archipelago session is null, not sending item");
            }
        }

        public static void SendPhotoOfAlinaLocation(string sceneName)
        {
            if (sceneName.Contains("LOV") && !SaveManagement.LocationsChecked.Contains("Receive Photo of Alina"))
            {
                MelonLogger.Msg("Manually sending location Receive Photo of Alina to Archipelago");
                SendCheckToArchipelago("AlinaPhoto");
                RemoveItemFromInventory("AlinaPhoto");
            }
        }

        private static void RemoveItemFromInventory(string itemName)
        {
            List<string> elsterItems = new List<string>();
            foreach (var item in InventoryManager.elsterItems)
            {
                elsterItems.Add(item.key._item.ToString());
            }

            foreach (AnItem item in InventoryManager.allItems.Values)
            {
                if (string.Equals(itemName, item.name, StringComparison.OrdinalIgnoreCase))
                {
                    InventoryManager.RemoveItem(item);
                }
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

                    if (!string.Equals(item._item.ToString(), "YellowKing", StringComparison.OrdinalIgnoreCase)
                        || !SaveManagement.ItemsReceived.Contains(item.name))
                    {
                        InventoryManager.RemoveItem(item);
                        MelonLogger.Msg($"Removed item {item._item} from inventory");
                    }
                    else
                    {
                        MelonLogger.Msg("Yellow King location checked, storing all items");
                        InventoryManager.storeAllItems();
                        RetrieveItem.AddItemToInventory("YellowKing");
                        // TODO: Add all items to inventory EXCEPT yellow king
                    }


                    var fullItemName = item._item.ToString();
                    var playerState = PlayerState.currentRoom;
                    if (fullItemName.Contains("Ammo") || fullItemName.Contains("Health") || fullItemName.Contains("Injector"))
                    {
                        fullItemName += "_" + playerState.roomName;
                    }

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
