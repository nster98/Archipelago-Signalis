﻿using System;
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
        public static Queue<string> RemoveItemQueue = new();

        public static List<PlayerState.gameStates> validGameStatesForItems =
        [
            PlayerState.gameStates.play,
            PlayerState.gameStates.eventScreen
        ];

        public static void SendCheckToArchipelago(string itemName, string position)
        {
            var translatedArchipelagoItemName = ArchipelagoStart.GetArchipelagoItemNameFromLocation(itemName, LevelSelect.currentScene, PlayerState.currentRoom.roomName, position);
            SaveManagement.UpdateLocationsChecked(translatedArchipelagoItemName);

            if (null != ArchipelagoHelper.Session && ArchipelagoHelper.Session.Socket.Connected)
            {
                MelonLogger.Msg($"Sending item {translatedArchipelagoItemName} to Archipelago");
                long locationId = ArchipelagoHelper.Session.Locations.GetLocationIdFromName(ArchipelagoHelper.GameName, translatedArchipelagoItemName);
                MelonLogger.Msg($"Location ID: {locationId}");
                ArchipelagoHelper.Session.Locations.CompleteLocationChecks(locationId);
            }
            else
            {
                MelonLogger.Msg("Archipelago session is null, not sending item");
            }
        }

        public static void SendCheckToArchipelago(string itemName, string scene, string position)
        {
            var translatedArchipelagoItemName = ArchipelagoStart.GetArchipelagoItemNameFromLocation(itemName, scene, PlayerState.currentRoom.roomName, position);
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
                SendCheckToArchipelago("AlinaPhoto", "LOV", "");
                RemoveItemFromInventory("AlinaPhoto", 1);
            }
        }

        public static void RemoveItemFromInventory(string itemName, int count)
        {
            // TODO: This is likely more efficient now that it will end up calling here but I don't want to test it
            // foreach (AnItem item in InventoryManager.allItems.Values)
            // {
            //     if (string.Equals(itemName, item.name, StringComparison.OrdinalIgnoreCase))
            //     {
            //         InventoryManager.RemoveItem(item, count);
            //         break;
            //     }
            // }

            if (!validGameStatesForItems.Contains(PlayerState.gameState))
            {
                MelonLogger.Msg($"Queuing item to remove : {itemName},{count}");
                RemoveItemQueue.Enqueue(itemName + "," + count);
            }
            else
            {
                var itemCount = count;
                List<string> elsterItems = new List<string>();
                do
                {
                    elsterItems.Clear();
                    MelonLogger.Msg($"Removing item {itemName} from inventory");
                    foreach (AnItem item in InventoryManager.allItems.Values)
                    {
                        if (string.Equals(itemName, item.name, StringComparison.OrdinalIgnoreCase))
                        {
                            InventoryManager.RemoveItem(item);
                        }
                    }
                    foreach (var item in InventoryManager.elsterItems)
                    {
                        elsterItems.Add(item.key._item.ToString());
                    }
                    itemCount--;
                } while (itemCount != 0);
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

                MelonLogger.Msg($"Position: {__instance.gameObject.transform.position.x} {__instance.gameObject.transform.position.y}");

                // Infinite pickups in Mynah Arena to avoid softlock
                if (playerState.roomName == "Surgery")
                {
                    __instance.dontDestroyOnPickup = true;
                }

                MelonLogger.Msg($"Item picked up: {item._item} in room: {playerState.roomName}");
            }

            private static void Postfix(ItemPickup __instance)
            {
                var currentNumItemsInInventory = InventoryManager.elsterItems.Count;
                var item = __instance._item;
                var positionX = __instance.gameObject.transform.position.x.ToString();
                if (currentNumItemsInInventory != numItemsInInventory
                    || (currentNumItemsInInventory == numItemsInInventory && IsStackingItem(item._item.ToString())))
                {
                    if (string.Equals(item._item.ToString(), "FalkeSpear", StringComparison.OrdinalIgnoreCase))
                    {
                        return;
                    }
                    if (PlayerState.currentRoom.roomName == "Surgery")
                    {
                        return;
                    }

                    var archipelagoLocation = ArchipelagoStart.GetArchipelagoItemNameFromLocation(
                        item._item.ToString(),
                        LevelSelect.currentScene,
                        PlayerState.currentRoom.roomName,
                        positionX);
                    var isYellowKing = string.Equals(item._item.ToString(), "YellowKing", StringComparison.OrdinalIgnoreCase);
                    var isDuplicateItem = CheckForDuplicateItem(item._item.ToString());
                    var locationChecked = SaveManagement.LocationsChecked.Contains(archipelagoLocation);
                    var locationInRightRoom = !archipelagoLocation.Equals("null");

                    MelonLogger.Msg($"isYellowKing {isYellowKing} : isDuplicateItem {isDuplicateItem} : locationChecked : {locationChecked} : locationInRightRoom : {locationInRightRoom}");

                    if (!isYellowKing && locationInRightRoom)
                    {
                        RemoveItemFromInventory(item._item.ToString(), __instance.count);
                        MelonLogger.Msg($"Removed item {item._item} from inventory");
                    }
                    else if (string.Equals(item._item.ToString(), "YellowKing", StringComparison.OrdinalIgnoreCase))
                    {
                        MelonLogger.Msg("Yellow King location checked, storing all items");
                        RemoveItemFromInventory("YellowKing", 1);
                        InventoryManager.storeAllItems();
                        RetrieveItem.AddItemToInventory("YellowKing");
                    }

                    var fullItemName = item._item.ToString();
                    var playerState = PlayerState.currentRoom;

                    SendCheckToArchipelago(fullItemName, positionX);
                    ArchipelagoHelper.Session.DataStorage[Scope.Slot, "LastItemSent"] = fullItemName;
                }

            }

            private static bool CheckForDuplicateItem(string item)
            {
                var isHealth = SaveManagement.ItemsReceived.Contains(item) && (item.Contains("Health") || item.Contains("Injector"));
                var isAmmo = SaveManagement.ItemsReceived.Contains(item) && item.Contains("Ammo");
                var isFlare = SaveManagement.ItemsReceived.Contains(item) && item.Contains("SignalFlare");
                return SaveManagement.ItemsReceived.Contains(item) || (isHealth || isAmmo || isFlare);
            }

            private static bool IsStackingItem(string item)
            {
                var isHealth = (item.Contains("Health") || item.Contains("Injector"));
                var isAmmo = item.Contains("Ammo");
                var isFlare = item.Contains("SignalFlare");
                var isTaser = item.Contains("Taser");
                return (isHealth || isAmmo || isFlare || isTaser);
            }
        }
    }

    [HarmonyPatch(typeof(RadioModuleAcquirer), "getRadioModule")]
    public static class DetectRadioPickup
    {
        private static void Postfix()
        {
            // Ensure that Radio location can not be checked unless from the cutscene to pick it up
            if (PlayerState.currentRoom.roomName == "Evidence Storage")
            {
                MelonLogger.Msg("Sending location for Radio Module");
                long locationId = ArchipelagoHelper.Session.Locations.GetLocationIdFromName(ArchipelagoHelper.GameName, "Radio Module");
                MelonLogger.Msg($"Location ID: {locationId}");
                ArchipelagoHelper.Session.Locations.CompleteLocationChecks(locationId);

                // If Radio hasn't been sent as an item, keep the radio off
                if (!RadioManager.moduleInstalled)
                {
                    RadioManager.moduleInstalled = false;
                }
            }
        }
    }
}
